using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using GestionLibreriaCristal.Views;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class ProveedoresViewModel : ObservableObject
    {
        private List<Proveedor> _todosLosProveedores = new();

        public ObservableCollection<Proveedor> ProveedoresFiltrados { get; } = new();

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private Proveedor? proveedorSeleccionado;

        public ProveedoresViewModel()
        {
            CargarProveedores();
        }

        partial void OnTextoBusquedaChanged(string value)
        {
            AplicarFiltro();
        }

        public void CargarProveedores()
        {
            using var db = new LibreriaDbContext();
            _todosLosProveedores = db.Proveedores
                .OrderBy(p => p.Nombre)
                .ToList();

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var filtro = TextoBusqueda?.Trim().ToLower() ?? string.Empty;

            var resultado = string.IsNullOrEmpty(filtro)
                ? _todosLosProveedores
                : _todosLosProveedores
                    .Where(p => p.Nombre.ToLower().Contains(filtro))
                    .ToList();

            ProveedoresFiltrados.Clear();
            foreach (var p in resultado)
                ProveedoresFiltrados.Add(p);
        }

        [RelayCommand]
        private void Agregar()
        {
            var vm = new ProveedorEditViewModel();
            var ventana = new ProveedorEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarProveedores();
        }

        [RelayCommand]
        private void Editar()
        {
            if (ProveedorSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un proveedor para editar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new ProveedorEditViewModel(ProveedorSeleccionado);
            var ventana = new ProveedorEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarProveedores();
        }

        [RelayCommand]
        private void Eliminar()
        {
            if (ProveedorSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un proveedor para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Advertencia si tiene productos asociados
            using var db = new LibreriaDbContext();
            var productosAsociados = db.Productos.Count(p => p.ProveedorId == ProveedorSeleccionado.Id);

            var mensaje = productosAsociados > 0
                ? $"Este proveedor tiene {productosAsociados} producto(s) asociado(s).\n\n" +
                  "Si lo eliminás, esos productos quedarán sin proveedor (no se borran).\n\n" +
                  $"¿Seguro que querés eliminar \"{ProveedorSeleccionado.Nombre}\"?"
                : $"¿Seguro que querés eliminar \"{ProveedorSeleccionado.Nombre}\"?";

            var resultado = MessageBox.Show(
                mensaje,
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes)
                return;

            var provDb = db.Proveedores.FirstOrDefault(p => p.Id == ProveedorSeleccionado.Id);
            if (provDb != null)
            {
                db.Proveedores.Remove(provDb);
                db.SaveChanges();
            }

            CargarProveedores();
        }
    }
}