namespace R3EventsGenerator.Tests.Models;

internal sealed class Employee(string name, string department)
{
    public event EventHandler? NameChanged;
    public event EventHandler<string>? DepartmentChanged;

    private string? _name = name;
    private string? _department = department;

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
    public string? Department
    {
        get => _department;
        set
        {
            if (_department != value)
            {
                DepartmentChanged?.Invoke(this, value ?? string.Empty);
                _department = value;
            }
        }
    }
}
