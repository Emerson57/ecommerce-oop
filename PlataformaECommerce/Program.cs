using System;
using System.Globalization;
using PlataformaECommerce.Dominio;

namespace PlataformaECommerce
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Configuración cultural para formato de moneda en Colombia (COP).
            CultureInfo.CurrentCulture = new CultureInfo("es-CO");

            Console.Title = "Demo e-Commerce OOP - Asignación No. 3 (Herencia)";

            PrintHeader(
                "Demo e-Commerce OOP",
                "Herencia y Polimorfismo: Productos y Usuarios",
                "Asignación No. 3 - Extensión de funcionalidades mediante herencia"
            );

            try
            {
                // ==========================================================
                // 1) CREACIÓN DE PRODUCTOS DERIVADOS (HERENCIA)
                // ==========================================================
                Section("1) Creación de Productos Derivados (ProductoDigital / ProductoFisico)");

                // Producto digital (descargable)
                var ebook = new ProductoDigital(
                    id: 1,
                    nombre: "Ebook: Guía de C#",
                    descripcion: "Aprende C# desde cero con ejemplos prácticos.",
                    precio: 29900m,
                    stock: 50,             // En digital puede interpretarse como licencias/cupos.
                    formatoArchivo: "PDF",
                    tamanoMB: 12.5m
                );

                // Producto físico (requiere logística de envío)
                var mouse = new ProductoFisico(
                    id: 2,
                    nombre: "Mouse Gamer",
                    descripcion: "Mouse con DPI ajustable y retroiluminación.",
                    precio: 79900m,
                    stock: 15,
                    pesoKg: 0.18m,
                    altoCm: 4.0m,
                    anchoCm: 6.5m,
                    largoCm: 12.0m
                );

                PrintProduct(ebook);
                PrintProduct(mouse);

                // ==========================================================
                // 2) CREACIÓN DE USUARIOS DERIVADOS (HERENCIA)
                // ==========================================================
                Section("2) Creación de Usuarios Derivados (Cliente / Administrador)");

                var cliente = new Cliente(
                    id: 101,
                    nombre: "Juan Pérez",
                    correo: "juan@email.com",
                    contrasena: "123456"
                );

                var admin = new Administrador(
                    id: 201,
                    nombre: "Admin Operaciones",
                    correo: "admin@techmarket.com",
                    contrasena: "Admin123",
                    area: "Inventario"
                );

                // Mostrar perfil (polimorfismo: cada clase define su rol)
                Info("Perfil Cliente:");
                Console.WriteLine(cliente.MostrarPerfil());

                Info("Perfil Administrador:");
                Console.WriteLine(admin.MostrarPerfil());

                // ==========================================================
                // 3) CLIENTE: PREFERENCIAS + HISTORIAL
                // ==========================================================
                Section("3) Cliente: Preferencias e Historial de Compras");

                Info("Agregando preferencias al cliente...");
                cliente.AgregarPreferencia("Gaming");
                cliente.AgregarPreferencia("Tecnología");
                Success("Preferencias agregadas.");

                Info("Registrando compras (IDs de pedidos)...");
                cliente.AgregarCompra(5001);
                cliente.AgregarCompra(5002);
                Success("Compras registradas.");

                Console.WriteLine(cliente.MostrarPerfil());
                Console.WriteLine(cliente.VerHistorial());

                // ==========================================================
                // 4) CARRITO: POLIMORFISMO (List<Producto> con derivados)
                // ==========================================================
                Section("4) CarritoCompra: Polimorfismo con ProductoDigital/ProductoFisico");

                var carrito = new CarritoCompra();
                Success("Carrito creado correctamente.");
                Info($"Total inicial: {FormatMoney(carrito.Total)}");

                Info("Agregando productos derivados al carrito...");
                carrito.AgregarProducto(ebook);
                Success($"Agregado: {ebook.Nombre}");

                carrito.AgregarProducto(mouse);
                Success($"Agregado: {mouse.Nombre}");

                Info($"Items: {carrito.CantidadItems}");
                Info($"Total carrito: {FormatMoney(carrito.Total)}");

                Console.WriteLine();
                Console.WriteLine("Detalle (Descripción detallada - método virtual sobrescrito):");
                foreach (var p in carrito.Productos)
                {
                    Console.WriteLine($" - {p.ObtenerDescripcionDetallada()}");
                }

                // ==========================================================
                // 5) ADMIN: GESTIÓN DE INVENTARIO (AJUSTE DE STOCK)
                // ==========================================================
                Section("5) Administrador: Gestión de Inventario (actualizar stock)");

                Info($"Stock actual del producto '{mouse.Nombre}': {mouse.Stock}");
                Info("Administrador ajusta el stock del Mouse a 20...");

                admin.GestionarInventario(mouse, nuevoStock: 20);

                Success($"Stock actualizado. Nuevo stock de '{mouse.Nombre}': {mouse.Stock}");

                // ==========================================================
                // 6) ADMIN: PROMOCIÓN (DESCUENTO) Y EFECTO EN EL CARRITO
                // ==========================================================
                Section("6) Administrador: Establecer promoción y evidenciar impacto en total");

                Info($"Precio actual del Ebook: {FormatMoney(ebook.Precio)}");
                Info("Administrador aplica promoción del 10% al Ebook...");

                admin.EstablecerPromocion(ebook, porcentajeDescuento: 10m);

                Success($"Nuevo precio del Ebook: {FormatMoney(ebook.Precio)}");

                // El carrito recalcula automáticamente al agregar/remover, pero aquí ya estaba agregado.
                // En este modelo básico, el total depende del precio actual del objeto Producto.
                // Para evidenciarlo, vamos a quitar y volver a agregar el Ebook, o simplemente mostrar el total
                // ya que el total se recalcula por cambios. Para dejarlo perfecto, recalculamos:
                Info("Actualizando total del carrito para reflejar el nuevo precio...");
                // Como RecalcularTotal es privado, la forma correcta es hacer un cambio controlado:
                carrito.RemoverProducto(ebook.Id);
                carrito.AgregarProducto(ebook);

                Success($"Total actualizado del carrito: {FormatMoney(carrito.Total)}");

                // ==========================================================
                // 7) RESUMEN FINAL
                // ==========================================================
                Section("7) Resumen final (captura recomendada)");

                Console.WriteLine($"Cliente: {cliente.Nombre} | Correo: {cliente.Correo} | Rol: {cliente.ObtenerRol()}");
                Console.WriteLine($"Administrador: {admin.Nombre} | Correo: {admin.Correo} | Rol: {admin.ObtenerRol()}");
                Console.WriteLine($"Items en carrito: {carrito.CantidadItems}");
                Console.WriteLine($"Total final carrito: {FormatMoney(carrito.Total)}");

                PrintFooter("Fin de la demostración - Asignación No. 3 (Herencia)");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Error("Ocurrió un error inesperado durante la demo.");
                Console.WriteLine(ex);
            }

            Console.WriteLine();
            Info("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }

        // ==========================================================
        // Helpers: salida a consola
        // ==========================================================

        static void PrintHeader(string title, string subtitle, string assignment)
        {
            Console.WriteLine("============================================================");
            Console.WriteLine(title.ToUpper());
            Console.WriteLine(subtitle);
            Console.WriteLine(assignment);
            Console.WriteLine($"Fecha/Hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("============================================================");
            Console.WriteLine();
        }

        static void PrintFooter(string message)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(message);
            Console.WriteLine("============================================================");
        }

        static void Section(string title)
        {
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine(title);
            Console.WriteLine("------------------------------------------------------------");
        }

        static void Info(string msg) => Console.WriteLine($"[INFO] {msg}");
        static void Success(string msg) => Console.WriteLine($"[OK]   {msg}");
        static void Warning(string msg) => Console.WriteLine($"[WARN] {msg}");
        static void Error(string msg) => Console.WriteLine($"[ERR]  {msg}");

        static string FormatMoney(decimal value)
        {
            return value.ToString("C0", CultureInfo.CurrentCulture);
        }

        /// Imprime un producto usando ToString() y la descripción detallada.
        static void PrintProduct(Producto p)
        {
            Console.WriteLine($" - {p}");
            Console.WriteLine($"   Detalle: {p.ObtenerDescripcionDetallada()}");
        }
    }
}