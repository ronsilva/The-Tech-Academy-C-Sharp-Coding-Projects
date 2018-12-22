using NewsletterAppMVC.Models;
using NewsletterAppMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewsletterAppMVC.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            using (var db = new NewsletterEntities())
            {
                //var signups = db.SignUps.Where(x => x.Removed == null).ToList();
                var signups = (from c in db.SignUps
                               where c.Removed == null
                               select c).ToList();
                var signupVms = new List<SignupVm>();
                foreach (var signup in signups)
                {
                    var signupVm = new SignupVm()
                    {
                        Id = signup.Id,
                        FirstName = signup.FirstName,
                        LastName = signup.LastName,
                        EmailAddress = signup.EmailAddress
                    };
                    signupVms.Add(signupVm);
                }
                return View(signupVms);
            }
        }

        public ActionResult Unsubscribe(int Id)
        {
            using(var db = new NewsletterEntities())
            {
                var signup = db.SignUps.Find(Id);
                signup.Removed = DateTime.Now;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}