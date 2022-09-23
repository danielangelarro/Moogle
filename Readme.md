![MoogleInitPage](./MoogleServer/wwwroot/img/logo.png)

# ¿De qué se trata?

Moogle es un buscador como cualquier otro. Su tarea es encontrar archivos .txt correspondientes a la búsqueda realizada dentro de una carpeta que sirve de base de datos. El sitio además muestra el contenido del documento y permite realizar búsquedas aproximadas basadas en sinónimos.

# Interfaz gráfica.

![MoogleInitPage](./MoogleServer/wwwroot/img/moogle.png)

La interfaz básica es sencilla. Con un fondo oscuro y un efecto cristalizado y 3D en los elementos se ha ganado la aprobacíon de muchos beta-testers(y la mía, por supuesto). La página inicial contiene el logo y el formulario de búsqueda. 

![MoogleInitPage](./MoogleServer/wwwroot/img/search.png)

Al realizar la búsqueda se muestran los resultados en tarjetas personalizadas con efecto 3D y cristalización, así como un ícono característico a la izquierda. A la hora de hacer la búsqueda podemos realizarlo presionando en el botón **Buscar** o simplemente presinando la tecla **ENTER**.

![MoogleInitPage](./MoogleServer/wwwroot/img/not_found.png)

En caso de no aparecer resultados se muestra una imagen y el respectivo texto de “Resultado no encontrado”, además de una sugerencia de la posible frase que quisiste decir realmente. Al presionar sobre esta sugerencia nos lleva automáticamente a la búsqueda que el buscador considera correcta.

![MoogleInitPage](./MoogleServer/wwwroot/img/pagination.png)

En esta vista para facilitar la visualización de los resultados y evitar el desbordamiento en la página, fue dotada con una barra de paginación que permite dividir los resultados en grupos de a 10 e ir navegando entre ellos de manera más cómoda. 

![MoogleInitPage](./MoogleServer/wwwroot/img/view_document.png)

Al hacer _click_ en un resultado se muestra el texto correspondiente al documento y un botón de **SALIR** para regresar al buscador.

# Aspectos destacados de la implementación.

Para la realización de la búsqueda fue utilizado el cálculo de Modelo Vectorial con los algoritmos de TF-IDF el cual será explicado más adelante en este documento. Además para la búsqueda por similitud, para la correción de alguna palabra mal escrita en la búsqueda y generar la sugerencia, fue utilizado otro algoritmo conocido como Edit Distance o Levensthein Distance.

# Operadores de Búsqueda.

Para mejorar la búsqueda se han implementado operadores que permiten especificar exactamente que queremos que nos aparezca.

- `!` Este operador indica que la palabra no puede aparecer. (Ej. `“algoritmos de búsqueda !ordenación”`)
- `^` Este operador indica que la palabra debe aparecer “obligatoriamente” en el documento (Ej. `"algoritmos de ^ordenación"`)
- `~` Este operador puesto entre 2 o más palabras indica que las 2 palabras deben aparecer lo más cercanas posibles (Ej. `"algoritmos ~ ordenación"`)
- `*` Este operador indica que una palabra es más importante que otra, entre más * hallan antes de una palabra más importante será ésta. (Ej. `"algoritmos de **ordenación"`)
- `[]` Este operador indica que en la búsqueda pueden aparecer sinónimos de las palabras que se encuentren encerradas entre [] (Ej. `“algoritmos de [ordenación]”`)

# Explicación de algoritmos

## Modelo Vectorial

El modelo vectorial se utiliza para calcular numéricamente que tan similares son 2 documentos, en este caso un documento de la base de datos y la _query_ de la búsqueda. El método se apoya en el cálculo de la relevancia de una palabra en un documento (**TF-IDF**).

**Term Frequency**
> `TF` es la frequencia con que aparece una palabra en el documento.
>
> **w** = Cantidad de apariciones de una palabra en un documento.
>
> **d** = Cantidad de palabras en total que contiene el documento.
>
> `TF = (w / d)`

**Inverse Document Frequency**
> `IDF` es la cantidad _inversa_ de documentos en los que aparece la palabra.
>
> **N** = Cantidad de documentos en total.
>
> **q** = Cantidad de documentos en los que aparecen las palabras.
>
> `TF = (w / d)`

Para ponderar cada palabra solo faltaría multiplicar ambos valores.

> **tfidf(w) = tf(w) * idf(w)**

Para ponderar cada documento solo bastaría sumar todos los valores de `tfidf(w)` de cada palabra de éste y el resultado seria el valor del documento.

## Edit Distance

Este algoritmo se encarga de calcular la cantidad de cambios que hay que hacerle a un palabra para convertirla en otra. Éstos cambios pueden ser:
- Agregar una letra.
- Eliminar una letra.
- Cambiar una letra por toda.

Teniendo las 2 palabras **a** y **b** de tamaño `n` y `m` respectivamente conformamos una matriz `T` de tamaño **max(N,M) * max(N,M)** y completamos las posiciones `T[_i_, 0] = T[0, _i_] = _i_` para todo _i_ que va desde 1 hasta **max(N,M)**.

> Al recorrer las posiciones de cada palabra y comparar se puede determinar la cantidad de cambios. Si las letras en ambas posiciones _i_ y _j_ son iguales (`a[i] == b[j]`) entonces mantengo la cantidad de cambios inicial ya que no hay que hacer nada para que ambas posiciones sean iguales.
>
> En caso contrario nos quedamos con el valor mínimo de uno de éstos 3 valores y le sumamos 1:
> - `T[i, j - 1]` => **eliminar letra**
> - `T[i - 1, j]` => **agregar letra**
> - `T[i - 1, j - 1]` => **cambiar letra**
>
> El valor final sería el de la celda `T[N - 1, M - 1]`.

# Explicación del proceso de búsqueda.

## Moogle (Moogle.cs)
Mientras comila el programa, este se encarga además de cargar la interfaz de usuario, de procesar los documentos que integran la búsqueda guardados en la carpeta **./Content/** de la raiz principal del proyecto. Como la clase `Moogle` es de tipo estática se encargará de guardar todo lo que haga en la memoria RAM de la computadora.

``` c#
// Este método se llama en ./MoogleServer/Program.cs

MoogleEngine.Moogle.Init();
```

En este momento se crea la clase estática `Moogle`, y con ello se crea la case `Load`. Al crearse esta clase el programa guarda en la RAM el valor asociado a cada documento representado por el cálculo de similitud de cosenos aplicado a los valores de TF-IDF.

Primero convierte cada documento en un vector donde cada elemento guarda la información asociada a cada archivo. Antes de hacer cualquier cosa primero _normaliza_ el texto eliminano caracteres complejos de procesar como las tildes, signos de puntuación y letas de otro idioma. Luego teniendo la información realiza el cálculo del TF-IDF y ordena las palabras por orden de menor a mayor en tamaño y luego lexicográficamente.

Cuando el usuario introduce su texto de búsqueda, este es enviado a la clase `Moogle`, al método **Moogle.Query** el cual se encarga de procesar todo lo relacionado a la búsqueda y devuelve los documentos asociados a la búsqueda. Esto se divide en 2 partes: conseguir los documentos asociados y corregir el texto de búsqueda en caso de tener algún error gramatical en comparación a las palabras que aparecen en los textos. De esto se encarga la clase `Load`.

## Process (Process.cs)

Primero vectorializa la _query_ transformandola en un vector el cual almacena el valor de TF-IDF de las palabras que quedan luego de normalizar el texto y eliminar las palabras y caracteres inválidos, así como las palabras que no aparecen en ningún documento. Si solo está escrito cosas al azar que no conforman ninguna palabra(solo simbolos), entonces no retorna ningun documento, se cancela el proceso de búsqueda.

Ahora viene la parte donde se procesan los operadores. El texto de la query original pasa por un proceso de filtrado el cual localiza la presencia de operadores y realiaz acciones en dependencia de ellos, guardando un vector con los valores por los cuales se multiplican los _score_ finales de los documentos para modificar los resultados en dependencia de los operadores.

Una vez procesado esto se realiza el cálculo del modelo vectorial dándole un valor a cada documento por el cual van a aparecer ordenados en los resultados, pero no sin antes descartar los documentos que no cumplen con los requisitos:

    - Al menos una palabra de la query debe aparecer en el documento.
    - Las palabras que usen el operador `^` tienen la obligación de aparecer.
    - Las palabras que usen el operador `!` no pueden aparecer en el texto.

Luego se clona EL DICCIONARIO **Query** a un tipo _List<SearchItem>_ para su envío como respuesta a la búsqueda. Los objetos de tipo **SearchItem** contienen 3 valores:
- Título del documento.
- Fragmento del texto donde se muestran palabras de la búsqueda utilizadas en el texto _(Snippet)_, dando una pequeña descripción de éste.
- Puntuación del documento asociada a la búsqueda con respecto a su proximidad con la búsqueda deseada.

> **Para la obtención del _Snippet_ el algoritmo:**
> - Escoge un fragmento determinado por la primera oración del texto.
> - Cuenta la cantidad de palabras en el fragmento que coinciden con palabras de la query.
> - Maximiza la cantidad de palabras en el resultado final del documento.
> - Se mueve al siguiente fragmento con un tamaño mínimo de 150 caracteres siempre que sea posible.
> - Repite a partir del PASO 2.
>
> Luego se resaltan en el texto las palabras que pertenecen a la búsqueda.

Finalmente los documentos se ordenan en dependencia de aquellos que tengan mayor puntuación y se devuelven a la clase `Moogle` convirtiendo la lista en un arreglo.

## GetSuggestion (Process.cs)

Una vez tenido los documentos correspondientes a la búsqueda procedemos a sugerir una posible búsqueda en caso de que el usuario se haya equivocado a la hora de escribir alguna palabra. Para ésto fue necesario el uso de un algoritmo conocido como _Edit Distance_ o _Levensthein Distance_ para poder determinar la cantidad de cambios que hay que realizarle a una palabra para convertirla en otra. De esta manera el algoritmo se va aproximando a una posible búsqueda que haya querido realizar el usuario. 

> Para minimizar el rango de búsqueda de las palabras se acotó con ayuda de una búsqueda binaria a las palabras que en dependencia de su tamaño se encuentren en el rango de diferencia de `-1 <= x <= 2`.

# Perspectivas para futuras actualizaciones.

Estoy contento con el resultado pero aún queda mucho por hacer para hacer de este buscador el _“buscador de texto ideal”_. En futuras ediciones pensé que le vendría bien los siguientes cambios:
- Un sistema de Base de Datos para optimizar la RAM (SQLite resultó ser algo lento para nuestro próposito, así que habria que probar con otros frameworks para trabajo con Base de Datos). 
- Ajuste de snippet con Modelo Vectorial
- Ajuste de sugerencias con Modelo Vectorial
- Configurar sugerencias por familia de palabras (sufijos)
- Ordenar documentos por popularidad.
- Agregar informacion sobre palabras de la busqueda  no encontradas a la interfaz.

# Bibliografía.

- https://en.m.wikipedia.org/wiki/Edit_distance
- https://en.m.wikipedia.org/wiki/Levensthein_distance
- https://ccdoc-tecnicasrecuperacioninformacion.blogspot.com