using R3Events;

namespace R3EventsGenerator.Tests.Extensions;

#pragma warning disable R3W001 // Non-generic attribute is intentionally used here to verify backward compatibility
[R3Event(typeof(Models.Person))]
internal static partial class PersonExtensions;
