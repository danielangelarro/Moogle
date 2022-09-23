namespace MoogleEngine;

///<summary>
///Clase que representa el resultado de la búsqueda.
///</summary>
public class SearchResult
{
    private FilesManager FilesManager = new FilesManager();
    private SearchItem[] items; // Documentos encontrados en la búsqueda.

    ///<summary>
    ///Constructor de la clase.
    ///</summary>
    ///<param name="items">
    ///Elementos que forman parte del resultado de la búsqueda.
    ///</param>
    ///<param name="suggestion">
    ///Sugerencia de búsqueda.
    ///</param>
    public SearchResult(SearchItem[] items, string suggestion="")
    {
        if (items == null) {
            throw new ArgumentNullException("items");
        }

        this.items = items;
        this.Suggestion = suggestion;
    }

    ///<summary>
    ///Constructor de la clase.
    ///</summary>
    public SearchResult() : this(new SearchItem[0]) {

    }

    ///<summary>
    ///Sugerencia de búsqueda.
    ///</summary>
    public string Suggestion { get; private set; }

    ///<summary>
    ///Devuelve Informacion asociada al elemento de la búsqueda con determinado título.
    ///</summary>
    ///<remarks>
    ///Retorna un Pair de 2 valores:
    ///- TÍtulo del documento.
    ///- Texto del documento.
    ///</remarks>
    ///<param name="title">
    ///TÍtulo del documento del cual se quiere extraer la información.
    ///</param>
    public Pair<string, string> GetItemById(string title) {
        Normalizate normalizate = new Normalizate();
        Pair<string, string> item;

        item.A = normalizate.NormalizateResultsTitles(title);
        item.B = this.FilesManager.ReadDocument(title);

        return item;
    }

    ///<summary>
    ///Conjunto itearable con todos los resultados de la búsqueda.
    ///</summary>
    public IEnumerable<SearchItem> Items() {
        return this.items;
    }

    ///<summary>
    ///Devuelve los resultados por pequeños lotes para minimizar la cantidad de resultados en una sola página.
    ///</summary>
    ///<return>
    ///Devuelve los resultados pertenecientes al lote actual.
    ///</return>
    ///<param name="page">
    ///Id del lote del cual se quiere conocer sus elementos.
    ///</param>
    public IEnumerable<SearchItem> Page(int page) {
        if(this.Count == 0)
            return Items();

        List<SearchItem> views = new List<SearchItem>();    // Items pertenecientes al lote actual.
        int cant = Math.Min(page * 10, this.Count);         // Id máximo correspondiente al último elemento del lote.

        for (int i = (page - 1) * 10; i < cant; i++)
        {
            views.Add(this.items[i]);
        }

        return views.ToArray();
    }

    ///<summary>
    ///Cantidad de resultados de la búsqueda.
    ///</summary>
    public int Count { get { return this.items.Length; } }
}
