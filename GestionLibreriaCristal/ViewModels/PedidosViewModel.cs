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
    public partial class PedidosViewModel : ObservableObject
    {
        private List<Pedido> _todosLosPedidos = new();

        public ObservableCollection<Pedido> PedidosFiltrados { get; } = new();

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private Pedido? pedidoSeleccionado;

        [ObservableProperty]
        private bool mostrarSoloConSaldo = false;

        [ObservableProperty]
        private DateTime? fechaDesde;

        [ObservableProperty]
        private DateTime? fechaHasta;

        public PedidosViewModel()
        {
            CargarPedidos();
        }

        partial void OnTextoBusquedaChanged(string value) => AplicarFiltro();
        partial void OnMostrarSoloConSaldoChanged(bool value) => AplicarFiltro();
        partial void OnFechaDesdeChanged(DateTime? value) => AplicarFiltro();
        partial void OnFechaHastaChanged(DateTime? value) => AplicarFiltro();

        public void CargarPedidos()
        {
            using var db = new LibreriaDbContext();
            _todosLosPedidos = db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.Numero)
                .ToList();

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var filtro = TextoBusqueda?.Trim().ToLower() ?? string.Empty;

            IEnumerable<Pedido> resultado = _todosLosPedidos;

            if (!string.IsNullOrEmpty(filtro))
            {
                resultado = resultado.Where(p =>
                    p.Numero.ToString().Contains(filtro) ||
                    p.Cliente.Nombre.ToLower().Contains(filtro));
            }

            if (MostrarSoloConSaldo)
            {
                resultado = resultado.Where(p => p.SaldoPendiente > 0 && !p.Anulado);
            }

            if (FechaDesde.HasValue)
            {
                resultado = resultado.Where(p => p.Fecha.Date >= FechaDesde.Value.Date);
            }

            if (FechaHasta.HasValue)
            {
                resultado = resultado.Where(p => p.Fecha.Date <= FechaHasta.Value.Date);
            }

            PedidosFiltrados.Clear();
            foreach (var p in resultado)
                PedidosFiltrados.Add(p);
        }

        [RelayCommand]
        private void Nuevo()
        {
            var vm = new PedidoEditViewModel();
            var ventana = new PedidoEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarPedidos();
        }

        [RelayCommand]
        private void Editar()
        {
            if (PedidoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un pedido para editar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (PedidoSeleccionado.Anulado)
            {
                MessageBox.Show("No se puede editar un pedido anulado.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vm = new PedidoEditViewModel(PedidoSeleccionado);
            var ventana = new PedidoEditWindow(vm) { Owner = Application.Current.MainWindow };

            if (ventana.ShowDialog() == true)
                CargarPedidos();
        }

        [RelayCommand]
        private void Anular()
        {
            if (PedidoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un pedido para anular.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (PedidoSeleccionado.Anulado)
            {
                MessageBox.Show("Este pedido ya está anulado.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Seguro que querés anular el pedido Nº {PedidoSeleccionado.Numero}?\n\n" +
                "Se devolverá el stock de los productos y se ajustará el saldo del cliente si corresponde.",
                "Confirmar anulación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes)
                return;

            AnularPedido(PedidoSeleccionado.Id);
            CargarPedidos();
        }

        private void AnularPedido(int pedidoId)
        {
            using var db = new LibreriaDbContext();

            var pedido = db.Pedidos
                .Include(p => p.Detalles)
                .Include(p => p.Cliente)
                .First(p => p.Id == pedidoId);

            // 1. Devolver stock si corresponde
            if (pedido.DescontarStock)
            {
                foreach (var detalle in pedido.Detalles)
                {
                    var producto = db.Productos.First(p => p.Id == detalle.ProductoId);
                    producto.StockActual += detalle.Cantidad;
                    producto.StockComprometido -= detalle.Cantidad;
                    if (producto.StockComprometido < 0) producto.StockComprometido = 0;
                }
            }
            else
            {
                // Era un encargo: solo liberamos el stock comprometido
                foreach (var detalle in pedido.Detalles)
                {
                    var producto = db.Productos.First(p => p.Id == detalle.ProductoId);
                    producto.StockComprometido -= detalle.Cantidad;
                    if (producto.StockComprometido < 0) producto.StockComprometido = 0;
                }
            }

            // Eliminar el movimiento de cuenta corriente del pedido anulado
            if (pedido.FormaPago == FormaPago.CuentaCorriente)
            {
                var movimientos = db.MovimientosCuenta
                    .Where(m => m.PedidoId == pedido.Id)
                    .ToList();
                db.MovimientosCuenta.RemoveRange(movimientos);
            }

            // 3. Marcar como anulado
            pedido.Anulado = true;
            pedido.FechaAnulacion = DateTime.Now;

            db.SaveChanges();
        }
    }
}