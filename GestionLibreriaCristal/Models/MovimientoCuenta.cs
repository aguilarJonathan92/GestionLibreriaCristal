namespace GestionLibreriaCristal.Models
{
    public class MovimientoCuenta
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public DateTime Fecha { get; set; } = DateTime.Now;

        public TipoMovimiento Tipo { get; set; }

        public decimal Monto { get; set; }

        public string? Descripcion { get; set; }

        // Si el movimiento viene de un pedido, guardamos la referencia
        public int? PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        // ===== Cálculo del signo =====
        // Cargo → suma al saldo
        // Pago → resta al saldo
        // Ajuste → puede ser positivo o negativo, según el Monto
        public decimal MontoConSigno =>
            Tipo == TipoMovimiento.Pago ? -Monto : Monto;
    }

    public enum TipoMovimiento
    {
        Cargo,   // deuda (ej: venta fiada)
        Pago,    // abono (ej: cliente paga)
        Ajuste   // corrección manual
    }
}