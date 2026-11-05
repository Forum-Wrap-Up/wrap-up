namespace ForumWrapUp.Data.Entities.Raw;

/// <summary>Sezione del forum descritta dall'oggetto forum associato a un post nell'API.</summary>
public sealed class ForumSection
{
    /// <summary>Chiave locale della sezione, generata dal database.</summary>
    public long Id { get; set; }

    /// <summary>Identificativo remoto del forum sorgente a cui appartiene la sezione.</summary>
    public int ApiForumId { get; set; }

    /// <summary>Identificativo remoto della sezione, univoco nel modello insieme ad ApiForumId.</summary>
    public int ExternalId { get; set; }

    /// <summary>Nome della sezione restituito dall'API, se disponibile.</summary>
    public string? Name { get; set; }

    /// <summary>Riferimento al forum sorgente.</summary>
    public ForumInstance ForumInstance { get; set; } = null!;

    /// <summary>Post importati associati alla sezione.</summary>
    public ICollection<ForumPost> Posts { get; } = [];
}
