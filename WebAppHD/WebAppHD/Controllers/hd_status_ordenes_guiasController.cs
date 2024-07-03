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
    public class hd_status_ordenes_guiasController : Controller
    {
        private DB_A3F19C_OGEntities1 db = new DB_A3F19C_OGEntities1();

        // GET: hd_status_ordenes_guias
        public ActionResult Index()
        {
            return View(db.hd_status_ordenes_guias.ToList());
        }

        [HttpPost]
        public JsonResult ListaStatus()
        {
            List<SelectListItem> liststatus = new List<SelectListItem>();

            foreach (var item in db.hd_status_ordenes_guias.ToList())
            {
                liststatus.Add(new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Descripcion
                });
            }

            return Json(liststatus);
        }

        // GET: hd_status_ordenes_guias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            hd_status_ordenes_guias hd_status_ordenes_guias = db.hd_status_ordenes_guias.Find(id);
            if (hd_status_ordenes_guias == null)
            {
                return HttpNotFound();
            }
            return View(hd_status_ordenes_guias);
        }

        // GET: hd_status_ordenes_guias/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: hd_status_ordenes_guias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Descripcion")] hd_status_ordenes_guias hd_status_ordenes_guias)
        {
            if (ModelState.IsValid)
            {
                db.hd_status_ordenes_guias.Add(hd_status_ordenes_guias);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(hd_status_ordenes_guias);
        }

        // GET: hd_status_ordenes_guias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            hd_status_ordenes_guias hd_status_ordenes_guias = db.hd_status_ordenes_guias.Find(id);
            if (hd_status_ordenes_guias == null)
            {
                return HttpNotFound();
            }
            return View(hd_status_ordenes_guias);
        }

        // POST: hd_status_ordenes_guias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Descripcion")] hd_status_ordenes_guias hd_status_ordenes_guias)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hd_status_ordenes_guias).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(hd_status_ordenes_guias);
        }

        // GET: hd_status_ordenes_guias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            hd_status_ordenes_guias hd_status_ordenes_guias = db.hd_status_ordenes_guias.Find(id);
            if (hd_status_ordenes_guias == null)
            {
                return HttpNotFound();
            }
            return View(hd_status_ordenes_guias);
        }

        // POST: hd_status_ordenes_guias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            hd_status_ordenes_guias hd_status_ordenes_guias = db.hd_status_ordenes_guias.Find(id);
            db.hd_status_ordenes_guias.Remove(hd_status_ordenes_guias);
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
    }
}
