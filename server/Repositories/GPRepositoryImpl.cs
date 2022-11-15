using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Npgsql;
using GpEnerSaf.Data;
using GpEnerSaf.Models.BD;
using System.Linq;
using Npgsql.Bulk;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Claims;
using Microsoft.Identity.Client;
using project.Models.DTO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace GpEnerSaf.Repositories
{
    public class GPRepositoryImpl : IGPRepository
    {
        IConfiguration configuration;
        DbGpContext context; 
        public GPRepositoryImpl(IConfiguration _configuration, DbGpContext _context)
        {
            configuration = _configuration;
            context = _context;
        }

        public List<GPLiquidacion> GetPendingInvoiceLocal(string period, int status)
        {
            List<GPLiquidacion> items = context.GPLiquidacionEntity
                    .Where(s => s.Fechafacturacion.Substring(0, 6) == period && s.Estado == status)
                    .OrderBy( s=> s.Factura_id)
                    .ToList();
            
            return items;
        }

        public void DeletePendingInvoiceLocal(string period, int status)
        {
            List<GPLiquidacion> items = GetPendingInvoiceLocal(period, status)
                .Where(s => s.Estado == status && s.Avance == null)
                .ToList();
            context.GPLiquidacionEntity.RemoveRange(items);
            context.SaveChanges();
        }

        private string ConvertDateToString(DateTime? date)
        {
            if (date != null)
            {
                return date.Value.ToString("yyyyMM");
            } else
            {
                return " ";
            }
        }

        public List<GPLiquidacion> SaveSettlement(IEnumerable<dynamic> rows, string username)
        {
            List<GPLiquidacion> items = new List<GPLiquidacion>();
            List<GPLiquidacionLog> itemsLog = new List<GPLiquidacionLog>();
            DateTime dateTime = DateTime.Now;
            foreach (dynamic row in rows)
            {
                GPLiquidacion item = CreateGPLiquidacionRow(row);
                items.Add(item);

                GPLiquidacionLog itemLog = CreateGPLiquidacionLogRow(row, item.Estado, "", dateTime, username);
                itemsLog.Add(itemLog);
            }
            NpgsqlBulkUploader uploader = new NpgsqlBulkUploader(context);
            try
            {
                uploader.Update(items, GetPropertyToUpdate());
                uploader.Insert(items, InsertConflictAction.DoNothing());
                
            } catch (Exception )
            {
            }
            uploader.Insert(itemsLog, InsertConflictAction.DoNothing());
            return items;
        }

        public List<PropertyInfo> GetPropertyToUpdate()
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            foreach (var prop in typeof(GPLiquidacion).GetProperties())
            {
                if (!prop.Name.ToLower().Equals("ultimo_error") && !prop.Name.ToLower().Equals("avance") && !prop.Name.ToLower().Equals("estado"))
                {
                    list.Add(prop);
                }
            }

            return list;
        }

        public GPLiquidacion CreateGPLiquidacionRow(dynamic row)
        {
            GPLiquidacion liquidacion = new GPLiquidacion();
            if (row.Fechafacturacion is DateTime)
            {
                liquidacion.Fechafacturacion = row.Fechafacturacion.ToString("yyyyMMdd");
            } else
            {
                liquidacion.Fechafacturacion = row.Fechafacturacion;
            }
            liquidacion.Version = row.Version;
            liquidacion.Factura_id = row.Factura_id;
            liquidacion.Frontera = row.Frontera;
            liquidacion.Factura_dian = row.Factura_dian;
            liquidacion.Cliente_nombre = row.Cliente_nombre;
            liquidacion.Municipio_id = row.Municipio_id.ToString();
            liquidacion.Municipio_nombre = row.Municipio_nombre;
            liquidacion.Departamento_nombre = row.Departamento_nombre;
            liquidacion.Operador_sigla = row.Operador_sigla;
            liquidacion.Nivel_tension = row.Nivel_tension;
            liquidacion.Q_activa = row.Q_activa;
            liquidacion.Q_inductiva = row.Q_inductiva;
            liquidacion.Q_inductiva_pen = row.Q_inductiva_pen;
            liquidacion.Q_capacitiva = row.Q_capacitiva;
            liquidacion.Q_reactiva_pen = row.Q_reactiva_pen;
            liquidacion.Gm_redo = row.Gm_redo;
            liquidacion.Rm_redo = row.Rm_redo;
            liquidacion.Cm_redo = row.Cm_redo;
            liquidacion.Dm_redo = row.Dm_redo;
            liquidacion.Om_redo = row.Om_redo;
            liquidacion.Ppond_redo = row.Ppond_redo;
            liquidacion.Tpond_redo = row.Tpond_redo;
            liquidacion.V_gm = row.V_gm;
            liquidacion.V_rm = row.V_rm;
            liquidacion.V_cm = row.V_cm;
            liquidacion.V_dm = row.V_dm;
            liquidacion.V_om = row.V_om;
            liquidacion.V_ppond = row.V_ppond;
            liquidacion.V_tpond = row.V_tpond;
            liquidacion.V_activa = row.V_activa;
            liquidacion.V_reactiva_pen = row.V_reactiva_pen;
            liquidacion.V_consumo_energia = row.V_consumo_energia;
            liquidacion.V_gm_ajuste = row.V_gm_ajuste;
            liquidacion.V_rm_ajuste = row.V_rm_ajuste;
            liquidacion.V_cm_ajuste = row.V_cm_ajuste;
            liquidacion.V_dm_ajuste = row.V_dm_ajuste;
            liquidacion.V_om_ajuste = row.V_om_ajuste;
            liquidacion.V_ppond_ajuste = row.V_ppond_ajuste;
            liquidacion.V_tpond_ajuste = row.V_tpond_ajuste;
            liquidacion.V_activa_ajuste = row.V_activa_ajuste;
            liquidacion.V_reactiva_pen_ajuste = row.V_reactiva_pen_ajuste;
            liquidacion.V_consumo_energia_ajuste = row.V_consumo_energia_ajuste;
            liquidacion.V_gm_ajustado = row.V_gm_ajustado;
            liquidacion.V_rm_ajustado = row.V_rm_ajustado;
            liquidacion.V_cm_ajustado = row.V_cm_ajustado;
            liquidacion.V_dm_ajustado = row.V_dm_ajustado;
            liquidacion.V_om_ajustado = row.V_om_ajustado;
            liquidacion.V_ppond_ajustado = row.V_ppond_ajustado;
            liquidacion.V_tpond_ajustado = row.V_tpond_ajustado;
            liquidacion.V_activa_ajustado = row.V_activa_ajustado;
            liquidacion.V_reactiva_pen_ajustado = row.V_reactiva_pen_ajustado;
            liquidacion.V_consumo_energia_ajustado = row.V_consumo_energia_ajustado;
            liquidacion.V_contribucion = row.V_contribucion;
            liquidacion.V_sobretasa = row.V_sobretasa;
            liquidacion.V_adcn = row.V_adcn;
            liquidacion.V_iapb = row.V_iapb;
            liquidacion.V_iap_ajuste = row.V_iap_ajuste;
            liquidacion.V_rrnt = row.V_rrnt;
            liquidacion.V_arnt = row.V_arnt;
            liquidacion.V_rfntica = row.V_rfntica;
            liquidacion.V_afntica = row.V_afntica;
            liquidacion.V_rrntbmb = row.V_rrntbmb;
            liquidacion.V_otros_total = row.V_otros_total;
            liquidacion.V_neto_factura = row.V_neto_factura;
            liquidacion.V_compensacion = row.V_compensacion;
            liquidacion.V_arntbmb = row.V_arntbmb;
            liquidacion.Tipo_factura = row.Tipo_factura;
            liquidacion.V_sgcv = row.V_sgcv;
            liquidacion.Factor_m = row.Factor_m;
            liquidacion.V_asgcv = row.V_asgcv;
            liquidacion.Estado = 1;
            liquidacion.Avance = "NA";
            return liquidacion;
        }

        public GPLiquidacionLog CreateGPLiquidacionLogRow(dynamic row, int estado, string errorMessage, DateTime dateTime, string username)
        {
            GPLiquidacionLog liquidacion = new GPLiquidacionLog();
            if (row.Fechafacturacion is DateTime)
            {
                liquidacion.Fechafacturacion = row.Fechafacturacion.ToString("yyyyMMdd");
            }
            else
            {
                liquidacion.Fechafacturacion = row.Fechafacturacion;
            }
            liquidacion.Version = row.Version;
            liquidacion.Factura_id = row.Factura_id;
            liquidacion.Frontera = row.Frontera;
            liquidacion.Factura_dian = row.Factura_dian;
            liquidacion.Cliente_nombre = row.Cliente_nombre;
            liquidacion.Municipio_id = row.Municipio_id.ToString();
            liquidacion.Municipio_nombre = row.Municipio_nombre;
            liquidacion.Departamento_nombre = row.Departamento_nombre;
            liquidacion.Operador_sigla = row.Operador_sigla;
            liquidacion.Nivel_tension = row.Nivel_tension;
            liquidacion.Q_activa = row.Q_activa;
            liquidacion.Q_inductiva = row.Q_inductiva;
            liquidacion.Q_inductiva_pen = row.Q_inductiva_pen;
            liquidacion.Q_capacitiva = row.Q_capacitiva;
            liquidacion.Q_reactiva_pen = row.Q_reactiva_pen;
            liquidacion.Gm_redo = row.Gm_redo;
            liquidacion.Rm_redo = row.Rm_redo;
            liquidacion.Cm_redo = row.Cm_redo;
            liquidacion.Dm_redo = row.Dm_redo;
            liquidacion.Om_redo = row.Om_redo;
            liquidacion.Ppond_redo = row.Ppond_redo;
            liquidacion.Tpond_redo = row.Tpond_redo;
            liquidacion.V_gm = row.V_gm;
            liquidacion.V_rm = row.V_rm;
            liquidacion.V_cm = row.V_cm;
            liquidacion.V_dm = row.V_dm;
            liquidacion.V_om = row.V_om;
            liquidacion.V_ppond = row.V_ppond;
            liquidacion.V_tpond = row.V_tpond;
            liquidacion.V_activa = row.V_activa;
            liquidacion.V_reactiva_pen = row.V_reactiva_pen;
            liquidacion.V_consumo_energia = row.V_consumo_energia;
            liquidacion.V_gm_ajuste = row.V_gm_ajuste;
            liquidacion.V_rm_ajuste = row.V_rm_ajuste;
            liquidacion.V_cm_ajuste = row.V_cm_ajuste;
            liquidacion.V_dm_ajuste = row.V_dm_ajuste;
            liquidacion.V_om_ajuste = row.V_om_ajuste;
            liquidacion.V_ppond_ajuste = row.V_ppond_ajuste;
            liquidacion.V_tpond_ajuste = row.V_tpond_ajuste;
            liquidacion.V_activa_ajuste = row.V_activa_ajuste;
            liquidacion.V_reactiva_pen_ajuste = row.V_reactiva_pen_ajuste;
            liquidacion.V_consumo_energia_ajuste = row.V_consumo_energia_ajuste;
            liquidacion.V_gm_ajustado = row.V_gm_ajustado;
            liquidacion.V_rm_ajustado = row.V_rm_ajustado;
            liquidacion.V_cm_ajustado = row.V_cm_ajustado;
            liquidacion.V_dm_ajustado = row.V_dm_ajustado;
            liquidacion.V_om_ajustado = row.V_om_ajustado;
            liquidacion.V_ppond_ajustado = row.V_ppond_ajustado;
            liquidacion.V_tpond_ajustado = row.V_tpond_ajustado;
            liquidacion.V_activa_ajustado = row.V_activa_ajustado;
            liquidacion.V_reactiva_pen_ajustado = row.V_reactiva_pen_ajustado;
            liquidacion.V_consumo_energia_ajustado = row.V_consumo_energia_ajustado;
            liquidacion.V_contribucion = row.V_contribucion;
            liquidacion.V_sobretasa = row.V_sobretasa;
            liquidacion.V_adcn = row.V_adcn;
            liquidacion.V_iapb = row.V_iapb;
            liquidacion.V_iap_ajuste = row.V_iap_ajuste;
            liquidacion.V_rrnt = row.V_rrnt;
            liquidacion.V_arnt = row.V_arnt;
            liquidacion.V_rfntica = row.V_rfntica;
            liquidacion.V_afntica = row.V_afntica;
            liquidacion.V_rrntbmb = row.V_rrntbmb;
            liquidacion.V_otros_total = row.V_otros_total;
            liquidacion.V_neto_factura = row.V_neto_factura;
            liquidacion.V_compensacion = row.V_compensacion;
            liquidacion.V_arntbmb = row.V_arntbmb;
            liquidacion.Tipo_factura = row.Tipo_factura;
            liquidacion.V_sgcv = row.V_sgcv;
            liquidacion.Factor_m = row.Factor_m;
            liquidacion.V_asgcv = row.V_asgcv;
            liquidacion.Fecha_registro = dateTime;
            liquidacion.Estado = estado;
            liquidacion.Error = errorMessage;
            liquidacion.Usuario = username;
            return liquidacion;
        }

        public GPLiquidacion GetSettlementById(string fechaFacturacion, string version, int factura_id)
        {
            List<GPLiquidacion> items = context.GPLiquidacionEntity
                .Where(s => s.Fechafacturacion == fechaFacturacion &&
                    s.Version == version &&
                    s.Factura_id == factura_id
                ).ToList();
            if (items.Count > 0)
                return items[0];
            else
                return null;
        }

        public List<GPLiquidacion> GetSettlementByPeriod(string fechaFacturacion)
        {
            List<GPLiquidacion> items = context.GPLiquidacionEntity
                .Where(s => s.Fechafacturacion.Substring(0,6) == fechaFacturacion 
                ).ToList();
            return items;
        }

        public void DeleteSettlementById(string fechaFacturacion, string version, int factura_id)
        {
            GPLiquidacion itemLocal = GetSettlementById(fechaFacturacion, version, factura_id);
            context.Remove(itemLocal);
            context.SaveChanges();
        }

        public List<GPConfiguracion> GetConfigurationByName(string name)
        {
            List<GPConfiguracion> confs = context.GPConfiguracionEntity
                .Where(s => s.Nombre.ToUpper().Contains(name)).ToList();

            return confs;
        }

        public double GetCalculatedValue(string sql)
        {
            System.Data.Common.DbConnection connection = context.Database.GetDbConnection();
            double results = connection.Query<double>(sql).FirstOrDefault();
            return Math.Round(results,0);
        }

        public void UpdateStatus(GPLiquidacion liq, int status, string errorMessage, DateTime dateTime, string username)
        {
            //Set original Values To Configuration
            context.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified &&
                            typeof(GPConfiguracion).IsAssignableFrom(x.Entity.GetType()))
                .ToList()
                .ForEach(entry => {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                });

            liq.Estado = status;
            liq.ultimo_error = errorMessage;
            context.GPLiquidacionEntity.Update(liq);
            context.SaveChanges();

            GPLiquidacionLog log = CreateGPLiquidacionLogRow(liq, liq.Estado, errorMessage, dateTime, username);
            log.Estado = status;
            context.GPLiquidacionLogEntity.Add(log);
            context.SaveChanges();
        }

        public string getNITMunicipio(int municipio)
        {
            return context.GPMunicipioEntity.Where( s => s.Municipio == municipio).FirstOrDefault().Nit;
        }

        public dynamic GetLoadedInvoiceByCompany()
        {
            System.Data.Common.DbConnection connection = context.Database.GetDbConnection();
            return connection.Query<dynamic>("SELECT codigo, STRING_AGG(distinct nombre, ', ' ORDER BY nombre) AS group_ids FROM gp_configuracion gc GROUP BY codigo");
            
        }

        public List<GPLiquidacion> GetPendingInvoiceLocalByName(string period, int status, string name)
        {
            List<GPLiquidacion> items = context.GPLiquidacionEntity
                    .Where(s =>
                        s.Fechafacturacion.Substring(0, 6) == period &&
                        s.Estado == status &&
                        s.Cliente_nombre.Substring(0, 9).ToUpper() == name.Substring(0,9).ToUpper()).ToList();

            return items;
        }


        public GPLiquidacionConcepto GetLiquidacionConceptoById(string fechafacturacion, int factura_id, string concepto)
        {
            return context.GPLiquidacionConceptoEntity.Where(s => s.Fechafacturacion == fechafacturacion && s.Factura_id == factura_id && s.Concepto.Trim().Equals(concepto.Trim())).FirstOrDefault();
        }

        public void UpdateLiquidacionConceptoById(List<GPLiquidacionConcepto> items)
        {
            NpgsqlBulkUploader uploader = new NpgsqlBulkUploader(context);
            uploader.Insert(items, InsertConflictAction.DoNothing());
            uploader.Update(items);
        }

        public List<GPSaldo> GetPaymentDifference(string code)
        {
            List<GPSaldo> saldo = context.GPSaldoEntity
                .Where(s => s.CodigoIngreso.Equals(code)).ToList();

            return saldo;
        }

        public void SaveSaldo(List<GPSaldo> saldoList)
        { 
            NpgsqlBulkUploader uploader = new NpgsqlBulkUploader(context);
            try
            {
                uploader.Insert(saldoList, InsertConflictAction.DoNothing());
                context.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        public string GetProfileUser(string username)
        {
            username = username.Split("@")[0];
            GPUsuario usuario = context.GPUsuarioEntity.Where(s => s.Usuario.Equals(username)).FirstOrDefault();
            if (usuario == null)
            {
                return "";
            } else
            {
                return usuario.Perfil;
            }
            
        }

        public List<GPConfiguracion> ListConfiguration()
        {
            return context.GPConfiguracionEntity.Where( s => !s.Codigo.Equals("")).ToList();
        }

        public GPConfiguracion DeleteConfiguration(GPConfiguracion conf)
        {
            context.GPConfiguracionEntity.Remove(conf);
            context.SaveChanges();
            return conf;
        }

        public GPConfiguracion UpdateConfiguration(GPConfiguracion conf)
        {
            try
            {
                context.GPConfiguracionEntity.Update(conf);
                context.SaveChanges();

                return conf;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public GPConfiguracion CreateConfiguracion(GPConfiguracion conf)
        {
            try
            {
                context.GPConfiguracionEntity.Add(conf);
                context.SaveChanges();

                return conf;
            } catch (Exception ex)
            {
                return null;
            }
            
        }
    }
}