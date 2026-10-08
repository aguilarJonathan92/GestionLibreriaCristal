using System;
using System.Collections.Generic;
using System.Text;

namespace GestionLibreriaCristal.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ColorHex { get; set; }

        // Relación: una categoría tiene muchos productos
        public List<Producto> Productos { get; set; } = new();
    }
}
