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
        [Column("codigoingreso")]
        public string CodigoIngreso { get; set; }
        [Required]
        [Column("fechafacturacion")]
        public string Fechafacturacion { get; set; }
        [Required]
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
