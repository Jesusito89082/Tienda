using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tienda.Models;

namespace Tienda.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<DetallesVentum> DetallesVenta { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }

        // 👇 Nueva entidad
        public DbSet<Factura> Facturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones existentes...
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1C5E1B9140C");
                entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
                entity.Property(e => e.Nombre).HasMaxLength(50);
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.ClienteId).HasName("PK__Clientes__71ABD0A731264900");
                entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
            });

            modelBuilder.Entity<DetallesVentum>(entity =>
            {
                entity.HasKey(e => e.DetalleId).HasName("PK__Detalles__6E19D6FA9EE798B5");
                entity.Property(e => e.DetalleId).HasColumnName("DetalleID");
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ProductoId).HasColumnName("ProductoID");
                entity.Property(e => e.VentaId).HasColumnName("VentaID");

                entity.HasOne(d => d.Producto).WithMany(p => p.DetallesVenta)
                    .HasForeignKey(d => d.ProductoId)
                    .HasConstraintName("FK__DetallesV__Produ__5AEE82B9");

                entity.HasOne(d => d.Venta).WithMany(p => p.DetallesVenta)
                    .HasForeignKey(d => d.VentaId)
                    .HasConstraintName("FK__DetallesV__Venta__59FA5E80");
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.ProductoId).HasName("PK__Producto__A430AE83627747B9");
                entity.Property(e => e.ProductoId).HasColumnName("ProductoID");
                entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
                entity.Property(e => e.Color).HasMaxLength(30);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Talla).HasMaxLength(10);

                entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                    .HasForeignKey(d => d.CategoriaId)
                    .HasConstraintName("FK__Productos__Categ__5165187F");
            });

            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.VentaId).HasName("PK__Ventas__5B41514CDC5FB213");
                entity.Property(e => e.VentaId).HasColumnName("VentaID");
                entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
                entity.Property(e => e.Fecha)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Cliente).WithMany(p => p.Venta)
                    .HasForeignKey(d => d.ClienteId)
                    .HasConstraintName("FK__Ventas__ClienteI__5629CD9C");
            });

            // 👇 Configuración de Factura
            modelBuilder.Entity<Factura>(entity =>
            {
                entity.ToTable("Facturas");
                entity.HasKey(e => e.FacturaId);

                entity.Property(e => e.NumeroConsecutivo).HasMaxLength(50);
                entity.Property(e => e.Clave).HasMaxLength(50);
                entity.Property(e => e.XmlFirmado).HasColumnType("nvarchar(max)");
                entity.Property(e => e.PdfPath).HasMaxLength(255);
                entity.Property(e => e.FechaEmision).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Estado).HasMaxLength(20);

                entity.HasOne(f => f.Venta)
                      .WithMany()
                      .HasForeignKey(f => f.VentaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}