using GpEnerSaf.Models.BD;
using System.Collections.Generic;

namespace project.Models.DTO
{
    public class InvoiceGroupDTO
    {
        public double TotalInvoice { set; get; }
        public double TotalPayment { set; get; }
        public double Difference{ set; get; }
        public double PendingAmount { set; get; }
        public List<Payment> Payments { get; set; }
        public List<InvoiceDTO> Invoices { get; set; }
        public string Period { set; get; }
        public string GroupName { set; get; }

    }
            
}
