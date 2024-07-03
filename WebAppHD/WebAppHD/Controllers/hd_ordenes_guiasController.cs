using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebAppHD.Models;
using System.Linq.Dynamic;
using System.Text;

namespace WebAppHD.Controllers
{
    public class hd_ordenes_guiasController : Controller
    {
        private DB_A3F19C_OGEntities1 db = new DB_A3F19C_OGEntities1();

        // GET: hd_ordenes_guias
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ObetenerOrdenesGuias()
        {
            var Draw = Request.Form.GetValues("draw").FirstOrDefault();
            var Start = Request.Form.GetValues("start").FirstOrDefault();
            var Length = Request.Form.GetValues("length").FirstOrDefault();
            var SortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
            var SortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var orden = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
            var guia = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();            
            var fechaCierreInicio = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
            var fechaCierreFin = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
            var fechaAlta = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
            var Estado = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();

            int PageSize = Length != null ? Convert.ToInt32(Length) : 0;
            int Skip = Start != null ? Convert.ToInt32(Start) : 0;
            int TotalRecords = 0;

            try
            {
                List<hd_ordenes_guias> listaOrdenes = new List<hd_ordenes_guias>();

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
                {
                    con.Open();

                    string sql = "exec [SP_ORDENES_GUIAS_INDEX] @orden, @guia, @fechaCierreInicio, @fechaCierreFin, @fechaAlta, @idstatus";
                    var query = new SqlCommand(sql, con);

                    if (orden != "")
                    {                        
                        query.Parameters.AddWithValue("@orden", orden);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@orden", DBNull.Value);
                    }

                    if (guia != "")
                    {
                        query.Parameters.AddWithValue("@guia", guia);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@guia", DBNull.Value);
                    }

                    if (fechaCierreInicio != "")
                    {
                        DateTime date = Convert.ToDateTime(fechaCierreInicio);
                        query.Parameters.AddWithValue("@fechaCierreInicio", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaCierreInicio", DBNull.Value);
                    }

                    if (fechaCierreFin != "")
                    {                       
                        DateTime date = Convert.ToDateTime(fechaCierreFin);
                        query.Parameters.AddWithValue("@fechaCierreFin", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaCierreFin", DBNull.Value);
                    }

                    if (fechaAlta != "")
                    {
                        DateTime date = Convert.ToDateTime(fechaAlta);
                        query.Parameters.AddWithValue("@fechaAlta", date);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@fechaAlta", DBNull.Value);
                    }

                    if (Estado == "" || Estado == "0")
                    {
                        query.Parameters.AddWithValue("@idstatus", DBNull.Value);
                    }
                    else
                    {
                        query.Parameters.AddWithValue("@idstatus", Estado);
                    }

                    using (var dr = query.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // facturas
                            var ordenes = new hd_ordenes_guias();

                            ordenes.Id = Convert.ToInt32(dr["id"]);
                            ordenes.Orden = dr["Orden"].ToString();
                            ordenes.Guia = dr["Guia"].ToString();
                            ordenes.status = dr["Descripcion"].ToString();
                            DateTime dateFechaAlta = Convert.ToDateTime(dr["FechaAlta"].ToString());
                            ordenes.fechaAltaString = dateFechaAlta.ToShortDateString() + " " + dateFechaAlta.ToShortTimeString();

                            if (dr["FechaCierre"].ToString().Equals(string.Empty))
                            {
                                ordenes.fechaCierreString = "";
                            }
                            else
                            {
                                DateTime dateFechaCierre = Convert.ToDateTime(dr["FechaCierre"].ToString());                                                      
                                ordenes.fechaCierreString = dateFechaCierre.ToShortDateString() + " " + dateFechaCierre.ToShortTimeString();
                            }                            

                            listaOrdenes.Add(ordenes);
                        }
                    }
                }

                if (!(string.IsNullOrEmpty(SortColumn) && string.IsNullOrEmpty(SortColumnDir)))
                {
                    listaOrdenes = listaOrdenes.OrderBy(SortColumn + " " + SortColumnDir).ToList();
                }

                TotalRecords = listaOrdenes.ToList().Count();
                var NewItems = listaOrdenes.Skip(Skip).Take(PageSize == -1 ? TotalRecords : PageSize).ToList();

                return Json(new { draw = Draw, recordsFiltered = TotalRecords, recordsTotal = TotalRecords, data = NewItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.Message.ToString());
                return null;
            }
        }

        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult Importar() 
        {
            ViewBag.Mensaje = "Inicio";
            List<ErroresCargaOrdenes> listaErrores = new List<ErroresCargaOrdenes>();
            return View(listaErrores);
        }

        [HttpPost]
        public ActionResult Importar(HttpPostedFileBase postedFile)
        {
            try
            {
                string filePath = string.Empty;
                string path = Server.MapPath("~/Uploads/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(postedFile.FileName);
                string extension = Path.GetExtension(postedFile.FileName);
                postedFile.SaveAs(filePath);

                DataTable dtCharge = new DataTable();

                dtCharge.Columns.AddRange(new DataColumn[2] {
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
                        dtCharge.Rows.Add();
                        int i = 0;

                        //Execute a loop over the columns.
                        foreach (string cell in row.Split(','))
                        {
                            dtCharge.Rows[dtCharge.Rows.Count - 1][i] = cell;

                            i++;
                        }
                    }
                }

                DateTime fechaHoy = DateTime.Now.AddHours(2);
                string id = db.AspNetUsers.Where(x => x.Email.Contains(User.Identity.Name)).FirstOrDefault().Id;

                DataTable dtOrden = new DataTable();

                dtOrden.Columns.AddRange(new DataColumn[5] {
                            new DataColumn("FechaAlta", typeof(DateTime)),
                            new DataColumn("hd_status_ordenes_guias_Id", typeof(int)),
                            new DataColumn("Orden", typeof(string)),
                            new DataColumn("Guia", typeof(string)),
                            new DataColumn("AspNetUsers_Id", typeof(string)),                
                });

                List<ErroresCargaOrdenes> listaErrores = new List<ErroresCargaOrdenes>();

                int folioerror = 1;

                foreach (DataRow row in dtCharge.Rows)
                {
                    string orden_buscar = row[0].ToString().Trim();

                    var ordenBuscar = db.hd_ordenes_guias.OrderByDescending(x => x.Id).Take(50000).Where(x => x.Orden.Contains(orden_buscar)).FirstOrDefault();

                    if (ordenBuscar == null)
                    {
                        hd_ordenes_guias ordenes = new hd_ordenes_guias();

                        dtOrden.Rows.Add(new object[]
                        {
                        fechaHoy,
                        1,
                        row[0].ToString().Trim(),
                        row[1].ToString().Trim(),
                        id
                        });
                    }
                    else
                    {
                        ErroresCargaOrdenes erroresordenes = new ErroresCargaOrdenes();
                        string orden = row[0].ToString().Trim();
                        erroresordenes.Folio = folioerror++;
                        erroresordenes.Error = "Orden Ya existe";
                        erroresordenes.SKU = orden;
                        listaErrores.Add(erroresordenes);
                    }
                }

                if (listaErrores.Count > 0)
                {
                    ViewBag.Mensaje = "Error";
                    return View(listaErrores);
                }
                else
                {
                    if (CrearOrdenes(dtOrden))
                    {
                        ViewBag.Mensaje = "Correcto";
                    }
                    else
                    {
                        ViewBag.Mensaje = "Error Cargar Ordenes";
                    }

                    return View(listaErrores);
                }                
            }
            catch (Exception _ex)
            {
                ViewBag.Mensaje = _ex.Message.ToString();                
                return View();
            }            
        }

        public bool CrearOrdenes(DataTable dtOrdenes)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = "dbo.hd_ordenes_guias";

                bulkCopy.ColumnMappings.Add("FechaAlta", "FechaAlta");
                bulkCopy.ColumnMappings.Add("Guia", "Guia");
                bulkCopy.ColumnMappings.Add("Orden", "Orden");
                bulkCopy.ColumnMappings.Add("AspNetUsers_Id", "AspNetUsers_Id");
                bulkCopy.ColumnMappings.Add("hd_status_ordenes_guias_Id", "hd_status_ordenes_guias_Id");

                try
                {
                    bulkCopy.WriteToServer(dtOrdenes);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        // GET: hd_ordenes_guias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            hd_ordenes_guias hd_ordenes_guias = db.hd_ordenes_guias.Find(id);
            if (hd_ordenes_guias == null)
            {
                return HttpNotFound();
            }
            return View(hd_ordenes_guias);
        }

        // GET: hd_ordenes_guias/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: hd_ordenes_guias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FechaAlta,Guia,Orden,AspNetUsers_Id,hd_status_ordenes_guias_Id,FechaCierre")] hd_ordenes_guias hd_ordenes_guias)
        {
            if (ModelState.IsValid)
            {
                db.hd_ordenes_guias.Add(hd_ordenes_guias);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(hd_ordenes_guias);
        }

        // GET: hd_ordenes_guias/Edit/5
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            hd_ordenes_guias hd_ordenes_guias = db.hd_ordenes_guias.Find(id);
            ViewBag.hd_status_ordenes_guias_Id = new SelectList(db.hd_status_ordenes_guias, "Id", "Descripcion", hd_ordenes_guias.hd_status_ordenes_guias_Id);
            if (hd_ordenes_guias == null)
            {
                return HttpNotFound();
            }
            return View(hd_ordenes_guias);
        }

        // POST: hd_ordenes_guias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(hd_ordenes_guias hd_ordenes_guias)
        {
            hd_ordenes_guias ordenTemp = db.hd_ordenes_guias.Find(hd_ordenes_guias.Id);

            try
            {
                ordenTemp.Guia = hd_ordenes_guias.Guia;
                ordenTemp.Orden = hd_ordenes_guias.Orden;
                ordenTemp.hd_status_ordenes_guias_Id = hd_ordenes_guias.hd_status_ordenes_guias_Id;
                db.SaveChanges();
                return Json(new { success = true, message = "Editado Correctamente." });
            }
            catch (Exception _ex) 
            {
                return Json(new { success = false, message = _ex.Message.ToString() });
            }            
        }

        // GET: hd_ordenes_guias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            hd_ordenes_guias hd_ordenes_guias = db.hd_ordenes_guias.Find(id);
            if (hd_ordenes_guias == null)
            {
                return HttpNotFound();
            }
            return View(hd_ordenes_guias);
        }

        // POST: hd_ordenes_guias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            hd_ordenes_guias hd_ordenes_guias = db.hd_ordenes_guias.Find(id);
            db.hd_ordenes_guias.Remove(hd_ordenes_guias);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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

    }
}
