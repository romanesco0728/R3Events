using R3;
using EventsR3Generator.Tests.Models;
using EventsR3Generator.Tests.Extensions;
using Shouldly;

namespace EventsR3Generator.Tests;

[TestClass]
public sealed class EventObservableTests
{
    [TestMethod]
    public void NameChangedAsObservable_ShouldEmitWhenEventFires()
    {
        // Arrange
        var person = new Person("Alice");
        var emissionCount = 0;

        // Act
        using var subscription = person.NameChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Trigger the event
        person.Name = "Bob";
        person.Name = "Charlie";

        // Assert
        emissionCount.ShouldBe(2, "Observable should have emitted exactly twice");
    }

    [TestMethod]
    public void NameChangedAsObservable_ShouldUnsubscribeWhenDisposed()
    {
        // Arrange
        var person = new Person("Alice");
        var emissionCount = 0;

        // Act
        var subscription = person.NameChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Trigger the event once
        person.Name = "Bob";
        emissionCount.ShouldBe(1, "Observable should have emitted once before disposal");

        // Dispose subscription
        subscription.Dispose();

        // Trigger the event again
        person.Name = "Charlie";

        // Assert
        emissionCount.ShouldBe(1, "Observable should not have emitted after disposal");
    }

    [TestMethod]
    public void NameChangedAsObservable_ShouldNotEmitWhenPropertyIsNotChanged()
    {
        // Arrange
        var person = new Person("Alice");
        var emissionCount = 0;

        // Act
        using var subscription = person.NameChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Try to set the same value (should not trigger event)
        person.Name = "Alice";

        // Assert
        emissionCount.ShouldBe(0, "Observable should not emit when property value is unchanged");
    }
}