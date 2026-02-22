using System;
using PlataformaECommerce.Dominio;


class Program
{
    static void Main()
    {
        Producto p1 = new Producto(1, "Mouse", "Mouse gamer RGB", 50000m, 10);
        Producto p2 = new Producto(2, "Teclado", "Teclado mecánico", 120000m, 5);

        Console.WriteLine(p1);

        p1.DisminuirStock(2);
        Console.WriteLine("\nDespués de vender 2 unidades:");
        Console.WriteLine(p1);

        Usuario usuario1 = new Usuario(1, "Emerson", "emerson@email.com", "12345");
        usuario1.MostrarInformacion();

        CarritoCompra carrito = new CarritoCompra();

        carrito.AgregarProducto(p1);
        carrito.AgregarProducto(p2);

        Console.WriteLine("\nProductos en carrito:");
        foreach (var p in carrito.Productos)
        {
            Console.WriteLine($"- {p.Nombre} - {p.Precio:C}");
        }

        Console.WriteLine($"Total: {carrito.Total:C}");

        carrito.RemoverProducto(1);

        Console.WriteLine("\nDespués de remover el producto 1:");
        Console.WriteLine($"Total: {carrito.Total:C}");

    }
}