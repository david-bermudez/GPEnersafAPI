using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading;
using Npgsql;
using GpEnerSaf.Data;
using GpEnerSaf.Models.BD;
using System.Linq;
using System.Globalization;

namespace GpEnerSaf.Repositories
{
    public class EnersincRepositoryImpl : IEnersincRepository
    {
        IConfiguration configuration;
        public EnersincRepositoryImpl(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public IEnumerable<dynamic> GetBatchPendingInvoiceList(string period)
        { 
            string query =
                " SELECT " +
                " fechafacturacion AS \"Fechafacturacion\", " +
                " version AS \"Version\", " +
                " factura_id AS \"Factura_id\", " +
                " frontera AS \"Frontera\", " +
                " factura_dian AS \"Factura_dian\", " +
                " cliente_nombre AS \"Cliente_nombre\", " +
                " municipio_id AS \"Municipio_id\", " +
                " municipio_nombre AS \"Municipio_nombre\", " +
                " departamento_nombre AS \"Departamento_nombre\", " +
                " operador_sigla AS \"Operador_sigla\", " +
                " nivel_tension AS \"Nivel_tension\", " +
                " q_activa AS \"Q_activa\", " +
                " q_inductiva AS \"Q_inductiva\", " +
                " q_inductiva_pen AS \"Q_inductiva_pen\", " +
                " q_capacitiva AS \"Q_capacitiva\", " +
                " q_reactiva_pen AS \"Q_reactiva_pen\", " +
                " gm_redo AS \"Gm_redo\", " +
                " rm_redo AS \"Rm_redo\", " +
                " cm_redo AS \"Cm_redo\", " +
                " dm_redo AS \"Dm_redo\", " +
                " om_redo AS \"Om_redo\", " +
                " ppond_redo AS \"Ppond_redo\", " +
                " tpond_redo AS \"Tpond_redo\", " +
                " v_gm AS \"V_gm\", " +
                " v_rm AS \"V_rm\", " +
                " v_cm AS \"V_cm\", " +
                " v_dm AS \"V_dm\", " +
                " v_om AS \"V_om\", " +
                " v_ppond AS \"V_ppond\", " +
                " v_tpond AS \"V_tpond\", " +
                " v_activa AS \"V_activa\", " +
                " v_reactiva_pen AS \"V_reactiva_pen\", " +
                " v_consumo_energia AS \"V_consumo_energia\", " +
                " v_gm_ajuste AS \"V_gm_ajuste\", " +
                " v_rm_ajuste AS \"V_rm_ajuste\", " +
                " v_cm_ajuste AS \"V_cm_ajuste\", " +
                " v_dm_ajuste AS \"V_dm_ajuste\", " +
                " v_om_ajuste AS \"V_om_ajuste\", " +
                " v_ppond_ajuste AS \"V_ppond_ajuste\", " +
                " v_tpond_ajuste AS \"V_tpond_ajuste\", " +
                " v_activa_ajuste AS \"V_activa_ajuste\", " +
                " v_reactiva_pen_ajuste AS \"V_reactiva_pen_ajuste\", " +
                " v_consumo_energia_ajuste AS \"V_consumo_energia_ajuste\", " +
                " v_gm_ajustado AS \"V_gm_ajustado\", " +
                " v_rm_ajustado AS \"V_rm_ajustado\", " +
                " v_cm_ajustado AS \"V_cm_ajustado\", " +
                " v_dm_ajustado AS \"V_dm_ajustado\", " +
                " v_om_ajustado AS \"V_om_ajustado\", " +
                " v_ppond_ajustado AS \"V_ppond_ajustado\", " +
                " v_tpond_ajustado AS \"V_tpond_ajustado\", " +
                " v_activa_ajustado AS \"V_activa_ajustado\", " +
                " v_reactiva_pen_ajustado AS \"V_reactiva_pen_ajustado\", " +
                " v_consumo_energia_ajustado AS \"V_consumo_energia_ajustado\", " +
                " v_contribucion AS \"V_contribucion\", " +
                " v_sobretasa AS \"V_sobretasa\", " +
                " v_adcn AS \"V_adcn\", " +
                " v_iapb AS \"V_iapb\", " +
                " v_iap_ajuste AS \"V_iap_ajuste\", " +
                " v_rrnt AS \"V_rrnt\", " +
                " v_arnt AS \"V_arnt\", " +
                " v_rfntica AS \"V_rfntica\", " +
                " v_afntica AS \"V_afntica\", " +
                " v_rrntbmb AS \"V_rrntbmb\", " +
                " v_otros_total AS \"V_otros_total\", " +
                " v_neto_factura AS \"V_neto_factura\", " +
                " v_compensacion AS \"V_compensacion\", " +
                " v_arntbmb AS \"V_arntbmb\", " +
                " tipo_factura AS \"Tipo_factura\", " +
                " v_sgcv AS \"V_sgcv\", " +
                " factor_m AS \"Factor_m\", " +
                " v_asgcv AS \"V_asgcv\" " +
                " FROM app_ectc_gecc.reporte_liquidacion " +
                " WHERE to_char(fechafacturacion, 'yyyymm') = '" + period + "' AND FACTURA_DIAN is not null ";
            return QueryList(query);
            
        }

        public IEnumerable<dynamic> GetBatchPendingInvoiceItem(string fechafacturacion, string version, string factura_id)
        {
            string query =
                " SELECT " +
                " fechafacturacion AS Fechafacturacion, " +
                " version AS Version, " +
                " factura_id AS Factura_id, " +
                " frontera AS Frontera, " +
                " factura_dian AS Factura_dian, " +
                " cliente_nombre AS Cliente_nombre, " +
                " municipio_id AS Municipio_id, " +
                " municipio_nombre AS Municipio_nombre, " +
                " departamento_nombre AS Departamento_nombre, " +
                " operador_sigla AS Operador_sigla, " +
                " nivel_tension AS Nivel_tension, " +
                " q_activa AS Q_activa, " +
                " q_inductiva AS Q_inductiva, " +
                " q_inductiva_pen AS Q_inductiva_pen, " +
                " q_capacitiva AS Q_capacitiva, " +
                " q_reactiva_pen AS Q_reactiva_pen, " +
                " gm_redo AS Gm_redo, " +
                " rm_redo AS Rm_redo, " +
                " cm_redo AS Cm_redo, " +
                " dm_redo AS Dm_redo, " +
                " om_redo AS Om_redo, " +
                " ppond_redo AS Ppond_redo, " +
                " tpond_redo AS Tpond_redo, " +
                " v_gm AS V_gm, " +
                " v_rm AS V_rm, " +
                " v_cm AS V_cm, " +
                " v_dm AS V_dm, " +
                " v_om AS V_om, " +
                " v_ppond AS V_ppond, " +
                " v_tpond AS V_tpond, " +
                " v_activa AS V_activa, " +
                " v_reactiva_pen AS V_reactiva_pen, " +
                " v_consumo_energia AS V_consumo_energia, " +
                " v_gm_ajuste AS V_gm_ajuste, " +
                " v_rm_ajuste AS V_rm_ajuste, " +
                " v_cm_ajuste AS V_cm_ajuste, " +
                " v_dm_ajuste AS V_dm_ajuste, " +
                " v_om_ajuste AS V_om_ajuste, " +
                " v_ppond_ajuste AS V_ppond_ajuste, " +
                " v_tpond_ajuste AS V_tpond_ajuste, " +
                " v_activa_ajuste AS V_activa_ajuste, " +
                " v_reactiva_pen_ajuste AS V_reactiva_pen_ajuste, " +
                " v_consumo_energia_ajuste AS V_consumo_energia_ajuste, " +
                " v_gm_ajustado AS V_gm_ajustado, " +
                " v_rm_ajustado AS V_rm_ajustado, " +
                " v_cm_ajustado AS V_cm_ajustado, " +
                " v_dm_ajustado AS V_dm_ajustado, " +
                " v_om_ajustado AS V_om_ajustado, " +
                " v_ppond_ajustado AS V_ppond_ajustado, " +
                " v_tpond_ajustado AS V_tpond_ajustado, " +
                " v_activa_ajustado AS V_activa_ajustado, " +
                " v_reactiva_pen_ajustado AS V_reactiva_pen_ajustado, " +
                " v_consumo_energia_ajustado AS V_consumo_energia_ajustado, " +
                " v_contribucion AS V_contribucion, " +
                " v_sobretasa AS V_sobretasa, " +
                " v_adcn AS V_adcn, " +
                " v_iapb AS V_iapb, " +
                " v_iap_ajuste AS V_iap_ajuste, " +
                " v_rrnt AS V_rrnt, " +
                " v_arnt AS V_arnt, " +
                " v_rfntica AS V_rfntica, " +
                " v_afntica AS V_afntica, " +
                " v_rrntbmb AS V_rrntbmb, " +
                " v_otros_total AS V_otros_total, " +
                " v_neto_factura AS V_neto_factura, " +
                " v_compensacion AS V_compensacion, " +
                " v_arntbmb AS V_arntbmb, " +
                " tipo_factura AS Tipo_factura, " +
                " v_sgcv AS V_sgcv, " +
                " factor_m AS Factor_m, " +
                " v_asgcv AS V_asgcv " +
                " FROM app_ectc_gecc.reporte_liquidacion " +
                " WHERE to_char(fechafacturacion, 'yyyymmdd') = '" + fechafacturacion + "' AND version = '" + version + "' AND factura_id = " + factura_id + " AND FACTURA_DIAN IS NOT NULL ";
            return QueryList(query);

        }

        private IEnumerable<dynamic> QueryList(string query)
        {
            IEnumerable<dynamic> result = null;
            IDbConnection conn = null;
            try
            {
                conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                int intentos = 0;
                while (intentos < 3)
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        intentos = 99; // Éxito
                        result = SqlMapper.Query(conn, query.Replace("\r\n", " "));

                        conn.Close();
                        return result;
                    }
                    else
                    {
                        intentos++;
                        Thread.Sleep(1000);
                        conn.Open();
                    }
                }
                if (intentos < 99) // Falla
                {
                    throw new Exception("Error de conexión - Estado: " + conn.State);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message + " -> " + ex.InnerException.Message); 
            }
            finally
            {
                try { conn.Close();  } catch (Exception) { }
            }
            return null;
        }

        public IDbConnection GetConnection()
        {
            var connectionString = configuration.GetSection("ConnectionStrings").GetSection("DBEnersincConnection").Value;
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            return conn;
        }
    }
}