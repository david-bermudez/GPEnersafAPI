using GpEnerSaf.Models.BD;
using Newtonsoft.Json.Linq;
using project.Models.DTO;
using System;
using System.Collections.Generic;

namespace GpEnerSaf.Repositories
{
    public interface IGPRepository
    {
        List<GPConfiguracion> GetConfigurationByName(string name);
        List<GPLiquidacion> SaveSettlement(IEnumerable<dynamic> rows, string username);
        List<GPLiquidacion> GetPendingInvoiceLocal(string period, int status);
        void DeletePendingInvoiceLocal(string period, int status);
        GPLiquidacion GetSettlementById(string fechaFacturacion, string version, int factura_id);
        List<GPLiquidacion> GetSettlementByPeriod(string fechaFacturacion);
        void DeleteSettlementById(string fechaFacturacion, string version, int factura_id);
        double GetCalculatedValue(string sql);
        void UpdateStatus(GPLiquidacion liq, int status, string errorMessage, DateTime dateTime, string username);
        string getNITMunicipio(int municipio_id);
        dynamic GetLoadedInvoiceByCompany();
        public List<GPLiquidacion> GetPendingInvoiceLocalByName(string period, int status, string name);
        GPLiquidacionConcepto GetLiquidacionConceptoById(string fechafacturacion, int factura_id, string concepto);
        void UpdateLiquidacionConceptoById(List<GPLiquidacionConcepto> items);
        List<GPSaldo> GetPaymentDifference(string code);
        void SaveSaldo(List<GPSaldo> saldoList);
        string GetProfileUser(string username);
        List<GPConfiguracion> ListConfiguration();
        GPConfiguracion DeleteConfiguration(GPConfiguracion conf);
        GPConfiguracion UpdateConfiguration(GPConfiguracion conf);
        GPConfiguracion CreateConfiguracion(GPConfiguracion conf);
    }
}