using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoogleEngine;

///<summary>
///Clase encargada de procesar los resultados la búsqueda basada en sinónimos.
///</summary>
static class Synonyms
{
    private static Normalizate Normalizate = new Normalizate();

    // Conjunto de palabras con sus sinónimos.
    static Dictionary<string, List<string>> JsonItems = new Dictionary<string, List<string>>(); 

    ///<summary>
    ///Carga los datos desde el JSon correspondiente.
    ///</summary>
    ///<return>
    ///Devuelve una lista de tipo Files con todos los documentos.
    ///</return>
    ///<param name="DocumentsForWords">
    ///Cantidad de documentos en los que aparecen las palabras.
    ///</param>
    public static string[] Load(string[] query)
    {
        foreach (string word in query)
        {
            if(!JsonItems.ContainsKey(word))
                continue;
            
            foreach (var item in JsonItems[word])
            {
                string[] text = Normalizate.Filter(item);

                query = query.Concat(text).ToArray();
            }
        }

        return query;
    }

    ///<summary>
    ///Carga los datos desde el JSon correspondiente.
    ///</summary>
    public static void Read()
    {
        JsonItems = new Dictionary<string, List<string>>();
        string jsonDoc = File.ReadAllText("wwwroot/json/synonyms.json");    // Almacena el texto del JSON.

        ConvertToDict(JsonSerializer.Deserialize<List<JsonModel>>(jsonDoc));
    }

    ///<summary>
    /// Guarda en un diccionario los resultados para accedr a ellos de manera más cómoda.
    ///</summary>
    ///<param name="words">
    /// Lista con todas las palabras y sus sinónimos.
    ///</param>
    private static void ConvertToDict(List<JsonModel> words)
    {
        foreach (JsonModel item in words)
            if(!JsonItems.ContainsKey(item.Key.ToLower()))
                JsonItems.Add(item.Key.ToLower(), item.Value);
    }
}