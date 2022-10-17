using GpEnerSaf.Repositories;
using GpEnerSaf.Commons;
using GpEnerSaf.Data;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using GpEnerSaf.Models.BD;
using System.Linq;
using project.Models.DTO;

namespace GpEnerSaf.Services
{
    public class EnersafServiceImpl : IEnerSafService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGPRepository _gpRepository;
        private readonly IEnersincRepository _enersincECPRepository;
        private readonly ISAFIRORepository _safiroRepository;

        public EnersafServiceImpl(DbGpContext context, 
            IEnersincRepository enersincECPRepository,
            IGPRepository gpRepository,
            ISAFIRORepository safiroRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            this._webHostEnvironment = webHostEnvironment;
            this._enersincECPRepository = enersincECPRepository;
            this._gpRepository = gpRepository;
            this._safiroRepository = safiroRepository;
        }

        /**
        * Metodo que Carga las facturas de las tablas propias
        */
        public JObject GetLoadedInvoice(InvoiceDTO param)
        {
            IEnumerable<dynamic> rows = _enersincECPRepository.GetBatchPendingInvoiceList(param.Period);
            return CommonsUtil.Convert("data", rows);
        }

        /**
        * Metodo que salva las facturas del batch contable a las tablas propias
        */
        public List<GPLiquidacion> SaveLoadedInvoice(IEnumerable<dynamic> rows, string username)
        {
            return _gpRepository.SaveSettlement(rows, username);
        }

        /***
        /**
         * Metodo que busca las facturas del batch contable
         */
        public List<GPLiquidacion> GetPendingInvoice(InvoiceDTO param)
        {
            IEnumerable<dynamic> rows = null;
            if (param.Period== null)
            {
                param.Period = DateTime.Now.ToString("yyyyMM");
            }

            _gpRepository.DeletePendingInvoiceLocal(param.Period, 1); //Se eliminan las facturas en estado 1 para refrescar
            rows = _enersincECPRepository.GetBatchPendingInvoiceList(param.Period); //Se traen las facturas de ENERSINC
            SaveLoadedInvoice(rows, param.Username); //Se guardan las facturas traidas de ENERSINC
            
            return _gpRepository.GetPendingInvoiceLocal(param.Period, 1);

        }

        public List<GPLiquidacion> GetPendingInvoiceLocal(InvoiceDTO param)
        {
            return _gpRepository.GetPendingInvoiceLocal(param.Period, param.Status);
        }

        public List<GPLiquidacion> ReloadPendingInvoice(InvoiceDTO param)
        {
            _gpRepository.DeleteSettlementById(param.FechaFacturacion, param.Version, param.Factura_id);
            IEnumerable<dynamic> rows = _enersincECPRepository.GetBatchPendingInvoiceItem(param.FechaFacturacion, param.Version, param.Factura_id.ToString());
            return SaveLoadedInvoice(rows, param.Username);   
        }

        /**
        * Metodo que valida las facturas del batch contable
        */
        public JObject ValidatePendingInvoice(InvoiceDTO item)
        {
            
            GPLiquidacion liq = _gpRepository.GetSettlementById(item.FechaFacturacion, item.Version, item.Factura_id);
            List<GPConfiguracion> confs = GetInvoiceConfigurationDetail(liq.Cliente_nombre.Substring(0, 5).ToUpper());
            confs = confs.FindAll(x => x.Tipo.Equals("CN")).ToList();

            DateTime processDate = DateTime.Now;
            string processStatus = null;
            string lastErrorMessage = null;
            bool foundError = false;

            foreach (GPConfiguracion conf in confs)
            {
                double calculatedValue = GetCalculatedValue(liq, conf);
                if (calculatedValue != 0)
                {
                    processStatus = _safiroRepository.ValidateInvoice(liq, conf, calculatedValue);
                    if (processStatus == null)
                    {
                        _gpRepository.UpdateStatus(liq, 2, "", processDate, item.Username);
                    }
                    else
                    {
                        _gpRepository.UpdateStatus(liq, 1, processStatus, processDate, item.Username);
                        lastErrorMessage = processStatus;
                        foundError = foundError || true;
                    }
                }
            }

            return GenerateResponse(lastErrorMessage);
        }

        private double GetCalculatedValue(GPLiquidacion liq, GPConfiguracion conf)
        {
            string sql = conf.Formula;
            sql = sql.Replace("{fechafacturacion}", liq.Fechafacturacion);
            sql = sql.Replace("{version}", liq.Version);
            sql = sql.Replace("{factura_id}", liq.Factura_id.ToString());

            return _gpRepository.GetCalculatedValue(sql);
        }

        /**
         * Metodo que lista los items de las facturas pendientes por cargar.
        **/
        public List<InvoiceItemDTO> GetPendingInvoiceItems(InvoiceDTO param)
        {
            GPLiquidacion liq = _gpRepository.GetSettlementById(param.FechaFacturacion, param.Version, param.Factura_id);
            List<GPConfiguracion> confs = _gpRepository.GetConfigurationByName(liq.Cliente_nombre.Substring(0, 5).ToUpper());
            confs = confs.FindAll(x => x.Tipo.Equals("CN")).ToList();
            List<InvoiceItemDTO> items = new List<InvoiceItemDTO>();
            
            int id = 1;
            foreach (GPConfiguracion conf in confs)
            {
                InvoiceItemDTO item = new InvoiceItemDTO();
                double value = GetCalculatedValue(liq, conf);
                if (conf.Tipo_asiento.Equals("D") && value != 0) {
                    item.Id = id++;
                    item.Description = conf.Concepto;
                    item.Value = value;
                    item.TipoAsiento = conf.Tipo_asiento;
                    items.Add(item);
                }
            }
            return items;
        }

        /**
         * Metodo que carga las facturas locales al sistema contable
         */
        public JObject GenerateInvoiceAcconting(InvoiceDTO param)
        {
            GPLiquidacion liq = _gpRepository.GetSettlementById(param.FechaFacturacion, param.Version, param.Factura_id);
            List<GPConfiguracion> confs = GetInvoiceConfigurationDetail(liq.Cliente_nombre)
                .Where( s => s.Tipo.Equals(param.Interfase)).ToList();

            confs.Sort((a, b) => a.Sort_order.CompareTo(b.Sort_order));

            List<InvoiceItemDTO> items = new List<InvoiceItemDTO>();
            DateTime dateTime = DateTime.Now;  
            string processStatus = null;
            string lastErrorMessage = null;
            bool foundError = false;

            foreach (GPConfiguracion conf in confs)
            {
                double calculatedValue = GetCalculatedValue(liq, conf);
                if (calculatedValue != 0) {
                    ReplaceValues(conf, liq);
                    processStatus = _safiroRepository.GenerateInvoiceAcconting(liq, conf, calculatedValue);

                }
            }

            if (processStatus == null)
            {
                _gpRepository.UpdateStatus(liq, 3, "", dateTime, param.Username);
            }
            else
            {
                _gpRepository.UpdateStatus(liq, 2, processStatus, dateTime, param.Username);
                lastErrorMessage = processStatus;
                foundError = foundError || true;
            }

            //No se encontro con configuracion
            if (confs.Count == 0)
            {
                processStatus = "No existe parametrizacion para el cliente " + liq.Cliente_nombre;
            }

            return GenerateResponse(lastErrorMessage);
        }

        private void ReplaceValues(GPConfiguracion conf, GPLiquidacion liq)
        {
            //PARAM ID_MUNICIPIO
            if (conf.Codniva2 != null && conf.Codniva1.Contains("{ID_MUNICIPIO}"))
            {
                conf.Codniva1 = _gpRepository.getNITMunicipio(Int32.Parse(liq.Municipio_id));
            }
            if (conf.Codniva2 != null && conf.Codniva2.Contains("{ID_MUNICIPIO}"))
            {
                conf.Codniva2 = _gpRepository.getNITMunicipio(Int32.Parse(liq.Municipio_id));
            }
            if (conf.Codniva3 != null && conf.Codniva3.Contains("{ID_MUNICIPIO}"))
            {
                conf.Codniva3 = _gpRepository.getNITMunicipio(Int32.Parse(liq.Municipio_id));
            }

            //PARAM YEAR
            string actualYear = DateTime.Now.ToString("YYYY");
            if (conf.Codniva1 != null && conf.Codniva1.Contains("{YEAR}"))
            {
                conf.Codniva1 = actualYear;
            }
            if (conf.Codniva2 != null && conf.Codniva2.Contains("{YEAR}"))
            {
                conf.Codniva2 = actualYear;
            }
            if (conf.Codniva3 != null && conf.Codniva3.Contains("{YEAR}"))
            {
                conf.Codniva3 = actualYear;
            }

        }

        public JObject GenerateResponse( string message)
        {
            JObject json;

            if (message == null)
            {
                json = JObject.FromObject(new
                {
                    code = "200",
                    mensaje = "Proceso realizado correctamente"
                });
            }
            else
            {
                json = JObject.FromObject(new
                {
                    code = "400",
                    mensaje = message
                });
            }
            return json;
        }

        public IEnumerable<dynamic> GetLoadedInvoiceByCompany()
        {
            return _gpRepository.GetLoadedInvoiceByCompany();
        }

        public List<GPLiquidacion> GetPendingInvoiceLocalByName(InvoiceDTO param)
        {
            return _gpRepository.GetPendingInvoiceLocalByName(param.Period, param.Status, param.Description);
        }

        public IEnumerable<Payment> GetPayments(InvoiceDTO param)
        {
            return _safiroRepository.GetPayments(param.Period, param.Description);
        }

        public double GetPreviousAmount(InvoiceDTO param)
        {
            return _gpRepository.GetPreviousAmount(param);
        }

        public List<GPConfiguracion> GetInvoiceConfigurationDetail(string name)
        {
            List<GPConfiguracion> confs = _gpRepository.GetConfigurationByName(name.Substring(0, 9).ToUpper());
            return confs;
        }

        public void GenerateReceivableAcconting(InvoiceGroupDTO data, string username)
        {
            DateTime dateTime = DateTime.Now;
            string processStatus = null;
            string lastErrorMessage = null;
            bool foundError = false;

            foreach (InvoiceDTO invoice in data.Invoices) {
                GPLiquidacion liq = _gpRepository.GetSettlementById(invoice.FechaFacturacion, invoice.Version, invoice.Factura_id);
                List<GPConfiguracion> confs = GetInvoiceConfigurationDetail(liq.Cliente_nombre);
                confs.Sort((a, b) => a.Sort_order.CompareTo(b.Sort_order));

                foreach (GPConfiguracion conf in confs)
                {
                    double calculatedValue = GetCalculatedValue(liq, conf);
                    if (calculatedValue != 0)
                    {
                        ReplaceValues(conf, liq);
                        processStatus = _safiroRepository.GeneratePayableAcconting(liq, conf, calculatedValue);
                    }
                }

                if (processStatus == null)
                {
                    _gpRepository.UpdateStatus(liq, 4, "", dateTime, username);
                }
                else
                {
                    _gpRepository.UpdateStatus(liq, 3, processStatus, dateTime, username);
                    lastErrorMessage = processStatus;
                    foundError = foundError || true;
                }
            }

        }

        public InvoiceGroupDTO GetLoadedInvoices(InvoiceGroupDTO paramGroup, string username)
        {
            InvoiceGroupDTO invoiceGroupDTO = new InvoiceGroupDTO();
            invoiceGroupDTO.Invoices = new List<InvoiceDTO>();
            invoiceGroupDTO.Payments = new List<Payment>();
            invoiceGroupDTO.GroupName = paramGroup.GroupName;
            invoiceGroupDTO.Period = paramGroup.Period;

            InvoiceDTO paramInvoice = new InvoiceDTO();
            paramInvoice.Description = invoiceGroupDTO.GroupName;
            paramInvoice.Period = invoiceGroupDTO.Period;
            paramInvoice.Username = username;

            double totalPayments = 0;
            double totalInvoices = 0;
            double previousAmount = GetPreviousAmount(paramInvoice);
            invoiceGroupDTO.PendingAmount = previousAmount;

            //Se obtiene el total de pagos realizados por el cliente
            foreach (string group_id in paramGroup.GroupName.Split(","))
            {
                invoiceGroupDTO.Payments.AddRange(GetPayments(paramInvoice));
                totalPayments += invoiceGroupDTO.Payments.Sum(s => s.PaymentValue);
            }
            invoiceGroupDTO.TotalPayment = totalPayments;

            //Se obtiene el total de los montos a recaudar
            foreach (string group_id in paramGroup.GroupName.Split(","))
            {
                paramInvoice.Status = 3;
                paramInvoice.Description = group_id;
                List<GPLiquidacion> gPLiquidacions = GetPendingInvoiceLocalByName(paramInvoice);
                foreach (GPLiquidacion item in gPLiquidacions)
                {
                    InvoiceDTO invoice = new InvoiceDTO();
                    invoice.Factura_id = item.Factura_id;
                    invoice.FechaFacturacion = item.Fechafacturacion;
                    invoice.Version = item.Version;

                    invoice.Description = item.Frontera + " - " + item.Municipio_nombre;
                    invoice.detail = new List<InvoiceItemDTO>();

                    List<GPConfiguracion> items = GetInvoiceConfigurationDetail(item.Cliente_nombre)
                        .Where(s => s.Tipo_asiento.Equals("C") && s.Tipo.Equals("RE")).ToList();

                    foreach (GPConfiguracion config in items)
                    {
                        InvoiceItemDTO itemDetailDTO = new InvoiceItemDTO();
                        itemDetailDTO.Description = config.Concepto;

                        if (totalPayments > 0)
                        {
                            double calculatedValue = GetCalculatedValue(item, config);
                            GPLiquidacionConcepto gPLiquidacionConcepto = _gpRepository.GetLiquidacionConceptoById(item.Fechafacturacion, item.Factura_id, config.Concepto);
                            itemDetailDTO.Value = calculatedValue;
                            
                            if (gPLiquidacionConcepto != null )
                            {
                                itemDetailDTO.SuggestedValue = gPLiquidacionConcepto.Valor;
                            }
                            else
                            {
                                if (totalPayments > itemDetailDTO.Value)
                                {
                                    itemDetailDTO.SuggestedValue = itemDetailDTO.Value;
                                }
                                else
                                {
                                    itemDetailDTO.SuggestedValue = totalPayments;
                                }
                            }

                            totalPayments = totalPayments - invoice.InvoiceAmount;
                        }


                        invoice.detail.Add(itemDetailDTO);
                    }

                    invoiceGroupDTO.Invoices.Add(invoice);
                }
            }
            invoiceGroupDTO.TotalInvoice = totalInvoices;
            invoiceGroupDTO.Difference = totalPayments - totalInvoices;

            return invoiceGroupDTO;
        }

        public void SaveLoadedInvoices(InvoiceGroupDTO data)
        {
            List<GPLiquidacionConcepto> items = new List<GPLiquidacionConcepto>();
            foreach (InvoiceDTO invoice in data.Invoices)
            {
                foreach ( InvoiceItemDTO item in invoice.detail)
                {
                    GPLiquidacionConcepto itemConcepto = new GPLiquidacionConcepto();
                    itemConcepto.Factura_id = invoice.Factura_id;
                    itemConcepto.Fechafacturacion = invoice.FechaFacturacion;
                    itemConcepto.Version = invoice.Version;
                    itemConcepto.Valor = item.SuggestedValue;
                    itemConcepto.Concepto = item.Description;
                    
                    items.Add(itemConcepto);
                }
            }

            _gpRepository.UpdateLiquidacionConceptoById(items);
        }

    }
}