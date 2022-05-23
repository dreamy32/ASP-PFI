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
        #region PageSerialNumbers
        private const string serialNumber = "ArtistsSerialNumber";
        public string GetArtistsSerialNumber
        {
            get
            {
                if (HttpRuntime.Cache[serialNumber] == null)
                {
                    RenewArtistsSerialNumber();
                }
                return (string)HttpRuntime.Cache[serialNumber];
            }
        }
        public bool IsPageUpToDate => ((string)Session[serialNumber] == (string)HttpRuntime.Cache[serialNumber]);
        public void SetLocalArtistsSerialNumber() => Session[serialNumber] = GetArtistsSerialNumber;
        public void RenewArtistsSerialNumber() => HttpRuntime.Cache[serialNumber] = Guid.NewGuid().ToString();
        #endregion
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

        public ActionResult GetPage(int? artistId, bool forceRefresh = false)
        {
            if (artistId == null)
                return RedirectToAction("Index");
            else if (forceRefresh || !IsPageUpToDate || OnlineUsers.NeedUpdate())
            {
                Artist artist = DB.Artists.Find(artistId);
                SetLocalArtistsSerialNumber();
                return PartialView(artist);
            }
            return null;
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
            string youtubeId = "";
            if (link.Contains("https://www.youtube.com/watch?v="))
            {
                youtubeId = link.Replace("https://www.youtube.com/watch?v=", "");
                if (youtubeId.IndexOf("&") > -1)
                {
                    youtubeId = youtubeId.Substring(0, youtubeId.IndexOf("&"));
                }
            }
            if (youtubeId != "")
            {
                Video video = new Video
                {
                    Creation = DateTime.Now,
                    ArtistId = artistId,
                    Title = title,
                    YoutubeId = youtubeId
                };
                DB.Add_Video(video);
                RenewArtistsSerialNumber();
            }
            return null;
        }
        public Action RemoveVideo(int videoId)
        {
            return null;
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
            DB.Add_Message(_message);
            RenewArtistsSerialNumber();
            return null;
        }
        public ActionResult ModifyPage(int id)
        {
            return View(DB.Artists.Find(id));
        }

        [HttpPost]
        public ActionResult ModifyPage(Artist ac)
        {
            if (ModelState.IsValid)
            {
                DB.Update_Artist(ac);
                return RedirectToAction("Page/" + ac.Id);
            }

            return View(ac);
        }
        public ActionResult AddRemoveLike(int artistId)
        {
            FanLike fanLike = DB.FanLikes.Where(
                fl => fl.UserId == OnlineUsers.CurrentUserId && fl.ArtistId == artistId
                ).FirstOrDefault();
            if (fanLike == null)
            {
                fanLike = new FanLike
                {
                    UserId = OnlineUsers.CurrentUserId,
                    ArtistId = artistId,
                    Creation = DateTime.Now
                };
                DB.Add_FanLike(fanLike);
            }
            else
            {
                DB.Remove_FanLike(fanLike);
            }
            RenewArtistsSerialNumber();
            return null;
        }
    }

}