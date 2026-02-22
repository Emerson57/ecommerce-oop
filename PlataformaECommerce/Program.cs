using System;
using System.Collections.Generic;
using System.Globalization;
using PlataformaECommerce.Dominio;

namespace PlataformaECommerce
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Para imprimir moneda en formato Colombia (COP) en consola.
            CultureInfo.CurrentCulture = new CultureInfo("es-CO");

            Console.Title = "Demo e-Commerce OOP - Asignación No. 2";

            PrintHeader(
                "Demo e-Commerce OOP",
                "Pruebas de clases: Producto, Usuario, CarritoCompra",
                "Asignación No. 2 - Implementación de clases básicas"
            );

            try
            {
                // ==========================================
                // 1) CREAR PRODUCTOS (Prueba de constructor + propiedades)
                // ==========================================
                Section("1) Creación de Productos (constructor + propiedades)");

                var productosCatalogo = new List<Producto>
                {
                    new Producto(1, "Mouse Logitech", "Mouse inalámbrico", 80000m, 10),
                    new Producto(2, "Teclado Mecánico", "Switch Blue, retroiluminado", 180000m, 5),
                    new Producto(3, "Audífonos", "Cancelación de ruido", 120000m, 8)
                };

                PrintProductCatalog(productosCatalogo);

                // ==========================================
                // 2) CREAR USUARIO (Prueba de constructor + getters/setters)
                // ==========================================
                Section("2) Creación de Usuario (constructor + gestión de datos)");

                var usuario = new Usuario(1, "Juan Pérez", "juan@email.com", "12345");
                PrintUser(usuario);

                // Prueba de modificación de datos del usuario (si tienes setters/propiedades)
                Info("Actualizando nombre del usuario para evidenciar setters...");
                // Ajusta el nombre de la propiedad si cambia en tu clase:
                usuario.Nombre = "Juan Pérez Gómez";
                PrintUser(usuario);

                // ==========================================
                // 3) CREAR CARRITO (Prueba de constructor + total inicial)
                // ==========================================
                Section("3) Creación de CarritoCompra (constructor + total inicial)");

                var carrito = new CarritoCompra();
                Success("Carrito creado correctamente.");
                Info($"Total inicial del carrito: {FormatMoney(carrito.Total)}"); // Ajusta propiedad si se llama diferente.

                // ==========================================
                // 4) AGREGAR PRODUCTOS (Pruebas de AgregarProducto + CalcularTotal)
                // ==========================================
                Section("4) Agregar productos al carrito (AgregarProducto + CalcularTotal)");

                AddToCart(carrito, productosCatalogo[0]); // Mouse
                AddToCart(carrito, productosCatalogo[1]); // Teclado
                AddToCart(carrito, productosCatalogo[2]); // Audífonos

                Info("Contenido del carrito después de agregar:");
                PrintCart(carrito);

                // ==========================================
                // 5) REMOVER PRODUCTOS (Pruebas de RemoverProducto + CalcularTotal)
                // ==========================================
                Section("5) Remover un producto del carrito (RemoverProducto + total actualizado)");

                Info("Intentando remover el producto con ID = 2 (Teclado Mecánico)...");
                bool removido = carrito.RemoverProducto(2); // Ajusta nombre del método si cambia.
                if (removido) Success("Producto removido correctamente.");
                else Warning("No se removió el producto (no encontrado).");

                Info("Contenido del carrito después de remover:");
                PrintCart(carrito);

                // ==========================================
                // 6) PRUEBA DE REMOVER INEXISTENTE (evidencia de control de casos)
                // ==========================================
                Section("6) Prueba: Remover producto inexistente (caso de error controlado)");

                Info("Intentando remover el producto con ID = 999 (no existe)...");
                bool removidoInexistente = carrito.RemoverProducto(999);
                if (removidoInexistente)
                    Warning("Se removió algo inesperadamente. Revisa tu lógica.");
                else
                    Success("Correcto: el producto no existía y no se removió nada.");

                // ==========================================
                // 7) PRUEBA DE ACTUALIZAR PRODUCTO (precio/stock) Y RECALCULAR
                // ==========================================
                Section("7) Prueba: Modificar precio/stock de un producto y recalcular");

                Info("Actualizando precio y stock del Mouse (ID = 1) para evidenciar setters...");
                var mouse = productosCatalogo[0];

                // Ajusta propiedades según tu implementación:
                mouse.Precio = 90000m;
                mouse.Stock = 7;

                Success("Producto actualizado:");
                PrintSingleProduct(mouse);

                Info("Recalculando total del carrito...");
                carrito.CalcularTotal(); // Ajusta si tu total se calcula automático; si no existe, quita esta línea.
                Info($"Total del carrito (recalculado): {FormatMoney(carrito.Total)}");

                // ==========================================
                // 8) PRUEBA DE VALIDACIONES (si tu clase lanza excepciones)
                // ==========================================
                Section("8) Pruebas de validación (si aplican): precio/stock inválidos");

                Info("Intentando crear un producto con precio negativo para validar control...");
                try
                {
                    var invalido = new Producto(99, "Producto inválido", "Prueba", -100m, 1);
                    Warning("Se creó un producto con precio negativo. Si no tienes validación, está bien, pero podrías mejorarla.");
                }
                catch (Exception ex)
                {
                    Success($"Validación correcta: se evitó crear el producto. Mensaje: {ex.Message}");
                }

                Info("Intentando crear un producto con stock negativo para validar control...");
                try
                {
                    var invalido2 = new Producto(100, "Producto inválido 2", "Prueba", 1000m, -5);
                    Warning("Se creó un producto con stock negativo. Si no tienes validación, está bien, pero podrías mejorarla.");
                }
                catch (Exception ex)
                {
                    Success($"Validación correcta: se evitó crear el producto. Mensaje: {ex.Message}");
                }

                // ==========================================
                // 9) RESUMEN FINAL (ideal para captura final)
                // ==========================================
                Section("9) Resumen final de la demostración");

                Console.WriteLine($"Cliente: {usuario.Nombre} ({usuario.Correo})");
                Console.WriteLine($"Cantidad de productos en carrito: {GetCartCount(carrito)}");
                Console.WriteLine($"Total final: {FormatMoney(carrito.Total)}");

                PrintFooter("Fin de la demostración ✅");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Error("Ocurrió un error inesperado en la demo.");
                Console.WriteLine(ex);
            }

            Console.WriteLine();
            Info("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }

        // ==========================================================
        // Helpers (impresión profesional en consola)
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
            // "C0" -> moneda sin decimales. En es-CO muestra $ y separadores.
            return value.ToString("C0", CultureInfo.CurrentCulture);
        }

        static void PrintProductCatalog(List<Producto> products)
        {
            Console.WriteLine("Catálogo de productos:");
            foreach (var p in products)
            {
                PrintSingleProduct(p);
            }
        }

        static void PrintSingleProduct(Producto p)
        {
            // Ajusta nombres de propiedades según tu clase:
            Console.WriteLine($" - ID: {p.Id} | {p.Nombre} | {FormatMoney(p.Precio)} | Stock: {p.Stock} | Desc: {p.Descripcion}");
        }

        static void PrintUser(Usuario u)
        {
            // Ajusta nombres de propiedades según tu clase:
            Console.WriteLine($"Usuario -> ID: {u.Id} | Nombre: {u.Nombre} | Correo: {u.Correo} | Password: {"*".PadLeft(u.Contrasena?.Length ?? 4, '*')}");
        }

        static void AddToCart(CarritoCompra carrito, Producto p)
        {
            carrito.AgregarProducto(p);
            Success($"Producto agregado: {p.Nombre}");
            // Si tu total se calcula solo, esto ya funcionará. Si no, llama CalcularTotal()
            carrito.CalcularTotal();
            Info($"Total actual: {FormatMoney(carrito.Total)}");
        }

        static void PrintCart(CarritoCompra carrito)
        {
            // Ajusta esto según cómo expongas la lista de productos en tu carrito:
            // Opción A: carrito.Productos
            // Opción B: carrito.ListaProductos
            // Opción C: un método carrito.ObtenerProductos()

            var productos = GetCartProducts(carrito);

            if (productos.Count == 0)
            {
                Warning("El carrito está vacío.");
                return;
            }

            Console.WriteLine("Productos en el carrito:");
            foreach (var p in productos)
            {
                Console.WriteLine($" - {p.Id} | {p.Nombre} | {FormatMoney(p.Precio)}");
            }

            Info($"Total carrito: {FormatMoney(carrito.Total)}");
        }

        static IReadOnlyList<Producto> GetCartProducts(CarritoCompra carrito)
        {
            // ✅ ADAPTA ESTA FUNCIÓN A TU IMPLEMENTACIÓN REAL
            // ------------------------------------------------
            // Si tu CarritoCompra tiene una propiedad pública List<Producto> Productos:
            return carrito.Productos;

            // Si se llama diferente, comenta la línea anterior y usa la tuya, por ejemplo:
            // return carrito.ListaProductos;
            // return carrito.ObtenerProductos();
        }

        static int GetCartCount(CarritoCompra carrito)
        {
            return GetCartProducts(carrito).Count;
        }
    }
}