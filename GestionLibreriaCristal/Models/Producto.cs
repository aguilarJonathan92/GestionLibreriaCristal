using System;
using System.Collections.Generic;
using System.Text;

namespace GestionLibreriaCristal.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public TipoProducto Tipo { get; set; } = TipoProducto.Simple;

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public decimal CostoCompra { get; set; }
        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }

        // ===== Propiedades calculadas (NO se guardan en la BD) =====

        public decimal MargenUnitario => PrecioVenta - CostoCompra;

        public decimal MargenPorcentaje =>
            CostoCompra == 0 ? 0 : (MargenUnitario / CostoCompra) * 100;

        public EstadoStock Estado =>
            StockActual == 0 ? EstadoStock.SinStock :
            StockActual <= StockMinimo ? EstadoStock.Reponer :
            StockActual <= StockMinimo * 2 ? EstadoStock.Bajo :
            EstadoStock.Optimo;

    }

    public enum EstadoStock
    {
        SinStock,
        Reponer,
        Bajo,
        Optimo
    }

    public enum TipoProducto
    {
        Simple,
        Combo
    }
}
