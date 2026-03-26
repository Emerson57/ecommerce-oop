# Arquitectura Base del Sistema E-Commerce

Este proyecto utiliza una arquitectura por capas inspirada en Clean Architecture y principios de separación de responsabilidades.

La solución se divide en cuatro capas principales:

- Domain
- Application
- Infrastructure
- Web

Cada capa tiene responsabilidades específicas y dependencias restringidas para garantizar mantenibilidad, escalabilidad y testabilidad del sistema.

---

# 1. Domain

La capa Domain contiene el núcleo del negocio.

Responsabilidades:

- Definir entidades del dominio
- Definir reglas de negocio
- Definir comportamiento del dominio
- Definir modelos conceptuales del sistema

Ejemplos:

Producto  
ProductoFisico  
ProductoDigital  

Restricciones:

- No puede depender de ninguna otra capa.
- No puede conocer EF Core, MongoDB, ASP.NET, ni frameworks externos.

# 2. Application

La capa Application contiene los casos de uso del sistema.

Responsabilidades:

- Orquestar lógica de negocio
- Definir servicios de aplicación
- Definir contratos de repositorio
- Definir DTOs de entrada y salida
- Validar reglas de negocio antes de persistir datos

Ejemplos:

ProductService  
IProductRepository  
CreateProductRequest  
UpdateProductRequest  

Dependencias permitidas:

Application → Domain

Restricciones:

- No puede depender de Web
- No puede depender directamente de Infrastructure

# 3. Infrastructure

La capa Infrastructure contiene implementaciones técnicas necesarias para el funcionamiento del sistema.

Responsabilidades:

- Acceso a base de datos SQL Server
- Configuración de Entity Framework Core
- Implementación de repositorios
- Integración con MongoDB
- Implementación de UnitOfWork

Ejemplos:

ECommerceDbContext  
ProductRepository  
InfrastructureServiceRegistration  

Dependencias permitidas:

Infrastructure → Application  
Infrastructure → Domain

Restricciones:

- No debe contener lógica de negocio
- No debe definir reglas del dominio

# 4. Web

La capa Web contiene la interfaz del sistema.

Responsabilidades:

- Exponer API REST
- Renderizar páginas web
- Manejar peticiones HTTP
- Configurar dependencias
- Manejar autenticación y middleware

Ejemplos:

ProductsController  
Program.cs  
ExceptionHandlingMiddleware  

Dependencias permitidas:

Web → Application  
Web → Infrastructure

Restricciones:

- No debe contener reglas de negocio
- No debe acceder directamente a la base de datos

# Reglas de dependencias entre proyectos

Las dependencias permitidas son:

Domain
    ↓
Application
    ↓
Infrastructure
    ↓
Web

Relación real:

Web → Application  
Web → Infrastructure  
Infrastructure → Application  
Application → Domain

Dependencias prohibidas:

Domain → Infrastructure
Domain → Web

Application → Web
Application → Infrastructure

Infrastructure → Web

