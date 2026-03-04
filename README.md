# Plataforma e-Commerce – Arquitectura OOP + Validador Dinámico Web
## 1. Descripción General del Proyecto
Este repositorio integra el desarrollo de dos asignaciones académicas correspondientes a diferentes materias del programa:
- Asignación No. 2 – Programación Orientada a Objetos
- Asignación No. 3 – Programming the Internet (Validador de Formularios Dinámico)

El proyecto modela una plataforma e-Commerce básica implementada bajo principios de Programación Orientada a Objetos (POO) en C#, y complementa dicha arquitectura con una aplicación web que implementa un validador dinámico del lado del cliente utilizando JavaScript ES6+.

La integración permite demostrar la relación entre:
- Modelado de dominio (backend estructural)
- Validación dinámica del lado del cliente
- Interacción entre frontend y backend en aplicaciones modernas

# PARTE I
## Asignación No. 2 – Implementación de Clases Básicas (POO)
## 2. Objetivo Académico
Aplicar los principios fundamentales de la Programación Orientada a Objetos mediante:
- Definición de clases y objetos
- Encapsulación
- Uso de propiedades (get/set)
- Implementación de constructores
- Manejo de colecciones (List<T>)
- Separación de responsabilidades

## 3. Tecnologías Utilizadas (Asignación 2)
- Lenguaje: C#
- Framework: .NET
- Entorno: Visual Studio 2026
- Control de versiones: Git
- Repositorio remoto: GitHub

## 4. Estructura del Proyecto (POO)
/PlataformaECommerce
│
├── Dominio
│   ├── Producto.cs
│   ├── Usuario.cs
│   ├── CarritoCompra.cs
│
├── Program.cs
└── README.md

## 5. Implementación de Clases
### Clase Producto
Representa los artículos disponibles para la venta.

Atributos:
- Id
- Nombre
- Descripción
- Precio
- Stock

Funcionalidades:
- Constructor parametrizado
- Validación para evitar valores negativos
- Propiedades encapsuladas

### Clase Usuario
Modela la información básica del cliente.

Atributos:
- Id
- Nombre
- Correo electrónico
- Contraseña

Funcionalidades:
- Constructor de inicialización
- Métodos de actualización
- Destructor opcional con fines académicos

### Clase CarritoCompra
Representa la colección de productos seleccionados por el usuario.

Atributos:
- Lista de productos
- Total calculado dinámicamente

Métodos:
- AgregarProducto()
- RemoverProducto()
- CalcularTotal()

## 6. Funcionamiento (POO)
En Program.cs se realiza una simulación que:
1. Crea productos.
2. Instancia un carrito.
3. Agrega y elimina productos.
4. Calcula el total final.

## 7. Desafíos Encontrados (Asignación 2)
- Separación correcta de responsabilidades.
- Cálculo dinámico del total dentro de la clase correspondiente.
- Manejo adecuado de colecciones.

## Asignación No. 3 (POO) – Extensión de Funcionalidades mediante Herencia
## 8. Objetivo Académico (Herencia)
Extender el modelo de dominio e-Commerce aplicando herencia para:
- Generalizar funcionalidades comunes en clases base.
- Especializar comportamientos y atributos en clases derivadas.
- Evidenciar polimorfismo al manipular objetos derivados mediante referencias a clases base.

La implementación se realizó de manera conceptual (sin base de datos), priorizando buenas prácticas de diseño orientado a dominio y código profesional con validaciones.

## 9. Tecnologías Utilizadas (Asignación 3 - POO Herencia)
- Lenguaje: C#
- Framework: .NET
- Entorno: Visual Studio 2026
- Control de versiones: Git
- Repositorio remoto: GitHub

## 10. Estructura del Dominio (Actualizada con Herencia)
/PlataformaECommerce
│
├── Dominio
│   ├── Producto.cs                (Clase base abstracta)
│   ├── ProductoDigital.cs         (Derivada)
│   ├── ProductoFisico.cs          (Derivada)
│   ├── Usuario.cs                 (Clase base abstracta)
│   ├── Cliente.cs                 (Derivada)
│   ├── Administrador.cs           (Derivada)
│   ├── CarritoCompra.cs           (Soporta polimorfismo con List<Producto>)
│
├── Program.cs                     (Demo completa Asignación 3 - Herencia)
└── README.md

## 11. Herencia aplicada en Productos
### 11.1 Producto (Clase base abstracta)
La clase Producto se convirtió en abstracta para representar el comportamiento común del catálogo:
- Id, Nombre, Descripción, Precio, Stock
- Métodos de negocio: AumentarStock(), DisminuirStock(), ActualizarPrecio()
- Método virtual: ObtenerDescripcionDetallada()

Este enfoque permite reutilización de código y asegura que los productos concretos se creen a través de clases derivadas.

### 11.2 ProductoDigital : Producto
Se implementó una clase derivada para productos descargables o digitales. Atributos adicionales:
- FormatoArchivo (ej: PDF, EPUB, MP4)
- TamanoMB

Incluye validaciones, constructor completo y sobrescritura de ObtenerDescripcionDetallada() para enriquecer la información mostrada.

### 11.3 ProductoFisico : Producto
Se implementó una clase derivada para productos que requieren logística. Atributos adicionales:
- PesoKg
- AltoCm, AnchoCm, LargoCm
- VolumenCm3 (cálculo útil para logística)

Incluye validaciones, constructor completo y sobrescritura de ObtenerDescripcionDetallada().

## 12. Herencia aplicada en Usuarios
### 12.1 Usuario (Clase base abstracta)
La clase Usuario se convirtió en abstracta para establecer un modelo profesional del dominio:
- Id, Nombre, Correo, Contraseña
- Método común: ActualizarDatos()
- Métodos virtuales: ObtenerRol(), MostrarPerfil()

Incluye validaciones de correo y contraseña (nota: en producción se usaría hash de contraseña).

### 12.2 Cliente : Usuario
Se implementó Cliente con atributos y métodos requeridos:
- HistorialCompras (List<int> con IDs de pedidos)
- Preferencias (HashSet<string> para evitar duplicados)
- Métodos: AgregarCompra(), VerHistorial(), AgregarPreferencia()

Se sobrescriben ObtenerRol() y MostrarPerfil() para demostrar polimorfismo.

### 12.3 Administrador : Usuario
Se implementó Administrador con capacidades de gestión:
- GestionarInventario(Producto producto, int nuevoStock)
- EstablecerPromocion(Producto producto, decimal porcentajeDescuento)

El enfoque es conceptual: sin BD, aplicando lógica y validaciones sobre objetos en memoria.

## 13. Polimorfismo evidenciado en CarritoCompra
CarritoCompra mantiene una colección List<Producto>, por lo cual puede almacenar:
- ProductoDigital
- ProductoFisico

Esto demuestra polimorfismo al operar sobre productos derivados mediante la clase base Producto, sin cambiar la lógica del carrito.

## 14. Cómo ejecutar la Demo (Asignación 3 - Herencia)
1. Clonar el repositorio:
   git clone URL_DEL_REPOSITORIO
2. Abrir la solución en Visual Studio.
3. Establecer el proyecto PlataformaECommerce como inicio.
4. Ejecutar (Ctrl + F5).
5. Tomar capturas en consola, especialmente de:
   - Creación de productos derivados
   - Creación de usuarios derivados
   - Carrito con productos digitales y físicos
   - Gestión de inventario y promoción por administrador
   - Preferencias e historial en cliente

## 15. Capturas (evidencia requerida)
Agrega aquí tus capturas en Markdown, por ejemplo:

### 15.1 Demo - Productos y Carrito
![Demo productos y carrito](ruta/imagen1.png)

### 15.2 Demo - Administrador y Cliente
![Demo roles y operaciones](ruta/imagen2.png)

## 16. Desafíos Encontrados y Soluciones (Asignación 3 - Herencia)
1. **Conversión de clases base a abstract sin romper el proyecto**
   - Solución: se ajustó Program.cs para instanciar únicamente clases derivadas (ProductoDigital/ProductoFisico y Cliente/Administrador).

2. **Encapsulación y cambios de stock sin exponer setters públicos**
   - Solución: el Administrador gestiona stock usando métodos de negocio (AumentarStock/DisminuirStock) en lugar de modificar Stock directamente.

3. **Evitar duplicados en preferencias del cliente**
   - Solución: se usó HashSet<string> con comparación sin distinguir mayúsculas/minúsculas.

4. **Demostrar impacto de promociones en el total del carrito**
   - Solución: se aplicó descuento con EstablecerPromocion() y se evidenció el cambio en el total mediante una operación controlada (remover y re-agregar).

# PARTE II
## Asignación No. 3 – Validador Dinámico del Lado del Cliente
## 17. Objetivo Académico
Desarrollar un formulario de registro de usuario con validaciones dinámicas en tiempo real utilizando JavaScript ES6+, integrándolo a una aplicación web construida con .NET Razor Pages.

El propósito es demostrar:
- Validación del lado del cliente
- Uso de módulos ES6
- Arquitectura limpia de JavaScript
- Retroalimentación instantánea al usuario
- Buenas prácticas modernas en desarrollo web

## 18. Tecnologías Utilizadas (Asignación 3)
- ASP.NET Core Razor Pages (.NET 8)
- JavaScript ES6 (módulos)
- Tailwind CSS (CDN)
- HTML5
- CSS moderno
- Git

## 19. Estructura Web del Proyecto
/PlataformaECommerce.Web
│
├── Pages
│   └── Registro
│       ├── Index.cshtml
│       └── Index.cshtml.cs
│
├── wwwroot
│   └── js
│       └── registro
│           ├── app.js
│           ├── reglas.js
│           ├── validadores.js
│           ├── utilidades.js
│           └── toast.js

## 20. Campos del Formulario
El formulario de registro incluye:
- Nombres
- Apellidos
- Correo electrónico
- Usuario (alias)
- Contraseña
- Confirmación de contraseña
- Fecha de nacimiento
- Edad (calculada automáticamente)
- Teléfono (opcional)
- Aceptación de términos

## 21. Validaciones Implementadas
Campo:				Validaciones Aplicadas
Nombres:			Obligatorio, mínimo 2 caracteres, solo letras
Apellidos:			Obligatorio, mínimo 2 caracteres, solo letras
Correo:				Formato válido + simulación async de “ya registrado”
Usuario:			4–16 caracteres, sin espacios, letras/números/_ .
Contraseña:			Mínimo 8, mayúscula, número y símbolo
Confirmación:		Debe coincidir con contraseña
Fecha nacimiento:	No futura, mayor o igual a 18 años
Edad:				Calculada automáticamente
Teléfono:			Opcional, 10 dígitos
Términos:			Obligatorio

## 22. Funcionalidades Dinámicas Implementadas
- Validación en tiempo real (input, blur, change)
- Motor de reglas centralizadas
- Arquitectura modular ES6
- Separación UI vs lógica
- Barra de fortaleza de contraseña
- Edad calculada automáticamente
- Resumen de errores
- Toast dinámico tipo SaaS
- Estado de carga en botón "Registrar"

## 23. Arquitectura JavaScript
El sistema de validación fue estructurado bajo principios de separación de responsabilidades:
- utilidades.js → funciones auxiliares
- validadores.js → reglas individuales
- reglas.js → configuración centralizada por campo
- ui.js → manipulación visual
- app.js → orquestador principal
- toast.js → sistema de notificaciones
Esta estructura facilita mantenimiento y escalabilidad.

## 24. Integración con el Proyecto OOP
Aunque el validador funciona del lado del cliente, su diseño está preparado para integrarse con el modelo de dominio desarrollado en la Asignación 2.
La arquitectura permite que el formulario pueda enviar posteriormente datos hacia una API REST basada en las clases Usuario y CarritoCompra.

## 25. Desafíos Encontrados (Asignación 3)
1. Separar la lógica de validación de la manipulación visual.
2. Implementar validación asíncrona simulada sin bloquear la interfaz.
3. Calcular la edad dinámicamente evitando manipulación manual.
4. Diseñar una arquitectura modular limpia con ES6.
Las soluciones aplicadas permitieron construir un sistema mantenible, escalable y profesional.

## 26. Cómo Ejecutar el Proyecto
### Parte Consola (Asignación 2)
1. Clonar el repositorio:
	git clone URL_DEL_REPOSITORIO
2. Abrir en Visual Studio.
3. Ejecutar PlataformaECommerce.

### Parte Web (Asignación 3)
1. Abrir solución en Visual Studio.
2. Establecer PlataformaECommerce.Web como proyecto de inicio.
3. Ejecutar.
4. Navegar a:
	https://localhost:PUERTO/Registro
	
## 27. Conclusión Académica
El proyecto demuestra la aplicación práctica de:
- Principios de Programación Orientada a Objetos.
- Desarrollo de interfaces web modernas.
- Validación dinámica del lado del cliente.
- Buenas prácticas de arquitectura en JavaScript.
- Separación de responsabilidades.
- Diseño profesional de experiencia de usuario.
La integración entre backend estructural (POO) y frontend dinámico (ES6) permite comprender la arquitectura completa de una aplicación web moderna.

## 28. Autor
Nombre del estudiante: Emerson Andrey Rodríguez Rincón
Curso: Programación Orientada a Objetos / Programming the Internet
Asignación No. 2 y Asignación No. 3
Año: 2026
