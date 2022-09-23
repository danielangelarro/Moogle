using System.Net.Http.Headers;
using System.Reflection;

namespace MoogleEngine;

///<summary>
///Clase encargada de realizar los procesos internos correspondientes a los archivos a buscar.
///</summary>
public class FilesManager
{
    const string BaseDir = "../Content/";   // Dirección donde se encuentran guardados los documentos.
    private Normalizate Normalizate = new Normalizate();

    ///<summary>
    ///Convierte cada documento en un vector de tipo Files.
    ///</summary>
    ///<return>
    ///Devuelve una lista de tipo Files con todos los documentos.
    ///</return>
    ///<param name="DocumentsForWords">
    ///Cantidad de documentos en los que aparecen las palabras.
    ///</param>
    ///<param name="WordsTotal">
    ///Todas las palabras que aparecen en todos los documentos.
    ///</param>
    ///<param name="hashGlobal">
    ///Todas las palabras que aparecen en todos los documentos.
    ///</param>
    public List<Files> FilesToVector(ref Dictionary<string, double> DocumentsForWords, ref List<string> WordsTotal,
        ref HashSet<string> hashGlobal)
    {
        DirectoryInfo directory = new DirectoryInfo(BaseDir);   // Directorio donde se encuentran los documentos.
        FileInfo[] docs = directory.GetFiles("*.txt");          // Array con todos los documentos.

        List<Files> Documents = new List<Files>();
        int id = 0; // Marca el índice del archivo en proceso.

        foreach (FileInfo file in docs)
        {
            Dictionary<string, double> Repeat = new Dictionary<string, double>();   // Guarda la cantidad de veces que una palabra se repite.
            Dictionary<string, bool> hash = new Dictionary<string, bool>();         // Marca si una palabra ya fue leída antes en el documento actual.
            string textPlane = ReadDocument(file.Name);     // Guarda el texto sin procesar del documento actual.
            string[] text = this.Normalizate.Filter(textPlane);  // Almacena las palabras que aparecen en el documento.
            
            for (int i = 0; i < text.Length; i++)
            {
                // Procesa cuantas veces se repite la palabra en el documento actual.

                if(!Repeat.ContainsKey(text[i]))   // Si no existe la crea.
                    Repeat.Add(text[i], 0);
                
                Repeat[text[i]]++;

                // Guarda todas las palabras a nivel global de todos los documentos.

                if(!hashGlobal.Contains(text[i]))  // Si no existe la crea.
                {
                    hashGlobal.Add(text[i]);
                    WordsTotal.Add(text[i]);
                }
            
                // Procesa en cuantos documentos aparece la palabra actual.

                if(!hash.ContainsKey(text[i])){   // Si no existe la crea.
                    hash.Add(text[i], true);

                    if(!DocumentsForWords.ContainsKey(text[i]))  // Si no existe la crea.
                        DocumentsForWords.Add(text[i], 0);
                    
                    DocumentsForWords[text[i]]++;
                }
            }

            Files doc = new Files(id, Repeat, file.Name, text, textPlane);  // Crea un nuevo documento de tipo Files.
            Documents.Add(doc); // Agrega el nuevo documento a la lista.
            id++;
        }

        return Documents;
    }

    ///<summary>
    ///Lee el contenido del documento.
    ///</summary>
    ///<return>
    ///Devuelve el texto del documento.
    ///</return>
    ///<param name="title">
    ///Nombre del archivo a procesar.
    ///</param>
    public string ReadDocument(string title)
    {
        return File.ReadAllText(BaseDir + title);
    }

    ///<summary>
    ///Obtiene un fragmento del texto donde se muestran palabras de la búsqueda.
    ///</summary>
    ///<param name="file">
    ///Un objeto de tipo Files representando al archivo a procesar.
    ///</param>
    ///<param name="Positions">
    ///Posiciones de las palabras en cada archivos.
    ///</param>
    ///<param name="query">
    ///Palabras de la query.
    ///</param>
    ///
    /// Explicación del algoritmo:
    /// 1- Escoge un fragmento determinado por la primera oración del texto.
    /// 2- Cuenta la cantidad de palabras en el fragmento que coinciden con palabras de la query.
    /// 3- Maximiza la cantidad de palabras en el resultado final del documento.
    /// 4- Se mueve al siguiente fragmento con un tamaño mínimo de 150 caracteres siempre que sea posible.
    /// 5- Repite a partir del PASO 2.
    public void GetSnippet(ref Files file, string[] query)
    {
        string text = file.TextPlane + ".";   // Texto sin procesar del documento.
        int len_max = 0;    // Cantidad de palabras del fragmento que coinciden con las de la query.

        int start = 0;  // Marca el inicio del fragmento.
        int p = text.IndexOf('.', start);   // Marca el final del fragmento.
        
        while(p != -1)
        {
            string fragment = text.Substring(start, p - start);
            string[] words = this.Normalizate.Filter(fragment);
            int len = 0;

            foreach (string word in words)
            {
                foreach (string q in query)
                {
                    if(word == q)
                    {
                        len++;
                        break;
                    }
                }
            }

            if(len > len_max)
            {
                len_max = len;
                file.Snippet = fragment;
            }

            start = p + 1;
            if(start + 150 < text.Length)
                p = text.IndexOf('.', 150 + start);
            else
                p = text.IndexOf('.', start);
        }
    }
}
