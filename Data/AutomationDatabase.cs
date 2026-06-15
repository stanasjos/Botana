using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;                    // ← нужно для Any/Where/etc.
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;

namespace Botana.Data;

public sealed class AutomationDatabase
{
    private static readonly Lazy<AutomationDatabase> _lazy = new(() => new AutomationDatabase());
    public static AutomationDatabase Instance => _lazy.Value;

    private SQLiteAsyncConnection _db = default!;
    private bool _inited;

    private AutomationDatabase() { }

    // используем тот же файл, что и для пользователей
    private string DbPath => Path.Combine(FileSystem.AppDataDirectory, "users.db3");

    public async Task InitAsync()
    {
        if (_inited) return;

        _db = new SQLiteAsyncConnection(DbPath);

        // создаст таблицу, если её ещё нет
        await _db.CreateTableAsync<Automation>();

        // если таблица старая — добавим недостающие колонки
        await EnsureColumnsAsync();

        _inited = true;
    }

    // миграция "на лету": добиваем отсутствующие поля
    private async Task EnsureColumnsAsync()
    {
        var cols = await _db.GetTableInfoAsync(nameof(Automation));
        bool Has(string name) => cols.Any(c => c.Name == name);

        if (!Has("Description"))
            await _db.ExecuteAsync("ALTER TABLE Automation ADD COLUMN Description TEXT");

        if (!Has("IsPublic"))
            await _db.ExecuteAsync("ALTER TABLE Automation ADD COLUMN IsPublic INTEGER NOT NULL DEFAULT 0");

        if (!Has("Category"))
            await _db.ExecuteAsync("ALTER TABLE Automation ADD COLUMN Category TEXT");
    }

    public async Task<List<Automation>> GetByOwnerAsync(string ownerEmail)
    {
        await InitAsync();
        var norm = (ownerEmail ?? string.Empty).Trim().ToLowerInvariant();
        return await _db.Table<Automation>()
                        .Where(a => a.OwnerEmail == norm)
                        .OrderByDescending(a => a.UpdatedAt)
                        .ToListAsync();
    }

    public async Task<int> CreateAsync(string ownerEmail, string name, string? sourceUrl = null)
    {
        await InitAsync();
        var a = new Automation
        {
            OwnerEmail = (ownerEmail ?? "").Trim().ToLowerInvariant(),
            Name = name.Trim(),
            SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim()
            // Description = null;
            // IsPublic    = false;
            // Category    = "general";
        };
        return await _db.InsertAsync(a);
    }

    public async Task SetActiveAsync(int id, bool active)
    {
        await InitAsync();
        var a = await _db.FindAsync<Automation>(id);
        if (a == null) return;
        a.IsActive = active;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.UpdateAsync(a);
    }

    // переключение паузы
    public async Task TogglePauseAsync(int id)
    {
        await InitAsync();
        var a = await _db.FindAsync<Automation>(id);
        if (a == null) return;

        a.IsActive = !a.IsActive;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.UpdateAsync(a);
    }

    public async Task DeleteAsync(int id)
    {
        await InitAsync();
        await _db.DeleteAsync<Automation>(id);
    }

    public async Task<Automation?> GetByIdAsync(int id)
    {
        await InitAsync();
        return await _db.FindAsync<Automation>(id);
    }

    public async Task UpdateAsync(Automation a)
    {
        await InitAsync();
        a.UpdatedAt = DateTime.UtcNow;
        await _db.UpdateAsync(a);
    }

    public async Task SetPublicAsync(int id, bool isPublic)
    {
        await InitAsync();
        var a = await _db.FindAsync<Automation>(id);
        if (a == null) return;
        a.IsPublic = isPublic;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.UpdateAsync(a);
    }

    public async Task<List<Automation>> GetPublicAsync(int take = 50)
    {
        await InitAsync();
        return await _db.Table<Automation>()
                        .Where(x => x.IsPublic)
                        .OrderByDescending(x => x.UpdatedAt)
                        .Take(take)
                        .ToListAsync();
    }

    // поиск для Explore: по тексту + по категории
    public async Task<List<Automation>> GetPublicFilteredAsync(string? query, string? category, int take = 100)
    {
        await InitAsync();

        var q = _db.Table<Automation>().Where(a => a.IsPublic);

        if (!string.IsNullOrWhiteSpace(category) && category != "all")
            q = q.Where(a => a.Category == category);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var needle = query.Trim().ToLowerInvariant();
            q = q.Where(a =>
                a.Name.ToLower().Contains(needle) ||
                (a.Description != null && a.Description.ToLower().Contains(needle)));
        }

        return await q.OrderByDescending(a => a.UpdatedAt)
                      .Take(take)
                      .ToListAsync();
    }

    public async Task IncrementRunsAsync(int id, bool success)
    {
        await InitAsync();
        var a = await _db.FindAsync<Automation>(id);
        if (a == null) return;

        a.TotalRuns++;
        if (success) a.SuccessRuns++;
        a.LastRunUtc = DateTime.UtcNow;
        a.UpdatedAt = DateTime.UtcNow;

        await _db.UpdateAsync(a);
    }
}