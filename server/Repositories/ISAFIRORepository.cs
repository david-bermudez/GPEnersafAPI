using GpEnerSaf.Models;
using GpEnerSaf.Models.BD;
using project.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpEnerSaf.Repositories
{
    public interface ISAFIRORepository
    {
        string ValidateInvoice(GPLiquidacion liquidacion, GPConfiguracion conf, double calculatedValue);

        string GenerateInvoiceAcconting(GPLiquidacion liquidacion, GPConfiguracion conf, double calculatedValue);

        string GeneratePayableAcconting(GPLiquidacion liquidacion, GPConfiguracion conf, double calculatedValue);

        List<Payment> GetPayments(string period, string prefixCompany);
    }
}