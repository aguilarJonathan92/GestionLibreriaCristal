using CommunityToolkit.Mvvm.ComponentModel;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class CategoriaEditViewModel : ObservableObject
    {
        private readonly Categoria _categoriaOriginal;

        public bool EsEdicion { get; }

        [ObservableProperty] private string nombre = string.Empty;
        [ObservableProperty] private string? colorHex;

        // Constructor para NUEVA categoría
        public CategoriaEditViewModel()
        {
            EsEdicion = false;
            _categoriaOriginal = new Categoria();
            ColorHex = "#89B4FA"; // color por defecto
        }

        // Constructor para EDITAR categoría existente
        public CategoriaEditViewModel(Categoria categoria)
        {
            EsEdicion = true;
            _categoriaOriginal = categoria;

            Nombre = categoria.Nombre;
            ColorHex = categoria.ColorHex;
        }

        public bool Guardar()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using var db = new LibreriaDbContext();

            // Verificar nombre duplicado
            var existe = db.Categorias.Any(c =>
                c.Nombre.ToLower() == Nombre.Trim().ToLower() &&
                c.Id != _categoriaOriginal.Id);

            if (existe)
            {
                MessageBox.Show("Ya existe una categoría con ese nombre.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EsEdicion)
            {
                var catDb = db.Categorias.First(c => c.Id == _categoriaOriginal.Id);
                catDb.Nombre = Nombre.Trim();
                catDb.ColorHex = ColorHex;
            }
            else
            {
                var nueva = new Categoria
                {
                    Nombre = Nombre.Trim(),
                    ColorHex = ColorHex
                };
                db.Categorias.Add(nueva);
            }

            db.SaveChanges();
            return true;
        }
    }
}