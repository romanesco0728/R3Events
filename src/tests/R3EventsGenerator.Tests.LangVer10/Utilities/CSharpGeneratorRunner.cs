using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using R3EventsGenerator.Tests.Shared.Utilities;
using System.Runtime.CompilerServices;

namespace R3EventsGenerator.Tests.LangVer10.Utilities;

internal static class CSharpGeneratorRunner
{
    /// <summary>
    /// Initializes shared Roslyn compilation state for this test assembly.
    /// </summary>
    [ModuleInitializer]
    public static void InitializeCompilation()
    {
        CSharpGeneratorRunnerCore.InitializeCompilation();
    }

    /// <summary>
    /// Runs the generator with legacy defaults used by the C# 10 compatibility project.
    /// </summary>
    public static Diagnostic[] RunGenerator(string source, string[]? preprocessorSymbols = null, AnalyzerConfigOptionsProvider? options = null, LanguageVersion languageVersion = LanguageVersion.CSharp10)
    {
        return CSharpGeneratorRunnerCore.RunGenerator(source, languageVersion, preprocessorSymbols, options);
    }

    /// <summary>
    /// Returns tracked incremental step reasons for multi-step source changes in legacy tests.
    /// </summary>
    public static (string Key, string Reasons)[][] GetIncrementalGeneratorTrackedStepsReasons(string keyPrefixFilter, params string[] sources)
    {
        return CSharpGeneratorRunnerCore.GetIncrementalGeneratorTrackedStepsReasons(keyPrefixFilter, LanguageVersion.CSharp10, sources);
    }
}
