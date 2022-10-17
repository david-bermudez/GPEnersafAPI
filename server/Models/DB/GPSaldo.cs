using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GpEnerSaf.Models.BD
{
    [Table("gp_saldo", Schema = "public")]
    public partial class GPSaldo
    {
        [Required]
        [Column("nombre_grupo")]
        public string Nombre_grupo { get; set; }
        [Required]
        [Column("periodo")]
        public string Periodo { get; set; }
        [Column("valor")]
        public double Valor { get; set; }
    }
}
