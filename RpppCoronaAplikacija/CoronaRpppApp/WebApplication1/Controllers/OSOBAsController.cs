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
    public class OSOBAsController : Controller
    {
        private RPPP09Entities1 db = new RPPP09Entities1();

        // GET: OSOBAs
        public ActionResult Index()
        {
            return View(db.OSOBAs.ToList());
        }

        // GET: OSOBAs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OSOBA oSOBA = db.OSOBAs.Find(id);
            if (oSOBA == null)
            {
                return HttpNotFound();
            }
            return View(oSOBA);
        }

        // GET: OSOBAs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OSOBAs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "identifikacijski_broj,ime,prezime,adresa,dat_rod,zanimanje")] OSOBA oSOBA)
        {
            if (ModelState.IsValid)
            {
                db.OSOBAs.Add(oSOBA);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(oSOBA);
        }

        // GET: OSOBAs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OSOBA oSOBA = db.OSOBAs.Find(id);
            if (oSOBA == null)
            {
                return HttpNotFound();
            }
            return View(oSOBA);
        }

        // POST: OSOBAs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "identifikacijski_broj,ime,prezime,adresa,dat_rod,zanimanje")] OSOBA oSOBA)
        {
            if (ModelState.IsValid)
            {
                db.Entry(oSOBA).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(oSOBA);
        }

        // GET: OSOBAs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OSOBA oSOBA = db.OSOBAs.Find(id);
            if (oSOBA == null)
            {
                return HttpNotFound();
            }
            return View(oSOBA);
        }

        // POST: OSOBAs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            OSOBA oSOBA = db.OSOBAs.Find(id);
            db.OSOBAs.Remove(oSOBA);
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
