using DocumentFormat.OpenXml.Wordprocessing;
using Parlot;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GpEnerSaf.Models.BD
{
    [Table("gp_municipio", Schema = "public")]
    public partial class GPMunicipio
    {

        [Required]
        [Column("municipio")]
        public int Municipio { get; set; }
        [Required]
        [Column("departamento")]
        public string Departamento { get; set; }
        [Required]
        [Column("departamento_nombre")]
        public string Departamento_nombre { get; set; }
        
        [Column("municipio_nombre")]
        public string Municipio_nombre { get; set; }
        [Required]
        [Column("nit")]
        public string Nit { get; set; }
        [Required]
        [Column("recauda_iap")]
        public string Recauda_iap { get; set; }

        [Column("ica")]
        public string Ica { get; set; }

        [Column("ica_tasa")]
        public double Ica_tasa { get; set; }

        [Column("ica_piso")]
        public double Ica_piso { get; set; }

        [Column("ica_cuenta")]
        public string Ica_cuenta { get; set; }

        [Column("autoica")]
        public string Autoica { get; set; }

        [Column("autoica_tasa")]
        public double Autoica_tasa { get; set; }

        [Column("autoica_cuenta_activo")]
        public string Autoica_cuenta_activo { get; set; }

        [Column("autoica_cuenta_pasivo")]
        public string Autoica_cuenta_pasivo { get; set; }

        [Column("bomberil")]
        public string Bomberil { get; set; }

        [Column("bomberil_tasa")]
        public double Bomberil_tasa { get; set; }

        [Column("bomberil_cuenta")]
        public string Bomberil_cuenta { get; set; }

        [Column("autobomberil_cuenta_pasivo")]
        public string Autobomberil_cuenta_pasivo { get; set; }
    }
}
