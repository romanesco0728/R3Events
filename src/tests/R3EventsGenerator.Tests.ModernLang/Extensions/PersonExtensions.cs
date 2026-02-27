using R3Events;
using R3EventsGenerator.Tests.Shared.Models;

namespace R3EventsGenerator.Tests.ModernLang.Extensions;

#pragma warning disable R3W001 // Non-generic attribute is intentionally used here to verify backward compatibility
[R3Event(typeof(Person))]
internal static partial class PersonExtensions;
