using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models.DTO
{
    public class InvoiceDTO
    {
        public int Factura_id { set; get; }

        public string FechaFacturacion { set; get; }

        public string Version { set; get; }

        public string Description { set; get; }

        public double InvoiceAmount { set; get; }

        public double InvoiceAmountModified { set; get; }

        public string Username { set; get; }

        public string Period { set; get; }

        public int Status { set; get; }

        public string Interfase { set; get; }

        public bool IsReverse { set; get; }

        public List<InvoiceItemDTO> detail { set; get; }
    }
}
