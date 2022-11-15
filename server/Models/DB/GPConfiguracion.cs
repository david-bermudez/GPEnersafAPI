using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GpEnerSaf.Models.BD
{
    [Table("gp_configuracion", Schema = "public")]
    public partial class GPConfiguracion
    {
        [Required]
        [Column("codigo")]
        public string Codigo { get; set; }
        [Required]
        [Column("nombre")]
        public string Nombre { get; set; }
        [Required]
        [Column("tipo")]
        public string Tipo { get; set; }
        
        [Column("tipo_asiento")]
        public string Tipo_asiento { get; set; }
        [Required]
        [Column("concepto")]
        public string Concepto { get; set; }
        [Required]
        [Column("codctaco")]
        public string Codctaco { get; set; }

        [Column("codsucur")]
        public string Codsucur { get; set; }

        [Column("codtipfu")]
        public string Codtipfu { get; set; }

        [Column("numtipfu")]
        public string Numtipfu { get; set; }

        [Column("codtipdc")]
        public string Codtipdc { get; set; }

        [Column("numdocso")]
        public string Numdocso { get; set; }

        [Column("codlibro")]
        public string Codlibro { get; set; }

        [Column("esquemat")]
        public string Esquemat { get; set; }

        [Column("nivanal1")]
        public string Nivanal1 { get; set; }

        [Column("codniva1")]
        public string Codniva1 { get; set; }

        [Column("nivanal2")]
        public string Nivanal2 { get; set; }

        [Column("codniva2")]
        public string Codniva2 { get; set; }

        [Column("nivanal3")]
        public string Nivanal3 { get; set; }

        [Column("codniva3")]
        public string Codniva3 { get; set; }

        [Column("formula")]
        public string Formula { get; set; }

        [Column("tipo_moneda")]
        public string Tipo_moneda { get; set; }
        
        [Column("codcompr")]
        public string Codcompr { get; set; }

        [Column("codopepr")]
        public string Codopepr { get; set; }

        [Column("sort_order")]
        public int Sort_order { get; set; }

        [Column("contrato")]
        public string Contrato { get; set; }


        [Column("activo")]
        public int activo { get; set; }
    }
}
