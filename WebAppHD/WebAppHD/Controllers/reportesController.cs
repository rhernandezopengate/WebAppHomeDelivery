using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebAppHD.Models;
using System.Linq.Dynamic;

namespace WebAppHD.Controllers
{
    public class reportesController : Controller
    {
        DB_A3F19C_OGEntities db = new DB_A3F19C_OGEntities();
        // GET: reportes
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd,gerentegeneral,coordinadorinventarios,analistainventarios,direcciongeneral")]
        public ActionResult Index()
        {
			return View();
		}

        [HttpPost]
        public ActionResult ReporteOrdenesCerradas()
        {
            var Draw = Request.Form.GetValues("draw").FirstOrDefault();
            var Start = Request.Form.GetValues("start").FirstOrDefault();
            var Length = Request.Form.GetValues("length").FirstOrDefault();
            var SortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
            var SortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var FechaAlta = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            var Estado = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
            var Orden = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
            var Guia = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
            var FechaCierreInicio = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
            var FechaCierreFin = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();

            int PageSize = Length != null ? Convert.ToInt32(Length) : 0;
            int Skip = Start != null ? Convert.ToInt32(Start) : 0;
            int TotalRecords = 0;

            try
            {
                List<OrdenesCerradasVM> listaOrdenesCerradas = new List<OrdenesCerradasVM>();

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
                {
                    con.Open();

                    string sql = "exec [SP_ORDENES_INDEX] @fechaAlta, @idstatus, @orden, @guia, @fechaCierreInicio, @fechaCierreFin";
                    var query = new SqlCommand(sql, con);

                    if (FechaAlta != "")
                    {
                        DateTime date = Convert.ToDateTime(FechaAlta);
                        query.Parameters.AddWithValue("@fechaAlta", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaAlta", DBNull.Value);
                    }

                    if (FechaCierreInicio != "")
                    {
                        DateTime date = Convert.ToDateTime(FechaCierreInicio);
                        query.Parameters.AddWithValue("@fechaCierreInicio", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaCierreInicio", DBNull.Value);
                    }

                    if (FechaCierreFin != "")
                    {
                        DateTime date = Convert.ToDateTime(FechaCierreFin);
                        query.Parameters.AddWithValue("@fechaCierreFin", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaCierreFin", DBNull.Value);
                    }

                    if (Estado == "" || Estado == "0")
                    {
                        query.Parameters.AddWithValue("@idstatus", DBNull.Value);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@idstatus", Estado);
                    }

                    if (Orden != "")
                    {                        
                        query.Parameters.AddWithValue("@orden", Orden);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@orden", DBNull.Value);
                    }

                    if (Guia != "")
                    {                        
                        query.Parameters.AddWithValue("@guia", Guia);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@guia", DBNull.Value);
                    }

                    using (var dr = query.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // facturas
                            var ordenes = new OrdenesCerradasVM();

                            ordenes.Id = Convert.ToInt32(dr["id"]);                            
                            ordenes.Orden = dr["Orden"].ToString();
                            ordenes.Guia = dr["Guia"].ToString();
                            ordenes.Estatus = dr["descripcion"].ToString();
                            ordenes.Picker = dr["Picker"].ToString();
                            ordenes.Auditor = dr["usuario"].ToString();

                            DateTime dateFechaAlta = Convert.ToDateTime(dr["FechaAlta"].ToString());
                            ordenes.FechaAlta = dateFechaAlta.ToShortDateString() + " " + dateFechaAlta.ToShortTimeString();

                            if (dr["FechaCierre"].ToString().Equals(string.Empty))
                            {
                                ordenes.FechaCierre = "";
                            }
                            else
                            {
                                DateTime dateFechaCierre = Convert.ToDateTime(dr["FechaCierre"].ToString());
                                ordenes.FechaCierre = dateFechaCierre.ToShortDateString() + " " + dateFechaCierre.ToShortTimeString();
                            }

                            listaOrdenesCerradas.Add(ordenes);
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(SortColumn) && string.IsNullOrEmpty(SortColumnDir)))
                {
                    listaOrdenesCerradas = listaOrdenesCerradas.OrderBy(SortColumn + " " + SortColumnDir).ToList();
                }

                TotalRecords = listaOrdenesCerradas.ToList().Count();
                var NewItems = listaOrdenesCerradas.Skip(Skip).Take(PageSize == -1 ? TotalRecords : PageSize).ToList();

                return Json(new { draw = Draw, recordsFiltered = TotalRecords, recordsTotal = TotalRecords, data = NewItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.Message.ToString());
                return null;
            }
        }

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }

        [Authorize(Roles = "admin")]

        public ActionResult ReporteGraficaPuntos() 
		{
			return View();
		}
		
		public ContentResult JSONView(string _mes, string _anio)
		{
			List<DataPoint> dataPoints = new List<DataPoint>();

            using (DB_A3F19C_OGEntities db = new DB_A3F19C_OGEntities())
            {
                int year = int.Parse(_anio);
                int moth = int.Parse(_mes);		

				List<int> dias = new List<int>() {
					1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30
				};

                foreach (var item in dias)
                {
                    int qtyordenes = (from o in db.ordenes
                                      where o.FechaCierre.Value.Year.Equals(year)
                                      && o.FechaCierre.Value.Month.Equals(moth)
                                      && o.FechaCierre.Value.Day.Equals(item)                                      
                                      && o.StatusOrdenImpresa_Id == 3
                                      select o.Orden).Count();
					
					DateTime fecha = new DateTime(year, moth, item, 0, 0, 0, 0);

                    if (item <= 30)
                    {
                        if (qtyordenes > 0)
                        {
                            dataPoints.Add(new DataPoint(ConvertToTS(fecha), qtyordenes));
                        }                        
                    }
                }
			}
			
			JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

			return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
		}

		public static long ConvertToTS(DateTime datetime)
		{
			DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return (long)(datetime.AddDays(1) - sTime).TotalMilliseconds;
		}

		public ActionResult PiezasSurtidas()
		{
			List<DataPoint2> dataPoints = new List<DataPoint2>();

            using (db = new DB_A3F19C_OGEntities())
            {
				var query = (from o in db.ordenes
							join dt in db.detordenproductoshd on o.id equals dt.Ordenes_Id
							join s in db.skus on dt.Skus_Id equals s.id
							 where o.FechaCierre.Value.Year.Equals(2023)
							 && o.FechaCierre.Value.Month.Equals(6)
							 && o.StatusOrdenImpresa_Id == 3
							 group dt by new { s.Sku, dt.cantidad } into g
							 select new { g.Key.Sku, cantidad = g.Sum(x => x.cantidad)  }).Take(15).ToList();

                foreach (var item in query.OrderBy(x => x.cantidad))
                {
					dataPoints.Add(new DataPoint2(item.Sku, (double)item.cantidad * -1));
				}
			}

			ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

			return View();
		}

        public ActionResult Dashboard() 
        {
            List<Dashboard> listaDashboard = new List<Dashboard>();
            using (db = new DB_A3F19C_OGEntities())
            {
                

                var listaOrdenes = (from o in db.ordenes
                                    where o.FechaAlta.Value.Year.Equals(2024) && 
                                          o.FechaAlta.Value.Month.Equals(2) && 
                                          o.FechaCierre.Value.Day.Equals(19)
                                    select new { o.id, o.Orden }).ToList();

                var listaCSR = (from c in db.csr
                                where c.FechaCarga.Value.Year.Equals(2024) && c.FechaCarga.Value.Month.Equals(2)
                                select new { c.Referencia }).ToList();

                var listaConciliador = (from c in db.guias
                                        select new { c.id, c.Ordenes_Id, c.Guia }).OrderByDescending(x => x.id).Take(50000).ToList();
                                

                foreach (var item in listaOrdenes)
                {
                    Dashboard dashboard = new Dashboard();
                    var csrTemp = listaCSR.Where(x => x.Referencia.Equals(item.Orden)).FirstOrDefault();
                    var guiasTemp = listaConciliador.Where(x => x.Ordenes_Id.Equals(item.id)).FirstOrDefault();

                    if (csrTemp != null)
                    {
                        dashboard.OrdenCSR = csrTemp.Referencia;
                    }
                    else
                    {
                        dashboard.OrdenCSR = "No se encuentra en CSR";
                    }

                    if (guiasTemp != null)
                    {
                        dashboard.GuiaConciliador = guiasTemp.Guia;
                    }
                    else
                    {
                        dashboard.GuiaConciliador = "No se encuentra en Conciliador";
                    }

                    dashboard.Orden = item.Orden;

                    listaDashboard.Add(dashboard);
                }
            }

            return View(listaDashboard);
        }

        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd,gerentegeneral,coordinadorinventarios,analistainventarios,direcciongeneral,finanzas")]
        public ActionResult Productividad()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;            

            var consulta_ordenes = db.ordenes.Where(o => o.FechaAlta.Value.Year == year && o.FechaAlta.Value.Month == month && o.FechaAlta.Value.Day == day).ToList();

            ViewBag.OrdenesTotales = consulta_ordenes.Count();

            ViewBag.OtrosStatus = consulta_ordenes.Where(x => x.StatusOrdenImpresa_Id == 2 || x.StatusOrdenImpresa_Id == 4 || x.StatusOrdenImpresa_Id == 5 || x.StatusOrdenImpresa_Id == 6 || x.StatusOrdenImpresa_Id == 7).Count();

            ViewBag.OrdenesCerradas = consulta_ordenes.Where(x => x.StatusOrdenImpresa_Id == 3).Count();

            ViewBag.OrdenesAbiertas = consulta_ordenes.Where(x => x.StatusOrdenImpresa_Id == 1).Count();

            ViewBag.PiezasAuditadas = db.detordenproductoshd.Where(o => o.ordenes.FechaAlta.Value.Year == year && o.ordenes.FechaAlta.Value.Month == month && o.ordenes.FechaAlta.Value.Day == day && o.ordenes.StatusOrdenImpresa_Id == 3).Sum(x => x.cantidad);

            ViewBag.DataPoints = JsonConvert.SerializeObject(listaOrdenes(year, month, day).OrderByDescending(x => x.Y));
            
            ViewBag.Tabla = listaPiezas(year, month, day).OrderByDescending(x => x.QtyPiezas);

            return View();
        }

        public List<reporteproductividades> listaPiezas(int year, int month, int day) 
        { 
            var pickers = db.pickers.OrderBy(x => x.Nombre).ToList();

            List<reporteproductividades> dataPoints = new List<reporteproductividades>();

            var detalles = db.detordenproductoshd.Where(o => o.ordenes.FechaAlta.Value.Year == year && o.ordenes.FechaAlta.Value.Month == month && o.ordenes.FechaAlta.Value.Day == day && o.ordenes.StatusOrdenImpresa_Id == 3);
            var errores = db.erroresordenes.Where(o => o.ordenes.FechaAlta.Value.Year == year && o.ordenes.FechaAlta.Value.Month == month && o.ordenes.FechaAlta.Value.Day == day && o.ordenes.StatusOrdenImpresa_Id == 3);

            foreach (var item in pickers)
            {
                
                int? piezas = detalles.Where(x => x.ordenes.Picker.Contains(item.Nombre)).Sum(x => x.cantidad) * -1;

                if (piezas > 0)
                {
                    reporteproductividades reporte = new reporteproductividades();
                    int qty_errores = (int)errores.Where(x => x.ordenes.Picker.Contains(item.Nombre)).Count();

                    reporte.Picker = item.Nombre;
                    reporte.QtyPiezas = (int)piezas;
                    reporte.QtyErrores = qty_errores;

                    dataPoints.Add(reporte);
                }               
            }

            return dataPoints;
        }

        public List<DataPoint3> listaOrdenes(int year, int month, int day)
        {

            List<DataPoint3> dataPoints = new List<DataPoint3>();

            foreach (var item in db.pickers.ToList())
            {
                string pk = item.Nombre.ToLower();

                var query = db.ordenes.Where(x => x.Picker.Contains(pk) && x.FechaCierre.Value.Year == year && 
                                             x.FechaCierre.Value.Month == month && 
                                             x.FechaCierre.Value.Day == day && 
                                             x.StatusOrdenImpresa_Id == 3).Select(x => x.Orden).Count();


                if (query > 0)
                {
                    DataPoint3 data = new DataPoint3(item.Nombre, query);
                    dataPoints.Add(data);
                }
            }  

            return dataPoints;
        }

        public ActionResult ConteoOrdenes(string _mes, string _anio)
        {
            List<DashboardGraficaPuntos> listaDashborad = new List<DashboardGraficaPuntos>();

            List<int> diasEnero = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
            int mes = int.Parse(_mes);
            int anio = int.Parse(_anio);

            foreach (var item in diasEnero)
            {               
                DashboardGraficaPuntos dash = new DashboardGraficaPuntos();

                var qty_ordenes = db.ordenes.Where(x => x.FechaCierre.Value.Year.Equals(anio) && x.FechaCierre.Value.Month.Equals(mes) && x.FechaCierre.Value.Day.Equals(item) && x.StatusOrdenImpresa_Id == 3).ToList().Count();

                var qty_lineas = (from o in db.ordenes
                                  join dt in db.detordenproductoshd on o.id equals dt.Ordenes_Id
                                  where o.FechaCierre.Value.Year.Equals(anio) && o.FechaCierre.Value.Month.Equals(mes) && o.FechaCierre.Value.Day.Equals(item) && o.StatusOrdenImpresa_Id == 3
                                  select new { o.Orden, dt.cantidad }).Count();

                var qty_sku = (from o in db.ordenes
                               join dt in db.detordenproductoshd on o.id equals dt.Ordenes_Id
                               where o.FechaCierre.Value.Year.Equals(anio) && o.FechaCierre.Value.Month.Equals(mes) && o.FechaCierre.Value.Day.Equals(item) && o.StatusOrdenImpresa_Id == 3
                               group dt by dt.Skus_Id into dtGroup
                               select dtGroup.Key).Count();

                var qty_piezas = (from o in db.ordenes
                                  join dt in db.detordenproductoshd on o.id equals dt.Ordenes_Id
                                  where o.FechaCierre.Value.Year.Equals(anio) && o.FechaCierre.Value.Month.Equals(mes) && o.FechaCierre.Value.Day.Equals(item) && o.StatusOrdenImpresa_Id == 3
                                  select dt).Sum(x => x.cantidad);


                if (qty_ordenes > 0)
                {
                    if (item < 10)
                    {
                        dash.Fecha = "0" + item + "/0" + mes + "/" + anio;
                    }
                    else
                    {
                        dash.Fecha = "" + item + "/0" + mes + "/" + anio;
                    }

                    dash.QtyOrdenes = qty_ordenes;
                    dash.QtySku = qty_sku;
                    dash.QtyPiezas = (int)qty_piezas;
                    dash.Lineas = qty_lineas;

                    listaDashborad.Add(dash);
                }
            }

            return View(listaDashborad);
        }

        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd,gerentegeneral,coordinadorinventarios,analistainventarios")]
        public ActionResult PiezasOrdenesCerradas()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ReportePiezasOrdenesCerradas()
        {
            var Draw = Request.Form.GetValues("draw").FirstOrDefault();
            var Start = Request.Form.GetValues("start").FirstOrDefault();
            var Length = Request.Form.GetValues("length").FirstOrDefault();
            var SortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
            var SortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
             
            var FechaCierreInicio = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            var FechaCierreFin = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
            var Orden = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
            var Sku = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();

            int PageSize = Length != null ? Convert.ToInt32(Length) : 0;
            int Skip = Start != null ? Convert.ToInt32(Start) : 0;
            int TotalRecords = 0;

            try
            {
                List<OrdenesCerradasVM> listaOrdenesCerradas = new List<OrdenesCerradasVM>();

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
                {
                    con.Open();

                    string sql = "exec [SP_PIEZAS_ORDENESCERRADAS] @fechaFInicio, @fechaFFin, @orden, @sku";
                    var query = new SqlCommand(sql, con);



                    if (FechaCierreInicio != "")
                    {
                        DateTime date = Convert.ToDateTime(FechaCierreInicio);
                        query.Parameters.AddWithValue("@fechaFInicio", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaFInicio", DBNull.Value);
                    }

                    if (FechaCierreFin != "")
                    {
                        DateTime date = Convert.ToDateTime(FechaCierreFin);
                        query.Parameters.AddWithValue("@fechaFFin", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaFFin", DBNull.Value);
                    }

                    if (Orden != "")
                    {
                        query.Parameters.AddWithValue("@orden", Orden);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@orden", DBNull.Value);
                    }

                    if (Sku != "")
                    {
                        query.Parameters.AddWithValue("@sku", Sku);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@sku", DBNull.Value);
                    }

                    using (var dr = query.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // facturas
                            var ordenes = new OrdenesCerradasVM();

                            ordenes.Id = Convert.ToInt32(dr["id"]);
                            ordenes.Orden = dr["Orden"].ToString();                            
                            ordenes.OracleID = dr["OracleID"].ToString();
                            ordenes.SKU = dr["Sku"].ToString();
                            ordenes.Cantidad = dr["cantidad"].ToString();
                            ordenes.Estatus = dr["descripcion"].ToString();
                            ordenes.Picker = dr["Picker"].ToString();
                            DateTime dateFechaAlta = Convert.ToDateTime(dr["FechaAlta"].ToString());
                            ordenes.FechaAlta = dateFechaAlta.ToShortDateString() + " " + dateFechaAlta.ToShortTimeString();

                            if (dr["FechaCierre"].ToString().Equals(string.Empty))
                            {
                                ordenes.FechaCierre = "";
                            }
                            else
                            {
                                DateTime dateFechaCierre = Convert.ToDateTime(dr["FechaCierre"].ToString());
                                ordenes.FechaCierre = dateFechaCierre.ToShortDateString() + " " + dateFechaCierre.ToShortTimeString();
                            }

                            listaOrdenesCerradas.Add(ordenes);
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(SortColumn) && string.IsNullOrEmpty(SortColumnDir)))
                {
                    listaOrdenesCerradas = listaOrdenesCerradas.OrderBy(SortColumn + " " + SortColumnDir).ToList();
                }

                TotalRecords = listaOrdenesCerradas.ToList().Count();
                var NewItems = listaOrdenesCerradas.Skip(Skip).Take(PageSize == -1 ? TotalRecords : PageSize).ToList();

                return Json(new { draw = Draw, recordsFiltered = TotalRecords, recordsTotal = TotalRecords, data = NewItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.Message.ToString());
                return null;
            }
        }

        [Authorize(Roles = "admin")]
        public ActionResult ProductividadesDashboard() 
        {
            return View();
        }

        public ActionResult ProductividadPickers(int mes, int anio)
        {            
            List<int> dias = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

            var pickers = db.pickers.Where(x => x.statuspickers_Id == 1).ToList();

            var picker_from_ordenes = db.ordenes.Where(x => x.FechaCierre.Value.Year == anio &&
                                                            x.FechaCierre.Value.Month == mes &&                                                               
                                                            x.StatusOrdenImpresa_Id == 3
                                                            ).GroupBy(x => x.Picker).Select(x => x.Key).ToList();
            
            List<Productividades> lista = new List<Productividades>();

            foreach (var item in dias)
            {
                foreach (var pk in picker_from_ordenes)
                {

                    var consulta_picker = db.ordenes.Where(x => x.Picker.Contains(pk) &&
                                                           x.FechaCierre.Value.Year == anio &&
                                                           x.FechaCierre.Value.Month == mes &&
                                                           x.FechaCierre.Value.Day == item &&
                                                           x.StatusOrdenImpresa_Id == 3
                                                           ).ToList();

                    var qty_ordenes = consulta_picker.Count();

                    if (qty_ordenes != 0)
                    {

                        var consulta_piezas = db.detordenproductoshd.Where(x => x.ordenes.Picker == pk &&
                                                           x.ordenes.FechaCierre.Value.Year == anio &&
                                                           x.ordenes.FechaCierre.Value.Month == mes &&
                                                           x.ordenes.FechaCierre.Value.Day == item &&
                                                           x.ordenes.StatusOrdenImpresa_Id == 3
                                                           ).Sum(x => x.cantidad);

                        Productividades productividad = new Productividades();

                        DateTime fechaTemp = new DateTime(anio, mes, item);

                        productividad.Cantidad = qty_ordenes;
                        productividad.Picker = pk;
                        productividad.Fecha = fechaTemp.ToShortDateString();
                        productividad.CantidadPiezas = (int)consulta_piezas;

                        lista.Add(productividad);
                    }
                }
            }

            ViewBag.TablaPickers = lista;

            return View();
        }

        public ActionResult ProductividadesAuditores(int mes, int anio) 
        {
            List<int> dias = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

            List<Productividades> lista = new List<Productividades>();

            var auditores = db.usuarios.ToList();

            foreach (var item in dias)
            {
                foreach (var item2 in auditores)
                {

                    var conteo_ordenes = db.detusuariosordenes.Where(x => x.usuarios.id.Equals(item2.id) &&
                                                                          x.ordenes.FechaCierre.Value.Year == anio &&
                                                                          x.ordenes.FechaCierre.Value.Month == mes &&
                                                                          x.ordenes.FechaCierre.Value.Day == item &&
                                                                          x.ordenes.StatusOrdenImpresa_Id == 3).Count();

                    if (conteo_ordenes > 0)
                    {

                        var consulta = (from o in db.ordenes
                                        join dt in db.detordenproductoshd on o.id equals dt.Ordenes_Id
                                        join du in db.detusuariosordenes on o.id equals du.Ordenes_Id
                                        where du.Usuarios_Id.Equals(item2.id) &&
                                              o.FechaCierre.Value.Year.Equals(anio) &&
                                              o.FechaCierre.Value.Month.Equals(mes) &&
                                              o.FechaCierre.Value.Day.Equals(item) &&
                                              o.StatusOrdenImpresa_Id == 3
                                        select new { dt.cantidad }).Sum(x => x.cantidad);

                        DateTime fecha = new DateTime(anio, mes, item);
                        Productividades productividad = new Productividades();

                        productividad.Picker = item2.nombre;
                        productividad.Cantidad = conteo_ordenes;
                        productividad.Fecha = fecha.ToShortDateString();
                        productividad.CantidadPiezas = (int)consulta;

                        lista.Add(productividad);
                    }
                }
            }


            ViewBag.TablaAuditores = lista;

            return View();
        }
    }
}