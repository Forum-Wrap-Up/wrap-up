namespace ForumWrapUp.Data.Entities.Raw;

/// <summary>Topic del forum acquisito insieme ai post importati.</summary>
public sealed class ForumTopic
{
    /// <summary>Chiave locale del topic, generata dal database.</summary>
    public long Id { get; set; }

    /// <summary>Identificativo remoto del forum sorgente a cui appartiene il topic.</summary>
    public int ApiForumId { get; set; }

    /// <summary>Identificativo remoto del topic, univoco nel modello insieme ad ApiForumId.</summary>
    public int ExternalId { get; set; }

    /// <summary>Titolo originale restituito dall'API, che puo contenere HTML non sanitizzato.</summary>
    public string? TitleHtml { get; set; }

    /// <summary>Totale dei post del topic dichiarato dall'API, non il numero di post importati dell'utente.</summary>
    public int PostCount { get; set; }

    /// <summary>Riferimento al forum sorgente.</summary>
    public ForumInstance ForumInstance { get; set; } = null!;

    /// <summary>Post importati associati al topic.</summary>
    public ICollection<ForumPost> Posts { get; } = [];
}
