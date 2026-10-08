using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using GestionLibreriaCristal.Views;
using Microsoft.EntityFrameworkCore;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class CategoriasViewModel : ObservableObject
    {
        private List<Categoria> _todasLasCategorias = new();

        public ObservableCollection<Categoria> CategoriasFiltradas { get; } = new();

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private Categoria? categoriaSeleccionada;

        public CategoriasViewModel()
        {
            CargarCategorias();
        }

        partial void OnTextoBusquedaChanged(string value)
        {
            AplicarFiltro();
        }

        public void CargarCategorias()
        {
            using var db = new LibreriaDbContext();
            _todasLasCategorias = db.Categorias
                .OrderBy(c => c.Nombre)
                .ToList();

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var filtro = TextoBusqueda?.Trim().ToLower() ?? string.Empty;

            var resultado = string.IsNullOrEmpty(filtro)
                ? _todasLasCategorias
                : _todasLasCategorias
                    .Where(c => c.Nombre.ToLower().Contains(filtro))
                    .ToList();

            CategoriasFiltradas.Clear();
            foreach (var c in resultado)
                CategoriasFiltradas.Add(c);
        }

        [RelayCommand]
        private void Agregar()
        {
            var vm = new CategoriaEditViewModel();
            var ventana = new CategoriaEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarCategorias();
        }

        [RelayCommand]
        private void Editar()
        {
            if (CategoriaSeleccionada == null)
            {
                MessageBox.Show("Seleccioná una categoría para editar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new CategoriaEditViewModel(CategoriaSeleccionada);
            var ventana = new CategoriaEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarCategorias();
        }

        [RelayCommand]
        private void Eliminar()
        {
            if (CategoriaSeleccionada == null)
            {
                MessageBox.Show("Seleccioná una categoría para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Verificar si tiene productos asociados
            using var db = new LibreriaDbContext();
            var tieneProductos = db.Productos.Any(p => p.CategoriaId == CategoriaSeleccionada.Id);

            if (tieneProductos)
            {
                MessageBox.Show(
                    "No se puede eliminar esta categoría porque tiene productos asociados.\n\n" +
                    "Primero mové o eliminá esos productos.",
                    "No se puede eliminar",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Seguro que querés eliminar la categoría \"{CategoriaSeleccionada.Nombre}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes)
                return;

            var catDb = db.Categorias.FirstOrDefault(c => c.Id == CategoriaSeleccionada.Id);
            if (catDb != null)
            {
                db.Categorias.Remove(catDb);
                db.SaveChanges();
            }

            CargarCategorias();
        }
    }
}