using CommunityToolkit.Mvvm.ComponentModel;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class ClienteEditViewModel : ObservableObject
    {
        private readonly Cliente _clienteOriginal;

        public bool EsEdicion { get; }

        [ObservableProperty] private string nombre = string.Empty;
        [ObservableProperty] private string? telefono;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? direccion;
        [ObservableProperty] private string? dniCuit;
        [ObservableProperty] private string? notas;
        [ObservableProperty] private decimal saldo;
        [ObservableProperty] private bool activo = true;

        // Constructor para NUEVO cliente
        public ClienteEditViewModel()
        {
            EsEdicion = false;
            _clienteOriginal = new Cliente();
        }

        // Constructor para EDITAR cliente existente
        public ClienteEditViewModel(Cliente cliente)
        {
            EsEdicion = true;
            _clienteOriginal = cliente;

            Nombre = cliente.Nombre;
            Telefono = cliente.Telefono;
            Email = cliente.Email;
            Direccion = cliente.Direccion;
            DniCuit = cliente.DniCuit;
            Notas = cliente.Notas;
            Saldo = cliente.Saldo;
            Activo = cliente.Activo;
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

            // Verificar duplicado por nombre
            var existe = db.Clientes.Any(c =>
                c.Nombre.ToLower() == Nombre.Trim().ToLower() &&
                c.Id != _clienteOriginal.Id);

            if (existe)
            {
                MessageBox.Show("Ya existe un cliente con ese nombre.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EsEdicion)
            {
                var cliDb = db.Clientes.First(c => c.Id == _clienteOriginal.Id);
                cliDb.Nombre = Nombre.Trim();
                cliDb.Telefono = Telefono?.Trim();
                cliDb.Email = Email?.Trim();
                cliDb.Direccion = Direccion?.Trim();
                cliDb.DniCuit = DniCuit?.Trim();
                cliDb.Notas = Notas?.Trim();
                cliDb.Saldo = Saldo;
                cliDb.Activo = Activo;
            }
            else
            {
                var nuevo = new Cliente
                {
                    Nombre = Nombre.Trim(),
                    Telefono = Telefono?.Trim(),
                    Email = Email?.Trim(),
                    Direccion = Direccion?.Trim(),
                    DniCuit = DniCuit?.Trim(),
                    Notas = Notas?.Trim(),
                    Saldo = Saldo,
                    Activo = Activo,
                    FechaAlta = DateTime.Now
                };
                db.Clientes.Add(nuevo);
            }

            db.SaveChanges();
            return true;
        }
    }
}