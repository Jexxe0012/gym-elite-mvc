namespace GYMELITEGiTHub.Dtos;

/// <summary>Datos de un proveedor listos para mostrar (sin exponer la entidad).</summary>
public class ProveedorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }

    /// <summary>Compras asociadas (registradas y anuladas). Si es mayor que cero, no se puede eliminar.</summary>
    public int CantidadCompras { get; set; }
    public DateTime? UltimaCompra { get; set; }

    /// <summary>Compras recientes del proveedor; solo se llena en el detalle.</summary>
    public List<CompraDto> Compras { get; set; } = new();

    public bool SePuedeEliminar => CantidadCompras == 0;
}
