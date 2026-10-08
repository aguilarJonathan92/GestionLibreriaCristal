using System;
using System.Collections.Generic;
using System.Text;

namespace GestionLibreriaCristal.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Notas { get; set; }

        // Relación: un proveedor provee muchos productos
        public List<Producto> Productos { get; set; } = new();
    }
}
