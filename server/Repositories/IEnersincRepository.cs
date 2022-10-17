using GpEnerSaf.Models.BD;
using System;
using System.Collections.Generic;

namespace GpEnerSaf.Repositories
{
    public interface IEnersincRepository
    {
        IEnumerable<dynamic> GetBatchPendingInvoiceList(string period);

        IEnumerable<dynamic> GetBatchPendingInvoiceItem(string fechafacturacion, string version, string factura_id);
    }
}