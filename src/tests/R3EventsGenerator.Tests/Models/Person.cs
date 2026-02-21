namespace R3EventsGenerator.Tests.Models;

internal sealed class Person(string name, int age)
{
    public event EventHandler? NameChanged;
    public event EventHandler<int>? AgeChanged;

    private string? _name = name;
    private int _age = age;

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
