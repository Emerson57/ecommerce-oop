using PlataformaECommerce.Dominio;

Producto p1 = new Producto(1, "Mouse", "Mouse gamer RGB", 50000m, 10);

Console.WriteLine(p1);

p1.DisminuirStock(2);
Console.WriteLine("Después de vender 2 unidades:");
Console.WriteLine(p1);