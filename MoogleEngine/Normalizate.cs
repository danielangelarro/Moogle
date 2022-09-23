using System;
using System.Text.RegularExpressions;

namespace MoogleEngine;

///<summary>
///Clase encargada de normalizar los textos.
///</summary>
public class Normalizate
{
    string ascii = " @#$_&-+()/¿?¡!;:'*,.~`|•√π÷×¶∆}{=°^¢$€£%©®™℅[]><\"\\\n\t";  // Caracteres del código ascci.
    Dictionary<char, string> Dict = new Dictionary<char, string>();  // Guarda las posibles variantes en las que puede aparecer cierto caracter.

    ///<summary>
    /// Traduce el texto a caracteres legibles que puedan ser interpretados.
    ///</summary>
    ///<return>
    ///Devuelve el texto traducido.
    ///</return>
    ///<param name="text">
    /// Texto a traducir.
    ///</param>
    private string Translate(string text)
    {
        #region Declaration

        Dict['a'] = @"áàäâæāªãåą";
        Dict['e'] = @"ėêęēèéë";
        Dict['i'] = @"ìïíįîī";
        Dict['o'] = @"ºōœøõôöòó";
        Dict['u'] = @"ūùûüú";
        Dict['c'] = @"ćçč";
        Dict['n'] = @"ńñ";

        Dict['$'] = @"ß€$¢£₹₱¥";
        Dict[' '] = @"†‡★—_–·";
        Dict['\"'] = @"„“”«»‚‘’‹›";
        
        #endregion
        
        string normalizate = text.ToLower();

        foreach (var item in Dict)
        {
            foreach (char c in item.Value)
            {
                normalizate = normalizate.Replace(c, (char)item.Key);   // Remplaza los caracteres especiales por los correspondientes.
            }
        }

        return normalizate;
    }

    ///<summary>
    /// Filtra las palabras que aparecen en el texto.
    ///</summary>
    ///<return>
    ///Devuelve las palabras que aparecen en el texto luego de normalizar el texto.
    ///</return>
    ///<param name="text">
    /// Texto del cual se quieren extaer las palabras.
    ///</param>
    public string[] Filter(string text)
    {
        text = Translate(text);

        // Separa las palabras determinadas por uno de los caracteres del codigo ascci almacenados en dicha variable.
        return text.Split(ascii.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    }

    ///<summary>
    /// Elimina los caracteres invalidos que aparecen en el texto.
    ///</summary>
    ///<return>
    /// Devuelve e texto luego de cambiar los caracteres invalidos por un caracter de espacio.
    ///</return>
    ///<param name="text">
    /// Texto del cual se quieren eliminar los caracteres inválidos.
    ///</param>
    public string DeleteInvalidCharacter(string text)
    {
        text = Translate(text);
        string expression = @"[^\w0-9]";    // Expresión regular que solo admite caracteres que no sean ni minúsculas ni dígitos.

        return Regex.Replace(text, expression, " ");
    }

    ///<summary>
    /// Normaliza el titulo de los documentos en los resultados de las busquedas.
    ///</summary>
    ///<param name="query">
    /// Texto del cual se quieren eliminar los caracteres inválidos.
    ///</param>
    public string NormalizateResultsTitles(string query)
    {
        query = query.Replace('_', ' ');
        query = query.Replace(".txt", "");
        query = query.ToUpper();

        return query;
    }
}
