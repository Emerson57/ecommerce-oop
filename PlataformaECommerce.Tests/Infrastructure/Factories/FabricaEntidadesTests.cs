using NUnit.Framework;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Infrastructure.Services.Products;

namespace PlataformaECommerce.Tests.Infrastructure.Factories;

[TestFixture]
public class FabricaEntidadesTests
{
    [Test]
    public void CrearProductoDigital_DatosValidos_CreaProductoDigitalCorrectamente()
    {
        ProductoDigital producto = FabricaEntidades.CrearProductoDigital(
            nombre: "Curso C#",
            descripcion: "Curso completo en video.",
            precio: 120000m,
            stock: 50,
            formatoArchivo: "MP4",
            tamanoMB: 850.75m,
            etiquetas: new[] { "nuevo", "backend" });

        Assert.That(producto.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(producto.Nombre, Is.EqualTo("Curso C#"));
        Assert.That(producto.FormatoArchivo, Is.EqualTo("MP4"));
        Assert.That(producto.TamanoArchivoMb, Is.EqualTo(850.75m));
        Assert.That(producto.Etiquetas.Select(x => x.Value), Is.EqualTo(new[] { "nuevo", "backend" }));
    }

    [Test]
    public void CrearProductoFisico_DatosValidos_CreaProductoFisicoCorrectamente()
    {
        ProductoFisico producto = FabricaEntidades.CrearProductoFisico(
            nombre: "Teclado Mecánico",
            descripcion: "Teclado con retroiluminación.",
            precio: 350000m,
            stock: 10,
            pesoKg: 1.2m,
            altoCm: 4.5m,
            anchoCm: 18m,
            largoCm: 45m,
            categoriaId: Guid.NewGuid());

        Assert.That(producto.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(producto.Nombre, Is.EqualTo("Teclado Mecánico"));
        Assert.That(producto.PesoKg, Is.EqualTo(1.2m));
        Assert.That(producto.CategoriaId, Is.Not.Null);
        Assert.That(producto.Sku.Value, Is.EqualTo("TECLADO-MECANICO"));
        Assert.That(producto.Slug, Is.EqualTo("teclado-mecanico"));
    }

    [Test]
    public void CrearCliente_DatosValidos_CreaClienteCorrectamente()
    {
        Cliente cliente = FabricaEntidades.CrearCliente(
            nombre: "Laura Gómez",
            correo: "laura@email.com",
            contrasenaHash: "Clave123Clave123Clave123");

        Assert.That(cliente.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(cliente.Nombre, Is.EqualTo("Laura Gómez"));
        Assert.That(cliente.CorreoElectronico.Value, Is.EqualTo("laura@email.com"));
    }

    [Test]
    public void CrearAdministrador_DatosValidos_CreaAdministradorCorrectamente()
    {
        Administrador administrador = FabricaEntidades.CrearAdministrador(
            nombre: "Admin Principal",
            correo: "admin@email.com",
            contrasenaHash: "Admin123Admin123Admin123",
            area: "Inventario");

        Assert.That(administrador.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(administrador.Area, Is.EqualTo("Inventario"));
    }

    [Test]
    public void CrearProductoPorTipo_TipoDigital_DatosValidos_CreaProductoDigital()
    {
        Producto producto = FabricaEntidades.CrearProductoPorTipo(
            tipoProducto: "digital",
            nombre: "Ebook Arquitectura",
            descripcion: "Libro digital sobre arquitectura de software.",
            precio: 75000m,
            stock: 100,
            "PDF",
            12.5m,
            true);

        Assert.That(producto, Is.TypeOf<ProductoDigital>());
    }

    [Test]
    public void CrearProductoPorTipo_TipoFisico_DatosValidos_CreaProductoFisico()
    {
        Producto producto = FabricaEntidades.CrearProductoPorTipo(
            tipoProducto: "fisico",
            nombre: "Mouse Profesional",
            descripcion: "Mouse ergonómico de precisión.",
            precio: 150000m,
            stock: 15,
            0.25m,
            4m,
            7m,
            12m,
            true);

        Assert.That(producto, Is.TypeOf<ProductoFisico>());
    }

    [Test]
    public void CrearProductoPorTipo_TipoVacio_LanzaFactoryException()
    {
        Assert.Throws<FactoryException>(() =>
            FabricaEntidades.CrearProductoPorTipo(
                tipoProducto: "",
                nombre: "Ebook Arquitectura",
                descripcion: "Libro digital sobre arquitectura de software.",
                precio: 75000m,
                stock: 100,
                "PDF",
                12.5m));
    }

    [Test]
    public void CrearProductoPorTipo_TipoNoSoportado_LanzaEntidadNoSoportadaException()
    {
        Assert.Throws<EntidadNoSoportadaException>(() =>
            FabricaEntidades.CrearProductoPorTipo(
                tipoProducto: "hibrido",
                nombre: "Producto híbrido",
                descripcion: "Descripción",
                precio: 75000m,
                stock: 100));
    }

    [Test]
    public void CrearProductoPorTipo_DigitalSinParametrosSuficientes_LanzaFactoryException()
    {
        Assert.Throws<FactoryException>(() =>
            FabricaEntidades.CrearProductoPorTipo(
                tipoProducto: "digital",
                nombre: "Ebook Arquitectura",
                descripcion: "Libro digital sobre arquitectura de software.",
                precio: 75000m,
                stock: 100,
                "PDF"));
    }

    [Test]
    public void CrearProductoPorTipo_DigitalConTipoParametroIncorrecto_LanzaFactoryException()
    {
        Assert.Throws<FactoryException>(() =>
            FabricaEntidades.CrearProductoPorTipo(
                tipoProducto: "digital",
                nombre: "Ebook Arquitectura",
                descripcion: "Libro digital sobre arquitectura de software.",
                precio: 75000m,
                stock: 100,
                123,
                12.5m));
    }
}