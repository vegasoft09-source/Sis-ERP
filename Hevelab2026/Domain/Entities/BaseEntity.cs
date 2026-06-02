namespace Hevelab2026.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public bool Activo { get; set; } = true;
    public bool Eliminado { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEn { get; set; }
}

public abstract class EmpresaEntity : BaseEntity
{
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
}
