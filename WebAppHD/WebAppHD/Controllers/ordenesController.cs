using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebAppHD.Models;

namespace WebAppHD.Controllers
{
    public class ordenesController : Controller
    {
        private DB_A3F19C_OGEntities db = new DB_A3F19C_OGEntities();

        // GET: ordenes
        public ActionResult Index()
        {
            var ordenes = db.ordenes.Include(o => o.statusordenimpresa);
            return View(ordenes.ToList());
        }

        // GET: ordenes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ordenes ordenes = db.ordenes.Find(id);
                        
            if (ordenes == null)
            {
                return HttpNotFound();
            }

            var guia = db.guias.Where(x => x.Ordenes_Id.Equals(ordenes.id)).FirstOrDefault();

            var auditor = db.detusuariosordenes.Where(x => x.Ordenes_Id.Equals(ordenes.id)).FirstOrDefault();

            if (auditor != null)
            {
                ordenes.Auditor = auditor.usuarios.nombre;
            }
            else
            {
                ordenes.Auditor = "";
            }

            if (guia != null)
            {
                ordenes.Guia = guia.Guia;
            }
            else
            {
                ordenes.Guia = "";
            }

            var detalleOrden = db.detordenproductoshd.Where(x => x.Ordenes_Id == id).ToList();
            List<detordenproductoshd> lista = new List<detordenproductoshd>();
            foreach (var item in detalleOrden)
            {
                detordenproductoshd detordenproductoshd = new detordenproductoshd();
                detordenproductoshd.cantidad = item.cantidad;
                detordenproductoshd.SKU = item.skus.Sku;
                detordenproductoshd.SKUDescripcion = item.skus.Descripcion;
                lista.Add(detordenproductoshd);
            }

            ViewBag.Detalle = lista;

            return View(ordenes);
        }

        // GET: ordenes/Create
        public ActionResult Create()
        {
            ViewBag.StatusOrdenImpresa_Id = new SelectList(db.statusordenimpresa, "id", "descripcion");
            return View();
        }

        // POST: ordenes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,FechaAlta,Orden,User,StatusOrdenImpresa_Id,Picker,FechaCierre")] ordenes ordenes)
        {
            if (ModelState.IsValid)
            {
                db.ordenes.Add(ordenes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StatusOrdenImpresa_Id = new SelectList(db.statusordenimpresa, "id", "descripcion", ordenes.StatusOrdenImpresa_Id);
            return View(ordenes);
        }

        // GET: ordenes/Edit/5
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ordenes ordenes = db.ordenes.Find(id);
            if (ordenes == null)
            {
                return HttpNotFound();
            }
            ViewBag.StatusOrdenImpresa_Id = new SelectList(db.statusordenimpresa, "id", "descripcion", ordenes.StatusOrdenImpresa_Id);
            return View(ordenes);
        }

        // POST: ordenes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ordenes ordenes)
        {
            try
            {
                ordenes ordenes1 = db.ordenes.Find(ordenes.id);
                ordenes1.StatusOrdenImpresa_Id = ordenes.StatusOrdenImpresa_Id;
                db.SaveChanges();
                return Json(new { success = true, message = "Editado Correctamente." });
            }
            catch (Exception _ex)
            {

                ViewBag.StatusOrdenImpresa_Id = new SelectList(db.statusordenimpresa, "id", "descripcion", ordenes.StatusOrdenImpresa_Id);
                return Json(new { success = false, message = _ex.Message.ToString() });
            }

        }

        // GET: ordenes/Delete/5
        [Authorize(Roles = "admin,supervisorhd,teamleaderhd,auxiliarhd")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ordenes ordenes = db.ordenes.Find(id);
            if (ordenes == null)
            {
                return HttpNotFound();
            }

            var guia = db.guias.Where(x => x.Ordenes_Id.Equals(ordenes.id)).FirstOrDefault();

            var auditor = db.detusuariosordenes.Where(x => x.Ordenes_Id.Equals(ordenes.id)).FirstOrDefault();

            if (auditor != null)
            {
                ordenes.Auditor = auditor.usuarios.nombre;
            }
            else
            {
                ordenes.Auditor = "";
            }

            if (guia != null)
            {
                ordenes.Guia = guia.Guia;
            }
            else
            {
                ordenes.Guia = "";
            }

            var detalleOrden = db.detordenproductoshd.Where(x => x.Ordenes_Id == id).ToList();
            List<detordenproductoshd> lista = new List<detordenproductoshd>();
            foreach (var item in detalleOrden)
            {
                detordenproductoshd detordenproductoshd = new detordenproductoshd();
                detordenproductoshd.cantidad = item.cantidad;
                detordenproductoshd.SKU = item.skus.Sku;
                detordenproductoshd.SKUDescripcion = item.skus.Descripcion;
                lista.Add(detordenproductoshd);
            }

            ViewBag.Detalle = lista;

            return View(ordenes);
        }

        // POST: ordenes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ordenes ordenes = db.ordenes.Find(id);

            try
            {
                if (ordenes.StatusOrdenImpresa_Id == 1)
                {
                    ElminarDetalleOrdenAsync(ordenes.id);
                    ElminarGuiaAsync(ordenes.id);
                    EliminarAuditor(ordenes.id);
                    ElminarOrdenAsync(ordenes.id);

                    return Json(new { success = true, message = "Orden Eliminada Correctamente." });                    
                }
                else
                {
                    return Json(new { success = false, message = "Solo se pueden eliminar ordenes CREADAS" });
                }                
            }
            catch (Exception _ex)
            {
                ViewBag.StatusOrdenImpresa_Id = new SelectList(db.statusordenimpresa, "id", "descripcion", ordenes.StatusOrdenImpresa_Id);
                return Json(new { success = false, message = _ex.Message.ToString() });                
            }  
        }

        public bool ElminarDetalleOrdenAsync(int idOrden)
        {
            List<detordenproductoshd> det_ordenes = db.detordenproductoshd.Where(x => x.Ordenes_Id.Equals(idOrden)).ToList();

            if (det_ordenes.Count > 0)
            {
                db.detordenproductoshd.RemoveRange(det_ordenes);
                db.SaveChanges();
            }

            return true;
        }

        public bool ElminarGuiaAsync(int idOrden)
        {
            var guia = db.guias.Where(x => x.Ordenes_Id.Equals(idOrden)).FirstOrDefault();

            if (guia != null)
            {
                db.guias.Remove(guia);
                db.SaveChanges();
            }

            return true;
        }

        public bool EliminarAuditor(int idOrden) 
        {
            try
            {
                var auditor = db.detusuariosordenes.Where(x => x.Ordenes_Id.Equals(idOrden)).FirstOrDefault();

                if (auditor != null)
                {
                    db.detusuariosordenes.Remove(auditor);
                    db.SaveChanges();                    
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ElminarOrdenAsync(int idOrden)
        {
            var orden = db.ordenes.Find(idOrden);

            if (orden != null)
            {
                db.ordenes.Remove(orden);
                db.SaveChanges();
            }

            return true;
        }        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
