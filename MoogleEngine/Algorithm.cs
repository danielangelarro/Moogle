using System.ComponentModel;

namespace MoogleEngine;

///<summary>
///Clase encargada de almacenar los algoritmos a utilizar.
///</summary>
public class Algorithm
{
    ///<summary>
    ///Busca la palabra más semejante en una lista de palabras.
    ///</summary>
    ///<remarks>
    ///La búsqueda se realiza en un radio de palabras con un tamaño mayor o menor a la palabra actual con diferencia de 1.
    ///</remarks>
    ///<return>
    ///Devuelve la palabra que más se asemeja en su escritura.
    ///</return>
    ///<param name="WordsTotal">
    ///Lista de palabras en total.
    ///</param>
    ///<param name="word">
    ///Palabra a procesar.
    ///</param>
    public string Suggest(List<string> WordsTotal, string word)
    {
        string suggest = word;        // Guarda el resultado de la búsqueda.
        int error = word.Length + 5;  // Guarda la distancia entre la palabra escogida y **suggest**.
        
        int start = BinarySearch(WordsTotal, word.Length - 1);  // Marca el inicio del intervalo de la búsqueda.
        int end = BinarySearch(WordsTotal, word.Length + 2);    // Marca el final del intervalo de la búsqueda.

        for (int i = start; i < end; i++)
        {
            int dist = EdistDistance(WordsTotal[i], word);  // Guarda la distancia entre **suggest** y la palabra que se está procesando actualmente.

            if(dist < error)
            {
                error = dist;
                suggest = WordsTotal[i];
            }
        }

        return suggest;
    }

    ///<summary>
    ///Realiza el cálculo para el valor del vector dependiendo al TF-IDF para una palabra determinada.
    ///</summary>
    ///<return>
    ///Devuelve el valor del vector.
    ///</return>
    ///<param name="repWordsInDocuments">
    ///Cantidad de veces que aparece repetida la palabra en el documento.
    ///</param>
    ///<param name="CantWords">
    ///Cantidad de palabras que tiene el documento en total.
    ///</param>
    ///<param name="CantDocs">
    ///Cantidad de documentos en total.
    ///</param>
    ///<param name="CantWords">
    ///Cantidad de documentos en los que aparecen la palabra.
    ///</param>
    public double TFIDF(double repWordsInDocuments, double CantWords, double CantDocs, double CantWordsFoundInDoc)
    {
        double tf = (repWordsInDocuments / CantWords);  // Valor de la frecuencia del termino (TF)
        double idf = (double)Math.Log(CantDocs / CantWordsFoundInDoc + 1);  // Valor inverso de la frecuencia en los documentos del término (IDF)

        return (tf * idf);
    }

    ///<summary>
    ///Realiza el proceso de Búsqueda Binaria para buscar la posición donde aparece por primera vez alguna palabra con el tamaño buscado.
    ///</summary>
    ///<return>
    ///Devuelve la posicion donde aparece por primera vez alguna palabra con el tamanio buscado.
    ///</return>
    ///<param name="arr">
    ///Lista de palabras en total.
    ///</param>
    ///<param name="x">
    ///Palabra a procesar.
    ///</param>
    ///
    /// Explicación del algoritmo:
    /// El algoritmo se basa en ir minimizando el rango de busqueda a la mitad en cada iteración acertando en el resultado con mayor rapidez.
    ///
    public int BinarySearch(List<string> arr, int x)
    {
        int i = 0, j = arr.Count - 1;   // Ambas variables marcan el inicio y el fin del intervalo en los que se realiza la búsqueda.

        while(i < j)
        {
            int mi = (i + j) / 2;

            if(arr[mi].Length >= x)
                j = mi;
            else
                i = mi + 1;
        }

        return j;
    }

    ///<summary>
    ///Algoritmo de Distancia Levenstein.
    ///</summary>
    ///<return>
    ///Devuelve la cantidad minima de cambios para transformar una palabra en otra
    ///</return>
    ///<param name="a">
    ///Palabra inicial
    ///</param>
    ///<param name="b">
    ///Palabra a conseguir a traves de las transformaciones.
    ///</param>
    ///
    /// Explicación del algoritmo:
    /// El algoritmo calcula la cantidad de cambios que hay que realizarle a una palabra para transformarla en otra.
    /// Estos cambios pueden ser eliminar, agregar o cambiar una letra.
    ///
    /// Para más información puede visitar los sitios:
    /// - https://en.m.wikipedia.org/wiki/Edit_distance
    /// - https://en.m.wikipedia.org/wiki/Levensthein_distance
    private int EdistDistance(string a, string b)
    {
        a = "." + a;
        b = "." + b;

        int lA = a.Length, lB = b.Length;
        int maxl = Math.Max(lA, lB);
        
        int[,] T = new int[50, 50];

        for (int i = 0; i <= maxl; i++)
            T[i, 0] = T[0, i] = i;
        
        for (int i = 1; i < lA; i++)
            for (int j = 1; j < lB; j++) {

                // Si las letras de ambas posiciones son iguales mantengo la cantidad de cambios anterior.
                if (a[i] == b[j])
                    T[i, j] = T[i - 1, j - 1];
                else
                    // T[i, j - 1]      -> eliminar letra
                    // T[i - 1, j]      -> agregar letra
                    // T[i - 1, j - 1]  -> cambiar letra
                    T[i, j] = Math.Min (Math.Min (T[i, j - 1], T[i - 1, j]), T[i - 1, j - 1]) + 1;
            }

        return T[lA - 1, lB - 1];
    }
}