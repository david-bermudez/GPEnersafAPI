using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GpEnerSaf.Services;
using Newtonsoft.Json.Linq;
using project.Models.DTO;
using System.Collections.Generic;
using GpEnerSaf.Models.BD;
using System.Linq;
using System.Security.Claims;
using System;

namespace GpEnerSaf.Controllers
{
    [Route("api/EnerSaf/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EnerSafController : Controller
    {
        private readonly IEnerSafService _enerSafService;

        public EnerSafController(IEnerSafService enerSafService)
        {
            this._enerSafService = enerSafService;
        }

        [HttpPost(Name = "GetPendingInvoice")]
        public List<GPLiquidacion> GetPendingInvoice([FromBody] JObject data)
        {
            InvoiceDTO param = new InvoiceDTO();
            param.Period = data.GetValue("Periodo").ToString();
            param.Username = GetLoggedUser();

            return _enerSafService.GetPendingInvoice(param);
        }

        [HttpPost(Name = "ReloadPendingInvoice")]
        public List<GPLiquidacion> ReloadPendingInvoice([FromBody] JObject data)
        {
            InvoiceDTO param = new InvoiceDTO();
            param.FechaFacturacion = data.GetValue("Fechafacturacion").ToString();
            param.Version = data.GetValue("Version").ToString();
            param.Factura_id = Int32.Parse(data.GetValue("Factura_id").ToString());
            param.Username = GetLoggedUser();

            return _enerSafService.ReloadPendingInvoice(param);
        }

        [HttpPost(Name = "GetPendingInvoiceItems")]
        public List<InvoiceItemDTO> GetPendingInvoiceItems([FromBody] JObject data)
        {
            InvoiceDTO param = new InvoiceDTO();
            param.FechaFacturacion = data.GetValue("Fechafacturacion").ToString();
            param.Version = data.GetValue("Version").ToString();
            param.Factura_id = Int32.Parse(data.GetValue("Factura_id").ToString());
            param.Username = GetLoggedUser();

            return _enerSafService.GetPendingInvoiceItems(param);
        }

        [HttpPost(Name = "GetErrorInvoice")]
        public JObject GetErrorInvoice([FromBody] JObject data)
        {
            InvoiceDTO param = new InvoiceDTO();
            param.FechaFacturacion = data.GetValue("Fechafacturacion").ToString();
            param.Version = data.GetValue("Version").ToString();
            param.Factura_id = Int32.Parse(data.GetValue("Factura_id").ToString());
            param.Username = GetLoggedUser();
            GPLiquidacion item = _enerSafService.GetInvoiceLocal(param);
            if (item.ultimo_error != null && item.ultimo_error.Equals(""))
            {
                item.ultimo_error = "Factura sin inconsistencias";
            }
            return _enerSafService.GenerateResponse(item.ultimo_error);
        }

        [HttpPost(Name = "ValidatePendingInvoice")]
        public JObject ValidatePendingInvoice([FromBody] InvoiceGroupDTO data)
        {
            foreach (InvoiceDTO param in data.Invoices)
            {
                param.FechaFacturacion = param.FechaFacturacion;
                param.Version = param.Version;
                param.Factura_id = param.Factura_id;
                param.Username = GetLoggedUser();
                _enerSafService.ValidatePendingInvoice(param);

                break;
            }

            return _enerSafService.GenerateResponse("Proceso terminado correctamente");
        }

        [HttpPost(Name = "GenerateInvoiceAcconting")]
        public JObject GenerateInvoiceAcconting([FromBody] JObject data)
        {
            InvoiceDTO param = new InvoiceDTO();
            param.FechaFacturacion = data.GetValue("Fechafacturacion").ToString();
            param.Version = data.GetValue("Version").ToString();
            param.Factura_id = Int32.Parse(data.GetValue("Factura_id").ToString());
            param.Interfase = data.GetValue("Interfase").ToString();
            param.Username = GetLoggedUser();

            return _enerSafService.GenerateInvoiceAcconting(param);
        }

        [HttpPost(Name = "GetLoadedInvoiceByCompany")]
        public JObject GetLoadedInvoiceByCompany()
        {
            JObject obj = Commons.CommonsUtil.Convert("data", _enerSafService.GetLoadedInvoiceByCompany());
            return obj;
        }

        [HttpPost(Name = "GetLoadedInvoices")]
        public InvoiceGroupDTO GetLoadedInvoices([FromBody] JObject param)
        {
            InvoiceGroupDTO invoiceGroupDTO = new InvoiceGroupDTO();
            invoiceGroupDTO.GroupName = param.GetValue("group_ids").ToString();
            invoiceGroupDTO.Period = param.GetValue("period").ToString();

            return _enerSafService.GetLoadedInvoices(invoiceGroupDTO, GetLoggedUser());
        }

        [HttpPost(Name = "SaveLoadedInvoices")]
        public JObject SaveLoadedInvoices([FromBody] InvoiceGroupDTO data)
        {
            _enerSafService.SaveLoadedInvoices(data);
            return _enerSafService.GenerateResponse("Datos guardados correctamente");
        }

        [HttpPost(Name = "GeneratePayableAcconting")]
        public JObject GeneratePayableAcconting([FromBody] InvoiceGroupDTO data)
        {
            return _enerSafService.GenerateReceivableAcconting(data, GetLoggedUser());
        }

        [HttpPost(Name = "GetVersion")]
        [AllowAnonymous]
        public IActionResult GetVersion()
        {
            var resp = Startup.StaticConfig.GetSection("APP").GetSection("Version").Value;
            return Json(resp);
        }

        public string GetLoggedUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            List<Claim> claims = identity.Claims.ToList();
            string username = claims[1].Value;

            return username;
        }
    }

}
