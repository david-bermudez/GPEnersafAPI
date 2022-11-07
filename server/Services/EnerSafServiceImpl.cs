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
using Microsoft.OData.UriParser;

namespace GpEnerSaf.Services
{
    public class EnersafServiceImpl : IEnerSafService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGPRepository _gpRepository;
        private readonly IEnersincRepository _enersincECPRepository;
        private readonly ISAFIRORepository _safiroRepository;

        private Dictionary<string, string> menuOptions = new Dictionary<string, string>
        {
            { "Provisionar", "CP" },
            { "Contabilizar", "CN" },
            { "Impuesto", "IM" },
            { "Presupuesto Ingreso", "PPI" },
            { "Presupuesto Egreso", "PPE" },
            { "Reversion", "RV" }
        };

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
            if (param.Period== null || param.Period.Equals(""))
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

        public GPLiquidacion GetInvoiceLocal(InvoiceDTO param)
        {
            return _gpRepository.GetSettlementById(param.FechaFacturacion, param.Version, param.Factura_id);
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
            List<GPConfiguracion> confs = GetInvoiceConfigurationDetail(liq.Cliente_nombre.ToUpper());
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
                    ReplaceValues(conf, liq);
                    processStatus = _safiroRepository.ValidateInvoice(liq, conf, calculatedValue);
                    if (processStatus != null)
                    {
                        lastErrorMessage = "Factura: " + liq.Factura_id + " Concepto:" + conf.Concepto  + " " + processStatus;
                        foundError = foundError || true;
                        break;
                    }
                }
            }

            if (lastErrorMessage == null)
            {
                liq.Avance = "VA";
                _gpRepository.UpdateStatus(liq, 1, "", processDate, item.Username);
            } else
            {
                liq.Avance = "NA";
                _gpRepository.UpdateStatus(liq, 1, lastErrorMessage, processDate, item.Username);

            }

            return GenerateResponse(lastErrorMessage);
        }

        private double GetCalculatedValue(GPLiquidacion liq, GPConfiguracion conf)
        {
            string sql = conf.Formula;
            sql = sql.Replace("{fechafacturacion}", liq.Fechafacturacion);
            sql = sql.Replace("{version}", liq.Version);
            sql = sql.Replace("{factura_id}", liq.Factura_id.ToString());
            try
            {
                return _gpRepository.GetCalculatedValue(sql);
            } catch (Exception)
            {
                return 0;
            }
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
            int actualProgress = GetProgressNumber(liq.Avance);
            int validateProgress = GetProgressNumber(param.Interfase);

            if (validateProgress == actualProgress + 1)
            {
                foreach (GPConfiguracion conf in confs)
                {
                    double calculatedValue = GetCalculatedValue(liq, conf);
                    if (calculatedValue != 0)
                    {
                        ReplaceValues(conf, liq);
                        processStatus = _safiroRepository.GenerateInvoiceAcconting(liq, conf, calculatedValue);
                        if (processStatus != null)
                        {
                            lastErrorMessage = "Factura: " + liq.Factura_id + " Concepto: " + conf.Concepto + processStatus;
                            break;
                        }
                    }
                }

                if (processStatus == null)
                {
                    liq.Avance = param.Interfase;
                    
                    if (liq.Avance.Equals("PP"))
                    {
                        _gpRepository.UpdateStatus(liq, 2, "", dateTime, param.Username);
                    } else
                    {
                        _gpRepository.UpdateStatus(liq, 1, "", dateTime, param.Username);
                    }
                }
                else
                {
                    liq.ultimo_error = lastErrorMessage;
                    _gpRepository.UpdateStatus(liq, 1, lastErrorMessage, dateTime, param.Username);
                    foundError = foundError || true;
                }

                //No se encontro con configuracion
                if (confs.Count == 0)
                {
                    processStatus = "No existe parametrizacion para el cliente " + liq.Cliente_nombre;
                }

                return GenerateResponse(lastErrorMessage);
            } else
            {
                return GenerateResponse("No es posible usar esta accion");
            }
            
        }

        private int GetProgressNumber(string progress)
        {
            if (progress == null )
            {
                return 0;
            }
            else if (progress.Equals("CP"))
            {
                return 2;
            } 
            else if (progress.Equals("CN"))
            {
                return 3;
            }
            else if (progress.Equals("IM"))
            {
                return 4;
            }
            else if (progress.Equals("PPI"))
            {
                return 5;
            }
            else if (progress.Equals("PPE"))
            {
                return 6;
            }
            else if (progress.Equals("VA"))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private void ReplaceValues(GPConfiguracion conf, GPLiquidacion liq)
        {
            //PARAM ID_MUNICIPIO
            if (conf.Codniva1 != null && conf.Codniva1.Contains("{ID_MUNICIPIO}"))
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
            string actualYear = DateTime.Now.Year.ToString();
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
            List<Payment> list = _safiroRepository.GetPayments(param.Period, param.Description);

            foreach (Payment payment in list)
            {
                GPSaldo saldo= _gpRepository.GetPaymentDifference(param.Period, param.Description, payment.Code);
                if (saldo != null )
                {
                    payment.PaymentValue = saldo.difference;
                    /*if (saldo.difference >= 0)
                    {
                        payment.PaymentValue = saldo.difference;
                    }
                    else
                    {
                        payment.PaymentValue = 0;
                    }*/
                }
            }

            return list;
        }

        public List<GPConfiguracion> GetInvoiceConfigurationDetail(string name)
        {
            List<GPConfiguracion> confs = _gpRepository.GetConfigurationByName(name.Substring(0, 9).ToUpper());
            return confs;
        }

        public JObject GenerateReceivableAcconting(InvoiceGroupDTO data, string username)
        {
            Payment totalPayment = null;
            List<GPSaldo> saldoList = new List<GPSaldo>();
            DateTime dateTime = DateTime.Now;
            string processStatus = null;
            string lastErrorMessage = null;
            double totalInvoiceDetail = 0;
            double totalInvoiceDetailReal = 0;
            bool foundError = false;

            if (data.Payments == null)
            {
                return GenerateResponse("No hay pagos seleccionados");
            }

            //Solo se envia un pago
            foreach (Payment payment in data.Payments)
            {
                //data.Period, data.GroupName, payment.Code
                InvoiceDTO invoiceDTO = new InvoiceDTO();
                invoiceDTO.Period = data.Invoices[0].FechaFacturacion.Substring(0,6);
                invoiceDTO.Description = data.GroupName;

                totalPayment = GetPayments(invoiceDTO).Where(s => s.Code.Equals(payment.Code)).FirstOrDefault();
            }

            totalInvoiceDetail = data.Invoices.Sum(s => s.detail.Sum(d => d.SuggestedValue));
            totalInvoiceDetailReal = data.Invoices.Sum(s => s.detail.Sum(d => d.Value));

            if (totalInvoiceDetail != totalPayment.PaymentValue)
            {
                return GenerateResponse("El monto de los items seleccionado no es igual al pago seleccionado.");
            }

            foreach (InvoiceDTO invoice in data.Invoices) {

                GPLiquidacion liq = _gpRepository.GetSettlementById(invoice.FechaFacturacion, invoice.Version, invoice.Factura_id);
                List<GPConfiguracion> confs = GetInvoiceConfigurationDetail(liq.Cliente_nombre).Where(s => s.Tipo.Equals("RE") || s.Tipo.Equals("RP")).ToList();
                confs.Sort((a, b) => a.Sort_order.CompareTo(b.Sort_order));

                foreach (GPConfiguracion conf in confs)
                {
                    double calculatedValue = GetCalculatedValue(liq, conf);
                    if (calculatedValue != 0)
                    {
                        ReplaceValues(conf, liq);
                        conf.Numdocso = totalPayment.Code;
                        processStatus = _safiroRepository.GeneratePayableAcconting(liq, conf, calculatedValue);

                        if (processStatus == null)
                        {
                            if (totalPayment.PaymentValue < totalInvoiceDetailReal)
                            {
                                _gpRepository.UpdateStatus(liq, 2, "", dateTime, username);
                            }
                            else
                            {
                                _gpRepository.UpdateStatus(liq, 3, "", dateTime, username);
                            }
                        }
                        else
                        {
                            _gpRepository.UpdateStatus(liq, 2, processStatus, dateTime, username);
                            lastErrorMessage = processStatus;
                            foundError = foundError || true;
                            return GenerateResponse(lastErrorMessage);
                        }
                    }
                }

                saldoList.Add(CreateGPSaldo(invoice, totalPayment, data.GroupName));
            }
            saldoList.ForEach(s => s.difference = totalPayment.PaymentValue - totalInvoiceDetailReal);

            SaveGPSaldo(saldoList);
            return GenerateResponse("Datos guardados correctamente");

        }   
        
        private GPSaldo CreateGPSaldo(InvoiceDTO invoice, Payment payment, string groupName)
        {
            GPSaldo saldo = new GPSaldo();
            saldo.Periodo = invoice.FechaFacturacion.Substring(0, 6);
            saldo.ValorFactura = invoice.detail.Sum(s => s.Value);
            saldo.CodigoFactura = invoice.Factura_id;
            saldo.ValorIngreso = payment.PaymentValue;
            saldo.CodigoIngreso = payment.Code;
            saldo.Nombre_grupo = groupName;

            return saldo;
        }

        private void SaveGPSaldo(List<GPSaldo> saldoList)
        {
            _gpRepository.SaveSaldo(saldoList);
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

            //Se obtiene el total de pagos realizados por el cliente
            foreach (string group_id in paramGroup.GroupName.Split(","))
            {
                paramInvoice.Description = group_id;
                invoiceGroupDTO.Payments.AddRange(GetPayments(paramInvoice));
                totalPayments += invoiceGroupDTO.Payments.Sum(s => s.PaymentValue);
            }
            invoiceGroupDTO.TotalPayment = totalPayments;

            //Se obtiene el total de los montos a recaudar
            foreach (string group_id in paramGroup.GroupName.Split(","))
            {
                paramInvoice.Status = 2;
                paramInvoice.Description = group_id;
                List<GPLiquidacion> gPLiquidacions = GetPendingInvoiceLocalByName(paramInvoice);
                foreach (GPLiquidacion item in gPLiquidacions)
                {
                    InvoiceDTO invoice = new InvoiceDTO();
                    invoice.Factura_id = item.Factura_id;
                    invoice.FechaFacturacion = item.Fechafacturacion;
                    invoice.Version = item.Version;

                    invoice.Description = "Factura: " + item.Factura_dian + " Version: " + item.Version + " - Frontera:"
                        + item.Frontera + " - " + item.Municipio_nombre + " - " + item.V_neto_factura;
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
                            totalInvoices = totalInvoices + calculatedValue;
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
            invoiceGroupDTO.GroupName = paramGroup.GroupName;
            return invoiceGroupDTO;
        }

        public JObject SaveLoadedInvoices(InvoiceGroupDTO data)
        {
            InvoiceDTO paramInvoice = new InvoiceDTO();
            paramInvoice.Description = data.GroupName;
            paramInvoice.Period = data.Period;
            //paramInvoice.Username = Getlo;

            double totalInvoice = 0;
            double totalInvoicesModified = 0;

            totalInvoicesModified = data.Invoices.Sum(s => s.detail.Sum(x => x.SuggestedValue) );
            totalInvoice = data.Invoices.Sum(s => s.detail.Sum(x => x.Value));

            if (totalInvoicesModified > totalInvoice)
            {
                return GenerateResponse("Las modificaciones no puede superar el monto de la factura");
            }

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

            return GenerateResponse("Proceso finalizado correctamente");
        }

        public List<MenuDTO> GenerateMenuInvoices(InvoiceDTO param)
        {
            string profile = _gpRepository.GetProfileUser(param.Username);

            List<MenuDTO> menuList = new List<MenuDTO>();
            GPLiquidacion liq = _gpRepository.GetSettlementById(param.FechaFacturacion, param.Version, param.Factura_id);
            GetProgressNumber(liq.Avance);

            foreach (string menuOption in menuOptions.Keys)
            {
                MenuDTO menu = new MenuDTO();
                menu.Name = menuOption;
                menu.Action = menuOptions[menuOption];
                menu.IsActive = true;

                if (GetProgressNumber(menu.Action) >= GetProgressNumber(liq.Avance))
                {
                    menu.IsActive = false;
                } else
                {
                    menu.IsActive = true;
                }

                if (profile.Equals("MODIFY"))
                {
                    if (liq.Tipo_factura.Equals("Factura"))
                    {
                        menuList.Add(menu);
                    }
                }
            }

            return menuList;

        }
    }
}