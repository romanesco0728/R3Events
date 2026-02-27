using R3;
using R3EventsGenerator.Tests.Shared.Models;
using R3EventsGenerator.Tests.ModernLang.Extensions;
using Shouldly;

namespace R3EventsGenerator.Tests.ModernLang;

/// <summary>
/// Integration tests for generic attribute syntax using Employee type.
/// These tests verify that the generic R3Event&lt;T&gt; attribute works correctly
/// with runtime behavior, similar to EventObservableTests but using the new syntax.
/// </summary>
[TestClass]
public sealed class GenericAttributeIntegrationTests
{
    [TestMethod]
    public void NameChangedAsObservable_ShouldEmitWhenEventFires()
    {
        // Arrange
        var employee = new Employee("Alice", "Engineering");
        var emissionCount = 0;

        // Act
        using var subscription = employee.NameChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Trigger the event
        employee.Name = "Bob";
        employee.Name = "Charlie";

        // Assert
        emissionCount.ShouldBe(2, "Observable should have emitted exactly twice");
    }

    [TestMethod]
    public void NameChangedAsObservable_ShouldUnsubscribeWhenDisposed()
    {
        // Arrange
        var employee = new Employee("Alice", "Engineering");
        var emissionCount = 0;

        // Act
        var subscription = employee.NameChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Trigger the event once
        employee.Name = "Bob";
        emissionCount.ShouldBe(1, "Observable should have emitted once before disposal");

        // Dispose subscription
        subscription.Dispose();

        // Trigger the event again
        employee.Name = "Charlie";

        // Assert
        emissionCount.ShouldBe(1, "Observable should not have emitted after disposal");
    }

    [TestMethod]
    public void NameChangedAsObservable_ShouldNotEmitWhenPropertyIsNotChanged()
    {
        // Arrange
        var employee = new Employee("Alice", "Engineering");
        var emissionCount = 0;

        // Act
        using var subscription = employee.NameChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Try to set the same value (should not trigger event)
        employee.Name = "Alice";

        // Assert
        emissionCount.ShouldBe(0, "Observable should not emit when property value is unchanged");
    }

    [TestMethod]
    public void DepartmentChangedAsObservable_ShouldEmitWhenEventFires()
    {
        // Arrange
        var employee = new Employee("Alice", "Engineering");
        var emissionCount = 0;
        var lastDepartment = string.Empty;

        // Act
        using var subscription = employee.DepartmentChangedAsObservable()
            .Subscribe(dept =>
            {
                emissionCount++;
                lastDepartment = dept;
            });

        // Trigger the event
        employee.Department = "Sales";
        lastDepartment.ShouldBe("Sales", "Observable should emit the changed department value");
        employee.Department = "Marketing";
        lastDepartment.ShouldBe("Marketing", "Observable should emit the changed department value");

        // Assert
        emissionCount.ShouldBe(2, "Observable should have emitted exactly twice");
    }

    [TestMethod]
    public void DepartmentChangedAsObservable_ShouldUnsubscribeWhenDisposed()
    {
        // Arrange
        var employee = new Employee("Alice", "Engineering");
        var emissionCount = 0;
        var lastDepartment = string.Empty;

        // Act
        var subscription = employee.DepartmentChangedAsObservable()
            .Subscribe(dept =>
            {
                emissionCount++;
                lastDepartment = dept;
            });

        // Trigger the event once
        employee.Department = "Sales";
        emissionCount.ShouldBe(1, "Observable should have emitted once before disposal");
        lastDepartment.ShouldBe("Sales", "Observable should emit the changed department value");

        // Dispose subscription
        subscription.Dispose();

        // Trigger the event again
        employee.Department = "Marketing";

        // Assert
        emissionCount.ShouldBe(1, "Observable should not have emitted after disposal");
        lastDepartment.ShouldBe("Sales", "Observable should not update after disposal");
    }

    [TestMethod]
    public void DepartmentChangedAsObservable_ShouldNotEmitWhenPropertyIsNotChanged()
    {
        // Arrange
        var employee = new Employee("Alice", "Engineering");
        var emissionCount = 0;

        // Act
        using var subscription = employee.DepartmentChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Try to set the same value (should not trigger event)
        employee.Department = "Engineering";

        // Assert
        emissionCount.ShouldBe(0, "Observable should not emit when property value is unchanged");
    }
}
