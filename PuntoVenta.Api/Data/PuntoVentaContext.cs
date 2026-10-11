using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Data;

public partial class PuntoVentaContext : DbContext
{
    public PuntoVentaContext(DbContextOptions<PuntoVentaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<ComprasDetalle> ComprasDetalles { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Puesto> Puestos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    public virtual DbSet<VentasDetalle> VentasDetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PRIMARY");

            entity.ToTable("clientes");

            entity.HasIndex(e => e.Nit, "uq_cliente_nit").IsUnique();

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(60)
                .HasColumnName("apellidos");
            entity.Property(e => e.CorreoElectronico)
                .HasMaxLength(45)
                .HasColumnName("correo_electronico");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ingreso");
            entity.Property(e => e.Genero)
                .HasColumnType("bit(1)")
                .HasColumnName("genero");
            entity.Property(e => e.Nit)
                .HasMaxLength(12)
                .HasColumnName("nit");
            entity.Property(e => e.Nombres)
                .HasMaxLength(60)
                .HasColumnName("nombres");
            entity.Property(e => e.Telefono)
                .HasMaxLength(25)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("PRIMARY");

            entity.ToTable("compras");

            entity.HasIndex(e => e.IdProveedor, "fk_compra_proveedor");

            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ingreso");
            entity.Property(e => e.FechaOrden).HasColumnName("fecha_orden");
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.NoOrdenCompra).HasColumnName("no_orden_compra");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_compra_proveedor");
        });

        modelBuilder.Entity<ComprasDetalle>(entity =>
        {
            entity.HasKey(e => e.IdCompraDetalle).HasName("PRIMARY");

            entity.ToTable("compras_detalle");

            entity.HasIndex(e => e.IdCompra, "fk_cd_compra");

            entity.HasIndex(e => e.IdProducto, "fk_cd_producto");

            entity.Property(e => e.IdCompraDetalle).HasColumnName("id_compra_detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.PrecioCostoUnitario)
                .HasPrecision(8, 2)
                .HasColumnName("precio_costo_unitario");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.ComprasDetalles)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cd_compra");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ComprasDetalles)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cd_producto");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado).HasName("PRIMARY");

            entity.ToTable("empleados");

            entity.HasIndex(e => e.IdPuesto, "fk_empleado_puesto");

            entity.HasIndex(e => e.Dpi, "uq_empleado_dpi").IsUnique();

            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(60)
                .HasColumnName("apellidos");
            entity.Property(e => e.Direccion)
                .HasMaxLength(80)
                .HasColumnName("direccion");
            entity.Property(e => e.Dpi)
                .HasMaxLength(15)
                .HasColumnName("dpi");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ingreso");
            entity.Property(e => e.FechaInicioLabores).HasColumnName("fecha_inicio_labores");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Genero)
                .HasColumnType("bit(1)")
                .HasColumnName("genero");
            entity.Property(e => e.IdPuesto).HasColumnName("id_puesto");
            entity.Property(e => e.Nombres)
                .HasMaxLength(60)
                .HasColumnName("nombres");
            entity.Property(e => e.Telefono)
                .HasMaxLength(25)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdPuestoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdPuesto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_empleado_puesto");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PRIMARY");

            entity.ToTable("marcas");

            entity.HasIndex(e => e.Marca1, "uq_marca").IsUnique();

            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.Marca1)
                .HasMaxLength(50)
                .HasColumnName("marca");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.IdMenu).HasName("PRIMARY");

            entity.ToTable("menu");

            entity.HasIndex(e => e.IdPadre, "fk_menu_padre");

            entity.Property(e => e.IdMenu).HasColumnName("id_menu");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("b'1'")
                .HasColumnType("bit(1)")
                .HasColumnName("activo");
            entity.Property(e => e.IdPadre).HasColumnName("id_padre");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.Property(e => e.Ruta)
                .HasMaxLength(100)
                .HasColumnName("ruta");

            entity.HasOne(d => d.IdPadreNavigation).WithMany(p => p.InverseIdPadreNavigation)
                .HasForeignKey(d => d.IdPadre)
                .HasConstraintName("fk_menu_padre");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PRIMARY");

            entity.ToTable("productos");

            entity.HasIndex(e => e.IdMarca, "fk_producto_marca");

            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.Existencia).HasColumnName("existencia");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ingreso");
            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.Imagen)
                .HasMaxLength(30)
                .HasColumnName("imagen");
            entity.Property(e => e.PrecioCosto)
                .HasPrecision(8, 2)
                .HasColumnName("precio_costo");
            entity.Property(e => e.PrecioVenta)
                .HasPrecision(8, 2)
                .HasColumnName("precio_venta");
            entity.Property(e => e.Producto1)
                .HasMaxLength(50)
                .HasColumnName("producto");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_producto_marca");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PRIMARY");

            entity.ToTable("proveedores");

            entity.HasIndex(e => e.Nit, "uq_proveedor_nit").IsUnique();

            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.Direccion)
                .HasMaxLength(80)
                .HasColumnName("direccion");
            entity.Property(e => e.Nit)
                .HasMaxLength(12)
                .HasColumnName("nit");
            entity.Property(e => e.Proveedor)
                .HasMaxLength(60)
                .HasColumnName("proveedor");
            entity.Property(e => e.Telefono)
                .HasMaxLength(25)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Puesto>(entity =>
        {
            entity.HasKey(e => e.IdPuesto).HasName("PRIMARY");

            entity.ToTable("puestos");

            entity.HasIndex(e => e.Puesto1, "uq_puesto").IsUnique();

            entity.Property(e => e.IdPuesto).HasColumnName("id_puesto");
            entity.Property(e => e.Puesto1)
                .HasMaxLength(50)
                .HasColumnName("puesto");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.IdEmpleado, "fk_usuario_empleado");

            entity.HasIndex(e => e.Usuario1, "uq_usuario").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("b'1'")
                .HasColumnType("bit(1)")
                .HasColumnName("activo");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ingreso");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(100)
                .HasColumnName("password_hash");
            entity.Property(e => e.Usuario1)
                .HasMaxLength(30)
                .HasColumnName("usuario");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("fk_usuario_empleado");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PRIMARY");

            entity.ToTable("ventas");

            entity.HasIndex(e => e.IdCliente, "fk_venta_cliente");

            entity.HasIndex(e => e.IdEmpleado, "fk_venta_empleado");

            entity.HasIndex(e => new { e.Serie, e.NoFactura }, "uq_factura").IsUnique();

            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.FechaFactura).HasColumnName("fecha_factura");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ingreso");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.NoFactura).HasColumnName("no_factura");
            entity.Property(e => e.Serie)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("serie");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_venta_cliente");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_venta_empleado");
        });

        modelBuilder.Entity<VentasDetalle>(entity =>
        {
            entity.HasKey(e => e.IdVentaDetalle).HasName("PRIMARY");

            entity.ToTable("ventas_detalle");

            entity.HasIndex(e => e.IdProducto, "fk_vd_producto");

            entity.HasIndex(e => e.IdVenta, "fk_vd_venta");

            entity.Property(e => e.IdVentaDetalle).HasColumnName("id_venta_detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(8, 2)
                .HasColumnName("precio_unitario");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.VentasDetalles)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_vd_producto");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.VentasDetalles)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_vd_venta");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
