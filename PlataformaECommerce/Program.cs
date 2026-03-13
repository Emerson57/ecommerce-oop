using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Infrastructure.Factories;
using PlataformaECommerce.Infrastructure.Settings;
using PlataformaECommerce.Infrastructure.Observers;
using System;
using System.Globalization;


namespace PlataformaECommerce
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Configuración cultural para formato de moneda en Colombia (COP).
            CultureInfo.CurrentCulture = new CultureInfo("es-CO");

            Console.Title = "Demo e-Commerce OOP - Asignación No. 7 (Patrones de Diseño)";

            PrintHeader(
                "Demo e-Commerce OOP",
                "Patrones de Diseño + Herencia y Polimorfismo",
                "Asignación No. 7 - Singleton, Factory y Observer"
            );

            try
            {
                // ==========================================================
                // 0) SINGLETON: CONFIGURACIÓN CENTRAL DEL SISTEMA
                // ==========================================================
                Section("0) Singleton: Configuración central del sistema");

                Info("Obteniendo la primera referencia a la configuración del sistema...");
                var configuracionA = ConfiguracionSistema.Instancia;

                Info("Obteniendo una segunda referencia a la configuración del sistema...");
                var configuracionB = ConfiguracionSistema.Instancia;

                Success("Se obtuvieron ambas referencias correctamente.");

                Info("Verificando si ambas referencias apuntan a la misma instancia...");
                if (ReferenceEquals(configuracionA, configuracionB))
                {
                    Success("Singleton validado: ambas referencias apuntan a la misma instancia.");
                }
                else
                {
                    Error("Singleton no válido: las referencias no apuntan a la misma instancia.");
                }

                Console.WriteLine();
                Info("Configuración inicial del sistema:");
                Console.WriteLine(configuracionA.ObtenerResumenConfiguracion());

                Info("Actualizando el nombre del sistema desde la primera referencia...");
                configuracionA.ActualizarNombreSistema("TechMarket Pro");
                Success("Nombre del sistema actualizado desde configuracionA.");

                Info("Leyendo el nombre del sistema desde la segunda referencia...");
                Console.WriteLine($"Nombre leído desde configuracionB: {configuracionB.NombreSistema}");

                Info("Actualizando moneda e impuesto desde la segunda referencia...");
                configuracionB.ActualizarMoneda("COP");
                configuracionB.ActualizarPorcentajeImpuesto(0.19m);
                Success("Configuración global actualizada desde configuracionB.");

                Console.WriteLine();
                Info("Resumen final de configuración (captura recomendada):");
                Console.WriteLine(configuracionA.ObtenerResumenConfiguracion());

                // ==========================================================
                // 1) FACTORY: CREACIÓN DE ENTIDADES MEDIANTE FabricaEntidades
                // ==========================================================
                Section("1) Factory: Creación de entidades mediante FabricaEntidades");

                Info("Creando productos usando la fábrica de entidades...");

                var productoDigitalFactory = FabricaEntidades.CrearProductoDigital(
                    id: 10,
                    nombre: "Curso Avanzado de .NET",
                    descripcion: "Curso completo sobre desarrollo profesional con .NET.",
                    precio: 120000m,
                    stock: 100,
                    formatoArchivo: "MP4",
                    tamanoMB: 850m
                );

                var productoFisicoFactory = FabricaEntidades.CrearProductoFisico(
                    id: 11,
                    nombre: "Teclado Mecánico RGB",
                    descripcion: "Teclado mecánico profesional para desarrollo y gaming.",
                    precio: 350000m,
                    stock: 20,
                    pesoKg: 0.9m,
                    altoCm: 4m,
                    anchoCm: 15m,
                    largoCm: 45m
                );

                Success("Productos creados mediante FabricaEntidades.");

                PrintProduct(productoDigitalFactory);
                PrintProduct(productoFisicoFactory);

                Console.WriteLine();

                Info("Creando usuarios mediante la fábrica...");

                var clienteFactory = FabricaEntidades.CrearCliente(
                    id: 301,
                    nombre: "Laura Gómez",
                    correo: "laura@email.com",
                    contrasena: "Cliente123"
                );

                var adminFactory = FabricaEntidades.CrearAdministrador(
                    id: 401,
                    nombre: "Administrador Plataforma",
                    correo: "admin@plataforma.com",
                    contrasena: "Admin456",
                    area: "Tecnología"
                );

                Success("Usuarios creados mediante FabricaEntidades.");

                Console.WriteLine(clienteFactory.MostrarPerfil());
                Console.WriteLine(adminFactory.MostrarPerfil());

                // ==========================================================
                // 2) CREACIÓN DE PRODUCTOS DERIVADOS (HERENCIA)
                // ==========================================================
                Section("2) Creación de Productos Derivados (ProductoDigital / ProductoFisico)");

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
                // 3) CREACIÓN DE USUARIOS DERIVADOS (HERENCIA)
                // ==========================================================
                Section("3) Creación de Usuarios Derivados (Cliente / Administrador)");

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
                // 4) CLIENTE: PREFERENCIAS + HISTORIAL
                // ==========================================================
                Section("4) Cliente: Preferencias e Historial de Compras");

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
                // 5) CARRITO: POLIMORFISMO (List<Producto> con derivados)
                // ==========================================================
                Section("5) CarritoCompra: Polimorfismo con ProductoDigital/ProductoFisico");

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
                // 6) ADMIN: GESTIÓN DE INVENTARIO (AJUSTE DE STOCK)
                // ==========================================================
                Section("6) Administrador: Gestión de Inventario (actualizar stock)");

                Info($"Stock actual del producto '{mouse.Nombre}': {mouse.Stock}");
                Info("Administrador ajusta el stock del Mouse a 20...");

                admin.GestionarInventario(mouse, nuevoStock: 20);

                Success($"Stock actualizado. Nuevo stock de '{mouse.Nombre}': {mouse.Stock}");

                // ==========================================================
                // 7) ADMIN: PROMOCIÓN (DESCUENTO) Y EFECTO EN EL CARRITO
                // ==========================================================
                Section("7) Administrador: Establecer promoción y evidenciar impacto en total");

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
                // 8) OBSERVER: SISTEMA DE NOTIFICACIONES BASADO EN EVENTOS
                // ==========================================================
                Section("8) Observer: Sistema de notificaciones del sistema e-Commerce");

                Info("Inicializando sistema de notificaciones...");

                // Sujeto observado (emisor de eventos)
                var notificador = new NotificadorEventos();

                // Observadores concretos
                var observadorUI = new ObservadorUI();
                var observadorInventario = new ObservadorInventario();
                var observadorLogs = new ObservadorLogs();

                Info("Registrando observadores en el sistema...");

                notificador.RegistrarObservador(observadorUI);
                notificador.RegistrarObservador(observadorInventario);
                notificador.RegistrarObservador(observadorLogs);

                Success("Observadores registrados correctamente.");

                Console.WriteLine();

                // ==========================================================
                // SIMULACIÓN DE EVENTOS DEL SISTEMA
                // ==========================================================

                Info("Simulando evento: Pedido creado");

                notificador.NotificarObservadores(
                    "PedidoCreado",
                    "El cliente Juan Pérez ha creado el pedido #5003"
                );

                Console.WriteLine();

                Info("Simulando evento: Inventario actualizado");

                notificador.NotificarObservadores(
                    "InventarioActualizado",
                    $"El producto '{mouse.Nombre}' ha sido actualizado a stock {mouse.Stock}"
                );

                Console.WriteLine();

                Info("Simulando evento: Compra confirmada");

                notificador.NotificarObservadores(
                    "CompraConfirmada",
                    $"El cliente {cliente.Nombre} ha confirmado su compra por {FormatMoney(carrito.Total)}"
                );

                Console.WriteLine();

                Info("Simulando evento: Producto enviado");

                notificador.NotificarObservadores(
                    "PedidoEnviado",
                    "El pedido #5003 ha sido enviado al cliente."
                );

                Success("Eventos notificados a todos los observadores.");

                // ==========================================================
                // 9) RESUMEN FINAL
                // ==========================================================
                Section("9) Resumen final (captura recomendada)");

                Console.WriteLine($"Sistema configurado: {configuracionA.NombreSistema}");
                Console.WriteLine($"Moneda por defecto: {configuracionA.MonedaPorDefecto}");
                Console.WriteLine($"Cliente: {cliente.Nombre} | Correo: {cliente.Correo} | Rol: {cliente.ObtenerRol()}");
                Console.WriteLine($"Administrador: {admin.Nombre} | Correo: {admin.Correo} | Rol: {admin.ObtenerRol()}");
                Console.WriteLine($"Items en carrito: {carrito.CantidadItems}");
                Console.WriteLine($"Total final carrito: {FormatMoney(carrito.Total)}");

                PrintFooter("Fin de la demostración - Asignación No. 7 (Singleton, Factory y Observer)");
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