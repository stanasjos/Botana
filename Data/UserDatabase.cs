using SQLite;
using System.Security.Cryptography;
using System.Text;

namespace Botana.Data;

public sealed class UserDatabase
{
    private static readonly Lazy<UserDatabase> _lazy = new(() => new UserDatabase());
    public static UserDatabase Instance => _lazy.Value;

    private SQLiteAsyncConnection? _db;
    private bool _initialized;

    private UserDatabase() { }

    private async Task InitAsync()
    {
        if (_initialized) return;

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "users.db3");

        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<User>();
        _initialized = true;
    }

    private static string NormalizeEmail(string email) =>
        (email ?? string.Empty).Trim().ToLowerInvariant();

    private static string Hash(string text)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty));
        return Convert.ToBase64String(bytes);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        await InitAsync();
        var norm = NormalizeEmail(email);
        return await _db!.Table<User>().Where(u => u.Email == norm).FirstOrDefaultAsync();
    }

    public async Task<(bool ok, bool isAdmin)> ValidateAsync(string email, string password)
    {
        await InitAsync();
        var u = await FindByEmailAsync(email);
        if (u == null) return (false, false);
        return (u.PasswordHash == Hash(password ?? ""), u.IsAdmin);
    }

    public async Task<bool> CreateAsync(string email, string password, bool isAdmin = false)
    {
        await InitAsync();
        var norm = NormalizeEmail(email);
        if (await FindByEmailAsync(norm) != null) return false;

        var user = new User
        {
            Email = norm,
            PasswordHash = Hash(password ?? ""),
            IsAdmin = isAdmin,
            CreatedAt = DateTime.UtcNow
        };

        await _db!.InsertAsync(user);
        return true;
    }

    public async Task EnsureAdminAsync(string email, string password)
    {
        await InitAsync();
        if (await FindByEmailAsync(email) == null)
            await CreateAsync(email, password, isAdmin: true);
    }
    // NEW: обновление профиля (имя/фамилия/компания и, при необходимости, e-mail)
    public async Task<(bool ok, string? newEmail)> UpdateProfileAsync(
        string currentEmail,
        string newEmail,
        string firstName,
        string lastName,
        string company)
    {
        await InitAsync();

        var user = await FindByEmailAsync(currentEmail);
        if (user is null) return (false, null);

        // если e-mail меняется — проверим уникальность и нормализуем
        var desired = NormalizeEmail(newEmail);
        if (!string.Equals(user.Email, desired, StringComparison.OrdinalIgnoreCase))
        {
            if (await FindByEmailAsync(desired) is not null)
                throw new InvalidOperationException("E-mail is already in use.");

            user.Email = desired;
        }

        user.FirstName = firstName?.Trim() ?? string.Empty;
        user.LastName = lastName?.Trim() ?? string.Empty;
        user.Company = company?.Trim() ?? string.Empty;

        await _db!.UpdateAsync(user);
        return (true, user.Email);
    }

    // NEW: смена пароля (с валидацией текущего)
    public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        await InitAsync();

        var user = await FindByEmailAsync(email);
        if (user is null) return false;

        if (user.PasswordHash != Hash(currentPassword ?? string.Empty))
            return false;

        user.PasswordHash = Hash(newPassword ?? string.Empty);
        await _db!.UpdateAsync(user);
        return true;
    }
}

