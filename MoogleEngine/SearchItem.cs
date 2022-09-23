namespace MoogleEngine;

///<summary>
/// Clase que representa un elemento del resultado de la búsqueda.
///</summary>
public class SearchItem
{
    ///<summary>
    /// Constructor de la clase.
    ///</summary>
    ///<param name="title">
    /// Título del documento.
    ///</param>
    ///<param name="snippet">
    /// Pequeño texto del documento.
    ///</param>
    ///<param name="score">
    /// Puntuación del documento con respecto a mas proximidad con la query.
    ///</param>
    public SearchItem(string title, string snippet, double score)
    {
        Normalizate normalizate = new Normalizate();

        this.Title = normalizate.NormalizateResultsTitles(title);
        this.Snippet = snippet;
        this.Score = score;
        this.Link = title;
    }

    ///<summary>
    /// Devuelve el título del documento.
    ///</summary>
    public string Title { get; private set; }

    ///<summary>
    /// Devuelve el título del documento.
    ///</summary>
    public string Snippet { get; private set; }

    ///<summary>
    /// Devuelve la puntuación del documento.
    ///</summary>
    public double Score { get; private set; }

    ///<summary>
    /// Devuelve el nombre del archivo.
    ///</summary>
    public string Link { get; private set; }
}
