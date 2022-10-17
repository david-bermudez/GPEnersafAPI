using GpEnerSaf.Models.BD;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json.Linq;
using project.Models.DTO;
using System;
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
        public JObject GenerateInvoiceAcconting(InvoiceDTO param);
        public List<GPLiquidacion> GetPendingInvoiceLocal(InvoiceDTO param);
        IEnumerable<dynamic> GetLoadedInvoiceByCompany();
        public List<GPLiquidacion> GetPendingInvoiceLocalByName(InvoiceDTO param);
        IEnumerable<Payment> GetPayments(InvoiceDTO param);
        double GetPreviousAmount(InvoiceDTO param);
        JObject GenerateResponse(string message);
        InvoiceGroupDTO GetLoadedInvoices(InvoiceGroupDTO param, string v);
        void SaveLoadedInvoices(InvoiceGroupDTO data);
        void GenerateReceivableAcconting(InvoiceGroupDTO data, string username);

        GPLiquidacion GetInvoiceLocal(InvoiceDTO param);
    }
}