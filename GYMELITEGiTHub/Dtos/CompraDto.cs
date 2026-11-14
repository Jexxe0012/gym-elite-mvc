using System.Globalization;
using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Dtos;

/// <summary>
/// Compra con proveedor y líneas, lista para mostrar. La entidad Compra no guarda
/// total: se calcula aquí sumando cantidad por costo unitario de cada línea.
/// </summary>
public class CompraDto
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string UsuarioResponsableId { get; set; } = string.Empty;
    public List<DetalleCompraDto> Lineas { get; set; } = new();

    public string Numero => $"C-{Id:000}";
    public decimal Total => Lineas.Sum(l => l.Subtotal);
    public int Unidades => Lineas.Sum(l => l.Cantidad);
    public bool EstaAnulada => Estado == EstadosNegocio.CompraAnulada;
    public string TotalTexto => FormatoCompra.Moneda(Total);
}

public class DetalleCompraDto
{
    public int ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }

    public decimal Subtotal => Cantidad * CostoUnitario;
    public string CostoUnitarioTexto => FormatoCompra.Moneda(CostoUnitario);
    public string SubtotalTexto => FormatoCompra.Moneda(Subtotal);
}

/// <summary>Cantidad de compras registradas y monto comprado en un período (tarjetas del Index).</summary>
public record ResumenComprasDto(int Cantidad, decimal Total)
{
    public string TotalTexto => FormatoCompra.Moneda(Total);
}

internal static class FormatoCompra
{
    public static string Moneda(decimal valor) => "$" + valor.ToString("N2", CultureInfo.InvariantCulture);
}
