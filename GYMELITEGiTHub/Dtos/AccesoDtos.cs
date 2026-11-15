namespace GYMELITEGiTHub.Dtos;

public enum ResultadoBusquedaAcceso
{
    NoEncontrado,
    VariosResultados,
    Encontrado
}

/// <summary>Un ingreso registrado, con los datos del socio y del plan listos para mostrar.</summary>
public class AccesoDto
{
    public int Id { get; set; }
    public int MembresiaId { get; set; }
    public int SocioId { get; set; }
    public string SocioCodigo { get; set; } = string.Empty;
    public string SocioNombre { get; set; } = string.Empty;
    public string PlanNombre { get; set; } = string.Empty;
    public DateTime FechaHoraIngreso { get; set; }
    public string? UsuarioResponsableId { get; set; }
}

/// <summary>Resultado de consultar si un socio puede ingresar. Todavía no registra nada.</summary>
public class VerificacionAccesoDto
{
    public string Busqueda { get; set; } = string.Empty;
    public ResultadoBusquedaAcceso Resultado { get; set; }

    public SocioDto? Socio { get; set; }

    /// <summary>La membresía que da acceso o, si no hay, la más relevante para explicar el rechazo.</summary>
    public MembresiaDto? Membresia { get; set; }

    public bool Autorizado { get; set; }

    /// <summary>Explica por qué no se autoriza (o por qué no se encontró al socio).</summary>
    public string Motivo { get; set; } = string.Empty;

    public DateTime? UltimoIngreso { get; set; }

    /// <summary>Socios que coinciden cuando la búsqueda por nombre o teléfono devuelve varios.</summary>
    public List<SocioDto> Candidatos { get; set; } = new();
    public int TotalCandidatos { get; set; }
}

public record ResumenAccesosDiaDto(int Ingresos, int SociosDistintos);
