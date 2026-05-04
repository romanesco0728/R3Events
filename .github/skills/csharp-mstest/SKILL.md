---
name: csharp-mstest
description: 'Get best practices for MSTest 3.x/4.x unit testing with Shouldly assertions, including modern assertion APIs and data-driven tests'
---

# MSTest Best Practices (MSTest 3.x/4.x + Shouldly)

Your goal is to help me write effective unit tests with modern MSTest, using Shouldly for assertions and following current best practices.

## Assertion Priority

**Always prefer Shouldly assertions over MSTest's `Assert` class.**
Shouldly gives better failure messages, natural read order (`actual.ShouldBe(expected)`), and supports context-appropriate custom messages as the last parameter.

- **Shouldly**: use for all value, null, collection, string, type, and exception assertions
- **MSTest `Assert`**: use only for things Shouldly cannot do (`Assert.Fail`, `Assert.Inconclusive`)

**Always provide a descriptive English `customMessage`** that explains what the assertion is verifying in context — not just "should be X", but *why* this particular value matters here.

## Project Setup

- Use a separate test project with naming convention `[ProjectName].Tests`
- Reference MSTest 3.x+ NuGet packages (includes analyzers); consider using MSTest.Sdk for simplified setup
- Reference `Shouldly` NuGet package for assertions
- Run tests with `dotnet test`

## Test Class Structure

- Use `[TestClass]` attribute for test classes
- **Seal test classes by default** for performance and design clarity
- Use `[TestMethod]` for test methods (prefer over `[DataTestMethod]`)
- Follow Arrange-Act-Assert (AAA) pattern
- Name tests using pattern `MethodName_Scenario_ExpectedBehavior`

```csharp
[TestClass]
public sealed class CalculatorTests
{
    [TestMethod]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(2, 3);

        // Assert — use Shouldly
        result.ShouldBe(5, "Add(2, 3) should produce 5");
    }
}
```

## Test Lifecycle

- **Prefer constructors over `[TestInitialize]`** - enables `readonly` fields and follows standard C# patterns
- Use `[TestCleanup]` for cleanup that must run even if test fails
- Combine constructor with async `[TestInitialize]` when async setup is needed

```csharp
[TestClass]
public sealed class ServiceTests
{
    private readonly MyService _service;  // readonly enabled by constructor

    public ServiceTests()
    {
        _service = new MyService();
    }

    [TestInitialize]
    public async Task InitAsync()
    {
        // Use for async initialization only
        await _service.WarmupAsync();
    }

    [TestCleanup]
    public void Cleanup() => _service.Reset();
}
```

### Execution Order

1. **Assembly Initialization** - `[AssemblyInitialize]` (once per test assembly)
2. **Class Initialization** - `[ClassInitialize]` (once per test class)
3. **Test Initialization** (for every test method):
   1. Constructor
   2. Set `TestContext` property
   3. `[TestInitialize]`
4. **Test Execution** - test method runs
5. **Test Cleanup** (for every test method):
   1. `[TestCleanup]`
   2. `DisposeAsync` (if implemented)
   3. `Dispose` (if implemented)
6. **Class Cleanup** - `[ClassCleanup]` (once per test class)
7. **Assembly Cleanup** - `[AssemblyCleanup]` (once per test assembly)

## Assertions with Shouldly (Preferred)

Shouldly uses natural object-oriented syntax — `actual.ShouldBe(expected)` — which eliminates the argument-order confusion of `Assert.AreEqual(expected, actual)`. All methods accept an optional `customMessage` as the last parameter.

### Value Equality

```csharp
result.ShouldBe(expected, "generator should emit the canonical event method name");
result.ShouldNotBe(unexpected, "diagnostic code must differ from the warning variant");
```

### Null Checks

```csharp
node.ShouldNotBeNull("syntax node must be present when attribute is applied");
fallback.ShouldBeNull("no fallback type should be inferred when explicit type is provided");
```

### Boolean

```csharp
isValid.ShouldBeTrue("attribute with a valid R3 type should pass validation");
hasError.ShouldBeFalse("no diagnostic should be emitted for a well-formed declaration");
```

### Collections

```csharp
diagnostics.ShouldBeEmpty("no diagnostics expected for a valid input");
diagnostics.ShouldNotBeEmpty("at least one diagnostic must be reported for the invalid input");
diagnostics.ShouldHaveSingleItem("exactly one diagnostic should be emitted");
diagnostics.Count.ShouldBe(2, "two diagnostics expected: one for each invalid attribute usage");

diagnostics.ShouldContain(d => d.Id == "R3E001",
    "R3E001 must be reported when the event type does not implement IObservable<T>");
diagnostics.ShouldNotContain(d => d.Id == "R3I001",
    "info diagnostic must not appear for a non-partial class");

// Sequence equality (same elements, same order)
actual.ShouldBe(expected, "generated source lines must match the expected output exactly");
```

### Strings

```csharp
source.ShouldContain("partial void On", "generated helper method must use the partial void pattern");
source.ShouldStartWith("// <auto-generated>", "generated files must have the auto-generated header");
source.ShouldEndWith("}", "generated class must be closed");
source.ShouldNotContain("file namespace", "generated code must use block-style namespace, not file-scoped");
```

### Comparisons

```csharp
value.ShouldBeGreaterThan(0, "diagnostic count must be positive when errors are present");
value.ShouldBeGreaterThanOrEqualTo(1, "at least one member must be generated");
value.ShouldBeLessThan(maxLimit, "generated output must stay within the line limit");
value.ShouldBeInRange(1, 10, "member count must be within the expected range");
```

### Type Assertions

```csharp
// ShouldBeOfType returns the strongly-typed value
var typedNode = node.ShouldBeOfType<ClassDeclarationSyntax>(
    "target of R3Event<T> must be a class, not a struct or interface");
typedNode.Identifier.Text.ShouldBe("MyViewModel");

obj.ShouldBeAssignableTo<IDisposable>("generated type must implement IDisposable");
```

### Exception Assertions

```csharp
// Synchronous
Should.Throw<ArgumentNullException>(
    () => generator.Execute(null!),
    "passing null context should throw ArgumentNullException");

// Capture exception to assert details
var ex = Should.Throw<InvalidOperationException>(
    () => Method(),
    "calling Method without initialization must throw");
ex.Message.ShouldContain("not initialized", "exception message must describe the missing initialization");

// Async
await Should.ThrowAsync<OperationCanceledException>(
    async () => await RunAsync(cancelledToken),
    "cancelling the token must surface as OperationCanceledException");
```

---

## Modern Assertion APIs (MSTest fallback)

Use MSTest's `Assert` class only when Shouldly has no equivalent.

### When to use MSTest Assert

```csharp
// Inconclusive / explicit fail — no Shouldly equivalent
Assert.Inconclusive("Test skipped: requires live Roslyn workspace");
Assert.Fail("Reached an unreachable code path");
```

### Exception Testing (MSTest fallback)

Prefer `Should.Throw` (Shouldly) above. Use `Assert.Throws` / `Assert.ThrowsExactly` when you need to match the exact type without subclass matching and Shouldly isn't sufficient:

```csharp
// Assert.Throws - matches TException or derived types
var ex = Assert.Throws<ArgumentException>(() => Method(null));
Assert.AreEqual("Value cannot be null.", ex.Message);

// Assert.ThrowsExactly - matches exact type only (no derived types)
var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Method());

// Async versions
var ex = await Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetAsync(url));
```

## Data-Driven Tests

### DataRow

```csharp
[TestMethod]
[DataRow(1, 2, 3)]
[DataRow(0, 0, 0, DisplayName = "Zeros")]
[DataRow(-1, 1, 0, IgnoreMessage = "Known issue #123")]  // MSTest 3.8+
public void Add_ReturnsSum(int a, int b, int expected)
{
    Calculator.Add(a, b).ShouldBe(expected, $"Add({a}, {b}) should return {expected}");
}
```

### DynamicData

The data source can return any of the following types:

- `IEnumerable<(T1, T2, ...)>` (ValueTuple) - **preferred**, provides type safety (MSTest 3.7+)
- `IEnumerable<Tuple<T1, T2, ...>>` - provides type safety
- `IEnumerable<TestDataRow>` - provides type safety plus control over test metadata (display name, categories)
- `IEnumerable<object[]>` - **least preferred**, no type safety

> **Note:** When creating new test data methods, prefer `ValueTuple` or `TestDataRow` over `IEnumerable<object[]>`. The `object[]` approach provides no compile-time type checking and can lead to runtime errors from type mismatches.

```csharp
[TestMethod]
[DynamicData(nameof(TestData))]
public void DynamicTest(int a, int b, int expected)
{
    Calculator.Add(a, b).ShouldBe(expected, $"Add({a}, {b}) should return {expected}");
}

// ValueTuple - preferred (MSTest 3.7+)
public static IEnumerable<(int a, int b, int expected)> TestData =>
[
    (1, 2, 3),
    (0, 0, 0),
];

// TestDataRow - when you need custom display names or metadata
public static IEnumerable<TestDataRow<(int a, int b, int expected)>> TestDataWithMetadata =>
[
    new((1, 2, 3)) { DisplayName = "Positive numbers" },
    new((0, 0, 0)) { DisplayName = "Zeros" },
    new((-1, 1, 0)) { DisplayName = "Mixed signs", IgnoreMessage = "Known issue #123" },
];

// IEnumerable<object[]> - avoid for new code (no type safety)
public static IEnumerable<object[]> LegacyTestData =>
[
    [1, 2, 3],
    [0, 0, 0],
];
```

## TestContext

The `TestContext` class provides test run information, cancellation support, and output methods.
See [TestContext documentation](https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-writing-tests-testcontext) for complete reference.

### Accessing TestContext

```csharp
// Property (MSTest suppresses CS8618 - don't use nullable or = null!)
public TestContext TestContext { get; set; }

// Constructor injection (MSTest 3.6+) - preferred for immutability
[TestClass]
public sealed class MyTests
{
    private readonly TestContext _testContext;

    public MyTests(TestContext testContext)
    {
        _testContext = testContext;
    }
}

// Static methods receive it as parameter
[ClassInitialize]
public static void ClassInit(TestContext context) { }

// Optional for cleanup methods (MSTest 3.6+)
[ClassCleanup]
public static void ClassCleanup(TestContext context) { }

[AssemblyCleanup]
public static void AssemblyCleanup(TestContext context) { }
```

### Cancellation Token

Always use `TestContext.CancellationToken` for cooperative cancellation with `[Timeout]`:

```csharp
[TestMethod]
[Timeout(5000)]
public async Task LongRunningTest()
{
    await _httpClient.GetAsync(url, TestContext.CancellationToken);
}
```

### Test Run Properties

```csharp
TestContext.TestName              // Current test method name
TestContext.TestDisplayName       // Display name (3.7+)
TestContext.CurrentTestOutcome    // Pass/Fail/InProgress
TestContext.TestData              // Parameterized test data (3.7+, in TestInitialize/Cleanup)
TestContext.TestException         // Exception if test failed (3.7+, in TestCleanup)
TestContext.DeploymentDirectory   // Directory with deployment items
```

### Output and Result Files

```csharp
// Write to test output (useful for debugging)
TestContext.WriteLine("Processing item {0}", itemId);

// Attach files to test results (logs, screenshots)
TestContext.AddResultFile(screenshotPath);

// Store/retrieve data across test methods
TestContext.Properties["SharedKey"] = computedValue;
```

## Advanced Features

### Retry for Flaky Tests (MSTest 3.9+)

```csharp
[TestMethod]
[Retry(3)]
public void FlakyTest() { }
```

### Conditional Execution (MSTest 3.10+)

Skip or run tests based on OS or CI environment:

```csharp
// OS-specific tests
[TestMethod]
[OSCondition(OperatingSystems.Windows)]
public void WindowsOnlyTest() { }

[TestMethod]
[OSCondition(OperatingSystems.Linux | OperatingSystems.MacOS)]
public void UnixOnlyTest() { }

[TestMethod]
[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
public void SkipOnWindowsTest() { }

// CI environment tests
[TestMethod]
[CICondition]  // Runs only in CI (default: ConditionMode.Include)
public void CIOnlyTest() { }

[TestMethod]
[CICondition(ConditionMode.Exclude)]  // Skips in CI, runs locally
public void LocalOnlyTest() { }
```

### Parallelization

```csharp
// Assembly level
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]

// Disable for specific class
[TestClass]
[DoNotParallelize]
public sealed class SequentialTests { }
```

### Work Item Traceability (MSTest 3.8+)

Link tests to work items for traceability in test reports:

```csharp
// GitHub issues (MSTest 3.8+)
[TestMethod]
[GitHubWorkItem("https://github.com/owner/repo/issues/42")]
public void BugFix_Issue42_IsResolved() { }
```

## Common Mistakes to Avoid

```csharp
// ❌ Using MSTest Assert when Shouldly is available
Assert.AreEqual(expected, actual);
// ✅ Shouldly with context-appropriate custom message
actual.ShouldBe(expected, "generated source must match the expected output");

// ❌ Wrong argument order with MSTest Assert (easy to mix up)
Assert.AreEqual(actual, expected);
// ✅ Shouldly has no order confusion — actual is always the receiver
actual.ShouldBe(expected);

// ❌ Using ExpectedException (obsolete)
[ExpectedException(typeof(ArgumentException))]
// ✅ Use Should.Throw with a descriptive message
Should.Throw<ArgumentException>(() => Method(null),
    "null input must be rejected immediately");

// ❌ Using LINQ Single() — unclear exception on failure
var item = items.Single();
// ✅ ShouldHaveSingleItem — clear failure message
var item = items.ShouldHaveSingleItem("exactly one diagnostic expected for this input");

// ❌ Hard cast — obscures actual type on failure
var handler = (MyHandler)result;
// ✅ ShouldBeOfType — shows actual type in failure message
var handler = result.ShouldBeOfType<MyHandler>(
    "result must be the concrete handler type, not a wrapper");

// ❌ Generic custom message that doesn't add context
result.ShouldBe(42, "result should be 42");
// ✅ Contextual custom message that explains WHY
result.ShouldBe(42, "event handler count must match the number of [R3Event<T>] attributes");

// ❌ Ignoring cancellation token
await client.GetAsync(url, CancellationToken.None);
// ✅ Flow test cancellation
await client.GetAsync(url, TestContext.CancellationToken);

// ❌ Making TestContext nullable - leads to unnecessary null checks
public TestContext? TestContext { get; set; }
// ❌ Using null! - MSTest already suppresses CS8618 for this property
public TestContext TestContext { get; set; } = null!;
// ✅ Declare without nullable or initializer - MSTest handles the warning
public TestContext TestContext { get; set; }
```

## Test Organization

- Group tests by feature or component
- Use `[TestCategory("Category")]` for filtering
- Use `[TestProperty("Name", "Value")]` for custom metadata
- Use `[Priority(1)]` for critical tests
- Enable relevant MSTest analyzers (MSTEST0020 for constructor preference)

## Mocking and Isolation

- Use Moq or NSubstitute for mocking dependencies
- Use interfaces to facilitate mocking
- Mock dependencies to isolate units under test
