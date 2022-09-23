namespace MoogleEngine;

///<summary>
///Clase que almacena la información de cada archivo.
///</summary>
public class Files
{
    public int Id;  // Índice del documento.
    public int Length;  // Tamaño del texto que contiene.
    public string Name { get; set; }    // Nombre del documento.
    public string Snippet { get; set; } // Snippet de la búsqueda realizada.
    public double Score { get; set; }   // Puntuación calculada por el cálculo del Modelo Vectorial.
    public string TextPlane { get; set; }   // Contiene el texto del documento.
    public string[] Text;   // Contiene las palabras resultantes luego de normalizar el texto.
    public Dictionary<string, double> Repeat;   // Contiene la cantidad de veces que s repiten cada palabra en el documento.
    public Dictionary<string, double> WordScore;    // Contiene el valor del vector de cada palabra en el documento.

    ///<summary>
    ///Constructor de la clase.
    ///</summary>
    ///<param name="id">
    ///Índice del archivo.
    ///</param>
    ///<param name="repeat">
    ///Cantidad de veces que se repite una palabra en el texto del archivo.
    ///</param>
    ///<param name="name">
    ///Nombre del archivo.
    ///</param>
    ///<param name="text">
    ///Palabras que contiene el archivo del archivo.
    ///</param>
    ///<param name="textPlane">
    ///Texto del archivo.
    ///</param>
    public Files(int id, Dictionary<string, double> repeat, string name, string[] text, string textPlane)
    {
        WordScore = new Dictionary<string, double>();

        this.Id = id;
        this.Name = name;
        this.Snippet = "";
        this.Text = text;
        this.Repeat = repeat;
        this.Length = text.Length;
        this.TextPlane = textPlane;
    }

    ///<summary>
    ///Devuelve el valor del TF-IDF asociado a una palabra en el documento.
    ///</summary>
    ///<return>
    ///Devuelve el valor del TF-IDF asociado a una palabra en el documento.
    ///</return>
    ///<param name="word">
    ///Palabra a la que se le quiere conocer su valor.
    ///</param>
    public double TFIDF(string word)
    {
        return WordScore[word];
    }

    ///<summary>
    ///Puntuación total del documento al sumar todos los valores de TF-IDF de las palabras que contiene.
    ///</summary>
    ///<return>
    ///Devuelve el valor total del documento.
    ///</return>
    public double ScoreTotal
    {
        set { }

        get {
            double score = 0;

            foreach (var word in Repeat)
            {
                score += (double)(word.Value * TFIDF(word.Key));
            }

            return score;
        }
    }

    // TODO: operators for cmp
}