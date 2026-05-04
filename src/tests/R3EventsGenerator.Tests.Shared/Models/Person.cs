namespace R3EventsGenerator.Tests.Shared.Models;

public sealed class Person(string name, int age)
{
    public event EventHandler? NameChanged;
    public event EventHandler<int>? AgeChanged;

    public string? Name
    {
        get;
        set
        {
            if (field != value)
            {
                NameChanged?.Invoke(this, EventArgs.Empty);
                field = value;
            }
        }
    } = name;

    public int Age
    {
        get;
        set
        {
            if (field != value)
            {
                AgeChanged?.Invoke(this, value);
                field = value;
            }
        }
    } = age;
}
