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
            InitSearchByArtistName();
            InitSortArtists();
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

                List<int> listVisites = (List<int>)Session["VisitsHistory"];

                if (listVisites == null)
                {
                    listVisites = new List<int>();
                }

                if (!listVisites.Contains((int)artistId))
                {
                    listVisites.Add((int)artistId);
                    DB.Add_Visit(artistId);
                }

                Session["VisitsHistory"] = listVisites;
                return PartialView(artist);
            }
            return null;
        }
        #region GroupEmail
        [ArtistAcess]
        public ActionResult GroupEmail()
        {
            Artist artist = DB.Artists.Where(a => a.UserId == OnlineUsers.CurrentUserId).FirstOrDefault();
            IEnumerable<User> fans = DB.FanLikes.Where(f => (f.ArtistId == artist.Id)).Select(fl => fl.User).ToList();
            ViewBag.SelectedUsers = new List<int>();
            ViewBag.Fans = fans;
            return View(new GroupEmail());
        }

        [HttpPost]
        public ActionResult GroupEmail(GroupEmail groupEmail, List<int> SelectedUsers)
        {
            if (ModelState.IsValid)
            {
                groupEmail.SelectedUsers = SelectedUsers;
                groupEmail.Send(DB);
                return RedirectToAction("Index");
            }
            Artist artist = DB.Artists.Where(a => a.UserId == OnlineUsers.CurrentUserId).FirstOrDefault();
            IEnumerable<User> fans = DB.FanLikes.Where(f => (f.ArtistId == artist.Id)).Select(fl => fl.User).ToList();
            ViewBag.SelectedUsers = new List<int>();
            ViewBag.Fans = fans;
            return View(groupEmail);
        }
        #endregion
        public ActionResult GetArtistsList(bool forceRefresh = false)
        {
            if (forceRefresh || !IsPageUpToDate || OnlineUsers.NeedUpdate())
            {
                SetLocalArtistsSerialNumber();
                List<Artist> artists = DB.Artists.OrderBy(a => a.Name).ToList();
                string name = (string)Session["name"];
                if (name != "")
                    artists = DBDAL.SearchArtistsByKeywords(artists, name);
                Response.Write($"<script>console.log('{(string)Session["ArtistFieldToSort"]}')</script>");
                switch ((string)Session["ArtistFieldToSort"])
                {
                    case "names":
                        if ((bool)Session["ArtistFieldSortDir"])
                        {
                            artists = artists.OrderBy(pr => pr.Name).ToList();
                        }
                        else
                        {
                            artists = artists.OrderByDescending(pr => pr.Name).ToList();
                        }
                        break;
                    case "vues":
                        if ((bool)Session["ArtistFieldSortDir"])
                        {
                            artists = artists.OrderBy(pr => pr.Visits).ToList();
                        }
                        else
                        {
                            artists = artists.OrderByDescending(pr => pr.Visits).ToList();
                        }
                        break;
                    case "likes":
                        if ((bool)Session["ArtistFieldSortDir"])
                        {
                            artists = artists.OrderBy(pr => pr.Likes).ToList();
                        }
                        else
                        {
                            artists = artists.OrderByDescending(pr => pr.Likes).ToList();
                        }
                        break;
                    default: break;
                }

                return PartialView(artists);
            }
            return null;
        }
        public void InitSearchByArtistName()
        {
            if (Session["name"] == null)
                Session["name"] = "";
        }
        public ActionResult SetSearchArtistName(string name)
        {
            Session["name"] = name.Trim().ToLower();
            RenewArtistsSerialNumber();
            return null;
        }
        public void InitSortArtists()
        {
            if (Session["ArtistFieldToSort"] == null)
                Session["ArtistFieldToSort"] = "names"; 
            if (Session["ArtistFieldSortDir"] == null)
                Session["ArtistFieldSortDir"] = false; 
        }
        public ActionResult SortArtistsBy(string fieldToSort)
        {
            RenewArtistsSerialNumber();
            if ((string)Session["ArtistFieldToSort"] == fieldToSort)
            {
                Session["ArtistFieldSortDir"] = !(bool)Session["ArtistFieldSortDir"];
            }
            else
            {
                Session["ArtistFieldToSort"] = fieldToSort;
                Session["ArtistFieldSortDir"] = true;
            }
            return null;
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
        public ActionResult RemoveVideo(int videoId)
        {
            DB.Remove_Video(videoId);
            RenewArtistsSerialNumber();
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
                DB.Add_FanLike(fanLike, artistId);
            }
            else
            {
                DB.Remove_FanLike(fanLike, artistId);
            }
            RenewArtistsSerialNumber();
            return null;
        }
    }

}