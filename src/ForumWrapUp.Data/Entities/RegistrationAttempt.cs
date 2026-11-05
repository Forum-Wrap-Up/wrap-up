namespace ForumWrapUp.Data.Entities;

/// <summary>Dati temporanei necessari a riprendere una registrazione e verificare il profilo forum.</summary>
public sealed class RegistrationAttempt
{
    /// <summary>Identificativo locale del tentativo di registrazione.</summary>
    public Guid Id { get; set; }

    /// <summary>MID del profilo forum di cui verificare il controllo; l'account locale puo non esistere ancora.</summary>
    public int Mid { get; set; }

    /// <summary>GUID in formato testuale da inserire nei Contatti del profilo forum.</summary>
    public string VerificationCode { get; set; } = string.Empty;

    /// <summary>Data di avvio della registrazione in UTC.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Scadenza in UTC entro cui il tentativo puo essere ripreso.</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>Data di verifica del profilo in UTC; nulla finche la verifica non e completata.</summary>
    public DateTime? ProfileVerifiedAtUtc { get; set; }
}
