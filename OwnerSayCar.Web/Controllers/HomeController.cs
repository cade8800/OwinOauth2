using System.Web.Mvc;

namespace OwnerSayCar.Web.Controllers
{
    public class HomeController : OwnerSayCarControllerBase
    {
        public ActionResult Index()
        { 
            return View("~/App/Main/views/layout/layout.cshtml"); //Layout of the angular application.
        }
	}
}