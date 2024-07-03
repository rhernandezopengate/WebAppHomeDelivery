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
    public class skusbenavidesController : Controller
    {
        private DB_A3F19C_OGEntities db = new DB_A3F19C_OGEntities();

        // GET: skusbenavides
        public ActionResult Index()
        {
            var skusbenavides = db.skusbenavides.Include(s => s.skus);
            return View(skusbenavides.ToList());
        }

        // GET: skusbenavides/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            skusbenavides skusbenavides = db.skusbenavides.Find(id);
            if (skusbenavides == null)
            {
                return HttpNotFound();
            }
            return View(skusbenavides);
        }

        // GET: skusbenavides/Create
        public ActionResult Create()
        {
            ViewBag.skus_Id = new SelectList(db.skus, "id", "Sku");
            return View();
        }

        // POST: skusbenavides/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Codigo,skus_Id")] skusbenavides skusbenavides)
        {
            if (ModelState.IsValid)
            {
                db.skusbenavides.Add(skusbenavides);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.skus_Id = new SelectList(db.skus, "id", "Sku", skusbenavides.skus_Id);
            return View(skusbenavides);
        }

        // GET: skusbenavides/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            skusbenavides skusbenavides = db.skusbenavides.Find(id);
            if (skusbenavides == null)
            {
                return HttpNotFound();
            }
            ViewBag.skus_Id = new SelectList(db.skus, "id", "Sku", skusbenavides.skus_Id);
            return View(skusbenavides);
        }

        // POST: skusbenavides/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Codigo,skus_Id")] skusbenavides skusbenavides)
        {
            if (ModelState.IsValid)
            {
                db.Entry(skusbenavides).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.skus_Id = new SelectList(db.skus, "id", "Sku", skusbenavides.skus_Id);
            return View(skusbenavides);
        }

        // GET: skusbenavides/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            skusbenavides skusbenavides = db.skusbenavides.Find(id);
            if (skusbenavides == null)
            {
                return HttpNotFound();
            }
            return View(skusbenavides);
        }

        // POST: skusbenavides/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            skusbenavides skusbenavides = db.skusbenavides.Find(id);
            db.skusbenavides.Remove(skusbenavides);
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
