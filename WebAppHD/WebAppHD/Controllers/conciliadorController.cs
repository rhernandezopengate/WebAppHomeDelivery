using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHD.Models;
using System.Linq.Dynamic;

namespace WebAppHD.Controllers
{
    public class conciliadorController : Controller
    {
        DB_A3F19C_OGEntities dbHD = new DB_A3F19C_OGEntities();

        // GET: conciliador
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BusquedaOrdenesGuias()
        {
            var Draw = Request.Form.GetValues("draw").FirstOrDefault();
            var Start = Request.Form.GetValues("start").FirstOrDefault();
            var Length = Request.Form.GetValues("length").FirstOrDefault();
            var SortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
            var SortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var Orden = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            var Guia = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
            
            int PageSize = Length != null ? Convert.ToInt32(Length) : 0;
            int Skip = Start != null ? Convert.ToInt32(Start) : 0;
            int TotalRecords = 0;

            try
            {
                List<OrdenesCerradasVM> listaOrdenesCerradas = new List<OrdenesCerradasVM>();

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
                {
                    con.Open();

                    string sql = "exec [SP_Guias_ParametrosOpcionales] @orden, @guia";
                    var query = new SqlCommand(sql, con);

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

        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult CargarConciliador()
        {
            ViewBag.Mensaje = "Inicio";
            return View();
        }

        [HttpPost]        
        public ActionResult CargarConciliador(HttpPostedFileBase postedFileBase)
        {
            try
            {
                string filePath = string.Empty;

                if (postedFileBase != null)
                {
                    string path = Server.MapPath("~/Uploads/");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    filePath = path + Path.GetFileName(postedFileBase.FileName);
                    string extension = Path.GetExtension(postedFileBase.FileName);
                    postedFileBase.SaveAs(filePath);

                    DataTable dt = new DataTable();

                    dt.Columns.AddRange(new DataColumn[2] {
                        //1
                        new DataColumn("orden", typeof(string)),
                        //2                        
                        new DataColumn("guia", typeof(string)),
                    });

                    string csvData = System.IO.File.ReadAllText(filePath);

                    foreach (string row in csvData.Split('\n'))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            dt.Rows.Add();
                            int i = 0;

                            //Execute a loop over the columns.
                            foreach (string cell in row.Split(','))
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = cell;

                                i++;
                            }
                        }
                    }

                    dt.AcceptChanges();

                    DataTable dtCaptura = new DataTable();

                    dtCaptura.Columns.AddRange(new DataColumn[2] {
                        //1
                        new DataColumn("Ordenes_Id", typeof(int)),
                        //2                        
                        new DataColumn("Guia", typeof(string)),
                    });

                    foreach (DataRow orow in dt.Select())
                    {                        
                        string guia = orow["guia"].ToString();
                        string orden = orow["orden"].ToString();

                        var guiaTemp = dbHD.guias.OrderByDescending(x => x.id).Take(10000).Where(x => x.Guia.Equals(guia)).FirstOrDefault();

                        if (guiaTemp == null)
                        {
                            var ordenes = dbHD.ordenes.OrderByDescending(x => x.id).Take(10000).Where(x => x.Orden.Equals(orden)).FirstOrDefault();

                            if (ordenes != null)
                            {
                                DataRow rowCaptura = dtCaptura.NewRow();
                                rowCaptura["Ordenes_Id"] = ordenes.id;
                                rowCaptura["Guia"] = guia;
                                dtCaptura.Rows.Add(rowCaptura);
                            }
                        }
                    }

                    if (dtCaptura.Rows.Count > 0)
                    {
                        string conString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                        using (SqlConnection con = new SqlConnection(conString))
                        {
                            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                            {
                                //Set the database table name.
                                sqlBulkCopy.DestinationTableName = "dbo.guias";
                                sqlBulkCopy.ColumnMappings.Add("Guia", "Guia");
                                sqlBulkCopy.ColumnMappings.Add("Ordenes_Id", "Ordenes_Id");

                                con.Open();

                                sqlBulkCopy.WriteToServer(dtCaptura);
                                con.Close();
                            }
                        }

                        ViewBag.Mensaje = "Correcto";
                        return View();
                    }
                    else
                    {
                        ViewBag.Mensaje = "No se encontraron registros nuevos";
                        return View();
                    }                   
                }
                else
                {
                    ViewBag.Mensaje = "Seleccione un archivo.";
                    return View();
                }                
            }
            catch (Exception _ex)
            {
                ViewBag.Mensaje = _ex.Message.ToString();
                return View();
            }
        }
    }
}