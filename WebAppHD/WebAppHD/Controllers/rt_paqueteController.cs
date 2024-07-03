using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHD.Models;
using System.Linq.Dynamic;

namespace WebAppHD.Controllers
{
    public class rt_paqueteController : Controller
    {
        // GET: rt_paquete
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,gerentegeneral,coordinadorinventarios")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ObtenerPaquetes()
        {
            var Draw = Request.Form.GetValues("draw").FirstOrDefault();
            var Start = Request.Form.GetValues("start").FirstOrDefault();
            var Length = Request.Form.GetValues("length").FirstOrDefault();
            var SortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
            var SortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            
            var FechaAltaInicio = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            var FechaAltaFin = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
            var Guia = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
            var Sku = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();

            int PageSize = Length != null ? Convert.ToInt32(Length) : 0;
            int Skip = Start != null ? Convert.ToInt32(Start) : 0;
            int TotalRecords = 0;

            try
            {
                List<rt_paquete> listaPaquetes = new List<rt_paquete>();

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
                {
                    con.Open();

                    string sql = "exec SP_PAQUETES_INDEX @guia, @sku, @fechaAltaInicio, @fechaAltaFin";
                    var query = new SqlCommand(sql, con);

                    if (FechaAltaInicio != "")
                    {
                        DateTime date = Convert.ToDateTime(FechaAltaInicio);
                        query.Parameters.AddWithValue("@fechaAltaInicio", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaAltaInicio", DBNull.Value);
                    }

                    if (FechaAltaFin != "")
                    {
                        DateTime date = Convert.ToDateTime(FechaAltaFin);
                        query.Parameters.AddWithValue("@fechaAltaFin", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaAltaFin", DBNull.Value);
                    }

                    if (Guia != "")
                    {
                        query.Parameters.AddWithValue("@guia", Guia);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@guia", DBNull.Value);
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
                            var paquetes = new rt_paquete();

                            paquetes.Id = Convert.ToInt32(dr["Id"]);
                            paquetes.Guia = dr["Guia"].ToString();
                            paquetes.Paquete = dr["Paquete"].ToString();
                            paquetes.sku = dr["Sku"].ToString();
                            paquetes.descripcion = dr["Descripcion"].ToString();
                            paquetes.cantidad = Convert.ToInt32(dr["Cantidad"]);
                            DateTime dateFechaAlta = Convert.ToDateTime(dr["Fecha_Alta"].ToString());
                            paquetes.fechaaltastring = dateFechaAlta.ToShortDateString();

                            listaPaquetes.Add(paquetes);
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(SortColumn) && string.IsNullOrEmpty(SortColumnDir)))
                {
                    listaPaquetes = listaPaquetes.OrderBy(SortColumn + " " + SortColumnDir).ToList();
                }

                TotalRecords = listaPaquetes.ToList().Count();
                var NewItems = listaPaquetes.Skip(Skip).Take(PageSize == -1 ? TotalRecords : PageSize).ToList();

                return Json(new { draw = Draw, recordsFiltered = TotalRecords, recordsTotal = TotalRecords, data = NewItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.Message.ToString());
                return null;
            }

        }
    }
}