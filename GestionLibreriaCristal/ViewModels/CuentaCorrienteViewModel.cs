using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class CuentaCorrienteViewModel : ObservableObject
    {
        private List<Cliente> _todosLosClientes = new();

        public ObservableCollection<Cliente> ClientesConDeuda { get; } = new();
        public ObservableCollection<MovimientoCuenta> Movimientos { get; } = new();

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private Cliente? clienteSeleccionado;

        [ObservableProperty]
        private decimal montoPago;

        [ObservableProperty]
        private string? descripcionPago;

        public CuentaCorrienteViewModel()
        {
            CargarClientes();
        }

        partial void OnTextoBusquedaChanged(string value) => AplicarFiltro();

        partial void OnClienteSeleccionadoChanged(Cliente? value)
        {
            CargarMovimientos();
        }

        public void CargarClientes()
        {
            using var db = new LibreriaDbContext();

            // 1. Traemos todos los clientes activos
            var clientesDb = db.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToList();

            // 2. Traemos TODOS los movimientos de la base de datos
            var movimientosDb = db.MovimientosCuenta.ToList();

            // 3. Asignamos los movimientos a cada cliente manualmente
            foreach (var cliente in clientesDb)
            {
                cliente.Movimientos = movimientosDb.Where(m => m.ClienteId == cliente.Id).ToList();
            }

            _todosLosClientes = clientesDb;
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var filtro = TextoBusqueda?.Trim().ToLower() ?? string.Empty;

            var resultado = string.IsNullOrEmpty(filtro)
                ? _todosLosClientes.Where(c => c.Saldo > 0)
                : _todosLosClientes.Where(c =>
                    c.Saldo > 0 &&
                    c.Nombre.ToLower().Contains(filtro));

            ClientesConDeuda.Clear();
            foreach (var c in resultado)
                ClientesConDeuda.Add(c);
        }

        private void CargarMovimientos()
        {
            Movimientos.Clear();

            if (ClienteSeleccionado == null)
                return;

            using var db = new LibreriaDbContext();
            var movs = db.MovimientosCuenta
                .Where(m => m.ClienteId == ClienteSeleccionado.Id)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            foreach (var m in movs)
                Movimientos.Add(m);
        }

        [RelayCommand]
        private void RegistrarPago()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un cliente.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MontoPago <= 0)
            {
                MessageBox.Show("El monto debe ser mayor a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MontoPago > ClienteSeleccionado.Saldo)
            {
                MessageBox.Show(
                    $"El pago supera la deuda actual (${ClienteSeleccionado.Saldo:N2}).",
                    "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new LibreriaDbContext();

            db.MovimientosCuenta.Add(new MovimientoCuenta
            {
                ClienteId = ClienteSeleccionado.Id,
                Fecha = DateTime.Now,
                Tipo = TipoMovimiento.Pago,
                Monto = MontoPago,
                Descripcion = string.IsNullOrWhiteSpace(DescripcionPago)
                    ? "Pago registrado"
                    : DescripcionPago.Trim()
            });

            db.SaveChanges();

            MessageBox.Show(
                $"Pago registrado correctamente.\n\n" +
                $"Cliente: {ClienteSeleccionado.Nombre}\n" +
                $"Monto: ${MontoPago:N2}\n" +
                $"Nuevo saldo: ${(ClienteSeleccionado.Saldo - MontoPago):N2}",
                "Pago registrado",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpiar
            MontoPago = 0;
            DescripcionPago = null;

            // Recargar
            var clienteIdActual = ClienteSeleccionado.Id;
            CargarClientes();
            ClienteSeleccionado = ClientesConDeuda.FirstOrDefault(c => c.Id == clienteIdActual);
        }

        [RelayCommand]
        private void Refrescar()
        {
            CargarClientes();
        }
    }
}