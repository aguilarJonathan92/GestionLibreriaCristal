using System;
using System.Collections.Generic;
using System.Text;

namespace GestionLibreriaCristal.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? DniCuit { get; set; }
        public string? Notas { get; set; }

        /// <summary>
        /// Saldo actual. Positivo = debe dinero. Negativo = tiene saldo a favor.
        /// </summary>
        public decimal Saldo { get; set; }

        /// <summary>
        /// Permite "ocultar" clientes sin borrarlos, para no perder el historial.
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha en que se dio de alta el cliente.
        /// </summary>
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        // ===== Propiedades calculadas =====

        public string EstadoCuenta =>
            Saldo == 0 ? "Al día" :
            Saldo > 0 ? "Debe" :
            "A favor";
    }
}
