namespace GYMELITEGiTHub.Models;

// Distingue rechazos previstos del negocio de fallos inesperados de infraestructura.
public sealed class ReglaNegocioException(string mensaje, TipoRechazo tipo = TipoRechazo.Conflicto)
    : InvalidOperationException(mensaje)
{
    public TipoRechazo Tipo { get; } = tipo;
}

public enum TipoRechazo
{
    Conflicto,
    DatoInvalido,
    ReferenciaInexistente
}
