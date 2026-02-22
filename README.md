# Plataforma e-Commerce - Implementación de Clases Básicas

## 1. Descripción del Proyecto

Este proyecto corresponde a la Asignación No. 2 del curso de Programación Orientada a Objetos. Su propósito es implementar clases básicas que modelen una plataforma e-Commerce, aplicando principios fundamentales del paradigma orientado a objetos.

El sistema simula el comportamiento básico de una tienda en línea mediante la implementación de tres clases principales:

- Producto
- Usuario
- CarritoCompra

El desarrollo se realizó utilizando C# y .NET, enfocándose en la correcta definición de atributos, propiedades, constructores y métodos que permitan representar el dominio del negocio de manera estructurada.

---

## 2. Objetivo Académico

Aplicar los conceptos fundamentales de Programación Orientada a Objetos mediante:

- Definición de clases y objetos
- Encapsulación de atributos
- Uso de propiedades (get/set)
- Implementación de constructores
- Manejo de colecciones (List<T>)
- Aplicación del principio de responsabilidad única

El proyecto busca demostrar la correcta estructuración de clases que representen entidades reales dentro de un sistema e-Commerce.

---

## 3. Tecnologías Utilizadas

- Lenguaje de programación: C#
- Framework: .NET
- Entorno de desarrollo: Visual Studio 2026
- Control de versiones: Git
- Repositorio remoto: GitHub

---

## 4. Estructura del Proyecto
/PlataformaEcommerce
│
├── Dominio
│ ├── Producto.cs
│ ├── Usuario.cs
│ ├── CarritoCompra.cs
│
├── Program.cs
└── README.md


La carpeta **Dominio** contiene las clases que representan las entidades principales del sistema.  
El archivo `Program.cs` permite ejecutar una simulación básica para validar el funcionamiento.

---

## 5. Implementación de Clases

### Clase Producto

Representa los artículos disponibles para la venta.

**Atributos principales:**
- id
- nombre
- descripción
- precio
- stock

**Funcionalidades:**
- Constructor para inicializar valores
- Propiedades para acceder y modificar atributos
- Validación básica para evitar valores negativos en precio y stock

---

### Clase Usuario

Modela la información de los usuarios del sistema.

**Atributos principales:**
- id
- nombre
- correo electrónico
- contraseña

**Funcionalidades:**
- Constructor para inicializar el objeto
- Métodos para actualizar información
- Destructor opcional con fines académicos

---

### Clase CarritoCompra

Representa el conjunto de productos seleccionados por el usuario.

**Atributos principales:**
- Lista de productos
- Total del carrito

**Métodos implementados:**
- AgregarProducto()
- RemoverProducto()
- CalcularTotal()

El total del carrito se calcula recorriendo la lista de productos almacenados.

---

## 6. Funcionamiento de la Aplicación

En el archivo `Program.cs` se realiza una simulación básica del sistema:

1. Se crean instancias de productos.
2. Se crea un carrito de compras.
3. Se agregan productos al carrito.
4. Se eliminan productos.
5. Se calcula y muestra el total final en consola.

Este flujo permite verificar que las clases interactúan correctamente entre sí.

---

## 7. Capturas de Pantalla

A continuación, se incluyen evidencias del funcionamiento del proyecto:

### Estructura del Proyecto
![Estructura](imagenes/estructura.png)

### Clase Producto
![Producto](imagenes/producto.png)

### Clase Usuario
![Usuario](imagenes/usuario.png)

### Clase CarritoCompra
![Carrito](imagenes/carrito.png)

### Ejecución en Consola
![Ejecucion](imagenes/ejecucion.png)

> Nota: Las imágenes deben almacenarse en una carpeta llamada `imagenes` dentro del repositorio.

---

## 8. Desafíos Encontrados y Soluciones

Uno de los principales desafíos fue definir correctamente la responsabilidad de cada clase sin mezclar lógica entre entidades. Inicialmente, el cálculo del total del carrito se encontraba fuera de la clase CarritoCompra. Sin embargo, aplicando el principio de responsabilidad única, se trasladó esta funcionalidad a la clase correspondiente.

Otro reto fue manejar adecuadamente la colección de productos dentro del carrito. Se resolvió utilizando la clase `List<Producto>` de C#, lo que permitió almacenar objetos y recorrerlos para calcular el total dinámicamente.

Asimismo, se implementaron validaciones básicas para evitar inconsistencias como precios negativos o stock inválido.

---

## 9. Cómo Ejecutar el Proyecto

1. Clonar el repositorio: git clone URL_DEL_REPOSITORIO
2. Abrir la solución en Visual Studio 2026.
3. Compilar el proyecto.
4. Ejecutar la aplicación desde `Program.cs`.

---

## 10. Autor

Nombre del estudiante: Emerson Andrey Rodríguez Rincón
Curso: Programación Orientada a Objetos  
Asignación No. 2  
Año: 2026