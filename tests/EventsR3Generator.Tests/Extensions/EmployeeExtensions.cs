using Events.R3;

namespace EventsR3Generator.Tests.Extensions;

// Test the generic attribute syntax (C# 11+)
[R3Event<Models.Employee>]
internal static partial class EmployeeExtensions;
