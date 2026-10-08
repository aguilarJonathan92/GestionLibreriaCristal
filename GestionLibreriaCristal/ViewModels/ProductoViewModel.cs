using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using GestionLibreriaCristal.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class ProductosViewModel : ObservableObject
    {
        private List<Producto> _todosLosProductos = new();

        public ObservableCollection<Producto> ProductosFiltrados { get; } = new();

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private Producto? productoSeleccionado;

        public ProductosViewModel()
        {
            CargarProductos();
        }

        // Cuando cambia el texto de búsqueda, se filtra automáticamente
        partial void OnTextoBusquedaChanged(string value)
        {
            AplicarFiltro();
        }

        public void CargarProductos()
        {
            using var db = new LibreriaDbContext();
            _todosLosProductos = db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .OrderBy(p => p.Nombre)
                .ToList();

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var filtro = TextoBusqueda?.Trim().ToLower() ?? string.Empty;

            var resultado = string.IsNullOrEmpty(filtro)
                ? _todosLosProductos
                : _todosLosProductos
                    .Where(p => p.Nombre.ToLower().Contains(filtro))
                    .ToList();

            ProductosFiltrados.Clear();
            foreach (var p in resultado)
                ProductosFiltrados.Add(p);
        }

        [RelayCommand]
        private void Agregar()
        {
            var vm = new ProductoEditViewModel();
            var ventana = new ProductoEditWindow(vm);

            if (ventana.ShowDialog() == true)
            {
                CargarProductos();
            }
        }

        [RelayCommand]
        private void Editar()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un producto para editar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new ProductoEditViewModel(ProductoSeleccionado);
            var ventana = new ProductoEditWindow(vm);

            if (ventana.ShowDialog() == true)
            {
                CargarProductos();
            }
        }

        [RelayCommand]
        private void Eliminar()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un producto para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Seguro que querés eliminar \"{ProductoSeleccionado.Nombre}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes)
                return;

            using var db = new LibreriaDbContext();
            var prodDb = db.Productos.FirstOrDefault(p => p.Id == ProductoSeleccionado.Id);
            if (prodDb != null)
            {
                db.Productos.Remove(prodDb);
                db.SaveChanges();
            }

            CargarProductos();
        }
    }
}