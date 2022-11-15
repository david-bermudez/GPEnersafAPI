using Dapper;
using GpEnerSaf.Models.BD;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Edm;
using Oracle.ManagedDataAccess.Client;
using project.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;

namespace GpEnerSaf.Repositories
{
    public class SAFIRORepositoryImpl : ISAFIRORepository
    {
        IConfiguration configuration;

        public SAFIRORepositoryImpl(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public string ValidateInvoice(GPLiquidacion liquidacion, GPConfiguracion conf, double calculatedValue )
        {
            string anonymous_block =
                "declare " +
                "    perg_movconta sfci001q.rg_movcontable;  " +
                "    psnu_coderror number; " +
                "    psvc_menerror varchar2(100); " +
                "begin " +
                "    perg_movconta.CODSUCUR := :1;  " +
                "    perg_movconta.ANOPERAF := :2;  " +
                "    perg_movconta.MESPERAF := :3;  " +
                "    perg_movconta.FECMVCON := :4;  " +
                "    perg_movconta.CODTIPFU := :5;  " +
                "    perg_movconta.NUMTIPFU := :6;  " +
                "    perg_movconta.CODTIPDC := :7;  " +
                "    perg_movconta.NUMDOCSO := :8;  " +
                "    perg_movconta.CODLIBRO := :9;  " +
                "    perg_movconta.ESQUEMAT := :10;  " +
                "    perg_movconta.CODCTACO := :11;  " +
                "    perg_movconta.NIVANAL1 := :12;  " +
                "    perg_movconta.CODNIVA1 := :13;  " +
                "    perg_movconta.NIVANAL2 := :14;  " +
                "    perg_movconta.CODNIVA2 := :15;  " +
                "    perg_movconta.NIVANAL3 := :16;  " +
                "    perg_movconta.CODNIVA3 := :17;  " +
                "    perg_movconta.NIVANAL4 := :18;  " +
                "    perg_movconta.CODNIVA4 := :19;  " +
                "    perg_movconta.NIVANAL5 := :20;  " +
                "    perg_movconta.CODNIVA5 := :21;  " +
                "    perg_movconta.DETASIEN := :22;  " +
                "    perg_movconta.VALASTMN := :23;  " +
                "    perg_movconta.VALASTME := :24;  " +
                "    perg_movconta.VALTRMAC := :25;  " +
                "    perg_movconta.TIPMONED := :26;  " +
                "    perg_movconta.TIPASIEN := :27;  " +
                "    perg_movconta.CANUPACS := :28;  " +
                "    perg_movconta.CANUNIPR := :29;  " +
                "    perg_movconta.TIPOPERA := :30;  " +
                "    perg_movconta.FECVENCI := :31;  " +
                "    perg_movconta.CODTDARF := :32;  " +
                "    perg_movconta.NUMDOARF := :33;  " +
                "    perg_movconta.FECDOARF := :34;  " +
                "    perg_movconta.VALBASEM := :35;  " +
                "    perg_movconta.PORUSADO := :36;  " +
                "    perg_movconta.ESTREGCO := :37;  " +
                "    perg_movconta.CODLIBEQ := :38;  " +
                "    perg_movconta.CODCTAEQ := :39;  " +
                "    perg_movconta.CODAPLIC := :40;  " +
                "    perg_movconta.CODOPERA := :41;  " +
                "    perg_movconta.FECCREAC := :42;  " +
                "    perg_movconta.USUCREAC := :43;  " +
                "    perg_movconta.FECACTLZ := :44;  " +
                "    perg_movconta.USUACTLZ := :45;  " +
                "    perg_movconta.USUMVCON := :46;  " +
                "    Sfci001q.provalidarmovcont(perg_movconta, :47, :48); " +
                "end; ";

            Dictionary<string, object> result;
            List<OracleParameter> list = CreateInvoiceParamOracle(liquidacion, conf, Math.Round( calculatedValue ), "");
            result = ExecuteStoreProcedure(anonymous_block, list);
            if (!result["48"].ToString().Equals("null"))
            {
                return "Error " + result["47"].ToString() + " - " + result["48"].ToString();
            }

            return null;
        }

        
        public string GenerateInvoiceAcconting(GPLiquidacion liquidacion, GPConfiguracion confs, double calculatedValue)
        {
            string result = null;
            //REGISTRO DE MOVIMIENTOS CONTABLES FACTURACION
            if (confs.Tipo.Equals("CN"))
            {
                result = generateInvoiceAccounting(liquidacion, confs, calculatedValue);
            }

            //REGISTRO DE MOVIMIENTOS CONTABLES PROVISION
            if (confs.Tipo.Equals("CP"))
            {
                result = generateInvoiceProvisionAccounting(liquidacion, confs, calculatedValue);
            }

            //REGISTRO DE MOVIMIENTOS DE IMPUESTOS
            if (confs.Tipo.Equals("IM"))
            {
                result = generateInvoiceTaxAccounting(liquidacion, confs, calculatedValue);
            }

            //REGISTRO DE MOVIMIENTOS DE PRESUPUESTO
            if (confs.Tipo.Equals("PPI"))
            {
                result = generateInvoiceBudgetAccounting(liquidacion, confs, calculatedValue);
            }

            if (confs.Tipo.Equals("PPE"))
            {
                result = generateInvoiceBudgetAccounting(liquidacion, confs, calculatedValue);
            }
            return result;
        }

        public string GeneratePayableAcconting(GPLiquidacion liquidacion, GPConfiguracion confs, double calculatedValue)
        {
            string result = null;
            //REGISTRO DE MOVIMIENTOS CONTABLES FACTURACION
            if (confs.Tipo.Equals("RE"))
            {
                result = generateInvoiceAccounting(liquidacion, confs, calculatedValue);
            }

            //REGISTRO DE MOVIMIENTOS CONTABLES PPTO
            if (confs.Tipo.Equals("RP"))
            {
                if (confs.Codcompr ==null || confs.Codcompr.Equals("") || confs.Codcompr.ToLower().Equals(liquidacion.Frontera.ToLower()))
                {
                    result = generateInvoiceBudgetAccounting(liquidacion, confs, calculatedValue);
                }
            }

            return result;
        }
        private string generateInvoiceAccounting(GPLiquidacion liquidacion, GPConfiguracion confs, double calculatedValue)
        {
            string anonymous_block =
               "declare " +
               "    perg_movconta sfci001q.rg_movcontable;  " +
               "    psnu_coderror number; " +
               "    psvc_menerror varchar2(100); " +
               "begin " +
               "    perg_movconta.CODSUCUR := :1;  " +
               "    perg_movconta.ANOPERAF := :2;  " +
               "    perg_movconta.MESPERAF := :3;  " +
               "    perg_movconta.FECMVCON := :4;  " +
               "    perg_movconta.CODTIPFU := :5;  " +
               "    perg_movconta.NUMTIPFU := :6;  " +
               "    perg_movconta.CODTIPDC := :7;  " +
               "    perg_movconta.NUMDOCSO := :8;  " +
               "    perg_movconta.CODLIBRO := :9;  " +
               "    perg_movconta.ESQUEMAT := :10;  " +
               "    perg_movconta.CODCTACO := :11;  " +
               "    perg_movconta.NIVANAL1 := :12;  " +
               "    perg_movconta.CODNIVA1 := :13;  " +
               "    perg_movconta.NIVANAL2 := :14;  " +
               "    perg_movconta.CODNIVA2 := :15;  " +
               "    perg_movconta.NIVANAL3 := :16;  " +
               "    perg_movconta.CODNIVA3 := :17;  " +
               "    perg_movconta.NIVANAL4 := :18;  " +
               "    perg_movconta.CODNIVA4 := :19;  " +
               "    perg_movconta.NIVANAL5 := :20;  " +
               "    perg_movconta.CODNIVA5 := :21;  " +
               "    perg_movconta.DETASIEN := :22;  " +
               "    perg_movconta.VALASTMN := :23;  " +
               "    perg_movconta.VALASTME := :24;  " +
               "    perg_movconta.VALTRMAC := :25;  " +
               "    perg_movconta.TIPMONED := :26;  " +
               "    perg_movconta.TIPASIEN := :27;  " +
               "    perg_movconta.CANUPACS := :28;  " +
               "    perg_movconta.CANUNIPR := :29;  " +
               "    perg_movconta.TIPOPERA := :30;  " +
               "    perg_movconta.FECVENCI := :31;  " +
               "    perg_movconta.CODTDARF := :32;  " +
               "    perg_movconta.NUMDOARF := :33;  " +
               "    perg_movconta.FECDOARF := :34;  " +
               "    perg_movconta.VALBASEM := :35;  " +
               "    perg_movconta.PORUSADO := :36;  " +
               "    perg_movconta.ESTREGCO := :37;  " +
               "    perg_movconta.CODLIBEQ := :38;  " +
               "    perg_movconta.CODCTAEQ := :39;  " +
               "    perg_movconta.CODAPLIC := :40;  " +
               "    perg_movconta.CODOPERA := :41;  " +
               "    perg_movconta.FECCREAC := :42;  " +
               "    perg_movconta.USUCREAC := :43;  " +
               "    perg_movconta.FECACTLZ := :44;  " +
               "    perg_movconta.USUACTLZ := :45;  " +
               "    perg_movconta.USUMVCON := :46;  " +
               "    Sfci001q.proingresarmovcont(perg_movconta, :47, :48); " +
               "end; ";

            Dictionary<string, object> result;
            List<OracleParameter> list = CreateInvoiceParamOracle(liquidacion, confs, calculatedValue ,"");
            result = ExecuteStoreProcedure(anonymous_block, list);
            if (!result["47"].ToString().Equals("0"))
            {
                return "Error " + result["47"].ToString() + " - " + result["48"].ToString();
            }

            return null;
        }

        private string generateInvoiceProvisionAccounting(GPLiquidacion liquidacion, GPConfiguracion confs, double calculatedValue)
        {
            string anonymous_block =
               "declare " +
               "    perg_movconta sfci001q.rg_movcontable;  " +
               "    psnu_coderror number; " +
               "    psvc_menerror varchar2(100); " +
               "begin " +  
               "    perg_movconta.CODSUCUR := :1;  " +
               "    perg_movconta.ANOPERAF := :2;  " +
               "    perg_movconta.MESPERAF := :3;  " +
               "    perg_movconta.FECMVCON := :4;  " +
               "    perg_movconta.CODTIPFU := :5;  " +
               "    perg_movconta.NUMTIPFU := :6;  " +
               "    perg_movconta.CODTIPDC := :7;  " +
               "    perg_movconta.NUMDOCSO := :8;  " +
               "    perg_movconta.CODLIBRO := :9;  " +
               "    perg_movconta.ESQUEMAT := :10;  " +
               "    perg_movconta.CODCTACO := :11;  " +
               "    perg_movconta.NIVANAL1 := :12;  " +
               "    perg_movconta.CODNIVA1 := :13;  " +
               "    perg_movconta.NIVANAL2 := :14;  " +
               "    perg_movconta.CODNIVA2 := :15;  " +
               "    perg_movconta.NIVANAL3 := :16;  " +
               "    perg_movconta.CODNIVA3 := :17;  " +
               "    perg_movconta.NIVANAL4 := :18;  " +
               "    perg_movconta.CODNIVA4 := :19;  " +
               "    perg_movconta.NIVANAL5 := :20;  " +
               "    perg_movconta.CODNIVA5 := :21;  " +
               "    perg_movconta.DETASIEN := :22;  " +
               "    perg_movconta.VALASTMN := :23;  " +
               "    perg_movconta.VALASTME := :24;  " +
               "    perg_movconta.VALTRMAC := :25;  " +
               "    perg_movconta.TIPMONED := :26;  " +
               "    perg_movconta.TIPASIEN := :27;  " +
               "    perg_movconta.CANUPACS := :28;  " +
               "    perg_movconta.CANUNIPR := :29;  " +
               "    perg_movconta.TIPOPERA := :30;  " +
               "    perg_movconta.FECVENCI := :31;  " +
               "    perg_movconta.CODTDARF := :32;  " +
               "    perg_movconta.NUMDOARF := :33;  " +
               "    perg_movconta.FECDOARF := :34;  " +
               "    perg_movconta.VALBASEM := :35;  " +
               "    perg_movconta.PORUSADO := :36;  " +
               "    perg_movconta.ESTREGCO := :37;  " +
               "    perg_movconta.CODLIBEQ := :38;  " +
               "    perg_movconta.CODCTAEQ := :39;  " +
               "    perg_movconta.CODAPLIC := :40;  " +
               "    perg_movconta.CODOPERA := :41;  " +
               "    perg_movconta.FECCREAC := :42;  " +
               "    perg_movconta.USUCREAC := :43;  " +
               "    perg_movconta.FECACTLZ := :44;  " +
               "    perg_movconta.USUACTLZ := :45;  " +
               "    perg_movconta.USUMVCON := :46;  " +
               "    Sfci001q.proingresarmovcont(perg_movconta, :47, :48); " +
               "end; ";

            Dictionary<string, object> result;

            List<OracleParameter> list = CreateInvoiceParamOracle(liquidacion, confs, calculatedValue, "PROVISION");
            result = ExecuteStoreProcedure(anonymous_block, list);
            if (!result["47"].ToString().Equals("0"))
            {
                return "Error " + result["47"].ToString() + " - " + result["48"].ToString();
            }

            return null;
        }

        private string generateInvoiceTaxAccounting(GPLiquidacion liquidacion, GPConfiguracion confs, double calculatedValue)
        {
            string anonymous_block =
                "begin " +
                "  Sfts040q.proRegistrarOpeConcepto( " +
                "    :1, :2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15,:16,:17); " +
                "end; ";

            Dictionary<string, object> result;
            List<OracleParameter> list = CreateTaxParamOracle(liquidacion, confs, calculatedValue);
            result = ExecuteStoreProcedure(anonymous_block, list);
            if (!result["16"].ToString().Equals(""))
            {
                return "Error " + result["16"].ToString() + " - " + result["17"].ToString();
            }

            return null;
        }

        private string generateInvoiceBudgetAccounting(GPLiquidacion liquidacion, GPConfiguracion confs, double calculatedValue)
        {
            string anonymous_block =
                "declare " +
                "  id_trx number; " +
                "begin " +
                "  id_trx := sfps001q.fnuObtenerSecuenciaTransaccion(); " +
                "  Sfps001q.proGenerarejecIngresosPresup( " +
                "    :1, id_trx , :2,:3,TO_DATE(:4,'YYYYMMDD'),:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15,:16,:17,:18,:19,:20,:21,:22,:23,:24,SYSDATE,:26,:27); " +
                "end; ";

            Dictionary<string, object> result;
            List<OracleParameter> list = CreateBudgetParamOracle(liquidacion, confs, calculatedValue);
            result = ExecuteStoreProcedure(anonymous_block, list);
            if (!result["26"].ToString().Equals("0"))
            {
                return "Error " + result["25"].ToString() + " - " + result["26"].ToString();
            }

            return null;
        }

        private Dictionary<string, object> ExecuteStoreProcedure(string procedureName, 
            List<OracleParameter> paramList)
        {
            Dictionary<string, object> valueOutList = new Dictionary<string, object>();
            OracleConnection conn = null;
            try
            {
                conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                OracleCommand comm = new OracleCommand();
                comm.Connection = conn;
                comm.CommandText = procedureName;
                foreach (OracleParameter oracleValue in paramList ){
                    comm.Parameters.Add(oracleValue);
                }

                comm.ExecuteNonQuery();

                foreach (OracleParameter oracleValue in paramList)
                {
                    if (oracleValue.Direction == ParameterDirection.Output)
                    {
                        valueOutList.Add(oracleValue.ParameterName, oracleValue.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                valueOutList.Add((paramList.Count - 1).ToString(), 9001);
                valueOutList.Add((paramList.Count).ToString(), ex.Message);
            }
            finally
            {
                try { conn.Close(); } catch (Exception) { }
            }
            return valueOutList;
        }

        public OracleConnection GetConnection()
        {
            var connectionString = configuration.GetSection("ConnectionStrings").GetSection("DBSAFIROConnection").Value;
            OracleConnection conn = new OracleConnection(connectionString);
            return conn;
        }

        private List<OracleParameter> CreateInvoiceParamOracle(GPLiquidacion liquidacion, GPConfiguracion conf, double calculatedValue, string prefix) {

            string Fechafacturacion = liquidacion.Fechafacturacion;
            string Factura_dian = liquidacion.Factura_dian.ToString();
            if (prefix.StartsWith("PROVISION"))
            {
                //DateTime previousMonth = DateTime.ParseExact(liquidacion.Fechafacturacion, "yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime previousMonth = Date.Now;
                Fechafacturacion = previousMonth.AddMonths(-1).Date.ToString("yyyyMM") + "30";
                Factura_dian = "01";
            }
            //Fechafacturacion = "20221105";
            //Fechafacturacion = "20221012";
            if (conf.Tipo.Equals("RE"))
            {
                //Fechafacturacion = "20221012";
                Factura_dian = conf.Numdocso;
            }
            OracleParameter p1 = new OracleParameter("1", OracleDbType.Varchar2, conf.Codsucur, ParameterDirection.Input);
            OracleParameter p2 = new OracleParameter("2", OracleDbType.Varchar2, Fechafacturacion.Substring(0,4), ParameterDirection.Input);
            OracleParameter p3 = new OracleParameter("3", OracleDbType.Varchar2, Int32.Parse(Fechafacturacion.Substring(4, 2)).ToString(), ParameterDirection.Input);
            OracleParameter p4 = new OracleParameter("4", OracleDbType.Date, DateTime.ParseExact(Fechafacturacion, "yyyyMMdd",CultureInfo.InvariantCulture), ParameterDirection.Input);
            OracleParameter p5 = new OracleParameter("5", OracleDbType.Varchar2, conf.Codtipfu, ParameterDirection.Input);
            OracleParameter p6 = new OracleParameter("6", OracleDbType.Varchar2, conf.Numtipfu, ParameterDirection.Input);
            OracleParameter p7 = new OracleParameter("7", OracleDbType.Varchar2, conf.Codtipdc, ParameterDirection.Input);
            OracleParameter p8 = new OracleParameter("8", OracleDbType.Varchar2, Factura_dian, ParameterDirection.Input);
            OracleParameter p9 = new OracleParameter("9", OracleDbType.Varchar2, conf.Codlibro, ParameterDirection.Input);
            OracleParameter p10 = new OracleParameter("10", OracleDbType.Varchar2, conf.Esquemat, ParameterDirection.Input);
            OracleParameter p11 = new OracleParameter("11", OracleDbType.Varchar2, conf.Codctaco, ParameterDirection.Input);
            OracleParameter p12 = new OracleParameter("12", OracleDbType.Varchar2, conf.Nivanal1, ParameterDirection.Input);
            OracleParameter p13 = new OracleParameter("13", OracleDbType.Varchar2, conf.Codniva1, ParameterDirection.Input);
            OracleParameter p14 = new OracleParameter("14", OracleDbType.Varchar2, conf.Nivanal2, ParameterDirection.Input);
            OracleParameter p15 = new OracleParameter("15", OracleDbType.Varchar2, conf.Codniva2, ParameterDirection.Input);
            OracleParameter p16 = new OracleParameter("16", OracleDbType.Varchar2, conf.Nivanal3, ParameterDirection.Input);
            OracleParameter p17 = new OracleParameter("17", OracleDbType.Varchar2, conf.Codniva3, ParameterDirection.Input);
            OracleParameter p18 = new OracleParameter("18", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p19 = new OracleParameter("19", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p20 = new OracleParameter("20", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p21 = new OracleParameter("21", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p22 = new OracleParameter("22", OracleDbType.Varchar2, prefix + " " + conf.Concepto + " - PERIODO " + liquidacion.Fechafacturacion.Substring(0,6), ParameterDirection.Input);
            OracleParameter p23 = new OracleParameter("23", OracleDbType.Double, Math.Abs(calculatedValue), ParameterDirection.Input);
            OracleParameter p24 = new OracleParameter("24", OracleDbType.Double, 0, ParameterDirection.Input);
            OracleParameter p25 = new OracleParameter("25", OracleDbType.Double, 0, ParameterDirection.Input);
            OracleParameter p26 = new OracleParameter("26", OracleDbType.Varchar2, conf.Tipo_moneda, ParameterDirection.Input);

            OracleParameter p27 = new OracleParameter("27", OracleDbType.Varchar2, conf.Tipo_asiento, ParameterDirection.Input);
            if (conf.Tipo_asiento.Equals("D") && calculatedValue < 0)
            {
                p27.Value = "C";
            }
            if (conf.Tipo_asiento.Equals("C") && calculatedValue < 0)
            {
                p27.Value = "D";
            }

            //Cuando es una Reversion de Factura
            if (!liquidacion.Tipo_factura.ToLower().Equals("factura"))
            {
                if (conf.Tipo_asiento.Equals("D") && calculatedValue < 0)
                {
                    p27.Value = "D";
                }
                if (conf.Tipo_asiento.Equals("C") && calculatedValue < 0)
                {
                    p27.Value = "C";
                }
            }

            OracleParameter p28 = new OracleParameter("28", OracleDbType.Double, null, ParameterDirection.Input);
            OracleParameter p29 = new OracleParameter("29", OracleDbType.Double, null, ParameterDirection.Input);
            OracleParameter p30 = new OracleParameter("30", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p31 = new OracleParameter("31", OracleDbType.Date, null, ParameterDirection.Input);
            OracleParameter p32 = new OracleParameter("32", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p33 = new OracleParameter("33", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p34 = new OracleParameter("34", OracleDbType.Date, null, ParameterDirection.Input);
            OracleParameter p35 = new OracleParameter("35", OracleDbType.Double, null, ParameterDirection.Input);
            OracleParameter p36 = new OracleParameter("36", OracleDbType.Double, null, ParameterDirection.Input);
            OracleParameter p37 = new OracleParameter("37", OracleDbType.Varchar2, "G", ParameterDirection.Input);
            OracleParameter p38 = new OracleParameter("38", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p39 = new OracleParameter("39", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p40 = new OracleParameter("40", OracleDbType.Varchar2, "ENERSINC", ParameterDirection.Input);
            OracleParameter p41 = new OracleParameter("41", OracleDbType.Varchar2, "FACTURACION", ParameterDirection.Input);
            OracleParameter p42 = new OracleParameter("42", OracleDbType.Date, DateTime.Now, ParameterDirection.Input);
            OracleParameter p43 = new OracleParameter("43", OracleDbType.Varchar2, "INTERFAZ", ParameterDirection.Input);
            OracleParameter p44 = new OracleParameter("44", OracleDbType.Date, null, ParameterDirection.Input);
            OracleParameter p45 = new OracleParameter("45", OracleDbType.Varchar2, null, ParameterDirection.Input);
            OracleParameter p46 = new OracleParameter("46", OracleDbType.Varchar2, null, ParameterDirection.Input);

            OracleParameter p47 = new OracleParameter("47", OracleDbType.Decimal, ParameterDirection.Output);
            OracleParameter p48 = new OracleParameter("48", OracleDbType.Varchar2, ParameterDirection.Output);

            List<OracleParameter> list = new List<OracleParameter>();
            list.Add(p1);list.Add(p2);list.Add(p3);list.Add(p4);list.Add(p5);list.Add(p6); list.Add(p7);list.Add(p8);list.Add(p9);list.Add(p10);
            list.Add(p11);list.Add(p12);list.Add(p13);list.Add(p14);list.Add(p15);list.Add(p16);list.Add(p17);list.Add(p18);list.Add(p19);list.Add(p20);
            list.Add(p21);list.Add(p22);list.Add(p23);list.Add(p24);list.Add(p25);list.Add(p26);list.Add(p27);list.Add(p28);list.Add(p29);list.Add(p30);
            list.Add(p31);list.Add(p32);list.Add(p33);list.Add(p34);list.Add(p35);list.Add(p36);list.Add(p37);list.Add(p38);list.Add(p39);list.Add(p40);
            list.Add(p41);list.Add(p42);list.Add(p43);list.Add(p44);list.Add(p45);list.Add(p46);
            list.Add(p47);list.Add(p48);

            return list;
        }

        private List<OracleParameter> CreateTaxParamOracle(GPLiquidacion liquidacion, GPConfiguracion conf, double calculatedValue)
        {
            double d = calculatedValue * double.Parse(conf.Codniva1);
            OracleParameter p1 = new OracleParameter("1", OracleDbType.Varchar2, "GC", ParameterDirection.Input);
            OracleParameter p2 = new OracleParameter("2", OracleDbType.Varchar2, conf.Codctaco, ParameterDirection.Input);
            OracleParameter p3 = new OracleParameter("3", OracleDbType.Varchar2, "C", ParameterDirection.Input);
            OracleParameter p4 = new OracleParameter("4", OracleDbType.Double, calculatedValue * double.Parse(conf.Codniva1) , ParameterDirection.Input);
            OracleParameter p5 = new OracleParameter("5", OracleDbType.Date, DateTime.ParseExact(liquidacion.Fechafacturacion, "yyyyMMdd", CultureInfo.InvariantCulture), ParameterDirection.Input);
            OracleParameter p6 = new OracleParameter("6", OracleDbType.Double, calculatedValue, ParameterDirection.Input);
            OracleParameter p7 = new OracleParameter("7", OracleDbType.Double, double.Parse(conf.Codniva1), ParameterDirection.Input);
            OracleParameter p8 = new OracleParameter("8", OracleDbType.Varchar2, conf.Codniva2, ParameterDirection.Input);
            OracleParameter p9 = new OracleParameter("9", OracleDbType.Varchar2, null , ParameterDirection.Input);
            OracleParameter p10 = new OracleParameter("10", OracleDbType.Varchar2, "FCTVNT", ParameterDirection.Input);
            OracleParameter p11 = new OracleParameter("11", OracleDbType.Varchar2, liquidacion.Factura_dian, ParameterDirection.Input);
            OracleParameter p12 = new OracleParameter("12", OracleDbType.Varchar2, "FCTVNT", ParameterDirection.Input);
            OracleParameter p13 = new OracleParameter("13", OracleDbType.Varchar2, liquidacion.Factura_dian, ParameterDirection.Input);
            OracleParameter p14 = new OracleParameter("14", OracleDbType.Varchar2, "INTERFAZ", ParameterDirection.Input);
            OracleParameter p15 = new OracleParameter("15", OracleDbType.Date, DateTime.Now, ParameterDirection.Input);

            OracleParameter p16 = new OracleParameter("16", OracleDbType.Varchar2, ParameterDirection.Output);
            OracleParameter p17 = new OracleParameter("17", OracleDbType.Decimal, ParameterDirection.Output);

            List<OracleParameter> list = new List<OracleParameter>();
            list.Add(p1); list.Add(p2); list.Add(p3); list.Add(p4); list.Add(p5); list.Add(p6); list.Add(p7); list.Add(p8); list.Add(p9); list.Add(p10);
            list.Add(p11); list.Add(p12); list.Add(p13); list.Add(p14); list.Add(p15); 
            list.Add(p16); list.Add(p17); 

            return list;
        }

        private List<OracleParameter> CreateBudgetParamOracle(GPLiquidacion liquidacion, GPConfiguracion conf, double calculatedValue)
        {
            string codoperPositive = "";
            string codoperNegative = "";
            string Codcompr = conf.Codcompr;
            string fecha = liquidacion.Fechafacturacion;

            if (conf.Tipo.Equals("PP"))
            {
                codoperPositive = "518";
                codoperNegative = "519";
            } 
            else if (conf.Tipo.Equals("RP")) 
            {
                codoperPositive = "518";
                codoperNegative = "519";
            }
            else if (conf.Tipo.Equals("PPI"))
            {
                Codcompr = conf.Contrato;
                //fecha = "20221013";
            }
            else if (conf.Tipo.Equals("PPE"))
            {
                //fecha = "20221013";
            }

            OracleParameter p1 = new OracleParameter("1", OracleDbType.Varchar2, "GC", ParameterDirection.Input);
            OracleParameter p2 = new OracleParameter("2", OracleDbType.Varchar2, conf.Tipo_asiento, ParameterDirection.Input);
            OracleParameter p3 = new OracleParameter("3", OracleDbType.Varchar2, fecha.Substring(0, 4), ParameterDirection.Input);
            OracleParameter p4 = new OracleParameter("4", OracleDbType.Varchar2, fecha, ParameterDirection.Input);
            OracleParameter p5 = new OracleParameter("5", OracleDbType.Varchar2, Codcompr, ParameterDirection.Input);
            OracleParameter p6 = new OracleParameter("6", OracleDbType.Varchar2, conf.Codopepr, ParameterDirection.Input);
            if (conf.Codopepr == null && calculatedValue > 0)
            {
                p6.Value = codoperPositive;
            }
            if (conf.Codopepr == null && calculatedValue < 0)
            {
                p6.Value = codoperNegative;
            }

            OracleParameter p7 = new OracleParameter("7", OracleDbType.Varchar2, conf.Codctaco, ParameterDirection.Input);
            OracleParameter p8 = new OracleParameter("8", OracleDbType.Varchar2, conf.Nivanal1, ParameterDirection.Input);
            OracleParameter p9 = new OracleParameter("9", OracleDbType.Varchar2, conf.Codniva1, ParameterDirection.Input);
            OracleParameter p10 = new OracleParameter("10", OracleDbType.Varchar2, conf.Nivanal2, ParameterDirection.Input);
            OracleParameter p11 = new OracleParameter("11", OracleDbType.Varchar2, conf.Codniva2, ParameterDirection.Input);
            OracleParameter p12 = new OracleParameter("12", OracleDbType.Varchar2, conf.Nivanal3, ParameterDirection.Input);
            OracleParameter p13 = new OracleParameter("13", OracleDbType.Varchar2, conf.Codniva3, ParameterDirection.Input);
            if (conf.Nivanal3 != null && conf.Nivanal3.Equals(""))
            {
                p12.Value = null;
                p13.Value = null;
            }
            
            OracleParameter p14 = new OracleParameter("14", OracleDbType.Varchar2, null , ParameterDirection.Input);
            OracleParameter p15 = new OracleParameter("15", OracleDbType.Varchar2, null , ParameterDirection.Input);
            OracleParameter p16 = new OracleParameter("16", OracleDbType.Varchar2, null , ParameterDirection.Input);
            OracleParameter p17 = new OracleParameter("17", OracleDbType.Varchar2, null , ParameterDirection.Input);
            OracleParameter p18 = new OracleParameter("18", OracleDbType.Varchar2, conf.Concepto, ParameterDirection.Input);
            OracleParameter p19 = new OracleParameter("19", OracleDbType.Decimal, Math.Abs(calculatedValue), ParameterDirection.Input);
            OracleParameter p20 = new OracleParameter("20", OracleDbType.Varchar2, conf.Codtipdc, ParameterDirection.Input);
            OracleParameter p21 = new OracleParameter("21", OracleDbType.Varchar2, liquidacion.Factura_dian, ParameterDirection.Input);
            OracleParameter p22 = new OracleParameter("22", OracleDbType.Varchar2, conf.Codtipfu, ParameterDirection.Input);
            OracleParameter p23 = new OracleParameter("23", OracleDbType.Varchar2, liquidacion.Factura_dian, ParameterDirection.Input);
            OracleParameter p24 = new OracleParameter("24", OracleDbType.Varchar2, "ENERSINC", ParameterDirection.Input);
            //OracleParameter p25 = new OracleParameter("25", OracleDbType.Date, Date.Now, ParameterDirection.Input);

            OracleParameter p26 = new OracleParameter("26", OracleDbType.Decimal, ParameterDirection.Output);
            OracleParameter p27 = new OracleParameter("27", OracleDbType.Varchar2, ParameterDirection.Output);

            List<OracleParameter> list = new List<OracleParameter>();
            list.Add(p1); list.Add(p2); list.Add(p3); list.Add(p4); list.Add(p5); list.Add(p6); list.Add(p7); list.Add(p8); list.Add(p9); list.Add(p10);
            list.Add(p11); list.Add(p12); list.Add(p13); list.Add(p14); list.Add(p15); list.Add(p16); list.Add(p17); list.Add(p18); list.Add(p19); list.Add(p20);
            list.Add(p21); list.Add(p22); list.Add(p23); list.Add(p24); //list.Add(p25); 
            list.Add(p26); list.Add(p27); 

            return list;
        }


        private IEnumerable<dynamic> QueryList(string query)
        {
            IEnumerable<dynamic> result = null;
            IDbConnection conn = null;
            try
            {
                conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                int intentos = 0;
                while (intentos < 3)
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        intentos = 99; // Éxito
                        result = SqlMapper.Query(conn, query.Replace("\r\n", " "));

                        conn.Close();
                        return result;
                    }
                    else
                    {
                        intentos++;
                        Thread.Sleep(1000);
                        conn.Open();
                    }
                }
                if (intentos < 99) // Falla
                {
                    throw new Exception("Error de conexión - Estado: " + conn.State);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " -> " + ex.InnerException.Message);
            }
            finally
            {
                try { conn.Close(); } catch (Exception) { }
            }
            return null;
        }

        public List<Payment> GetPayments(string period, string prefixCompany)
        {
            List<Payment> paymentList = new List<Payment>();
            string query =
                " Select e.fecingre, NUMCONSE, e.CODINGRE, VALINGRE " +
                " From   Sfts018t e, Sfts019t d, Sfts015t g " +
                " Where e.codempre = d.codempre " +
                " And e.codingre = d.codingre " +
                " And d.Codempre = g.Codempre " +
                " And d.codgring = g.codgring " +
                " And d.codsbing = g.codsbing " +
                " And g.gemerlin = 'S' " +
                " And TO_CHAR(e.fecingre,'YYYYMM') = '" + GetNextPeriod(period) + "'" +
                " and SUBSTR(d.DESTERCE,1,9) = '" + prefixCompany.Substring(0,9) + "'" +
                " order by e.fecingre, e.codingre ";

            IEnumerable<dynamic> items = QueryList(query);
            foreach (var item in items)
            {
                Payment payment = new Payment();
                payment.Code = item.CODINGRE;
                payment.FechaRecaudo = item.FECINGRE;
                payment.Consecutive = item.NUMCONSE;
                payment.PaymentValue= (double) item.VALINGRE;
                payment.group_id = prefixCompany;
                paymentList.Add(payment);
            }

            return paymentList;
        }

        private string GetNextPeriod(string period)
        {
            DateTime previousMonth = DateTime.ParseExact(period + "01", "yyyyMMdd", CultureInfo.InvariantCulture);
            return previousMonth.AddMonths(1).Date.ToString("yyyyMM");
        }
    }
}