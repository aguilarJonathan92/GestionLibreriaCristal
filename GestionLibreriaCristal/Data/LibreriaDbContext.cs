using GestionLibreriaCristal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GestionLibreriaCristal.Data
{
    public class LibreriaDbContext : DbContext
    {
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
        public DbSet<MovimientoCuenta> MovimientosCuenta => Set<MovimientoCuenta>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Ruta donde se va a guardar la base de datos.
            // %AppData%\GestionLibreriaCristal\libreria.db
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GestionLibreriaCristal");

            Directory.CreateDirectory(carpeta); // la crea si no existe

            var rutaDb = Path.Combine(carpeta, "libreria.db");

            optionsBuilder.UseSqlite($"Data Source={rutaDb}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de precisión para los decimales (importante para dinero)
            modelBuilder.Entity<Producto>()
                .Property(p => p.CostoCompra)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            // Relación: un Producto tiene una Categoria
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación: un Producto tiene un Proveedor (opcional)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuración de precisión para decimales
            modelBuilder.Entity<Pedido>()
                .Property(p => p.DescuentoPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.DescuentoMonto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.TotalPagado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            // Relación Pedido → Cliente
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Pedido → Detalles (cascade: al borrar pedido, se borran los detalles)
            modelBuilder.Entity<Pedido>()
                .HasMany(p => p.Detalles)
                .WithOne(d => d.Pedido)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Detalle → Producto
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Datos iniciales: categorías típicas de una librería cristiana
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Biblias", ColorHex = "#89B4FA" },
                new Categoria { Id = 2, Nombre = "Himnarios", ColorHex = "#A6E3A1" },
                new Categoria { Id = 3, Nombre = "Libros", ColorHex = "#F9E2AF" },
                new Categoria { Id = 4, Nombre = "Devocionales", ColorHex = "#FAB387" },
                new Categoria { Id = 5, Nombre = "Música", ColorHex = "#CBA6F7" },
                new Categoria { Id = 6, Nombre = "Mantillas", ColorHex = "#F5C2E7" },
                new Categoria { Id = 7, Nombre = "Libretas y Agendas", ColorHex = "#94E2D5" },
                new Categoria { Id = 8, Nombre = "Regalos", ColorHex = "#F38BA8" },
                new Categoria { Id = 9, Nombre = "Ropa", ColorHex = "#EBA0AC" },
                new Categoria { Id = 10, Nombre = "Otros", ColorHex = "#9399B2" }
            );

            modelBuilder.Entity<Cliente>()
                .Ignore(c => c.Saldo);

            // Cliente de ejemplo (opcional, podés borrarlo)
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente
                {
                    Id = 1,
                    Nombre = "Consumidor Final",
                    Telefono = "-",
                    Activo = true,
                    FechaAlta = new DateTime(2025, 1, 1)
                }
            );
            

            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.Numero)
                .IsUnique();


            // Precisión para el monto
            modelBuilder.Entity<MovimientoCuenta>()
                .Property(m => m.Monto)
                .HasPrecision(18, 2);

            // Relación Movimiento → Cliente
            modelBuilder.Entity<MovimientoCuenta>()
                .HasOne(m => m.Cliente)
                .WithMany()
                .HasForeignKey(m => m.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Movimiento → Pedido (opcional)
            modelBuilder.Entity<MovimientoCuenta>()
                .HasOne(m => m.Pedido)
                .WithMany()
                .HasForeignKey(m => m.PedidoId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
