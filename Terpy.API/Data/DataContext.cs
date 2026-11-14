using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Terpy.Shared.Entities;

namespace Terpy.API.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Huerta> Huertas { get; set; } = null!;
    public DbSet<Zona> Zonas { get; set; } = null!;
    public DbSet<Cultivo> Cultivos { get; set; } = null!;
    public DbSet<Siembra> Siembras { get; set; } = null!;
    public DbSet<RegistroSeguimiento> RegistrosSeguimiento { get; set; } = null!;
    public DbSet<ActividadCuidado> ActividadesCuidado { get; set; } = null!;
    public DbSet<Cosecha> Cosechas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Huerta>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Zona>().HasOne<Huerta>().WithMany()
            .HasForeignKey(x => x.HuertaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Siembra>().HasOne<Zona>().WithMany()
            .HasForeignKey(x => x.ZonaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Siembra>().HasOne<Cultivo>().WithMany()
            .HasForeignKey(x => x.CultivoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RegistroSeguimiento>().HasOne<Siembra>().WithMany()
            .HasForeignKey(x => x.SiembraId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ActividadCuidado>().HasOne<Siembra>().WithMany()
            .HasForeignKey(x => x.SiembraId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Cosecha>().HasOne<Siembra>().WithMany()
            .HasForeignKey(x => x.SiembraId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cultivo>().HasIndex(x => x.Nombre).IsUnique();
        modelBuilder.Entity<Zona>().HasIndex(x => new { x.HuertaId, x.Nombre }).IsUnique();
        modelBuilder.Entity<Siembra>().HasIndex(x => new { x.ZonaId, x.Estado });
        modelBuilder.Entity<Siembra>().HasIndex(x => new { x.CultivoId, x.FechaSiembra });
        modelBuilder.Entity<RegistroSeguimiento>().HasIndex(x => new { x.SiembraId, x.FechaHora });
        modelBuilder.Entity<ActividadCuidado>().HasIndex(x => new { x.SiembraId, x.FechaHora });
        modelBuilder.Entity<Cosecha>().HasIndex(x => new { x.SiembraId, x.FechaHora });

        modelBuilder.Entity<Siembra>().Property(x => x.FechaSiembra).HasColumnType("date");
        modelBuilder.Entity<RegistroSeguimiento>().Property(x => x.TemperaturaC).HasPrecision(5, 2);
        modelBuilder.Entity<RegistroSeguimiento>().Property(x => x.HumedadSueloPorcentaje).HasPrecision(5, 2);
        modelBuilder.Entity<Cosecha>().Property(x => x.CantidadKg).HasPrecision(10, 3);
    }
}
