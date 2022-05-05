using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySpace.Models;

namespace MySpace.Controllers
{
    [UserAccess]
    public class FriendShipsController : Controller
    {
        MySpaceDBEntities DB = new MySpaceDBEntities();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DB.Dispose();
            }
            base.Dispose(disposing);
        }
        public void RenewFriendShipsSerialNumber()
        {
            HttpRuntime.Cache["FriendShipsSerialNumber"] = Guid.NewGuid().ToString();
            OnlineUsers.RenewSerialNumber();
        }
        public string GetFriendShipsSerialNumber()
        {
            if (HttpRuntime.Cache["FriendShipsSerialNumber"] == null)
            {
                RenewFriendShipsSerialNumber();
            }
            return (string)HttpRuntime.Cache["FriendShipsSerialNumber"];
        }
        public void SetLocalFriendShipsSerialNumber()
        {
            Session["FriendShipsSerialNumber"] = GetFriendShipsSerialNumber();
        }
        public bool IsFriendShipsUpToDate()
        {
            return ((string)Session["FriendShipsSerialNumber"] == (string)HttpRuntime.Cache["FriendShipsSerialNumber"]);
        }


        // GET: FriendShips
        public ActionResult Index()
        {
            SetLocalFriendShipsSerialNumber();
            return View();
        }

        public PartialViewResult GetFriendShipsStatus(bool forceRefresh = false)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            if ((currentUser != null) && (forceRefresh || !IsFriendShipsUpToDate() || OnlineUsers.NeedUpdate()))
            {
                SetLocalFriendShipsSerialNumber();
                return PartialView(DB.FriendShipsStatus(currentUser.Id));
            }
            return null;
        }

        public ActionResult SendFriendShipRequest(int targetUserId)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Add_FiendShipRequest(currentUser.Id, targetUserId);
            RenewFriendShipsSerialNumber();
            return null;
        }
        public ActionResult RemoveFriendShipRequest(int targetUserId)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Remove_FiendShipRequest(currentUser.Id, targetUserId);
            RenewFriendShipsSerialNumber();
            return null;
        }
        public ActionResult AcceptFriendShipRequest(int targetUserId)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Accept_FriendShip(targetUserId, currentUser.Id);
            RenewFriendShipsSerialNumber();
            return null;
        }
        public ActionResult DeclineFriendShipRequest(int targetUserId)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Decline_FriendShip(targetUserId, currentUser.Id);
            RenewFriendShipsSerialNumber();
            return null;
        }
    }
}