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
    public class statuspickersController : Controller
    {
        private DB_A3F19C_OGEntities db = new DB_A3F19C_OGEntities();

        // GET: statuspickers
        public ActionResult Index()
        {
            return View(db.statuspickers.ToList());
        }

        // GET: statuspickers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statuspickers statuspickers = db.statuspickers.Find(id);
            if (statuspickers == null)
            {
                return HttpNotFound();
            }
            return View(statuspickers);
        }

        // GET: statuspickers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: statuspickers/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Descripcion")] statuspickers statuspickers)
        {
            if (ModelState.IsValid)
            {
                db.statuspickers.Add(statuspickers);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(statuspickers);
        }

        // GET: statuspickers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statuspickers statuspickers = db.statuspickers.Find(id);
            if (statuspickers == null)
            {
                return HttpNotFound();
            }
            return View(statuspickers);
        }

        // POST: statuspickers/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Descripcion")] statuspickers statuspickers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(statuspickers).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(statuspickers);
        }

        // GET: statuspickers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statuspickers statuspickers = db.statuspickers.Find(id);
            if (statuspickers == null)
            {
                return HttpNotFound();
            }
            return View(statuspickers);
        }

        // POST: statuspickers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            statuspickers statuspickers = db.statuspickers.Find(id);
            db.statuspickers.Remove(statuspickers);
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
