using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tienda.Models
{
    public partial class Producto
    {
        [ValidateNever]  // ⬅️ No validarla en formularios
        public ICollection<Comment> Comentarios { get; set; } = new List<Comment>();

        public int ProductoId { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Talla { get; set; }

        public string? Color { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int? CategoriaId { get; set; }

        public string? ImagenUrl { get; set; }

        public virtual Categoria? Categoria { get; set; }

        public virtual ICollection<DetallesVentum> DetallesVenta { get; set; } = new List<DetallesVentum>();

        [NotMapped]
        public IFormFile? ImagenArchivo { get; set; }
    }
}
