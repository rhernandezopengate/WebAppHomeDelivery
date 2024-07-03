using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebAppHD.Models;
using System.Configuration;

namespace WebAppHD.Controllers
{
    public class corteController : Controller
    {
        DB_A3F19C_OGEntities dbHD = new DB_A3F19C_OGEntities();

        // Vista Index
        public ActionResult Index()
        {
            return View();
        }
        // Vista Eliminar Corte

        //Vistas Para Validar y Cargar Cortes HD
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult CargarCorte()
        {
            List<ErroresCargaOrdenes> listaErrores = new List<ErroresCargaOrdenes>();
            ViewBag.Mensaje = "Inicio";

            return View(listaErrores);
        }

        [HttpPost]
        public ActionResult CargarCorte(HttpPostedFileBase postedFile)
        {
            List<ErroresCargaOrdenes> listaErrores = new List<ErroresCargaOrdenes>();

            try
            {
                //Inicio Recepcion y Procesamiento del Archivo

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

                dtCharge.Columns.AddRange(new DataColumn[5] {
                        //1
                        new DataColumn("item", typeof(string)),
                        //2                        
                        new DataColumn("qty", typeof(string)),
                        //3
                        new DataColumn("ea", typeof(string)),
                        //4
                        new DataColumn("order", typeof(string)),
                        //5
                        new DataColumn("createdby", typeof(string)),
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

                //Fin Recepcion y Procesamiento del Archivo

                //Inicio Transmicion de Datos de Un Datatble a una Lista (Mas Legible)

                List<ordenes> listaInformacion_Validar = new List<ordenes>();

                foreach (DataRow row in dtCharge.Rows)
                {

                    try
                    {
                        ordenes ordensku = new ordenes();
                        ordensku.SKU = row[0].ToString().Trim();

                        ordensku.Cantidad = Convert.ToInt32(row[1].ToString().Trim());

                        ordensku.Orden = row[3].ToString().Trim();

                        ordensku.User = row[4].ToString().Trim();

                        listaInformacion_Validar.Add(ordensku);
                    }
                    catch (Exception _ex)
                    {
                        Console.Write(_ex.Message.ToString());
                    }

                   
                }

                //Fin Transmicion de Datos de Un Datatble a una Lista

                //Inicio Busqueda de Errores en SKU y Espacios en Blanco

                var agrupar_skus = listaInformacion_Validar.GroupBy(x => x.SKU).ToList();

                foreach (var item in agrupar_skus)
                {
                    var skubusqueda = dbHD.skus.Where(x => x.Sku.Equals(item.Key)).FirstOrDefault();

                    if (skubusqueda == null)
                    {
                        ErroresCargaOrdenes error = new ErroresCargaOrdenes();
                        error.SKU = item.Key;
                        error.Error = "SKU no existe";
                        listaErrores.Add(error);
                    }
                }

                //Fin Busqueda de Errores en SKU y Espacios en Blanco                             

                if (listaErrores.Count > 0)
                {
                    ViewBag.Mensaje = "Error";
                }
                else
                {
                    List<ordenes> listaValidacionOrdenesDuplicadas = dbHD.ordenes.Where(x => x.FechaAlta.Value.Year.Equals(DateTime.Now.Year) && x.FechaAlta.Value.Month.Equals(DateTime.Now.Month)).ToList();
                    List<ordenes> listaNuevasOrdenes = new List<ordenes>();
                    DateTime FechaAlta = DateTime.Now;
                    DataTable dtOrden = new DataTable();

                    dtOrden.Columns.AddRange(new DataColumn[4] {
                            //1
                            new DataColumn("FechaAlta", typeof(DateTime)),
                            //2                        
                            new DataColumn("StatusOrdenImpresa_Id", typeof(int)),
                            //3
                            new DataColumn("Orden", typeof(string)),
                            //4
                            new DataColumn("User", typeof(string)),
                        });

                    var gourping_ordenes = (from l in listaInformacion_Validar
                                            group l by new { l.Orden, l.User } into g
                                            select new { g.Key.Orden, g.Key.User }).ToList();

                    //Inicio Validacion Ordenes Duplicadas o Nuevas

                    foreach (var item in gourping_ordenes)
                    {
                        var orden_existe = listaValidacionOrdenesDuplicadas.Where(x => x.Orden.Equals(item.Orden)).FirstOrDefault();
                        if (orden_existe == null)
                        {
                            ordenes ordenes = new ordenes();
                            ordenes.Orden = item.Orden;
                            listaNuevasOrdenes.Add(ordenes);

                            dtOrden.Rows.Add(new object[] {
                                    FechaAlta,
                                    1,
                                    item.Orden,
                                    item.User
                                });
                        }
                        else
                        {
                            Console.WriteLine("Orden ya existe");
                        }
                    }

                    //Fin Validacion Ordenes Duplicadas

                    if (listaNuevasOrdenes.Count > 0)
                    {
                        if (CrearOrdenes(dtOrden))
                        {
                            DataTable dtDetalles = new DataTable();

                            dtDetalles.Columns.AddRange(new DataColumn[3] {
                            //1
                            new DataColumn("Ordenes_Id", typeof(int)),
                            //2                        
                            new DataColumn("Skus_Id", typeof(int)),
                            //3
                            new DataColumn("cantidad", typeof(string)),
                        });

                            foreach (var item in listaNuevasOrdenes)
                            {
                                foreach (var item2 in listaInformacion_Validar.Where(x => x.Orden.Equals(item.Orden)).ToList())
                                {
                                    detordenproductoshd detorden = new detordenproductoshd();
                                    int Ordenes_Id = dbHD.ordenes.Where(x => x.Orden.Equals(item2.Orden)).FirstOrDefault().id;
                                    int Skus_Id = dbHD.skus.Where(x => x.Sku.Equals(item2.SKU)).FirstOrDefault().id;
                                    int cantidad = item2.Cantidad;

                                    dtDetalles.Rows.Add(new object[] {
                                    Ordenes_Id,
                                    Skus_Id,
                                    cantidad
                                });
                                }
                            }

                            if (CrearDetallesOrdenes(dtDetalles))
                            {
                                ViewBag.Mensaje = "Correcto";
                            }
                            else
                            {
                                ViewBag.Mensaje = "Error Detalles";
                            }
                        }
                        else
                        {
                            ViewBag.Mensaje = "Error Cargar Ordenes";
                        }
                    }
                    else
                    {
                        ViewBag.Mensaje = "No se encontraron ordenes nuevas";
                    }
                }

                return View(listaErrores);
            }
            catch (Exception _ex)
            {
                ViewBag.Mensaje = _ex.Message.ToString();
                return View(listaErrores);
            }
        }

        public bool CrearOrdenes(DataTable dtOrdenes)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = "dbo.ordenes";

                bulkCopy.ColumnMappings.Add("FechaAlta", "FechaAlta");
                bulkCopy.ColumnMappings.Add("Orden", "Orden");
                bulkCopy.ColumnMappings.Add("User", "User");
                bulkCopy.ColumnMappings.Add("StatusOrdenImpresa_Id", "StatusOrdenImpresa_Id");

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

        public bool CrearDetallesOrdenes(DataTable dtDetalles)
        {
            //string connectionString = @"Data Source=sql5054.site4now.net;Initial Catalog=DB_A3F19C_OG;User Id=DB_A3F19C_OG_admin;Password=xQ9znAhU;";

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = "dbo.detordenproductoshd";

                bulkCopy.ColumnMappings.Add("Ordenes_Id", "Ordenes_Id");
                bulkCopy.ColumnMappings.Add("Skus_Id", "Skus_Id");
                bulkCopy.ColumnMappings.Add("cantidad", "cantidad");

                try
                {
                    bulkCopy.WriteToServer(dtDetalles);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult EliminarCorte()
        {
            ViewBag.Mensaje = "Inicio";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EliminarCorte(string corte)
        {
            DateTime fechaCorteEliminar = DateTime.Parse(corte);

            var ordenes_buscar = (from o in dbHD.ordenes
                                  where o.StatusOrdenImpresa_Id == 1 &&
                                         o.FechaAlta.Value.Year.Equals(fechaCorteEliminar.Year) &&
                                         o.FechaAlta.Value.Month.Equals(fechaCorteEliminar.Month) &&
                                         o.FechaAlta.Value.Day.Equals(fechaCorteEliminar.Day) &&
                                         o.FechaAlta.Value.Hour.Equals(fechaCorteEliminar.Hour) &&
                                         o.FechaAlta.Value.Minute.Equals(fechaCorteEliminar.Minute) &&
                                         o.FechaAlta.Value.Second.Equals(fechaCorteEliminar.Second)
                                  select new { o.id }).ToList();

            int conteo = ordenes_buscar.Count();

            foreach (var item in ordenes_buscar)
            {
                await ElminarDetalleOrdenAsync(item.id);
                await ElminarGuiaAsync(item.id);
                await ElminarOrdenAsync(item.id);

                conteo--;
            }

            ViewBag.Mensaje = "Correcto";

            return View();
        }

        public async Task<bool> ElminarGuiaAsync(int idOrden)
        {
            var guia = dbHD.guias.Where(x => x.Ordenes_Id.Equals(idOrden)).FirstOrDefault();

            if (guia != null)
            {
                dbHD.guias.Remove(guia);
                await dbHD.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> ElminarOrdenAsync(int idOrden)
        {
            var orden = dbHD.ordenes.Find(idOrden);

            if (orden != null)
            {
                dbHD.ordenes.Remove(orden);
                await dbHD.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> ElminarDetalleOrdenAsync(int idOrden)
        {
            List<detordenproductoshd> det_ordenes = dbHD.detordenproductoshd.Where(x => x.Ordenes_Id.Equals(idOrden)).ToList();

            if (det_ordenes.Count > 0)
            {
                dbHD.detordenproductoshd.RemoveRange(det_ordenes);

                await dbHD.SaveChangesAsync();
            }

            return true;
        }

    }
}