using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;

namespace GYMELITEGiTHub.ViewModels;

public class ProductoCreateViewModel
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression(@"^PR-\d{3}$", ErrorMessage = "El código debe tener el formato PR-000.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public string Categoria { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
    [Display(Name = "Precio de venta")]
    public decimal PrecioVenta { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    [Display(Name = "Stock mínimo")]
    public int StockMinimo { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La existencia inicial no puede ser negativa.")]
    [Display(Name = "Existencia inicial")]
    public int ExistenciaInicial { get; set; }

    public static readonly string[] Categorias = { "Suplementos", "Nutrición", "Bebidas", "Accesorios" };
}

public class ProductoEditViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public string Categoria { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
    [Display(Name = "Precio de venta")]
    public decimal PrecioVenta { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    [Display(Name = "Stock mínimo")]
    public int StockMinimo { get; set; }

    [Display(Name = "Producto activo")]
    public bool Activo { get; set; }
}

public class ProductoAjusteViewModel
{
    public int ProductoId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int ExistenciasActuales { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de ajuste.")]
    public string Tipo { get; set; } = "Entrada";

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}

public class ProductoListaViewModel
{
    public string? Busqueda { get; set; }
    public string? Categoria { get; set; }
    public List<ProductoDto> Productos { get; set; } = new();
    public int ProductosActivos { get; set; }
    public int ProductosStockBajo { get; set; }
    public int UnidadesDisponibles { get; set; }
}
