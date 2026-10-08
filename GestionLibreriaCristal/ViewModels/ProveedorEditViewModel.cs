using CommunityToolkit.Mvvm.ComponentModel;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class ProveedorEditViewModel : ObservableObject
    {
        private readonly Proveedor _proveedorOriginal;

        public bool EsEdicion { get; }

        [ObservableProperty] private string nombre = string.Empty;
        [ObservableProperty] private string? telefono;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? direccion;
        [ObservableProperty] private string? notas;

        // Constructor para NUEVO proveedor
        public ProveedorEditViewModel()
        {
            EsEdicion = false;
            _proveedorOriginal = new Proveedor();
        }

        // Constructor para EDITAR proveedor existente
        public ProveedorEditViewModel(Proveedor proveedor)
        {
            EsEdicion = true;
            _proveedorOriginal = proveedor;

            Nombre = proveedor.Nombre;
            Telefono = proveedor.Telefono;
            Email = proveedor.Email;
            Direccion = proveedor.Direccion;
            Notas = proveedor.Notas;
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
            var existe = db.Proveedores.Any(p =>
                p.Nombre.ToLower() == Nombre.Trim().ToLower() &&
                p.Id != _proveedorOriginal.Id);

            if (existe)
            {
                MessageBox.Show("Ya existe un proveedor con ese nombre.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EsEdicion)
            {
                var provDb = db.Proveedores.First(p => p.Id == _proveedorOriginal.Id);
                provDb.Nombre = Nombre.Trim();
                provDb.Telefono = Telefono?.Trim();
                provDb.Email = Email?.Trim();
                provDb.Direccion = Direccion?.Trim();
                provDb.Notas = Notas?.Trim();
            }
            else
            {
                var nuevo = new Proveedor
                {
                    Nombre = Nombre.Trim(),
                    Telefono = Telefono?.Trim(),
                    Email = Email?.Trim(),
                    Direccion = Direccion?.Trim(),
                    Notas = Notas?.Trim()
                };
                db.Proveedores.Add(nuevo);
            }

            if (!string.IsNullOrWhiteSpace(Email) && !EmailEsValido(Email))
            {
                var resultado = MessageBox.Show(
                    $"El email \"{Email}\" no parece válido.\n\n¿Querés guardarlo igual?",
                    "Email sospechoso",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado != MessageBoxResult.Yes)
                    return false;
            }

            db.SaveChanges();
            return true;
        }
        private static bool EmailEsValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}