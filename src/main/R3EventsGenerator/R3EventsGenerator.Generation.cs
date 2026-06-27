using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using R3EventsGenerator.Utilities;

namespace R3EventsGenerator;

partial class R3EventsGenerator
{
    /// <summary>
    /// Extracts information about all public, non-static events declared in the specified type.
    /// </summary>
    /// <param name="targetType">
    /// The type symbol representing the target type from which to extract event method information.
    /// </param>
    /// <param name="obsoleteAttributeType">The <see cref="System.ObsoleteAttribute"/> type symbol used for symbol-based attribute matching.</param>
    /// <returns>
    /// An array containing information about each public, non-static event declared in the target type.
    /// The array is ordered by event name and will be empty if no such events are found.
    /// </returns>
    private static EquatableArray<GeneratedMethodInfo> ExtractGeneratedMethods(INamedTypeSymbol targetType, INamedTypeSymbol? obsoleteAttributeType)
    {
        var members = targetType.GetMembers();

        var count = 0;
        foreach (var member in members)
        {
            if (member is IEventSymbol { DeclaredAccessibility: Accessibility.Public, IsStatic: false })
                count++;
        }

        if (count == 0)
        {
            return new();
        }

        var methodInfos = new GeneratedMethodInfo[count];
        var index = 0;
        foreach (var member in members)
        {
            if (member is IEventSymbol { DeclaredAccessibility: Accessibility.Public, IsStatic: false } ev)
                methodInfos[index++] = GenerateMethodInfo(ev, obsoleteAttributeType);
        }

        Array.Sort(methodInfos, static (a, b) => string.Compare(a.EventName, b.EventName, StringComparison.Ordinal));
        return new(methodInfos);
    }

    private static GeneratedMethodInfo GenerateMethodInfo(IEventSymbol ev, INamedTypeSymbol? obsoleteAttributeType)
    {
        var eventType = ev.Type as INamedTypeSymbol;
        var obsoleteInfo = ExtractObsoleteInfo(ev, obsoleteAttributeType);
        ITypeSymbol? payloadType = null;

        var isNonGenericSystemEventHandler = eventType is { IsGenericType: false } &&
            eventType.ContainingNamespace?.Name is "System" &&
            eventType.MetadataName is "EventHandler";
        if (isNonGenericSystemEventHandler)
        {
            // EventHandler (non-generic) maps to R3.Unit — no payload extraction needed
        }
        else
        {
            var invoke = eventType?.DelegateInvokeMethod;
            if (invoke != null)
            {
                var ps = invoke.Parameters;
                if (ps.Length >= 1) payloadType = ps[^1].Type;
            }
        }

        TypeNameView observableElementType;
        bool useAsUnit = false;
        if (isNonGenericSystemEventHandler)
        {
            observableElementType = TypeNameView.Create("global::R3.Unit", "R3.Unit");
            useAsUnit = true;
        }
        else if (payloadType is not null)
        {
            observableElementType = TypeNameView.FromTypeSymbol(payloadType);
        }
        else
        {
            observableElementType = TypeNameView.Create("global::System.Object", "object");
        }

        var delegateType = eventType is not null
            ? TypeNameView.FromNamedTypeSymbol(eventType)
            : TypeNameView.Create("global::System.Delegate", "System.Delegate");

        return new()
        {
            EventName = ev.Name,
            ObservableElementType = observableElementType,
            UseAsUnit = useAsUnit,
            DelegateType = delegateType,
            ObsoleteInfo = obsoleteInfo,
        };
    }

    /// <summary>
    /// Extracts obsolete metadata from the source event when present.
    /// </summary>
    /// <param name="symbol">The event symbol to inspect.</param>
    /// <param name="obsoleteAttributeType">The <see cref="System.ObsoleteAttribute"/> type symbol used for symbol-based attribute matching.</param>
    /// <returns>The obsolete metadata to propagate, or <see langword="null"/> when the event is not obsolete.</returns>
    private static GeneratedObsoleteInfo? ExtractObsoleteInfo(ISymbol symbol, INamedTypeSymbol? obsoleteAttributeType)
    {
        AttributeData? obsoleteAttribute = null;
        foreach (var attribute in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, obsoleteAttributeType))
            {
                obsoleteAttribute = attribute;
                break;
            }
        }
        if (obsoleteAttribute is null)
        {
            return null;
        }

        var constructorArguments = obsoleteAttribute.ConstructorArguments;
        string? message = null;
        var hasMessageArgument = false;
        var isError = false;

        if (constructorArguments.Length >= 1)
        {
            message = constructorArguments[0].Value as string;
            hasMessageArgument = true;
        }

        if (constructorArguments.Length >= 2 && constructorArguments[1].Value is bool constructorIsError)
        {
            isError = constructorIsError;
        }

        return new()
        {
            Message = message,
            HasMessageArgument = hasMessageArgument,
            HasErrorArgument = constructorArguments.Length >= 2,
            IsError = isError,
        };
    }

    /// <summary>
    /// Generates C# source code for extension methods that expose events of a parsed property as observables.
    /// </summary>
    /// <remarks>
    /// The generated source code includes extension methods for each event defined in the parsed property.
    /// These methods allow consumers to subscribe to events using the R3 observable pattern.
    /// The output is intended for use in code generation scenarios and is marked as auto-generated.
    /// </remarks>
    /// <param name="item">The parsed property containing event information and target type details used to generate observable extension methods.</param>
    /// <returns>
    /// A string containing the generated C# source code for observable extension methods, including namespace and class declarations as appropriate.
    /// </returns>
    private static string GenerateSource(ParsedGenerationProperty item)
    {
        var methodsBuilder = new StringBuilder();
        var targetTypeCodeQualified = item.TargetTypeName.CodeQualified;

        foreach (var methodInfo in item.GeneratedMethods)
        {
            methodsBuilder.AppendLine(methodInfo.UseAsUnit
                ? BuildUnitMethodSource(methodInfo, targetTypeCodeQualified)
                : BuildEventMethodSource(methodInfo, targetTypeCodeQualified));
        }

        // Namespace of the attribute-bearing class. Empty for global namespace.
        var classNamespace = item.ClassNamespace;
        var className = item.ClassName;

        if (classNamespace.Length > 0)
        {
            return $$"""
// <auto-generated />
#nullable enable
namespace {{classNamespace}}
{
    partial class {{className}}
    {
{{methodsBuilder}}
    }
}
""";
        }
        else
        {
            return $$"""
// <auto-generated />
#nullable enable
partial class {{className}}
{
{{methodsBuilder}}
}
""";
        }
    }

    /// <summary>
    /// Builds the source for a single <c>AsObservable</c> extension method that wraps a
    /// non-generic <see cref="System.EventHandler"/> event as an <c>R3.Unit</c> observable.
    /// </summary>
    private static string BuildUnitMethodSource(GeneratedMethodInfo methodInfo, string targetTypeCodeQualified)
    {
        return $$"""
        /// <summary>
        /// Returns an <see cref="R3.Observable`1"/> for <c>{{methodInfo.EventName}}</c> with payload type <see cref="{{methodInfo.ObservableElementType.UserFacing}}"/>.
        /// </summary>
{{RenderObsoleteAttribute(methodInfo.ObsoleteInfo)}}        public static global::R3.Observable<{{methodInfo.ObservableElementType.CodeQualified}}> {{methodInfo.EventName}}AsObservable(this {{targetTypeCodeQualified}} instance, global::System.Threading.CancellationToken cancellationToken = default)
        {
            var rawObservable = global::R3.Observable.FromEventHandler(
                h => instance.{{methodInfo.EventName}} += h,
                h => instance.{{methodInfo.EventName}} -= h,
                cancellationToken
                );
            return global::R3.ObservableExtensions.AsUnitObservable(rawObservable);
        }
""";
    }

    /// <summary>
    /// Builds the source for a single <c>AsObservable</c> extension method that wraps a
    /// typed delegate event, projecting the last parameter as the observable element.
    /// </summary>
    private static string BuildEventMethodSource(GeneratedMethodInfo methodInfo, string targetTypeCodeQualified)
    {
        return $$"""
        /// <summary>
        /// Returns an <see cref="R3.Observable`1"/> for <c>{{methodInfo.EventName}}</c> with payload type <see cref="{{methodInfo.ObservableElementType.UserFacing}}"/>.
        /// </summary>
{{RenderObsoleteAttribute(methodInfo.ObsoleteInfo)}}        public static global::R3.Observable<{{methodInfo.ObservableElementType.CodeQualified}}> {{methodInfo.EventName}}AsObservable(this {{targetTypeCodeQualified}} instance, global::System.Threading.CancellationToken cancellationToken = default)
        {
            var rawObservable = global::R3.Observable.FromEvent<{{methodInfo.DelegateType.CodeQualified}}, (global::System.Object?, {{methodInfo.ObservableElementType.CodeQualified}} Args)>(
                h => new {{methodInfo.DelegateType.CodeQualified}}((s, e) => h((s, e))),
                h => instance.{{methodInfo.EventName}} += h,
                h => instance.{{methodInfo.EventName}} -= h,
                cancellationToken
                );
            return global::R3.ObservableExtensions.Select(rawObservable, ep => ep.Args);
        }
""";
    }

    /// <summary>
    /// Renders an obsolete attribute declaration for a generated method when required.
    /// </summary>
    /// <param name="obsoleteInfo">The obsolete metadata to render.</param>
    /// <returns>A formatted attribute line or an empty string.</returns>
    private static string RenderObsoleteAttribute(GeneratedObsoleteInfo? obsoleteInfo)
    {
        if (obsoleteInfo is null)
        {
            return string.Empty;
        }

        if (!obsoleteInfo.HasMessageArgument)
        {
            return "        [global::System.Obsolete]\n";
        }

        var messageLiteral = obsoleteInfo.Message is null
            ? "null"
            : SymbolDisplay.FormatLiteral(obsoleteInfo.Message, quote: true);
        if (!obsoleteInfo.HasErrorArgument)
        {
            return $"        [global::System.Obsolete({messageLiteral})]\n";
        }

        var isErrorLiteral = obsoleteInfo.IsError ? "true" : "false";
        return $"        [global::System.Obsolete({messageLiteral}, {isErrorLiteral})]\n";
    }
}
