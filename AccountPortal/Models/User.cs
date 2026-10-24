namespace AccountPortal.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public string FirstName =>
        string.IsNullOrWhiteSpace(Name) ? "" : Name.Trim().Split(' ')[0];

    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "U";
            return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
        }
    }
}
