using System;
using SQLite;

namespace Botana.Data;

public class Automation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // уникальность имени в пределах владельца
    [Indexed(Name = "UX_Owner_Name", Order = 1, Unique = true)]
    public string OwnerEmail { get; set; } = string.Empty; // хранить в lower-case

    [Indexed(Name = "UX_Owner_Name", Order = 2, Unique = true)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;   // TRUE = активна; FALSE = пауза
    public int TotalRuns { get; set; }        // всего запусков
    public int SuccessRuns { get; set; }        // успешных запусков
    public DateTime? LastRunUtc { get; set; }

    public string? SourceUrl { get; set; }

    // --- НОВЫЕ ПОЛЯ (витрина/категории) ---
    public string? Description { get; set; }           // краткое описание
    public bool IsPublic { get; set; } = false;  // опубликована в каталог?
    public string? Category { get; set; } = "general"; // ключ категории

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== UI-удобства (не пишутся в БД) =====
    [Ignore]
    public bool IsPaused { get => !IsActive; set => IsActive = !value; }

    [Ignore] public int Runs => TotalRuns;
    [Ignore] public double SuccessRate => TotalRuns > 0 ? (double)SuccessRuns / TotalRuns : 0.0;

    [Ignore]
    public string LastRunDisplay =>
        LastRunUtc.HasValue ? $"Last run: {LastRunUtc.Value.ToLocalTime():g}" : "No runs yet";
}