using System.Security.Cryptography;

namespace MoogleEngine;

///<summary>
/// Clase encargada de realizar los procesos de paginación.
///</summary>
///<remarks>
/// La paginación es lo que permite separa los resultados de la búsqueda en varias páginas
/// para evitar el desbordmiento de elementos en un solo sitio.
///</remarks>
public class Pagination
{
    const int ResultsForPages = 10;     // Cantidad máxima de resultados por páginas.
    const int MaxPageRange = 5;         // Cantidad maxima de páginas a mostrar en la barra de paginación.

    public int Count { get; private set; }  // Cantidad de páginas correspondientes a la búsqueda.
    public int First { get; private set; }  // Primera página que se muestra en la barra de paginación.
    public int Now  { get; private set; }   // Página actual.
    public int End { get; private set; }    // Última página que se muestra en la barra de paginación.
    private int PageRange;      // Cantidad de paginas que se muestran en la barra de paginación

    ///<summary>
    /// Inicializa los parámetros necesarios para efectuar la paginación.
    ///</summary>
    ///<param name="cantResults">
    /// Cantidad de resultados de la búsqueda.
    ///</param>
    public void Load(int cantResults)
    {
        Count = cantResults / ResultsForPages;
        PageRange = Math.Min(MaxPageRange, Count);

        First = Now = 1;
        End = PageRange;

        Update();
    }

    ///<summary>
    /// Efectúa la acción correspondiente al botón presionado.
    ///</summary>
    ///<param name="option">
    /// Información de la tarea a realizar.
    ///</param>
    public void SetPage(string option)
    {
        switch (option)
        {
            case "init":    // Ir a la página de inicio (1).
                Now = 1;
                break;
            
            case "prev":    // Ir a la página anterior.
                if (Now > 1)
                    Now--;
                break;
            
            case "next":    // Ir a la siguiente página.
                if (Now < Count)
                    Now++;
                break;
            
            case "end":     // Ir a la última página.
                Now = Count;
                break;

            default:        // Ir a una de las páginas disponibles.
                Now = int.Parse(option);
                break;
        }

        Update();
    }

    ///<summary>
    /// Actualiza los valores de **First** y **End** para mostrar las páginas correspondientes en la barra de paginación.
    ///</summary>
    ///<remarks>
    /// Si la página deseada no e encuentra en el campo de vision de la barra de paginación los valores se desplazan
    /// a izquierda o derecha respectivamente.
    ///</remarks>
    private void Update()
    {
        if (Now < First)
        {
            First = Now;
            End = First + PageRange - 1;
        }
        else if (Now > End)
        {
            End = Now;
            First = End - PageRange + 1;
        }
    }
}