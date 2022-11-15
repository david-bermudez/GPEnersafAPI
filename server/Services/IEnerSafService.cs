using GpEnerSaf.Models.BD;
using Newtonsoft.Json.Linq;
using project.Models.DTO;
using System.Collections.Generic;

namespace GpEnerSaf.Services
{
    public interface IEnerSafService
    {
        public JObject GetLoadedInvoice(InvoiceDTO param);
        public List<GPLiquidacion> SaveLoadedInvoice(IEnumerable<dynamic> rows, string username);
        public List<GPLiquidacion> GetPendingInvoice(InvoiceDTO param);
        public List<GPLiquidacion> ReloadPendingInvoice(InvoiceDTO param);
        public JObject ValidatePendingInvoice(InvoiceDTO param);
        public List<InvoiceItemDTO> GetPendingInvoiceItems(InvoiceDTO param);
        public JObject GenerateInvoiceAcconting(InvoiceGroupDTO param);
        public List<GPLiquidacion> GetPendingInvoiceLocal(InvoiceDTO param);
        IEnumerable<dynamic> GetLoadedInvoiceByCompany();
        public List<GPLiquidacion> GetPendingInvoiceLocalByName(InvoiceDTO param);
        IEnumerable<Payment> GetPayments(InvoiceDTO param);
        JObject GenerateResponse(string message);
        InvoiceGroupDTO GetLoadedInvoices(InvoiceGroupDTO param, string v);
        JObject SaveLoadedInvoices(InvoiceDetailGroupDTO data);
        JObject GeneratePayableAcconting(InvoiceDetailGroupDTO data, string username);
        GPLiquidacion GetInvoiceLocal(InvoiceDTO param);
        List<MenuDTO> GenerateMenuInvoices(InvoiceDTO param);
        GPConfiguracion CreateConfiguracion(GPConfiguracion conf);
        GPConfiguracion UpdateConfiguration(GPConfiguracion conf);
        GPConfiguracion DeleteConfiguration(GPConfiguracion conf);
        List<GPConfiguracion> ListConfiguration();
    }
}