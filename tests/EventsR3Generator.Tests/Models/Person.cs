namespace EventsR3Generator.Tests.Models;

internal sealed class Person(string name)
{
    public event EventHandler? NameChanged;
    private string? _name = name;
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
}
