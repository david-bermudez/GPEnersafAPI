using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GpEnerSaf.Models.BD
{
    [Table("gp_liquidacion", Schema = "public")]
    public partial class GPLiquidacion
    {
        [Required]
        [Column("fechafacturacion")]
        public string Fechafacturacion { get; set; }
        [Required]
        [Column("version")]
        public string Version { get; set; }
        [Required]
        [Column("factura_id")]
        public int Factura_id { get; set; }

        [Column("frontera")]
        public string Frontera { get; set; }

        [Column("factura_dian")]
        public string Factura_dian { get; set; }

        [Column("cliente_nombre")]
        public string Cliente_nombre { get; set; }

        [Column("municipio_id")]
        public string Municipio_id { get; set; }

        [Column("municipio_nombre")]
        public string Municipio_nombre { get; set; }

        [Column("departamento_nombre")]
        public string Departamento_nombre { get; set; }

        [Column("operador_sigla")]
        public string Operador_sigla { get; set; }

        [Column("nivel_tension")]
        public int Nivel_tension { get; set; }

        [Column("q_activa")]
        public double Q_activa { get; set; }

        [Column("q_inductiva")]
        public double Q_inductiva { get; set; }

        [Column("q_inductiva_pen")]
        public double Q_inductiva_pen { get; set; }

        [Column("q_capacitiva")]
        public double Q_capacitiva { get; set; }

        [Column("q_reactiva_pen")]
        public double Q_reactiva_pen { get; set; }

        [Column("gm_redo")]
        public double Gm_redo { get; set; }

        [Column("rm_redo")]
        public double Rm_redo { get; set; }

        [Column("cm_redo")]
        public double Cm_redo { get; set; }

        [Column("dm_redo")]
        public double Dm_redo { get; set; }

        [Column("om_redo")]
        public double Om_redo { get; set; }

        [Column("ppond_redo")]
        public double Ppond_redo { get; set; }

        [Column("tpond_redo")]
        public double Tpond_redo { get; set; }

        [Column("v_gm")]
        public double V_gm { get; set; }

        [Column("v_rm")]
        public double V_rm { get; set; }

        [Column("v_cm")]
        public double V_cm { get; set; }

        [Column("v_dm")]
        public double V_dm { get; set; }

        [Column("v_om")]
        public double V_om { get; set; }

        [Column("v_ppond")]
        public double V_ppond { get; set; }

        [Column("v_tpond")]
        public double V_tpond { get; set; }

        [Column("v_activa")]
        public double V_activa { get; set; }

        [Column("v_reactiva_pen")]
        public double V_reactiva_pen { get; set; }

        [Column("v_consumo_energia")]
        public double V_consumo_energia { get; set; }

        [Column("v_gm_ajuste")]
        public double V_gm_ajuste { get; set; }

        [Column("v_rm_ajuste")]
        public double V_rm_ajuste { get; set; }

        [Column("v_cm_ajuste")]
        public double V_cm_ajuste { get; set; }

        [Column("v_dm_ajuste")]
        public double V_dm_ajuste { get; set; }

        [Column("v_om_ajuste")]
        public double V_om_ajuste { get; set; }

        [Column("v_ppond_ajuste")]
        public double V_ppond_ajuste { get; set; }

        [Column("v_tpond_ajuste")]
        public double V_tpond_ajuste { get; set; }

        [Column("v_activa_ajuste")]
        public double V_activa_ajuste { get; set; }

        [Column("v_reactiva_pen_ajuste")]
        public double V_reactiva_pen_ajuste { get; set; }

        [Column("v_consumo_energia_ajuste")]
        public double V_consumo_energia_ajuste { get; set; }

        [Column("v_gm_ajustado")]
        public double V_gm_ajustado { get; set; }

        [Column("v_rm_ajustado")]
        public double V_rm_ajustado { get; set; }

        [Column("v_cm_ajustado")]
        public double V_cm_ajustado { get; set; }

        [Column("v_dm_ajustado")]
        public double V_dm_ajustado { get; set; }

        [Column("v_om_ajustado")]
        public double V_om_ajustado { get; set; }

        [Column("v_ppond_ajustado")]
        public double V_ppond_ajustado { get; set; }

        [Column("v_tpond_ajustado")]
        public double V_tpond_ajustado { get; set; }

        [Column("v_activa_ajustado")]
        public double V_activa_ajustado { get; set; }

        [Column("v_reactiva_pen_ajustado")]
        public double V_reactiva_pen_ajustado { get; set; }

        [Column("v_consumo_energia_ajustado")]
        public double V_consumo_energia_ajustado { get; set; }

        [Column("v_contribucion")]
        public double V_contribucion { get; set; }

        [Column("v_sobretasa")]
        public double V_sobretasa { get; set; }

        [Column("v_adcn")]
        public double V_adcn { get; set; }

        [Column("v_iapb")]
        public double V_iapb { get; set; }

        [Column("v_iap_ajuste")]
        public double V_iap_ajuste { get; set; }

        [Column("v_rrnt")]
        public double V_rrnt { get; set; }

        [Column("v_arnt")]
        public double V_arnt { get; set; }

        [Column("v_rfntica")]
        public double V_rfntica { get; set; }

        [Column("v_afntica")]
        public double V_afntica { get; set; }

        [Column("v_rrntbmb")]
        public double V_rrntbmb { get; set; }

        [Column("v_otros_total")]
        public double V_otros_total { get; set; }

        [Column("v_neto_factura")]
        public double V_neto_factura { get; set; }

        [Column("v_compensacion")]
        public double V_compensacion { get; set; }

        [Column("v_arntbmb")]
        public double V_arntbmb { get; set; }

        [Column("tipo_factura")]
        public string Tipo_factura { get; set; }

        [Column("v_sgcv")]
        public double V_sgcv { get; set; }

        [Column("factor_m")]
        public double Factor_m { get; set; }

        [Column("v_asgcv")]
        public double V_asgcv { get; set; }

        [Column("estado")]
        public int Estado {get; set; }

        [Column("ultimo_error")]
        public string ultimo_error { get; set; }   

    }
}
