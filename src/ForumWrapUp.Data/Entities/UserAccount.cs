using ForumWrapUp.Data.Entities.Raw;

namespace ForumWrapUp.Data.Entities;

/// <summary>Account locale associato a un profilo ForumCommunity verificato.</summary>
public sealed class UserAccount
{
    /// <summary>Identificativo stabile del profilo forum, usato come chiave dell'account locale.</summary>
    public int Mid { get; set; }

    public ForumProfile ForumProfile { get; set; } = null!;

    /// <summary>Hash della password locale; non contiene la password in chiaro.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Data di creazione dell'account locale in UTC.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Data in UTC in cui e stato verificato o attestato dall'amministratore il controllo del profilo forum.</summary>
    public DateTime ProfileVerifiedAtUtc { get; set; }

    /// <summary>Indica se l'account puo accedere alle funzionalita riservate agli utenti.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Data dell'ultimo login locale in UTC; nulla se non ancora registrata.</summary>
    public DateTime? LastLoginAtUtc { get; set; }

    /// <summary>Post importati di cui l'utente e autore.</summary>
    public ICollection<ForumPost> Posts { get; } = [];

    /// <summary>Operazioni di importazione relative ai post dell'utente.</summary>
    public ICollection<ImportOperation> ImportOperations { get; } = [];

    /// <summary>Riepiloghi annuali calcolati per l'utente.</summary>
    public ICollection<AnnualWrapUp> AnnualWrapUps { get; } = [];
}
