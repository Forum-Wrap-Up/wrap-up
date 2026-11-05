namespace ForumWrapUp.Data.Entities;

/// <summary>Conteggio dei post di un utente per un giorno del riepilogo annuale.</summary>
public sealed class DailyPostStatistic
{
    /// <summary>MID dell'utente, parte della chiave composta insieme ad anno e giorno.</summary>
    public int Mid { get; set; }

    /// <summary>Anno del riepilogo a cui appartiene il conteggio giornaliero.</summary>
    public int Year { get; set; }

    /// <summary>Giorno di calendario nel fuso Europe/Rome, senza componente oraria.</summary>
    public DateOnly Day { get; set; }

    /// <summary>Numero di post dell'utente nel giorno indicato; zero per i giorni senza attivita.</summary>
    public int PostCount { get; set; }

    /// <summary>Riepilogo annuale a cui appartiene la statistica.</summary>
    public AnnualWrapUp AnnualWrapUp { get; set; } = null!;
}
