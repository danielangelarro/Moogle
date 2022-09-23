namespace MoogleEngine;

///<summary>
///Clase encargada de realizar los procesos internos de la busqueda
///</summary>
///<remarks>
///Estos procesos solo los hace a nivel general
///</remarks>
public class Load
{
    #region GlobalsVariables
    
    private FilesManager FilesManager = new FilesManager();
    private QueryManager QueryManager = new QueryManager();
    private Algorithm Algorithm = new Algorithm();
    private Normalizate Normalizate = new Normalizate();

    #endregion

    Dictionary<string, double> DocumentsForWords;   // Cantidad de documentos en los que aparecen las palabras
    HashSet<string> HashGlobal;                     // Contiene todas las palabras que aparecen en todos los documentos (sin repetir)
                                                    // [Facil y rapida verificacion de aparicion o no de la palabra]
    List<string> WordsTotal;                        // Contiene todas las palabras que aparecen en todos los documentos (sin repetir)
                                                    // [Facil manera de recorrer los elementos]
    List<Files> Documents;                          // Guarda todos los documentos

    ///<summary>
    /// Método para supervisar el ordenamiento de arreglos de strings en orden ascendiente.
    ///</summary>
    int CmpByLength(string a, string b){
        if(a.Length < b.Length)
            return -1;
        return 1;
    }

    ///<summary>
    /// Método para supervisar el ordenamiento de elementos de búsqueda en orden descendiente.
    ///</summary>
    int CmpByScore(SearchItem a, SearchItem b){
        if(a.Score > b.Score)
            return -1;
        return 1;
    }

    ///<summary>
    /// Constructor de la clase.
    ///</summary>
    ///<remarks>
    /// Preprocesa la informacion necesaria para realizar la búsqueda.
    /// - Precalcula los valores de TF-IDF correspondientes a las palabras por documentos.
    /// - Guarda las  posiciones en las que aparecen cada palabra en los documentos.
    ///</remarks>
    public Load()
    {
        DocumentsForWords = new Dictionary<string, double>();
        HashGlobal = new HashSet<string>();
        WordsTotal = new List<string>();

        Documents = this.FilesManager.FilesToVector(ref DocumentsForWords, ref WordsTotal, ref HashGlobal);
        CalculateTFIDF(ref Documents);

        WordsTotal.Sort(CmpByLength);
    }

    ///<summary>
    /// Realiza las acciones correspondientes a la búsqueda.
    ///</summary>
    ///<param name="text">
    /// Texto de la query realizada.
    ///</param>
    public SearchItem[] Process(string text)
    {
        // Crea el vector correspondiente a la query almacenando el valor del TF-IDF de las palabras que contiene.
        Dictionary<string, double> Query = this.QueryManager.ToVectory(text, Documents, HashGlobal, DocumentsForWords);

        if(Query.Count == 0)
            return new SearchItem[0];

        // Verifica si se usó algun operador y realiza las acciones correspondientes.
        double[] IncrementScoreDocument  = this.QueryManager.FilterOperator(Documents.Count, text, ref Query, ref HashGlobal, ref Documents);

        CalculateVectorialModel(Query, IncrementScoreDocument);

        List<SearchItem> query = Clone(Query.Keys.ToArray());
        query.Sort(CmpByScore);

        return query.ToArray();
    }

    ///<summary>
    /// Procesa la sugerencia a la búsqueda realizada.
    ///</summary>
    ///<returns>
    /// Devuelve la sugerencia asignada a la búsqueda en caso de que la palabra asignada 
    /// no aparezca o las palabras no exista en el documento.
    ///</returns>
    ///<param name="text">
    /// Texto de la query realizada.
    ///</param>
    public string GetSuggestion(string text)
    {
        List<string> suggest = new List<string>();
        string[] words = this.Normalizate.Filter(text);

        /// Corrige cada palabra en caso de que esté mal escrita.
        foreach (string word in words)
        {
            if(!HashGlobal.Contains(word))
                suggest.Add(this.Algorithm.Suggest(WordsTotal, word));
            else
                suggest.Add(word);
        }

        string sugestText = String.Join(' ', suggest);

        if(sugestText != text)
            return sugestText;
        
        return "";
    }

    ///<summary>
    /// Procesa el TF-IDF correspondiente a cada documento.
    ///</summary>
    ///<remarks>
    /// El método contiene una pequeña implementación de una barra de carga que muestra el estado 
    /// de procesamiento de los documentos.
    ///</remarks>
    ///<param name="Documents">
    /// Documento de la base de datos.
    ///</param>
    private void CalculateTFIDF(ref List<Files> Documents)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"[..............................] Processing data");
            
        Console.ForegroundColor = ConsoleColor.Green;
        
        int cursor = 1;

        for (int i = 0; i < Documents.Count; i++)
        {
            foreach (string word in Documents[i].Repeat.Keys)
            {
                double value = this.Algorithm.TFIDF(Documents[i].Repeat[word], Documents[i].Length, Documents.Count, DocumentsForWords[word]);

                Documents[i].WordScore.Add(word, value);
            }

            // Rellena un nuevo espacio en la barra de carga.
            if (Math.Floor((decimal)(((i+1)*30)/Documents.Count)) == cursor)
            {
                Console.CursorLeft = cursor;
                Console.Write("#");
                cursor++;
            }
        }

        Console.WriteLine();
    }

    ///<summary>
    /// Calcula el valor de cada documento para asignarles prioridad en las respuesta a la búsqueda.
    /// Los documentos se ordenan basándose en la similitud del coseno del vector correspondiente a cada documento 
    /// con respecto al vector de la query usando el método del cálculo del modelo vectorial.
    ///</summary>
    ///<remarks>
    /// EL modelo vectorial se basa en la multiplicación matricial de [documentos, palabras] X [Query]
    /// Agrega determinado valor al documento en dependencia de los operadores utilizados.
    ///
    /// Ecuación de la similitud de cosenos:
    /// SimCos(d,q) = sum( P(n,d) * P(n,q) ) / sqrt( sum(P(n,d))^2 * sum(P(n,q))^2 + 1)
    ///
    /// d = id del documento
    /// n = id del término o palabra
    /// q = query
    /// P(n,d) = valor TF-IDF del n-ésimo término del documento
    /// P(n,q) = valor TF-IDF del n-ésimo término de la query
    /// sum() = suma de todos los pesos de los términos (sumatoria)
    ///
    /// Los operadores se determinan por los rango en los que se encuentre el valor de la plabra actual de la query.
    /// Estos valores son modificados y ajustados en el método correspondiente a los operadores de la clase Query.
    ///</remarks>
    ///<param name="Query">
    /// Vector correspondiente a la query [palabra, valor]
    ///</param>
    ///<param name="IncrementScoreDocument">
    /// Valor de incremento para cada documento luego de aplicar la accion correspondientes a los operadores.
    /// de búsqueda.
    ///</param>
    private void CalculateVectorialModel(Dictionary<string, double> Query, double[] IncrementScoreDocument)
    {
        for(int i = 0; i < Documents.Count; i++)
        {
            double sim = 0, sumScore = 0, sumScoreQuery = 0;
            int words_not_founds = 0;   // Cantidad de palabras de la query que no aparecen en el documento actual.

            foreach (var query in Query)
            {
                double value = query.Value;
                double priority = 1;

                if(!Documents[i].WordScore.ContainsKey(query.Key)){
                    words_not_founds++;
                    continue;
                }

                /// Operator [!]
                if(value == -1)
                {
                    if(Documents[i].WordScore.ContainsKey(query.Key))
                    {
                        words_not_founds = Query.Count;
                        break;
                    }

                    continue;
                }

                /// Operator [*]
                if(value > 1000)
                {
                    priority = (int)(value / 1000);
                    value -= (1000 * priority);
                    value *= priority;
                }

                /// Operator [^]
                if(value > 100)
                {
                    if(!Documents[i].WordScore.ContainsKey(query.Key))
                    {
                        words_not_founds = Query.Count;
                        break;
                    }

                    value -= 100;
                }

                sim += (Documents[i].TFIDF(query.Key) * value);
                sumScore += (Documents[i].TFIDF(query.Key) * Documents[i].TFIDF(query.Key));
                sumScoreQuery += (value * value);
            }

            // Si el documento no contiene ninguna palabra de la query lo marco para no retornarlo en la respuesta.
            if(words_not_founds == Query.Count){
                Documents[i].Score = -1;
            }
            else{
                double a1 = (double)Math.Sqrt(sumScore);
                double a2 = (double)Math.Sqrt(sumScoreQuery);

                Documents[i].Score = (double)(sim / (a1 * a2 + 1)) + IncrementScoreDocument[i];
            }
        }
    }

    ///<summary>
    /// Convierte la query desde un arreglo de palabras a una lista de tipo SearchItem con los documentos
    /// asociados a la búsqueda.
    ///</summary>
    ///<param name="query">
    /// Palabras de la query.
    ///</param>
    private List<SearchItem> Clone(string[] query)
    {
        List<SearchItem> Query = new List<SearchItem>();

        for (int i = 0; i < Documents.Count; i++){
            // Si el documento no contiene ninguna palabra de la query no lo agregamos a la respuesta.
            if(Documents[i].Score == -1)
                continue;

            Files doc = Documents[i];
            this.FilesManager.GetSnippet(ref doc, query);

            string snippet = MarkSnippet(doc.Snippet, query);

            Query.Add(new SearchItem(doc.Name, snippet, doc.Score));
        }

        return Query;
    }

    ///<summary>
    /// Resalta en el texto del Snippet las palabras de la query que se encuentren en él usando la etiqueta _mark_
    ///</summary>
    ///<remarks>
    /// El algoritmo va recorriendo palabra por palabra y comparándolas con las de la query, en caso de coincidir las marca.
    ///</remarks>
    ///<param name="snippet">
    /// Fragmento de texto del documento al cual se le va a realizar la operación.
    ///</param>
    ///<param name="query">
    /// Palabras de la query.
    ///</param>
    private string MarkSnippet(string snippet, string[] query)
    {
        string text = this.Normalizate.DeleteInvalidCharacter(snippet) + " ";
        string mark = "", word = "";
        int pos = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if(text[i] == ' ')
            {
                if(word != "")
                {
                    foreach (string key in query)
                    {
                        if(word == key)
                        {
                            string aux = snippet.Substring(pos, i - pos - word.Length);
                            string SelectWord = snippet.Substring(i - word.Length, word.Length);
                            mark += (aux + "<mark>" + SelectWord + "</mark>");
                            pos = i;

                            break;
                        }
                    }

                    word = "";
                }

                continue;
            }

            word += text[i];
        }

        return mark;
    }
}
