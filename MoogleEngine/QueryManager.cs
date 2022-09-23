using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MoogleEngine;

///<summary>
/// Clase encargada de Procesar todo lo referente a la query.
///</summary>
public class QueryManager
{
    private FilesManager FilesManager = new FilesManager();
    private Normalizate Normalizate = new Normalizate();
    private Algorithm Algorithm = new Algorithm();

    ///<summary>
    /// Convierte la query en un vector con los valores de cada palabra que contiene.
    ///</summary>
    ///<remarks>
    /// Un vector esta determinado por el valor de TF-IDF que posee cada palabra que contiene.
    ///</remarks>
    ///<return>
    /// Devuelve el vector relacionado a la query.
    ///</return>
    ///<param name="text">
    /// Texto de la query.
    ///</param>
    ///<param name="Documents">
    /// Matriz que contiene los vectores de los documentos.
    ///</param>
    ///<param name="hashGlobal">
    /// Todas las palabras que aparecen en todos los documentos.
    ///</param>
    ///<param name="DocumentsForWords">
    /// Cantidad de documentos en los que aparecen las palabras.
    ///</param>
    public Dictionary<string, double> ToVectory(
        string text,
        List<Files> Documents,
        HashSet<string> HashGlobal,
        Dictionary<string, double> DocumentsForWords
    )
    {
        Dictionary<string, double> Query = new Dictionary<string, double>();    // Contiene el vector correspondiente a la query.
        
        string[] words = this.Normalizate.Filter(text);  // Contiene las palabras que aparecen en la query.
        words = this.SynonymsFilter(words, text);  // Operador de busqueda por sinonimos.
        
        PreProcessQueryData(words, HashGlobal, ref Query);

        foreach (var query in Query)
        {
            double scoreQuery = this.Algorithm.TFIDF(query.Value, words.Length, Documents.Count, query.Value + DocumentsForWords[query.Key]);

            Query[query.Key] = scoreQuery;
        }

        return Query;
    }

    ///<summary>
    /// Guarda en Query la cantidad de veces que se repiten las palbras en la query siempre que aparezcan en algún documento.
    ///</summary>
    ///<param name="words">
    /// Palabras que aparecen en la query y sus sinónimos.
    ///</param>
    ///<param name="hashGlobal">
    /// Todas las palabras que aparecen en todos los documentos.
    ///</param>
    ///<param name="Query">
    /// Vector correspondiente a la query.
    ///</param>
    private void PreProcessQueryData(
        string[] words,
        HashSet<string> HashGlobal,
        ref Dictionary<string, double> Query
    )
    {
        for(int i = 0; i < words.Length; i++)
        {
            if(!HashGlobal.Contains(words[i]))
                continue;

            if(!Query.ContainsKey(words[i]))
                Query.Add(words[i], 1);
            else
                Query[words[i]]++;
        }
    }

    ///<summary>
    /// Analiza si la busqueda se expande a los sinonimos de las palabras que aparecen en la query.
    ///</summary>
    ///<remarks>
    /// Para realizar este tipo de busquedas el texto de la query debe tener el siguiente formato:
    /// "hola [mundo] nuevo", donde las palabras que puedan ser sustituidas por sus respectivos
    /// sinonimos deben estar encerradas entre [] 
    ///</remarks>
    ///<param name="words">
    /// Palabras de la query.
    ///</param>
    ///<param name="query">
    /// Texto original de la busqueda.
    ///</param>
    private string[] SynonymsFilter(string[] words, string query)
    {
        // Busca expresiones con el formato requerido.
        string expression = @"\[[\w\s]*\]*";
        var match = Regex.Match(query, expression);

        if(match.Success)
        {
            foreach (var item in match.Groups)
            {
                words = words.Concat(Synonyms.Load(this.Normalizate.Filter(item.ToString()))).ToArray();
            }
        }
            
        return words;
    }

    // Operaciones relacionadas con los operadores de búsqueda.
    #region Operators   

    ///<summary>
    /// Filtra los operadores que aparezcan en el texto de la query y realiza las acciones correspondientes a cada operador.
    ///</summary>
    ///<param name="CantFiles">
    /// Cantidad de documentos en total.
    ///</param>
    ///<param name="query">
    /// Texto original de la query.
    ///</param>
    ///<param name="Query">
    /// Vector correspondiente a la query.
    ///</param>
    ///<param name="hashGlobal">
    /// Todas las palabras que aparecen en todos los documentos.
    ///</param>
    ///<param name="Positions">
    /// Posición de las paabras en cada documento.
    ///</param>
    public double[] FilterOperator(
        int CantFiles, 
        string query,
        ref Dictionary<string, double> Query,
        ref HashSet<string> HashGlobal,
        ref List<Files> Documents
    )
    {
        string[] words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);   // Divide el texo en varias partes separados por el caracter de espacio.
        double[] IncrementScoreDocument = new double[CantFiles];    // Incremento del valor del documento luego de aplicar los operadores.

        /// Operator [~]
        if (OperatorNear(words))
        {
            for (int i = 0; i < CantFiles; i++)
            {
                double value = this.OperatorNear(HashGlobal, words, Documents[i].Text);

                if (value != -1)
                    IncrementScoreDocument[i] += value;
                else
                    IncrementScoreDocument[i] = 0;
            }
        }
        
        foreach (string item in words)
        {
            string[] norm = this.Normalizate.Filter(item);

            if(norm.Length == 0 || !HashGlobal.Contains(norm[0]))  // Si la palabra no aparece en el documento no hay nada que hacer.
                continue;
            
            /// Operator [*]
            Query[norm[0]] += OperatorPriority(item);

            /// Operator [!]
            if(OperatorNotFound(item))
                Query[norm[0]] = -1;

            /// Operator [^]            
            if(OperatorFound(item))
                Query[norm[0]] += 100;
        }

        return IncrementScoreDocument;
    }

    ///<summary>
    /// Verifica si existe el operadoe de cercanía en la expresión.
    ///</summary>
    ///<param name="words">
    /// Palabras separadas por espacio a procesar.
    ///</param>
    private bool OperatorNear(string[] words)
    {
        foreach (string item in words)
        {
            if (item == "~")
                return true;
        }

        return false;
    }

    ///<summary>
    /// Analiza el incremento del valor de importancia de la palabra en la búsqueda.
    ///</summary>
    ///<remarks>
    /// A mayor cantidad de * mayor será el valor de importancia.
    ///</remarks>
    ///<param name="word">
    /// Palabra a la que se le quiere medir el valor de importancia.
    ///</param>
    private int OperatorPriority(string word)
    {
        if(!word.StartsWith('*'))
            return 1;
        
        return ((word.LastIndexOf('*') + 2) * 1000);
    }

    ///<summary>
    /// Verifica si la palabra no debe aparecer en los documentos a buscar.
    ///</summary>
    ///<param name="word">
    /// Palabra a procesar.
    ///</param>
    private bool OperatorNotFound(string word)
    {
        return word[0] == '!';
    }

    ///<summary>
    /// Verifica si existe un operador de aparición obligatoria en el fragmento de texto actual.
    ///</summary>
    ///<param name="word">
    /// Fragmento de texto a analizar.
    ///</param>
    private bool OperatorFound(string word)
    {
        return word[0] == '^';
    }

    ///<summary>
    /// Realiza el proceso correspondiente al operador de cercnía.
    ///</summary>
    ///<param name="hashGlobal">
    /// Todas las palabras que aparecen en todos los documentos.
    ///</param>
    ///<param name="Positions">
    /// Posición de las palabras en cada documento.
    ///</param>
    ///<param name="word">
    /// Fragmento de texto a analizar.
    ///</param>
    ///<param name="FileId">
    /// Índice del documento a procesar.
    ///</param>
    public double OperatorNear(HashSet<string> HashGlobal, string[] words, string[] textWords)
    {
        string[] terms = (string[])words.Clone();

        for (int i = 1; i < terms.Length - 1; i++)
        {
            if (terms[i] == "~")
                continue;

            terms[i - 1] = this.Normalizate.DeleteInvalidCharacter(terms[i - 1]);

            // Si falta al menos 1 se cancela la operación y se disminuye en 1 el valor del documento.
            if(!HashGlobal.Contains(terms[i - 1]))
                return -1;
        }

        double value = ProcessOperatorNear(terms, textWords);   // Valor de incremento del documento.
        
        return value;
    }

    ///<summary>
    /// Calcula el incremento del valor del documento en dependencia a la cercanía de 2 o más palabras determinadas.
    ///</summary>
    ///<remarks>
    /// El valor de incremento está determinado por la distancia mínima entre 2 de las palabras a procesar. Para optimizar el algoritmo se ordenan
    /// las palabras en dependencia de las posiciones en las que aparezcan y solo se procesa las que sean consecutivas entre ellas. El resultado
    /// se acota entre 0 y 1.
    ///</remarks>
    ///<param name="positions">
    /// Posición de las palabras en cada documento.
    ///</param>
    ///<param name="terms">
    /// Palabras que deben aparecer lo más cerca posible.
    ///</param>
    ///<param name="FileId">
    /// Índice del documento a procesar.
    ///</param>
    private double ProcessOperatorNear(string[] terms, string[] textWords)
    {
        double dist, value = -1;
        int word_selected = -1, pos_last = -1, pos_now = -1;

        foreach (string word in textWords)
        {
            for (int i = 0; i < terms.Length; i++)
            {
                if( word == terms[i] )
                {
                    if( word_selected == -1 )
                    {
                        word_selected = i;
                    }
                    else if( i != word_selected )
                    {
                        dist = pos_now - pos_last;  // Distancia entre 2 de las palabras a procesar.

                        if(dist > 0)    // Actualiza el valor que modifica el valor del documento.
                        {
                            if (value == -1)
                                value = (double)(1 / dist);
                            else
                                value = Math.Min(value, (double)(1 / dist));
                        }
                    }

                    word_selected = i;
                    break;
                }
            }
        }

        return value;
    }

    ///<summary>
    /// Ordena la lista en dependencia del primer valor de cada Pair en orden ascendente.
    ///</summary>
    ///<param name="a">
    /// Primer elemento a comparar
    ///</param>
    ///<param name="b">
    /// Segundo elemento a comparar
    ///</param>
    private int Cmp(Pair<int, string> a, Pair<int, string> b)
    {
        if(a.A < b.A)
            return -1;
        return 1;
    }

    #endregion
}
