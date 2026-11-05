namespace ForumWrapUp.Data.Entities.Raw;

/// <summary>Post importato dal forum, con contenuto originale e riferimenti alle entita locali.</summary>
public sealed class ForumPost
{
    /// <summary>Chiave locale del post, generata dal database.</summary>
    public long Id { get; set; }

    /// <summary>Identificativo remoto del forum sorgente.</summary>
    public int ApiForumId { get; set; }

    /// <summary>Identificativo remoto del post, univoco nel modello insieme ad ApiForumId.</summary>
    public int ExternalId { get; set; }

    /// <summary>MID dell'autore e chiave esterna dell'account locale.</summary>
    public int AuthorMid { get; set; }

    /// <summary>Data di pubblicazione del post normalizzata in UTC.</summary>
    public DateTime PublishedAtUtc { get; set; }

    /// <summary>Saldo netto di like e dislike interpretato come intero; nullo se il valore non e interpretabile.</summary>
    public int? Points { get; set; }

    /// <summary>Valore originale del campo points nell'API, che puo essere una stringa vuota.</summary>
    public string? RawPoints { get; set; }

    /// <summary>Contenuto HTML originale del post, non sanitizzato per la visualizzazione.</summary>
    public string? ContentHtml { get; set; }

    /// <summary>Chiave locale del topic; nulla se il riferimento non e disponibile.</summary>
    public long? TopicId { get; set; }

    /// <summary>Chiave locale della sezione; nulla se il riferimento non e disponibile.</summary>
    public long? SectionId { get; set; }

    /// <summary>Riferimento al forum sorgente.</summary>
    public ForumInstance ForumInstance { get; set; } = null!;

    /// <summary>Account locale dell'autore del post.</summary>
    public UserAccount Author { get; set; } = null!;

    /// <summary>Topic locale associato al post, se disponibile.</summary>
    public ForumTopic? Topic { get; set; }

    /// <summary>Sezione locale associata al post, se disponibile.</summary>
    public ForumSection? Section { get; set; }
}
