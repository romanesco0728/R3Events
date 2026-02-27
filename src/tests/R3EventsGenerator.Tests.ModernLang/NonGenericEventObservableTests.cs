using R3;
using R3EventsGenerator.Tests.Shared.Models;
using R3EventsGenerator.Tests.ModernLang.Extensions;
using Shouldly;

namespace R3EventsGenerator.Tests.ModernLang;

[TestClass]
public sealed class NonGenericEventObservableTests
{
    [TestMethod]
    public void NameChangedAsObservable_ShouldEmitWhenEventFires()
    {
        // Arrange
        var person = new Person("Alice", 18);
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
        var person = new Person("Alice", 18);
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
        var person = new Person("Alice", 18);
        var emissionCount = 0;

        // Act
        using var subscription = person.NameChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Try to set the same value (should not trigger event)
        person.Name = "Alice";

        // Assert
        emissionCount.ShouldBe(0, "Observable should not emit when property value is unchanged");
    }

    [TestMethod]
    public void AgeChangedAsObservable_ShouldEmitWhenEventFires()
    {
        // Arrange
        var person = new Person("Alice", 18);
        var emissionCount = 0;
        var lastAge = -1;

        // Act
        using var subscription = person.AgeChangedAsObservable()
            .Subscribe(age =>
            {
                emissionCount++;
                lastAge = age;
            });

        // Trigger the event
        person.Age = 19;
        lastAge.ShouldBe(19, "Observable should emit the changed age value");
        person.Age = 20;
        lastAge.ShouldBe(20, "Observable should emit the changed age value");

        // Assert
        emissionCount.ShouldBe(2, "Observable should have emitted exactly twice");
    }

    [TestMethod]
    public void AgeChangedAsObservable_ShouldUnsubscribeWhenDisposed()
    {
        // Arrange
        var person = new Person("Alice", 18);
        var emissionCount = 0;
        var lastAge = -1;

        // Act
        var subscription = person.AgeChangedAsObservable()
            .Subscribe(age =>
            {
                emissionCount++;
                lastAge = age;
            });

        // Trigger the event once
        person.Age = 19;
        emissionCount.ShouldBe(1, "Observable should have emitted once before disposal");
        lastAge.ShouldBe(19, "Observable should emit the changed age value");

        // Dispose subscription
        subscription.Dispose();

        // Trigger the event again
        person.Age = 20;

        // Assert
        emissionCount.ShouldBe(1, "Observable should not have emitted after disposal");
        lastAge.ShouldBe(19, "Observable should not update after disposal");
    }

    [TestMethod]
    public void AgeChangedAsObservable_ShouldNotEmitWhenPropertyIsNotChanged()
    {
        // Arrange
        var person = new Person("Alice", 18);
        var emissionCount = 0;

        // Act
        using var subscription = person.AgeChangedAsObservable()
            .Subscribe(_ => emissionCount++);

        // Try to set the same value (should not trigger event)
        person.Age = 18;

        // Assert
        emissionCount.ShouldBe(0, "Observable should not emit when property value is unchanged");
    }
}
