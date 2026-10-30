using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionLibreriaCristal.Data;
using GestionLibreriaCristal.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionLibreriaCristal.ViewModels
{
    /// <summary>
    /// ViewModel auxiliar que representa una línea del pedido en la UI.
    /// </summary>
    public partial class DetalleItemViewModel : ObservableObject
    {
        public int? DetalleId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;

        [ObservableProperty] private int cantidad;
        [ObservableProperty] private decimal precioUnitario;

        public decimal Subtotal => Cantidad * PrecioUnitario;

        partial void OnCantidadChanged(int value) => OnPropertyChanged(nameof(Subtotal));
        partial void OnPrecioUnitarioChanged(decimal value) => OnPropertyChanged(nameof(Subtotal));
    }

    public partial class PedidoEditViewModel : ObservableObject
    {
        private readonly Pedido? _pedidoOriginal;

        public bool EsEdicion { get; }

        public ObservableCollection<Cliente> Clientes { get; } = new();
        public ObservableCollection<Producto> Productos { get; } = new();
        public ObservableCollection<DetalleItemViewModel> Items { get; } = new();

        [ObservableProperty] private Cliente? clienteSeleccionado;
        [ObservableProperty] private Producto? productoSeleccionado;
        [ObservableProperty] private int cantidadAgregar = 1;
        [ObservableProperty] private decimal precioAgregar;
        [ObservableProperty] private decimal descuentoPorcentaje;
        [ObservableProperty] private decimal descuentoMonto;
        [ObservableProperty] private decimal totalPagado;
        [ObservableProperty] private FormaPago formaPago = FormaPago.Efectivo;
        [ObservableProperty] private bool descontarStock = true;
        [ObservableProperty] private string? observaciones;
        [ObservableProperty] private DetalleItemViewModel? itemSeleccionado;

        public List<FormaPago> FormasPago { get; } =
            Enum.GetValues<FormaPago>().ToList();

        // ===== Totales calculados =====

        public decimal Subtotal => Items.Sum(i => i.Subtotal);

        public decimal DescuentoTotal =>
            (Subtotal * DescuentoPorcentaje / 100) + DescuentoMonto;

        public decimal Total => Subtotal - DescuentoTotal;

        public decimal Saldo => Total - TotalPagado;

        // Recalcular cuando cambian los valores
        partial void OnDescuentoPorcentajeChanged(decimal value) => RecalcularTotales();
        partial void OnDescuentoMontoChanged(decimal value) => RecalcularTotales();
        partial void OnTotalPagadoChanged(decimal value) => RecalcularTotales();

        partial void OnProductoSeleccionadoChanged(Producto? value)
        {
            if (value != null)
                PrecioAgregar = value.PrecioVenta;
        }

        private void RecalcularTotales()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(DescuentoTotal));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Saldo));
        }

        // ===== Constructores =====

        // Nuevo pedido
        public PedidoEditViewModel()
        {
            EsEdicion = false;
            CargarCombos();

            // Por defecto: Consumidor Final
            ClienteSeleccionado = Clientes.FirstOrDefault(c => c.Nombre == "Consumidor Final")
                                  ?? Clientes.FirstOrDefault();

            Items.CollectionChanged += (s, e) => RecalcularTotales();
        }

        // Editar pedido existente
        public PedidoEditViewModel(Pedido pedido)
        {
            EsEdicion = true;
            _pedidoOriginal = pedido;
            CargarCombos();

            // Cargar datos del pedido
            DescuentoPorcentaje = pedido.DescuentoPorcentaje;
            DescuentoMonto = pedido.DescuentoMonto;
            TotalPagado = pedido.TotalPagado;
            FormaPago = pedido.FormaPago;
            DescontarStock = pedido.DescontarStock;
            Observaciones = pedido.Observaciones;

            // Cargar items
            using var db = new LibreriaDbContext();
            var detalles = db.DetallesPedido
                .Where(d => d.PedidoId == pedido.Id)
                .Include(d => d.Producto)
                .ToList();

            foreach (var d in detalles)
            {
                Items.Add(new DetalleItemViewModel
                {
                    DetalleId = d.Id,
                    ProductoId = d.ProductoId,
                    NombreProducto = d.Producto.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                });
            }

            // Cliente
            ClienteSeleccionado = Clientes.FirstOrDefault(c => c.Id == pedido.ClienteId);

            Items.CollectionChanged += (s, e) => RecalcularTotales();
        }

        private void CargarCombos()
        {
            using var db = new LibreriaDbContext();

            Clientes.Clear();
            foreach (var c in db.Clientes.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList())
                Clientes.Add(c);

            Productos.Clear();
            foreach (var p in db.Productos.OrderBy(p => p.Nombre).ToList())
                Productos.Add(p);
        }

        // ===== Comandos =====

        [RelayCommand]
        private void AgregarItem()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un producto.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CantidadAgregar <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PrecioAgregar <= 0)
            {
                MessageBox.Show("El precio debe ser mayor a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar stock si corresponde
            if (DescontarStock)
            {
                var yaEnPedido = Items.Where(i => i.ProductoId == ProductoSeleccionado.Id)
                                      .Sum(i => i.Cantidad);
                var stockDisponible = ProductoSeleccionado.StockActual - ProductoSeleccionado.StockComprometido;

                if (yaEnPedido + CantidadAgregar > stockDisponible)
                {
                    MessageBox.Show(
                        $"Stock insuficiente para \"{ProductoSeleccionado.Nombre}\".\n\n" +
                        $"Disponible: {stockDisponible}\n" +
                        $"Ya en el pedido: {yaEnPedido}\n" +
                        $"Intentás agregar: {CantidadAgregar}",
                        "Stock insuficiente",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Si el producto ya está en el pedido, sumar cantidad
            var existente = Items.FirstOrDefault(i => i.ProductoId == ProductoSeleccionado.Id);
            if (existente != null)
            {
                existente.Cantidad += CantidadAgregar;
            }
            else
            {
                Items.Add(new DetalleItemViewModel
                {
                    ProductoId = ProductoSeleccionado.Id,
                    NombreProducto = ProductoSeleccionado.Nombre,
                    Cantidad = CantidadAgregar,
                    PrecioUnitario = PrecioAgregar
                });
            }

            // Resetear formulario
            ProductoSeleccionado = null;
            CantidadAgregar = 1;
            PrecioAgregar = 0;
        }

        [RelayCommand]
        private void QuitarItem()
        {
            if (ItemSeleccionado == null) return;
            Items.Remove(ItemSeleccionado);
        }

        // ===== Guardar =====

        public bool Guardar()
        {
            // Validaciones
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un cliente.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Items.Count == 0)
            {
                MessageBox.Show("Agregá al menos un producto al pedido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TotalPagado < 0)
            {
                MessageBox.Show("El pago no puede ser negativo.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TotalPagado > Total)
            {
                MessageBox.Show("El pago no puede superar el total del pedido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using var db = new LibreriaDbContext();

            if (EsEdicion && _pedidoOriginal != null)
            {
                return GuardarEdicion(db);
            }
            else
            {
                return GuardarNuevo(db);
            }
        }

        private bool GuardarNuevo(LibreriaDbContext db)
        {
            // Generar número correlativo
            var ultimoNumero = db.Pedidos.Any() ? db.Pedidos.Max(p => p.Numero) : 0;

            var pedido = new Pedido
            {
                Numero = ultimoNumero + 1,
                Fecha = DateTime.Now,
                ClienteId = ClienteSeleccionado!.Id,
                DescuentoPorcentaje = DescuentoPorcentaje,
                DescuentoMonto = DescuentoMonto,
                TotalPagado = TotalPagado,
                FormaPago = FormaPago,
                DescontarStock = DescontarStock,
                Observaciones = Observaciones?.Trim()
            };

            // Agregar detalles
            foreach (var item in Items)
            {
                pedido.Detalles.Add(new DetallePedido
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            }

            db.Pedidos.Add(pedido);

            // Actualizar stock
            ActualizarStock(db, Items.ToList(), DescontarStock, esAlta: true);
            db.SaveChanges();

            // Generar movimiento de cuenta corriente si corresponde
            if (FormaPago == FormaPago.CuentaCorriente && Saldo > 0)
            {
                var clienteDb = db.Clientes.FirstOrDefault(c => c.Id == ClienteSeleccionado!.Id)
                    ?? db.Clientes.FirstOrDefault(c => c.Nombre == ClienteSeleccionado!.Nombre);

                if (clienteDb == null)
                {
                    MessageBox.Show(
                        "Error: El cliente no existe en la base de datos. No se puede registrar la deuda.",
                        "Error de integridad",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                db.MovimientosCuenta.Add(new MovimientoCuenta
                {
                    ClienteId = clienteDb.Id,
                    Fecha = DateTime.Now,
                    Tipo = TipoMovimiento.Cargo,
                    Monto = Saldo,
                    Descripcion = $"Pedido Nº {pedido.Numero} (fiado)",
                    PedidoId = pedido.Id
                });
            }

            db.SaveChanges();
            return true;
        }

        private bool GuardarEdicion(LibreriaDbContext db)
        {
            var pedido = db.Pedidos
                .Include(p => p.Detalles)
                .First(p => p.Id == _pedidoOriginal!.Id);

            // 1. Revertir el stock del pedido original
            ActualizarStock(db, pedido.Detalles.Select(d => new DetalleItemViewModel
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad
            }).ToList(), pedido.DescontarStock, esAlta: false);

            // Revertir el movimiento de cuenta corriente del pedido original
            if (pedido.FormaPago == FormaPago.CuentaCorriente && pedido.SaldoPendiente > 0)
            {
                var movimientoViejo = db.MovimientosCuenta
                    .FirstOrDefault(m => m.PedidoId == pedido.Id && m.Tipo == TipoMovimiento.Cargo);
                if (movimientoViejo != null)
                    db.MovimientosCuenta.Remove(movimientoViejo);
            }

            // 3. Borrar los detalles viejos
            db.DetallesPedido.RemoveRange(pedido.Detalles);
            pedido.Detalles.Clear();

            // 4. Actualizar datos del pedido
            pedido.ClienteId = ClienteSeleccionado!.Id;
            pedido.DescuentoPorcentaje = DescuentoPorcentaje;
            pedido.DescuentoMonto = DescuentoMonto;
            pedido.TotalPagado = TotalPagado;
            pedido.FormaPago = FormaPago;
            pedido.DescontarStock = DescontarStock;
            pedido.Observaciones = Observaciones?.Trim();

            // 5. Agregar los detalles nuevos
            foreach (var item in Items)
            {
                pedido.Detalles.Add(new DetallePedido
                {
                    PedidoId = pedido.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            }

            db.SaveChanges(); // Guardamos primero para que se apliquen los cambios

            // 6. Aplicar el stock nuevo
            ActualizarStock(db, Items.ToList(), DescontarStock, esAlta: true);

            // Generar nuevo movimiento de cuenta corriente
            if (FormaPago == FormaPago.CuentaCorriente && Saldo > 0)
            {
                db.MovimientosCuenta.Add(new MovimientoCuenta
                {
                    ClienteId = ClienteSeleccionado!.Id,
                    Fecha = DateTime.Now,
                    Tipo = TipoMovimiento.Cargo,
                    Monto = Saldo,
                    Descripcion = $"Pedido Nº {pedido.Numero} (fiado) - editado",
                    PedidoId = pedido.Id
                });
            }
            db.SaveChanges();
            return true;
        }

        /// <summary>
        /// Ajusta el stock de los productos según los items del pedido.
        /// Si esAlta=true, descuenta (o compromete) stock. Si esAlta=false, lo devuelve.
        /// </summary>
        private static void ActualizarStock(
            LibreriaDbContext db,
            List<DetalleItemViewModel> items,
            bool descontarStock,
            bool esAlta)
        {
            foreach (var item in items)
            {
                var producto = db.Productos.First(p => p.Id == item.ProductoId);
                var cantidad = esAlta ? item.Cantidad : -item.Cantidad;

                if (descontarStock)
                {
                    // Descontar stock actual (y liberar comprometido si estaba)
                    producto.StockActual -= cantidad;
                    if (producto.StockComprometido > 0)
                    {
                        producto.StockComprometido -= Math.Min(producto.StockComprometido, cantidad);
                        if (producto.StockComprometido < 0) producto.StockComprometido = 0;
                    }
                }
                else
                {
                    // Es un encargo: comprometer stock (no descontar)
                    producto.StockComprometido += cantidad;
                    if (producto.StockComprometido < 0) producto.StockComprometido = 0;
                }
            }
        }
    }
}