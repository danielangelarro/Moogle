using System.Diagnostics;

namespace MoogleEngine;

///<summary>
/// Clase principal del buscador.
///</summary>

public static class Moogle
{
    static Load Manager = new Load(); // Variable general donde se ejecutan todas las tareas.

    ///<summary>
    /// Inicializa los procesos cargando los sinónimos.
    ///</summary>
    public static void Init()
    {
        Synonyms.Read();
    }

    ///<summary>
    ///Inicializa los procesos creando el objeto Manager y cargando los sinónimos.
    ///</summary>
    ///<return>
    ///Devuelve los resultados de la búsqueda realizada.
    ///</return>
    ///<param name="query">
    ///Texto con la búsqueda realizada.
    ///</param>
    public static SearchResult Query(string query) {
        SearchItem[] items = Manager.Process(query);   // Guarda la información de los documentos correspondientes al resultado la búsqueda.
        string suggest = Manager.GetSuggestion(query); // Guarda la sugerencia de busqueda en caso de no aparecer resultados en la búsqueda.

        return new SearchResult(items, suggest);
    }
}
