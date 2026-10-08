using System;
using System.Collections.Generic;
using System.Text;

namespace GestionLibreriaCristal.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public List<DetallePedido> Detalles { get; set; } = new();

        public decimal DescuentoPorcentaje { get; set; }
        public decimal DescuentoMonto { get; set; }

        public decimal TotalPagado { get; set; }
        public FormaPago FormaPago { get; set; } = FormaPago.Efectivo;

        /// <summary>
        /// Si está marcado, al confirmar el pedido se descuenta el stock.
        /// </summary>
        public bool DescontarStock { get; set; } = true;

        public string? Observaciones { get; set; }

        // ===== Anulación =====
        public bool Anulado { get; set; }
        public DateTime? FechaAnulacion { get; set; }

        // ===== Propiedades calculadas =====

        public decimal Subtotal => Detalles.Sum(d => d.Subtotal);

        public decimal DescuentoTotal =>
            (Subtotal * DescuentoPorcentaje / 100) + DescuentoMonto;

        public decimal Total => Subtotal - DescuentoTotal;

        public decimal SaldoPendiente => Total - TotalPagado;

        public string EstadoPago =>
            Anulado ? "Anulado" :
            SaldoPendiente <= 0 ? "Pagado" :
            TotalPagado == 0 ? "Sin pago" :
            "Parcial";
    }

    public enum FormaPago
    {
        Efectivo,
        Transferencia,
        Tarjeta,
        CuentaCorriente
    }
}
