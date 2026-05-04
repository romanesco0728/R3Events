namespace R3EventsGenerator.Tests.Shared.Models;

public sealed class Employee(string name, string department)
{
    public event EventHandler? NameChanged;
    public event EventHandler<string>? DepartmentChanged;

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

    public string? Department
    {
        get;
        set
        {
            if (field != value)
            {
                DepartmentChanged?.Invoke(this, value ?? string.Empty);
                field = value;
            }
        }
    } = department;
}
