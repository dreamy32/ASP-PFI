using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySpace.Models;

namespace MySpace.Controllers
{
    public class ArtistsController : Controller
    {
        MySpaceDBEntities DB = new MySpaceDBEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Page(int id)
        {
            Artist artist = DB.Artists.Find(id);
            if (artist != null)
                return View(artist);

            RedirectToAction("Index");
        }

        public ActionResult GetPage(int artistId)
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
            return null;
        }
    }
}