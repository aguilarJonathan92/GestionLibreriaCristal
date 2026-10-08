using CommunityToolkit.Mvvm.ComponentModel;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class ProductoEditViewModel : ObservableObject
    {
        private readonly Producto _productoOriginal;

        public bool EsEdicion { get; }

        public ObservableCollection<Categoria> Categorias { get; } = new();
        public ObservableCollection<Proveedor> Proveedores { get; } = new();

        [ObservableProperty] private string nombre = string.Empty;
        [ObservableProperty] private string? descripcion;
        [ObservableProperty] private Categoria? categoriaSeleccionada;
        [ObservableProperty] private Proveedor? proveedorSeleccionado;
        [ObservableProperty] private decimal costoCompra;
        [ObservableProperty] private decimal precioVenta;
        [ObservableProperty] private int stockActual;
        [ObservableProperty] private int stockMinimo;

        // Constructor para NUEVO producto
        public ProductoEditViewModel()
        {
            EsEdicion = false;
            _productoOriginal = new Producto();
            CargarCombos();
            CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Nombre == "Otros") ?? Categorias.FirstOrDefault();
        }

        // Constructor para EDITAR producto existente
        public ProductoEditViewModel(Producto producto)
        {
            EsEdicion = true;
            _productoOriginal = producto;
            CargarCombos();

            Nombre = producto.Nombre;
            Descripcion = producto.Descripcion;
            CostoCompra = producto.CostoCompra;
            PrecioVenta = producto.PrecioVenta;
            StockActual = producto.StockActual;
            StockMinimo = producto.StockMinimo;
            CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);
            ProveedorSeleccionado = Proveedores.FirstOrDefault(p => p.Id == producto.ProveedorId);
        }

        private void CargarCombos()
        {
            using var db = new LibreriaDbContext();

            Categorias.Clear();
            foreach (var c in db.Categorias.OrderBy(c => c.Nombre).ToList())
                Categorias.Add(c);

            Proveedores.Clear();
            foreach (var p in db.Proveedores.OrderBy(p => p.Nombre).ToList())
                Proveedores.Add(p);
        }

        /// <summary>
        /// Guarda el producto en la base. Devuelve true si tuvo éxito.
        /// </summary>
        public bool Guardar()
        {
            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                System.Windows.MessageBox.Show("El nombre es obligatorio.", "Validación",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            if (PrecioVenta <= 0)
            {
                System.Windows.MessageBox.Show("El precio de venta debe ser mayor a 0.", "Validación",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            if (CategoriaSeleccionada == null)
            {
                System.Windows.MessageBox.Show("Seleccioná una categoría.", "Validación",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            using var db = new LibreriaDbContext();

            if (EsEdicion)
            {
                // Cargar el producto desde la BD para que EF lo rastree
                var prodDb = db.Productos.First(p => p.Id == _productoOriginal.Id);

                prodDb.Nombre = Nombre.Trim();
                prodDb.Descripcion = Descripcion?.Trim();
                prodDb.CategoriaId = CategoriaSeleccionada.Id;
                prodDb.ProveedorId = ProveedorSeleccionado?.Id;
                prodDb.CostoCompra = CostoCompra;
                prodDb.PrecioVenta = PrecioVenta;
                prodDb.StockActual = StockActual;
                prodDb.StockMinimo = StockMinimo;
            }
            else
            {
                var nuevo = new Producto
                {
                    Nombre = Nombre.Trim(),
                    Descripcion = Descripcion?.Trim(),
                    CategoriaId = CategoriaSeleccionada.Id,
                    ProveedorId = ProveedorSeleccionado?.Id,
                    CostoCompra = CostoCompra,
                    PrecioVenta = PrecioVenta,
                    StockActual = StockActual,
                    StockMinimo = StockMinimo,
                    Tipo = TipoProducto.Simple
                };

                db.Productos.Add(nuevo);
            }

            db.SaveChanges();
            return true;
        }
    }
}