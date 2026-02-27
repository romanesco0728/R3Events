using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace R3EventsGenerator.Tests.Shared.Utilities;

public static class CSharpGeneratorRunnerCore
{
    private static readonly object SyncRoot = new();
    private static Compilation? baseCompilation;

    /// <summary>
    /// Ensures a baseline Roslyn compilation is initialized exactly once.
    /// </summary>
    public static void InitializeCompilation()
    {
        if (baseCompilation is not null)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (baseCompilation is not null)
            {
                return;
            }

            baseCompilation = CreateBaseCompilation();
        }
    }

    /// <summary>
    /// Runs the source generator and returns all generator and compilation diagnostics.
    /// </summary>
    public static Diagnostic[] RunGenerator(
        string source,
        LanguageVersion languageVersion,
        string[] preprocessorSymbols,
        AnalyzerConfigOptionsProvider? options)
    {
        InitializeCompilation();

        var parseOptions = new CSharpParseOptions(languageVersion, preprocessorSymbols: preprocessorSymbols);
        var driver = CSharpGeneratorDriver.Create(new global::R3EventsGenerator.R3EventsGenerator())
            .WithUpdatedParseOptions(parseOptions);

        if (options is not null)
        {
            driver = (CSharpGeneratorDriver)driver.WithUpdatedAnalyzerConfigOptions(options);
        }

        var compilation = baseCompilation!.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        return diagnostics.Concat(newCompilation.GetDiagnostics()).ToArray();
    }

    /// <summary>
    /// Runs tracked incremental generator steps and returns reason summaries per tracked key.
    /// </summary>
    public static (string Key, string Reasons)[][] GetIncrementalGeneratorTrackedStepsReasons(
        string keyPrefixFilter,
        LanguageVersion languageVersion,
        params string[] sources)
    {
        InitializeCompilation();

        var parseOptions = new CSharpParseOptions(languageVersion);
        var driver = CSharpGeneratorDriver.Create(
                [new global::R3EventsGenerator.R3EventsGenerator().AsSourceGenerator()],
                driverOptions: new GeneratorDriverOptions(
                    IncrementalGeneratorOutputKind.None,
                    trackIncrementalGeneratorSteps: true))
            .WithUpdatedParseOptions(parseOptions);

        var generatorResults = sources
            .Select(source =>
            {
                var compilation = baseCompilation!.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));
                driver = driver.RunGenerators(compilation);
                return driver.GetRunResult().Results[0];
            })
            .ToArray();

        return generatorResults
            .Select(result => result.TrackedSteps
                .Where(step =>
                    step.Key.StartsWith(keyPrefixFilter, global::System.StringComparison.Ordinal) ||
                    step.Key == "SourceOutput")
                .Select(step =>
                {
                    if (step.Key == "SourceOutput")
                    {
                        var values = step.Value.Where(value =>
                            value.Inputs[0].Source.Name?.StartsWith(keyPrefixFilter, global::System.StringComparison.Ordinal) ??
                            false);

                        return (
                            step.Key,
                            Reasons: string.Join(
                                ", ",
                                values.SelectMany(value => value.Outputs).Select(output => output.Reason).ToArray())
                        );
                    }

                    return (
                        Key: step.Key.Substring(keyPrefixFilter.Length),
                        Reasons: string.Join(
                            ", ",
                            step.Value.SelectMany(value => value.Outputs).Select(output => output.Reason).ToArray())
                    );
                })
                .OrderBy(item => item.Key)
                .ToArray())
            .ToArray();
    }

    /// <summary>
    /// Creates a baseline compilation with framework and R3 references.
    /// </summary>
    private static Compilation CreateBaseCompilation()
    {
        var baseAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var systemAssemblies = Directory.GetFiles(baseAssemblyPath)
            .Where(path =>
            {
                var fileName = Path.GetFileName(path);
                if (fileName.EndsWith("Native.dll", global::System.StringComparison.Ordinal))
                {
                    return false;
                }

                return fileName.StartsWith("System", global::System.StringComparison.Ordinal) ||
                       fileName is "mscorlib.dll" or "netstandard.dll";
            });

        var references = systemAssemblies
            .Append(typeof(global::R3.Observable).Assembly.Location)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();

        return CSharpCompilation.Create(
            "generatortest",
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
