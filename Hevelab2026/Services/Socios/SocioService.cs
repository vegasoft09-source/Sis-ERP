using Hevelab2026.Common;
using Hevelab2026.Common.Exceptions;
using Hevelab2026.Data;
using Hevelab2026.Domain.Entities;
using Hevelab2026.DTOs.Socios;
using Hevelab2026.Models;
using Hevelab2026.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Socios;

public class SocioService : ISocioService
{
    private readonly IUnitOfWork _uow;
    private readonly ApplicationDbContext _db;

    public SocioService(IUnitOfWork uow, ApplicationDbContext db)
    {
        _uow = uow;
        _db = db;
    }

    public async Task<PagedResult<SocioResponseDto>> GetClientesAsync(PagedQuery query, string? estado, CancellationToken ct = default)
    {
        var q = _db.Socios.AsNoTracking().Where(s => s.EsCliente);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(x => x.RazonSocial.ToLower().Contains(s) || x.NumeroDocumento.ToLower().Contains(s));
        }

        if (!string.IsNullOrEmpty(estado))
        {
            if (estado.Equals("activo", StringComparison.OrdinalIgnoreCase))
                q = q.Where(x => x.Activo);
            else if (estado.Equals("inactivo", StringComparison.OrdinalIgnoreCase))
                q = q.Where(x => !x.Activo);
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.RazonSocial)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(MapProjection())
            .ToListAsync(ct);

        return new PagedResult<SocioResponseDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SocioResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Socios.AsNoTracking()
            .Where(s => s.Id == id && s.EsCliente)
            .Select(MapProjection())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SocioResponseDto> CreateClienteAsync(SocioCreateDto dto, int empresaId = 1, CancellationToken ct = default)
    {
        var maxId = await _db.Socios.Where(s => s.EmpresaId == empresaId).CountAsync(ct);
        var entity = new Socio
        {
            EmpresaId = empresaId,
            Codigo = $"CLI-{(maxId + 1):D3}",
            RazonSocial = dto.RazonSocial,
            TipoDocumento = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            Telefono = dto.Telefono,
            Correo = dto.Email,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            LimiteCredito = dto.LimiteCredito,
            PuestoTrabajo = dto.PuestoTrabajo,
            GrupoClientes = dto.GrupoClientes,
            EsCliente = true,
            EsProveedor = dto.EsProveedor,
            Activo = true
        };
        await _uow.Socios.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<SocioResponseDto> UpdateClienteAsync(int id, SocioUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Socios.FirstOrDefaultAsync(s => s.Id == id && s.EsCliente, ct)
            ?? throw new NotFoundException("Cliente no encontrado");

        entity.RazonSocial = dto.RazonSocial;
        entity.TipoDocumento = dto.TipoDocumento;
        entity.NumeroDocumento = dto.NumeroDocumento;
        entity.Telefono = dto.Telefono;
        entity.Correo = dto.Email;
        entity.Direccion = dto.Direccion;
        entity.Ciudad = dto.Ciudad;
        entity.LimiteCredito = dto.LimiteCredito;
        entity.PuestoTrabajo = dto.PuestoTrabajo;
        entity.GrupoClientes = dto.GrupoClientes;
        entity.Activo = dto.Activo;

        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task ToggleActivoAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Socios.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Cliente no encontrado");
        entity.Activo = !entity.Activo;
        await _uow.SaveChangesAsync(ct);
    }

    public Cliente ToViewModel(SocioResponseDto dto) => new()
    {
        Id = dto.Id,
        Codigo = dto.Codigo,
        RazonSocial = dto.RazonSocial,
        TipoDocumento = dto.TipoDocumento,
        NumeroDocumento = dto.NumeroDocumento,
        TipoCliente = dto.TipoCliente,
        Telefono = dto.Telefono ?? "",
        Email = dto.Email ?? "",
        Direccion = dto.Direccion ?? "",
        Ciudad = dto.Ciudad ?? "",
        LimiteCredito = dto.LimiteCredito,
        Activo = dto.Activo,
        FechaRegistro = dto.FechaRegistro,
        PuestoTrabajo = dto.PuestoTrabajo ?? "",
        GrupoClientes = dto.GrupoClientes ?? ""
    };

    public SocioResponseDto FromClienteForm(Cliente c) => new()
    {
        RazonSocial = c.RazonSocial,
        TipoDocumento = c.TipoDocumento,
        NumeroDocumento = c.NumeroDocumento,
        TipoCliente = c.TipoCliente,
        Telefono = c.Telefono,
        Email = c.Email,
        Direccion = c.Direccion,
        Ciudad = c.Ciudad,
        LimiteCredito = c.LimiteCredito,
        PuestoTrabajo = c.PuestoTrabajo,
        GrupoClientes = c.GrupoClientes,
        Activo = c.Activo
    };

    private static SocioResponseDto Map(Socio s) => new()
    {
        Id = s.Id,
        Codigo = s.Codigo,
        RazonSocial = s.RazonSocial,
        TipoDocumento = s.TipoDocumento,
        NumeroDocumento = s.NumeroDocumento,
        TipoCliente = s.TipoPersona == "natural" ? "Persona Natural" : "Persona Jurídica",
        Telefono = s.Telefono,
        Email = s.Correo,
        Direccion = s.Direccion,
        Ciudad = s.Ciudad,
        LimiteCredito = s.LimiteCredito,
        Activo = s.Activo,
        FechaRegistro = s.CreadoEn,
        PuestoTrabajo = s.PuestoTrabajo,
        GrupoClientes = s.GrupoClientes
    };

    private static System.Linq.Expressions.Expression<Func<Socio, SocioResponseDto>> MapProjection() =>
        s => new SocioResponseDto
        {
            Id = s.Id,
            Codigo = s.Codigo,
            RazonSocial = s.RazonSocial,
            TipoDocumento = s.TipoDocumento,
            NumeroDocumento = s.NumeroDocumento,
            TipoCliente = s.TipoPersona,
            Telefono = s.Telefono,
            Email = s.Correo,
            Direccion = s.Direccion,
            Ciudad = s.Ciudad,
            LimiteCredito = s.LimiteCredito,
            Activo = s.Activo,
            FechaRegistro = s.CreadoEn,
            PuestoTrabajo = s.PuestoTrabajo,
            GrupoClientes = s.GrupoClientes
        };
}
