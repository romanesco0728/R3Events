namespace R3EventsGenerator.Tests.Models;

internal sealed class Person
{
    public event EventHandler? NameChanged;
    public event EventHandler<int>? AgeChanged;

    private string? _name;
    private int _age;

    public Person(string name, int age)
    {
        _name = name;
        _age = age;
    }

    public string? Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                NameChanged?.Invoke(this, EventArgs.Empty);
                _name = value;
            }
        }
    }
    public int Age
    {
        get => _age;
        set
        {
            if (_age != value)
            {
                AgeChanged?.Invoke(this, value);
                _age = value;
            }
        }
    }
}
