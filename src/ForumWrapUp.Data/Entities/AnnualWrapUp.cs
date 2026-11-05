namespace ForumWrapUp.Data.Entities;

/// <summary>Riepilogo annuale persistito di un utente, con dati aggregati per generare le immagini.</summary>
public sealed class AnnualWrapUp
{
    /// <summary>MID dell'utente, parte della chiave composta del riepilogo insieme all'anno.</summary>
    public int Mid { get; set; }

    /// <summary>Anno solare di riferimento, parte della chiave composta insieme al MID.</summary>
    public int Year { get; set; }

    /// <summary>Indica se il riepilogo e pronto; l'anno deve anche essere abilitato per renderlo disponibile.</summary>
    public bool IsReady { get; set; }

    /// <summary>Numero totale di post inclusi nel riepilogo annuale.</summary>
    public int TotalPosts { get; set; }

    /// <summary>Somma netta dei points dei post inclusi nel riepilogo, anche negativa.</summary>
    public long TotalPoints { get; set; }

    /// <summary>Identificativo remoto del topic con piu post dell'utente; nullo se non esiste un risultato.</summary>
    public int? TopTopicExternalId { get; set; }

    /// <summary>Copia del titolo HTML del topic piu frequentato acquisita durante il calcolo.</summary>
    public string? TopTopicTitleHtml { get; set; }

    /// <summary>Numero di post dell'utente nel topic piu frequentato durante l'anno.</summary>
    public int TopTopicPostCount { get; set; }

    /// <summary>Identificativo remoto del post con il saldo points piu alto; nullo se non esiste un risultato.</summary>
    public int? MostSuccessfulPostExternalId { get; set; }

    /// <summary>Saldo netto dei points del post di maggior successo, se disponibile.</summary>
    public int? MostSuccessfulPostPoints { get; set; }

    /// <summary>Copia del contenuto HTML del post di maggior successo acquisita durante il calcolo.</summary>
    public string? MostSuccessfulPostContentHtml { get; set; }

    /// <summary>Data di pubblicazione in UTC del post di maggior successo, se disponibile.</summary>
    public DateTime? MostSuccessfulPostDateUtc { get; set; }

    /// <summary>Data dell'ultimo calcolo delle aggregazioni in UTC; nulla se non ancora calcolate.</summary>
    public DateTime? CalculatedAtUtc { get; set; }

    /// <summary>Account locale a cui appartiene il riepilogo.</summary>
    public UserAccount User { get; set; } = null!;

    /// <summary>Configurazione dell'anno di riferimento del riepilogo.</summary>
    public WrapUpYear WrapUpYear { get; set; } = null!;

    /// <summary>Conteggi giornalieri dell'anno, inclusi i giorni senza attivita.</summary>
    public ICollection<DailyPostStatistic> DailyPostStatistics { get; } = [];
}
