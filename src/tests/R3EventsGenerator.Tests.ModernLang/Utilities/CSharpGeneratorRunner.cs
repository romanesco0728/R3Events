using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using R3EventsGenerator.Tests.Shared.Utilities;

namespace R3EventsGenerator.Tests.ModernLang.Utilities;

internal static class CSharpGeneratorRunner
{
    /// <summary>
    /// Initializes shared Roslyn compilation state for this test assembly.
    /// </summary>
    static CSharpGeneratorRunner()
    {
        CSharpGeneratorRunnerCore.InitializeCompilation();
    }

    /// <summary>
    /// Runs the generator with modern defaults used by the main test project.
    /// </summary>
    public static Diagnostic[] RunGenerator(string source, string[]? preprocessorSymbols = null, AnalyzerConfigOptionsProvider? options = null, LanguageVersion languageVersion = LanguageVersion.CSharp11, NullableContextOptions nullableContextOptions = NullableContextOptions.Disable)
    {
        return CSharpGeneratorRunnerCore.RunGenerator(source, languageVersion, preprocessorSymbols, options, nullableContextOptions);
    }

    /// <summary>
    /// Returns tracked incremental step reasons for multi-step source changes.
    /// </summary>
    public static (string Key, string Reasons)[][] GetIncrementalGeneratorTrackedStepsReasons(string keyPrefixFilter, params string[] sources)
    {
        return CSharpGeneratorRunnerCore.GetIncrementalGeneratorTrackedStepsReasons(keyPrefixFilter, LanguageVersion.CSharp11, sources);
    }

    /// <summary>
    /// Runs the generator and returns generated source texts for assertion-focused tests.
    /// </summary>
    public static string[] RunGeneratorAndGetGeneratedSources(string source, string[]? preprocessorSymbols = null, AnalyzerConfigOptionsProvider? options = null, LanguageVersion languageVersion = LanguageVersion.CSharp11, NullableContextOptions nullableContextOptions = NullableContextOptions.Disable)
    {
        return CSharpGeneratorRunnerCore.RunGeneratorAndGetGeneratedSources(source, languageVersion, preprocessorSymbols, options, nullableContextOptions);
    }
}
