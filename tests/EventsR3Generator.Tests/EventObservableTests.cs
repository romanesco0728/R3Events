using R3;
using EventsR3Generator.Tests.Models;
using EventsR3Generator.Tests.Extensions;

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
        Assert.AreEqual(2, emissionCount, "Observable should have emitted twice");
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
        Assert.AreEqual(1, emissionCount);

        // Dispose subscription
        subscription.Dispose();

        // Trigger the event again
        person.Name = "Charlie";

        // Assert
        Assert.AreEqual(1, emissionCount, "Observable should not have emitted after disposing");
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
        Assert.AreEqual(0, emissionCount, "Observable should not have emitted when property value didn't change");
    }
}