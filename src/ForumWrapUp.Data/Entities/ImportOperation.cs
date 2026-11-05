namespace ForumWrapUp.Data.Entities;

/// <summary>Stato persistito di un'importazione dei post di un utente per un intervallo temporale.</summary>
public sealed class ImportOperation
{
    /// <summary>Identificativo locale dell'operazione, utilizzabile per consultarne lo stato.</summary>
    public Guid Id { get; set; }

    /// <summary>MID dell'utente i cui post devono essere importati.</summary>
    public int Mid { get; set; }

    /// <summary>Primo giorno incluso nell'importazione, interpretato nel fuso Europe/Rome.</summary>
    public DateOnly FromDate { get; set; }

    /// <summary>Ultimo giorno incluso nell'importazione, interpretato nel fuso Europe/Rome.</summary>
    public DateOnly ToDate { get; set; }

    /// <summary>Stato corrente dell'esecuzione dell'importazione.</summary>
    public ImportOperationStatus Status { get; set; }

    /// <summary>Data in UTC in cui l'amministratore ha richiesto l'importazione.</summary>
    public DateTime RequestedAtUtc { get; set; }

    /// <summary>Data di inizio dell'esecuzione in UTC; nulla finche il lavoro non viene avviato.</summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>Data di termine dell'esecuzione in UTC; nulla finche l'operazione non termina.</summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>Numero di pagine API acquisite durante l'operazione.</summary>
    public int PagesFetched { get; set; }

    /// <summary>Numero totale di pagine dichiarato dall'API, se noto; puo includere post fuori intervallo.</summary>
    public int? TotalPagesExpected { get; set; }

    /// <summary>Numero di post acquisiti durante l'operazione, distinto dai post gia persistiti nel database.</summary>
    public int PostsFetched { get; set; }

    /// <summary>Numero totale di post dichiarato dall'API, se noto; non necessariamente limitato all'intervallo richiesto.</summary>
    public int? TotalPostsExpected { get; set; }

    /// <summary>Descrizione dell'eventuale errore dell'operazione, da memorizzare senza segreti o credenziali.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Account locale per cui viene eseguita l'importazione.</summary>
    public UserAccount User { get; set; } = null!;
}

public enum ImportOperationStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Cancelled
}
