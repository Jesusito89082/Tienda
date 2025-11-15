using System;
using System.ComponentModel.DataAnnotations;

namespace Tienda.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Texto { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Si el comentario es para un producto
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}
