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

# PARTE II
## Asignación No. 3 – Validador Dinámico del Lado del Cliente
## 8. Objetivo Académico
Desarrollar un formulario de registro de usuario con validaciones dinámicas en tiempo real utilizando JavaScript ES6+, integrándolo a una aplicación web construida con .NET Razor Pages.

El propósito es demostrar:
- Validación del lado del cliente
- Uso de módulos ES6
- Arquitectura limpia de JavaScript
- Retroalimentación instantánea al usuario
- Buenas prácticas modernas en desarrollo web

## 9. Tecnologías Utilizadas (Asignación 3)
- ASP.NET Core Razor Pages (.NET 8)
- JavaScript ES6 (módulos)
- Tailwind CSS (CDN)
- HTML5
- CSS moderno
- Git

## 10. Estructura Web del Proyecto
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

## 11. Campos del Formulario
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

## 12. Validaciones Implementadas
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

## 13. Funcionalidades Dinámicas Implementadas
- Validación en tiempo real (input, blur, change)
- Motor de reglas centralizadas
- Arquitectura modular ES6
- Separación UI vs lógica
- Barra de fortaleza de contraseña
- Edad calculada automáticamente
- Resumen de errores
- Toast dinámico tipo SaaS
- Estado de carga en botón "Registrar"

## 14. Arquitectura JavaScript
El sistema de validación fue estructurado bajo principios de separación de responsabilidades:
- utilidades.js → funciones auxiliares
- validadores.js → reglas individuales
- reglas.js → configuración centralizada por campo
- ui.js → manipulación visual
- app.js → orquestador principal
- toast.js → sistema de notificaciones
Esta estructura facilita mantenimiento y escalabilidad.

## 15. Integración con el Proyecto OOP
Aunque el validador funciona del lado del cliente, su diseño está preparado para integrarse con el modelo de dominio desarrollado en la Asignación 2.
La arquitectura permite que el formulario pueda enviar posteriormente datos hacia una API REST basada en las clases Usuario y CarritoCompra.

## 16. Desafíos Encontrados (Asignación 3)
1. Separar la lógica de validación de la manipulación visual.
2. Implementar validación asíncrona simulada sin bloquear la interfaz.
3. Calcular la edad dinámicamente evitando manipulación manual.
4. Diseñar una arquitectura modular limpia con ES6.
Las soluciones aplicadas permitieron construir un sistema mantenible, escalable y profesional.

## 17. Cómo Ejecutar el Proyecto
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
	
## 18. Conclusión Académica
El proyecto demuestra la aplicación práctica de:
- Principios de Programación Orientada a Objetos.
- Desarrollo de interfaces web modernas.
- Validación dinámica del lado del cliente.
- Buenas prácticas de arquitectura en JavaScript.
- Separación de responsabilidades.
- Diseño profesional de experiencia de usuario.
La integración entre backend estructural (POO) y frontend dinámico (ES6) permite comprender la arquitectura completa de una aplicación web moderna.

## 19. Autor
Nombre del estudiante: Emerson Andrey Rodríguez Rincón
Curso: Programación Orientada a Objetos / Programming the Internet
Asignación No. 2 y Asignación No. 3
Año: 2026
