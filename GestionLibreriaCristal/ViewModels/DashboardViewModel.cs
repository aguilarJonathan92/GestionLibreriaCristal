using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionLibreriaCristal.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Microsoft.EntityFrameworkCore;

namespace GestionLibreriaCristal.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        // ===== 1. Ventas del mes =====
        public ISeries[] SeriesVentas { get; set; } = Array.Empty<ISeries>();
        public Axis[] EjeXVentas { get; set; } = Array.Empty<Axis>();
        public Axis[] EjeYVentas { get; set; } = Array.Empty<Axis>();

        // ===== 2. Productos más vendidos =====
        public ISeries[] SeriesProductos { get; set; } = Array.Empty<ISeries>();

        // ===== 3. Deudas por cliente =====
        public ObservableCollection<DeudaCliente> ClientesConDeuda { get; } = new();

        // ===== 4. Stock bajo =====
        public ObservableCollection<ProductoStockBajo> ProductosStockBajo { get; } = new();

        public DashboardViewModel()
        {
            CargarDatos();
        }

        public void CargarDatos()
        {
            using var db = new LibreriaDbContext();

            // --- Ventas del mes (últimos 30 días) ---
            var hace30Dias = DateTime.Now.AddDays(-30);

            // 1. Traemos los pedidos crudos a memoria primero (ToList)
            var pedidosRecientes = db.Pedidos
                .Where(p => p.Fecha >= hace30Dias && !p.Anulado)
                .ToList();

            var ventasPorDia = pedidosRecientes
                .GroupBy(p => p.Fecha.Date)
                .Select(g => new
                {
                    Dia = g.Key,
                    Total = g.Sum(p => p.Total)
                })
                .OrderBy(x => x.Dia)
                .ToList();

            // Si no hay ventas, mostramos datos vacíos para que no explote
            var labels = ventasPorDia.Any()
                ? ventasPorDia.Select(x => x.Dia.ToString("dd/MM")).ToArray()
                : new[] { "Sin ventas" };
            var valores = ventasPorDia.Any()
                ? ventasPorDia.Select(x => (double)x.Total).ToArray()
                : new double[] { 0 };

            SeriesVentas = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = valores,
                    Fill = new SolidColorPaint(SKColors.Pink.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.DeepPink) { StrokeThickness = 3 },
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColors.DeepPink) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColors.White)
                }
            };

            EjeXVentas = new Axis[]
            {
                new Axis { Labels = labels, LabelsRotation = 45 }
            };

            EjeYVentas = new Axis[]
            {
                new Axis { Labeler = value => $"${value:N0}" }
            };

            // --- Productos más vendidos (Top 5) ---
            // --- Productos más vendidos (Top 5) ---
            // 1. Traemos los detalles a memoria primero para evitar el error de GroupBy en EF Core
            var detallesDb = db.DetallesPedido
                .Include(d => d.Pedido)
                .Include(d => d.Producto)
                .Where(d => !d.Pedido.Anulado)
                .ToList();

            // 2. Ahora agrupamos en C# normal
            var topProductos = detallesDb
                .GroupBy(d => d.Producto.Nombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            // 3. Definimos una paleta de colores lindos para el gráfico
            var coloresGrafico = new[] {
    SKColors.DeepPink,
    SKColors.MediumPurple,
    SKColors.Orange,
    SKColors.LightSkyBlue,
    SKColors.LightGreen
};

            // 4. Armamos el gráfico
            if (topProductos.Any())
            {
                int index = 0;
                SeriesProductos = topProductos.Select(x =>
                {
                    var serie = new PieSeries<double>
                    {
                        Values = new double[] { x.Cantidad },
                        Name = x.Nombre,
                        DataLabelsPaint = new SolidColorPaint(SKColors.White),
                        DataLabelsSize = 12,
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                        Fill = new SolidColorPaint(coloresGrafico[index % coloresGrafico.Length]) // Asignamos el color
                    };
                    index++;
                    return serie;
                }).ToArray();
            }
            else
            {
                SeriesProductos = new ISeries[]
                {
        new PieSeries<double>
        {
            Values = new double[] { 1 },
            Name = "Sin ventas",
            DataLabelsPaint = new SolidColorPaint(SKColors.Gray),
            Fill = new SolidColorPaint(SKColors.LightGray)
        }
                };
            }

            // --- Deudas por cliente (Top 5) ---
            var clientesDb = db.Clientes.Where(c => c.Activo).ToList();
            var movimientosDb = db.MovimientosCuenta.ToList();

            foreach (var cliente in clientesDb)
            {
                cliente.Movimientos = movimientosDb.Where(m => m.ClienteId == cliente.Id).ToList();
            }

            var clientesDeuda = clientesDb
                .Where(c => c.Saldo > 0)
                .OrderByDescending(c => c.Saldo)
                .Take(5)
                .Select(c => new DeudaCliente
                {
                    Nombre = c.Nombre,
                    Saldo = c.Saldo
                })
                .ToList();

            ClientesConDeuda.Clear();
            foreach (var c in clientesDeuda)
                ClientesConDeuda.Add(c);

            // --- Stock bajo (Top 5) ---
            var stockBajo = db.Productos
                .Where(p => p.StockActual <= p.StockMinimo && p.StockActual > 0)
                .OrderBy(p => p.StockActual)
                .Take(5)
                .Select(p => new ProductoStockBajo
                {
                    Nombre = p.Nombre,
                    StockActual = p.StockActual,
                    StockMinimo = p.StockMinimo
                })
                .ToList();

            ProductosStockBajo.Clear();
            foreach (var p in stockBajo)
                ProductosStockBajo.Add(p);
        }
    }

    // Clases auxiliares para mostrar en las listas
    public class DeudaCliente
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
    }

    public class ProductoStockBajo
    {
        public string Nombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }
}