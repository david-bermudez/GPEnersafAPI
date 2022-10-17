using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GpEnerSaf.Models.BD
{
    [Table("gp_liquidacion_concepto", Schema = "public")]
    public partial class GPLiquidacionConcepto
    {
        [Required]
        [Column("fechafacturacion")]
        public string Fechafacturacion { get; set; }

        [Column("version")]
        public string Version { get; set; }

        [Required]
        [Column("factura_id")]
        public int Factura_id { get; set; }

        [Required]
        [Column("concepto")]
        public string Concepto { get; set; }

        [Column("valor")]
        public double Valor { get; set; }   
    }
}
