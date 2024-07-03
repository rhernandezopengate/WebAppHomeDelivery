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
    public class pickersController : Controller
    {
        private DB_A3F19C_OGEntities db = new DB_A3F19C_OGEntities();

        // GET: pickers
        public ActionResult Index()
        {
            var pickers = db.pickers.Include(p => p.statuspickers);
            return View(pickers.ToList());
        }

        // GET: pickers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pickers pickers = db.pickers.Find(id);
            if (pickers == null)
            {
                return HttpNotFound();
            }
            return View(pickers);
        }

        // GET: pickers/Create
        public ActionResult Create()
        {
            ViewBag.statuspickers_Id = new SelectList(db.statuspickers, "Id", "Descripcion");
            return View();
        }

        // POST: pickers/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,statuspickers_Id")] pickers pickers)
        {
            if (ModelState.IsValid)
            {
                db.pickers.Add(pickers);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.statuspickers_Id = new SelectList(db.statuspickers, "Id", "Descripcion", pickers.statuspickers_Id);
            return View(pickers);
        }

        // GET: pickers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pickers pickers = db.pickers.Find(id);
            if (pickers == null)
            {
                return HttpNotFound();
            }
            ViewBag.statuspickers_Id = new SelectList(db.statuspickers, "Id", "Descripcion", pickers.statuspickers_Id);
            return View(pickers);
        }

        // POST: pickers/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,statuspickers_Id")] pickers pickers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pickers).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.statuspickers_Id = new SelectList(db.statuspickers, "Id", "Descripcion", pickers.statuspickers_Id);
            return View(pickers);
        }

        // GET: pickers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pickers pickers = db.pickers.Find(id);
            if (pickers == null)
            {
                return HttpNotFound();
            }
            return View(pickers);
        }

        // POST: pickers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            pickers pickers = db.pickers.Find(id);
            db.pickers.Remove(pickers);
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
