using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PUTOVANJEsController : Controller
    {
        private RPPP09Entities db = new RPPP09Entities();

        // GET: PUTOVANJEs
        public ActionResult Index()
        {
            return View(db.PUTOVANJE.ToList());
        }

        // GET: PUTOVANJEs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PUTOVANJE pUTOVANJE = db.PUTOVANJE.Find(id);
            if (pUTOVANJE == null)
            {
                return HttpNotFound();
            }
            return View(pUTOVANJE);
        }

        // GET: PUTOVANJEs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PUTOVANJEs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sifra_putovanja,identifikacijski_broj,datum_polaska,datum_vracanja")] PUTOVANJE pUTOVANJE)
        {
            if (ModelState.IsValid)
            {
                db.PUTOVANJE.Add(pUTOVANJE);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(pUTOVANJE);
        }

        // GET: PUTOVANJEs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PUTOVANJE pUTOVANJE = db.PUTOVANJE.Find(id);
            if (pUTOVANJE == null)
            {
                return HttpNotFound();
            }
            return View(pUTOVANJE);
        }

        // POST: PUTOVANJEs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "sifra_putovanja,identifikacijski_broj,datum_polaska,datum_vracanja")] PUTOVANJE pUTOVANJE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pUTOVANJE).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(pUTOVANJE);
        }

        // GET: PUTOVANJEs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PUTOVANJE pUTOVANJE = db.PUTOVANJE.Find(id);
            if (pUTOVANJE == null)
            {
                return HttpNotFound();
            }
            return View(pUTOVANJE);
        }

        // POST: PUTOVANJEs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PUTOVANJE pUTOVANJE = db.PUTOVANJE.Find(id);
            db.PUTOVANJE.Remove(pUTOVANJE);
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
