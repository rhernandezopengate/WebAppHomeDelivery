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
    public class statusordenimpresasController : Controller
    {
        private DB_A3F19C_OGEntities db = new DB_A3F19C_OGEntities();

        // GET: statusordenimpresas
        public ActionResult Index()
        {
            return View(db.statusordenimpresa.ToList());
        }

        [HttpPost]
        public JsonResult ListaStatus()
        {
            List<SelectListItem> liststatus = new List<SelectListItem>();

            foreach (var item in db.statusordenimpresa.ToList())
            {
                liststatus.Add(new SelectListItem
                {
                    Value = item.id.ToString(),
                    Text = item.descripcion
                });
            }

            return Json(liststatus);
        }


        // GET: statusordenimpresas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statusordenimpresa statusordenimpresa = db.statusordenimpresa.Find(id);
            if (statusordenimpresa == null)
            {
                return HttpNotFound();
            }
            return View(statusordenimpresa);
        }

        // GET: statusordenimpresas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: statusordenimpresas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,descripcion")] statusordenimpresa statusordenimpresa)
        {
            if (ModelState.IsValid)
            {
                db.statusordenimpresa.Add(statusordenimpresa);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(statusordenimpresa);
        }

        // GET: statusordenimpresas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statusordenimpresa statusordenimpresa = db.statusordenimpresa.Find(id);
            if (statusordenimpresa == null)
            {
                return HttpNotFound();
            }
            return View(statusordenimpresa);
        }

        // POST: statusordenimpresas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,descripcion")] statusordenimpresa statusordenimpresa)
        {
            if (ModelState.IsValid)
            {
                db.Entry(statusordenimpresa).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(statusordenimpresa);
        }

        // GET: statusordenimpresas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statusordenimpresa statusordenimpresa = db.statusordenimpresa.Find(id);
            if (statusordenimpresa == null)
            {
                return HttpNotFound();
            }
            return View(statusordenimpresa);
        }

        // POST: statusordenimpresas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            statusordenimpresa statusordenimpresa = db.statusordenimpresa.Find(id);
            db.statusordenimpresa.Remove(statusordenimpresa);
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
