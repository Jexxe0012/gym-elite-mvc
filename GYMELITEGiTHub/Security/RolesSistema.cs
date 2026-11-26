namespace GYMELITEGiTHub.Security;

/// <summary>Nombres de roles y combinaciones usadas en [Authorize(Roles = ...)].</summary>
public static class RolesSistema
{
    public const string Administrador = "Administrador";
    public const string Recepcion = "Recepcion";
    public const string Inventario = "Inventario";

    public const string AdminYRecepcion = Administrador + "," + Recepcion;
    public const string AdminEInventario = Administrador + "," + Inventario;
    public const string Todos = Administrador + "," + Recepcion + "," + Inventario;
}
