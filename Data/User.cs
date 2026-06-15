using SQLite;

namespace Botana.Data;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Unique = true)]
    public string Email { get; set; } = string.Empty;   // хранить в lower

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // NEW:
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
}