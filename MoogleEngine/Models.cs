namespace MoogleEngine;

///<summary>
///Estructura simple para almacenar 2 valores de cualquier tipo.
///</summary>
public struct Pair<TValue1, TValue2>
{
    public TValue1 A;   // Primer valor.
    public TValue2 B;   // Segundo valor.

    ///<summary>
    ///Constructor de la clase.
    ///</summary>
    ///<param name="a">
    ///Primer valor a almacenar en la estructura.
    ///</param>
    ///<param name="b">
    ///Segundo valor a almacenar en la estructura.
    ///</param>
    public Pair(TValue1 a, TValue2 b)
    {
        this.A = a;
        this.B = b;
    }
}

///<summary>
///Estructura para almacenar el modelo del JSON a la hora de deserializarlo.
///</summary>
public class JsonModel
{
    public string? Key { get; set; }     // Almacena la palabra inicial.
    public List<string>? Value { get; set; }     // Almacena todos sus sinónimos.
}