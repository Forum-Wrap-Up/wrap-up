namespace ForumWrapUp.Data.Entities;

/// <summary>Anno configurato per il servizio e relativa abilitazione amministrativa.</summary>
public sealed class WrapUpYear
{
    /// <summary>Anno solare del wrap-up, usato come chiave senza generazione automatica.</summary>
    public int Year { get; set; }

    /// <summary>Indica se l'amministratore ha abilitato l'anno; non garantisce che i riepiloghi siano pronti.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Data di creazione della configurazione dell'anno in UTC.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Riepiloghi degli utenti relativi a questo anno.</summary>
    public ICollection<AnnualWrapUp> AnnualWrapUps { get; } = [];
}
