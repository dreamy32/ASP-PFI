using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySpace.Controllers
{
    public class ArtistsController : Controller
    {
        // GET: Artists
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Page(int id)
        {
            return View();
        }
        public ActionResult GroupEmail()
        {
            return View();
        }
        public ActionResult GetArtistsList()
        {
            return View();
        }
        public ActionResult SetSearchArtistName(string name)
        {
            return View();
        }
        public ActionResult SortArtistsBy(string fieldToSort)
        {
            return View();
        }
        public ActionResult GetPage(int artistId)
        {
            return View();
        }
        public ActionResult AddVideo(int artistId, string title, string link)
        {
            return View();
        }
        public ActionResult AddMessage(int artistId, string message)
        {
            return View();
        }
        public ActionResult ModifyPage(int id)
        {

        }
    }
}