using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GpEnerSaf.Models.BD
{
    [Table("gp_usuario", Schema = "public")]
    public partial class GPUsuario
    {
        [Required]
        [Column("usuario")]
        public string Usuario { get; set; }
        
        [Column("perfil")]
        public string Perfil { get; set; }
    }
}
