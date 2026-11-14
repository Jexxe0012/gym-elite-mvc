using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;

namespace GYMELITEGiTHub.ViewModels;

public class VentaLineaViewModel
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Existencias { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa.")]
    public int Cantidad { get; set; }
}

public class VentaNuevaViewModel
{
    public List<VentaLineaViewModel> Lineas { get; set; } = new();
}

public class VentaListaViewModel
{
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }
    public List<VentaDto> Ventas { get; set; } = new();
}
