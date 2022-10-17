using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models.DTO
{
    public class InvoiceItemDTO
    {
        public int Id { set; get; }

        public string NombreCliente { set; get; }

        public string TipoAsiento { set; get; }

        public string Description { set; get; }

        public double Value { set; get; }

        public double SuggestedValue { set; get; }

        public double PaidValue { set; get; }
    }
}
