using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GpEnerSaf.Models.BD
{
    [Table("gp_saldo", Schema = "public")]
    public partial class GPSaldo
    {
        //Cliente
        [Required]
        [Column("nombre_grupo")]
        public string Nombre_grupo { get; set; }

        [Required]
        [Column("periodo")]
        public string Periodo { get; set; }

        [Column("codigoingreso")]
        public string CodigoIngreso { get; set; }

        [Column("valoringreso")]
        public double ValorIngreso { get; set; }

        [Column("codigofactura")]
        public int CodigoFactura{ get; set; }

        [Column("valorfactura")]
        public double ValorFactura { get; set; }

        public double difference { get; set; }
    }
}
