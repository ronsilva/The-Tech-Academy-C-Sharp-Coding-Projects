using NewsletterAppMVC.Models;
using System.Web.Mvc;

namespace NewsletterAppMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(string firstName, string lastName, string emailAddress)
        {
            if(string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(emailAddress))
            {
               return View("~/Views/Shared/Error.cshtml");
            }
            else
            {
                using (NewsletterEntities db = new NewsletterEntities())
                {
                    var signup = new SignUp()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        EmailAddress = emailAddress
                    };
                    db.SignUps.Add(signup);
                    db.SaveChanges();
                }
                return View("Success");
            }
        }
    }
}