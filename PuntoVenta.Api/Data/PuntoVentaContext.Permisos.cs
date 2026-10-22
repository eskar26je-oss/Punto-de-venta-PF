using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Data;

// Parte del contexto que agrega la tabla "permisos".
// Va en un archivo aparte para no tocar PuntoVentaContext.cs.
public partial class PuntoVentaContext
{
    public virtual DbSet<Permiso> Permisos { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PRIMARY");

            entity.ToTable("permisos");

            entity.HasIndex(e => new { e.IdUsuario, e.IdMenu }, "uq_permiso").IsUnique();
            entity.HasIndex(e => e.IdMenu, "fk_permiso_menu");

            entity.Property(e => e.IdPermiso).HasColumnName("id_permiso");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdMenu).HasColumnName("id_menu");
            entity.Property(e => e.PuedeVer)
                .HasColumnType("bit(1)")
                .HasColumnName("puede_ver");
            entity.Property(e => e.PuedeEditar)
                .HasColumnType("bit(1)")
                .HasColumnName("puede_editar");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_permiso_usuario");

            entity.HasOne(d => d.IdMenuNavigation).WithMany()
                .HasForeignKey(d => d.IdMenu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_permiso_menu");
        });
    }
}
