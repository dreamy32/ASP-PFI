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

            return RedirectToAction("Index");
        }

        public ActionResult GetPage(int artistId)
        {
            return PartialView(DB.Artists.Find(artistId));
        }
        public ActionResult GroupEmail()
        {
            return View();
        }
        public ActionResult GetArtistsList(bool forceRefresh = false)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            if ((currentUser != null) && (forceRefresh || OnlineUsers.NeedUpdate()))
            {
                return PartialView(DB.Artists);
            }
            return null;
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
            Message _message = new Message
            {
                ArtistId = artistId,
                Text = message,
                UserId = OnlineUsers.CurrentUserId,
                Creation = DateTime.Now
            };
            DB.AddArtistMessage(_message);
            //RenewArtistSerialNumber();
            return null;
        }
        public ActionResult ModifyPage(int id)
        {
            return null;
        }
    }
}