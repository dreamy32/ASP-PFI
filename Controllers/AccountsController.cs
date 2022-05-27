using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySpace.Models;

namespace MySpace.Controllers
{
    public class AccountsController : Controller
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

        #region Account creation
        [HttpPost]
        public JsonResult EmailAvailable(string email, int Id = 0)
        {
            return Json(DB.EmailAvailable(email, Id));
        }
        [HttpPost]
        public JsonResult EmailExist(string email)
        {
            return Json(DB.EmailExist(email));
        }
        public ActionResult Subscribe()
        {
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
            ViewBag.UserTypes = SelectListItemConverter<UserType>.Convert(DB.UserTypes.Where(u => u.Id > 1).ToList());
            User user = new User();
            return View(user);
        }
        [HttpPost]
        public ActionResult Subscribe(User user)
        {
            if (ModelState.IsValid)
            {
                user = DB.Add_User(user);

                if (user.IsArtist)
                {
                    Artist artiste = new Artist();
                    DB.Add_Artist(artiste, user);
                    Session["artisteId"] = artiste.Id;
                }
                SendEmailVerification(user, user.Email);
                return RedirectToAction("SubscribeDone/" + user.Id.ToString());
            }
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
            ViewBag.UserTypes = SelectListItemConverter<UserType>.Convert(DB.UserTypes.Where(u => u.Id > 1).ToList());
            return View(user);
        }
        public ActionResult SubscribeDone(int id)
        {
            User newlySubscribedUser = DB.Users.Find(id);
            if (newlySubscribedUser != null)
                return View(newlySubscribedUser);
            return RedirectToAction("Login");
        }
        #endregion

        #region Account Verification
        public void SendEmailVerification(User user, string newEmail)
        {
            if (user.Id != 0)
            {
                UnverifiedEmail unverifiedEmail = DB.Add_UnverifiedEmail(user.Id, newEmail);
                if (unverifiedEmail != null)
                {
                    string verificationUrl = Url.Action("VerifyUser", "Accounts", null, Request.Url.Scheme);
                    String Link = @"<br/><a href='" + verificationUrl + "?userid=" + user.Id + "&code=" + unverifiedEmail.VerificationCode + @"' > Confirmez votre inscription...</a>";

                    String suffixe = "";
                    if (user.GenderId == 2)
                    {
                        suffixe = "e";
                    }
                    string Subject = "MySpace - Vérification d'inscription...";

                    string Body = "Bonjour " + user.GetFullName(true) + @",<br/><br/>";
                    Body += @"Merci de vous être inscrit" + suffixe + " au site [nom de l'application]. <br/>";
                    Body += @"Pour utiliser votre compte vous devez confirmer votre inscription en cliquant sur le lien suivant : <br/>";
                    Body += Link;
                    Body += @"<br/><br/>Ce courriel a été généré automatiquement, veuillez ne pas y répondre.";
                    Body += @"<br/><br/>Si vous éprouvez des difficultés ou s'il s'agit d'une erreur, veuillez le signaler à <a href='mailto:"
                         + Gmail.SMTP.OwnerEmail + "'>" + Gmail.SMTP.OwnerName + "</a> (Webmestre du site [nom de l'application])";

                    Gmail.SMTP.SendEmail(user.GetFullName(), unverifiedEmail.Email, Subject, Body);
                }
            }
        }
        public ActionResult VerifyDone(int id)
        {
            User newlySubscribedUser = DB.Users.Find(id);
            if (newlySubscribedUser != null)
                return View(newlySubscribedUser);
            return RedirectToAction("Login");
        }
        public ActionResult VerifyError()
        {
            return View();
        }
        public ActionResult AlreadyVerified()
        {
            return View();
        }
        public ActionResult VerifyUser(int userid, int code)
        {
            User newlySubscribedUser = DB.FindUser(userid);
            if (newlySubscribedUser != null)
            {
                if (!DB.HaveUnverifiedEmail(userid, code))
                {
                    if (DB.EmailVerified(newlySubscribedUser.Email))
                        return RedirectToAction("AlreadyVerified");
                }
                if (DB.Verify_User(userid, code))
                    return RedirectToAction("VerifyDone/" + userid);
            }
            return RedirectToAction("VerifyError");
        }
        #endregion

        #region EmailChange
        public ActionResult EmailChangedAlert()
        {
            OnlineUsers.RemoveSessionUser();
            return View();
        }
        public void SendEmailChangedVerification(User user, string newEmail)
        {
            if (user.Id != 0)
            {
                UnverifiedEmail unverifiedEmail = DB.Add_UnverifiedEmail(user.Id, newEmail);
                if (unverifiedEmail != null)
                {
                    string verificationUrl = Url.Action("VerifyUser", "Accounts", null, Request.Url.Scheme);
                    String Link = @"<br/><a href='" + verificationUrl + "?userid=" + user.Id + "&code=" + unverifiedEmail.VerificationCode + @"' > Confirmez votre adresse...</a>";

                    string Subject = "MySpace - Vérification de courriel...";

                    string Body = "Bonjour " + user.GetFullName(true) + @",<br/><br/>";
                    Body += @"Vous avez modifié votre adresse de courriel. <br/>";
                    Body += @"Pour que ce changement soit pris en compte, vous devez confirmer cette adresse en cliquant sur le lien suivant : <br/>";
                    Body += Link;
                    Body += @"<br/><br/>Ce courriel a été généré automatiquement, veuillez ne pas y répondre.";
                    Body += @"<br/><br/>Si vous éprouvez des difficultés ou s'il s'agit d'une erreur, veuillez le signaler à <a href='mailto:"
                         + Gmail.SMTP.OwnerEmail + "'>" + Gmail.SMTP.OwnerName + "</a> (Webmestre du site [nom de l'application])";

                    Gmail.SMTP.SendEmail(user.GetFullName(), unverifiedEmail.Email, Subject, Body);
                }
            }
        }
        #endregion

        #region ResetPassword
        public ActionResult ResetPasswordCommand()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ResetPasswordCommand(string Email)
        {
            if (ModelState.IsValid)
            {
                SendResetPasswordCommandEmail(Email);
                return RedirectToAction("ResetPasswordCommandAlert");
            }
            return View(Email);
        }
        public void SendResetPasswordCommandEmail(string email)
        {
            ResetPasswordCommand resetPasswordCommand = DB.Add_ResetPasswordCommand(email);
            if (resetPasswordCommand != null)
            {
                User user = DB.Users.Find(resetPasswordCommand.UserId);
                string verificationUrl = Url.Action("ResetPassword", "Accounts", null, Request.Url.Scheme);
                String Link = @"<br/><a href='" + verificationUrl + "?userid=" + user.Id + "&code=" + resetPasswordCommand.VerificationCode + @"' > Reinitialisation de mot de passe...</a>";

                string Subject = "MySpace - Reinitialisaton ...";

                string Body = "Bonjour " + user.GetFullName(true) + @",<br/><br/>";
                Body += @"Vous avez demandé de reinitialiser votre mot de passe. <br/>";
                Body += @"Procedez en cliquant sur le lien suivant : <br/>";
                Body += Link;
                Body += @"<br/><br/>Ce courriel a été généré automatiquement, veuillez ne pas y répondre.";
                Body += @"<br/><br/>Si vous éprouvez des difficultés ou s'il s'agit d'une erreur, veuillez le signaler à <a href='mailto:"
                     + Gmail.SMTP.OwnerEmail + "'>" + Gmail.SMTP.OwnerName + "</a> (Webmestre du site [nom de l'application])";

                Gmail.SMTP.SendEmail(user.GetFullName(), user.Email, Subject, Body);
            }
        }
        public ActionResult ResetPassword(int userid, int code)
        {
            ResetPasswordCommand resetPasswordCommand = DB.Find_ResetPasswordCommand(userid, code);
            if (resetPasswordCommand != null)
                return View(new PasswordView() { UserId = userid });
            return RedirectToAction("ResetPasswordError");
        }
        [HttpPost]
        public ActionResult ResetPassword(PasswordView passwordView)
        {
            if (ModelState.IsValid)
            {
                if (DB.ResetPassword(passwordView.UserId, passwordView.Password))
                    return RedirectToAction("ResetPasswordSuccess");
                else
                    return RedirectToAction("ResetPasswordError");
            }
            return View(passwordView);
        }
        public ActionResult ResetPasswordCommandAlert()
        {
            return View();
        }
        public ActionResult ResetPasswordSuccess()
        {
            return View();
        }
        public ActionResult ResetPasswordError()
        {
            return View();
        }
        #endregion

        #region Profil
        [UserAccess]
        public ActionResult Profil()
        {
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
                        ViewBag.UserTypes = SelectListItemConverter<UserType>.Convert(DB.UserTypes.Where(u => u.Id > 1).ToList());
            return View(OnlineUsers.GetSessionUser());
        }

        [HttpPost]
        public ActionResult Profil(User user)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            string newEmail = "";
            if (ModelState.IsValid)
            {
                if (user.Password == "Not Changed")
                {
                    user.Password = currentUser.Password;
                    user.ConfirmPassword = currentUser.Password;
                }

                if (user.Email != currentUser.Email)
                {
                    newEmail = user.Email;
                    user.Email = user.ConfirmEmail = currentUser.Email;
                }

                user = DB.Update_User(user);

                if (newEmail != "")
                {
                    SendEmailChangedVerification(user, newEmail);
                    return RedirectToAction("EmailChangedAlert");
                }
                else
                    return RedirectToAction("Index", "Application");
            }
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
                        ViewBag.UserTypes = SelectListItemConverter<UserType>.Convert(DB.UserTypes.Where(u => u.Id > 1).ToList());
            return View(currentUser);
        }
        #endregion

        #region Login and Logout
        public ActionResult Login(string message)
        {
            ViewBag.Message = message;
            return View(new LoginCredential());
        }

        [HttpPost]
        public ActionResult Login(LoginCredential loginCredential)
        {
            if (ModelState.IsValid)
            {
                if (DB.EmailBlocked(loginCredential.Email))
                {
                    ModelState.AddModelError("Email", "Ce courriel est bloqué.");
                    return View(loginCredential);
                }
                if (!DB.EmailVerified(loginCredential.Email))
                {
                    ModelState.AddModelError("Email", "Ce courriel n'est pas vérifié.");
                    return View(loginCredential);
                }
                User user = DB.GetUser(loginCredential);
                if (user == null)
                {
                    ModelState.AddModelError("Password", "Mot de passe incorrecte.");
                    return View(loginCredential);
                }
                if (OnlineUsers.IsOnLine(user.Id))
                {
                    ModelState.AddModelError("Email", "Cet usager est déjà connecté.");
                    return View(loginCredential);
                }
                //OnlineUsers.AddSessionUser(user.Id);
                OnlineUsers.MakeCurrentUser(user);
                DateTime serverDate = DateTime.Now;
                DateTime universalDate = serverDate.ToUniversalTime();
                int serverTimeZoneOffset = serverDate.Hour - universalDate.Hour;
                if (user.IsArtist)
                {
                    Session["artisteId"] = DB.Artists.Where(a => a.UserId == user.Id).ToArray()[0].Id;
                }
                Session["TimeZoneOffset"] = loginCredential.TimeZoneOffset + serverTimeZoneOffset;
                Session["currentLoginId"] = DB.AddLogin(user.Id).Id;
                return RedirectToAction("Index", "Artists");
            }
            return View(loginCredential);
        }

        public ActionResult Logout()
        {
            DB.UpdateLogout((int)Session["currentLoginId"]);
            OnlineUsers.RemoveSessionUser();
            return RedirectToAction("Login");
        }
        #endregion

        #region Administrator actions
        public JsonResult NeedUpdate()
        {
            return Json(OnlineUsers.NeedUpdate(), JsonRequestBehavior.AllowGet);
        }
        [AdminAccess]
        public ActionResult UserList()
        {
            return View();
        }

        [AdminAccess]
        public ActionResult ChangeUserBlockedStatus(int userid, bool blocked)
        {
            User user = DB.FindUser(userid);
            user.Blocked = blocked;
            DB.Update_User(user);
            return null;
        }

        [AdminAccess]
        public ActionResult Delete(int id)
        {
            DB.RemoveUser(id);
            return null;
        }

        [AdminAccess(false)] // RefreshTimout = false otherwise periodical refresh with lead to never timed out session
        public ActionResult GetUsersList(bool forceRefresh = false)
        {
            if (forceRefresh || OnlineUsers.NeedUpdate())
            {
                return PartialView(DB.SortedUsers());
            }
            return null;
        }

        [AdminAccess]
        public ActionResult ChangeUserAccess(int userid)
        {
            User user = DB.FindUser(userid);
            user.UserTypeId = (user.UserTypeId + 1) % 3 + 1;
            DB.Update_User(user);
            return null;
        }
        [AdminAccess]
        public ActionResult EditProfil(int id)
        {
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
            ViewBag.UserTypes = SelectListItemConverter<UserType>.Convert(DB.UserTypes.Where(u => u.Id > 1).ToList());
            User user = DB.Users.Find(id);
            if (user != null)
            {
                UserClone uc = new UserClone(user);
                return View(uc);
            }
            else
                return null;
        }

        [HttpPost]
        public ActionResult EditProfil(UserClone uc)
        {
            if (ModelState.IsValid)
            {
                User user = DB.Users.Find(uc.Id);
                uc.CopyToUser(user);
                DB.Update_User(user);
                return RedirectToAction("UserList");
            }
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
            ViewBag.UserTypes = SelectListItemConverter<UserType>.Convert(DB.UserTypes.Where(u => u.Id > 1).ToList());
            return View(uc);
        }
        #endregion

        #region GroupEmail
        [AdminAccess]
        public ActionResult GroupEmail()
        {
            ViewBag.SelectedUsers = new List<int>();
            ViewBag.Users = DB.SortedUsers();
            return View(new GroupEmail());
        }
        [HttpPost]
        public ActionResult GroupEmail(GroupEmail groupEmail, List<int> SelectedUsers)
        {
            if (ModelState.IsValid)
            {
                groupEmail.SelectedUsers = SelectedUsers;
                groupEmail.Send(DB);
                return RedirectToAction("UserList");
            }
            ViewBag.SelectedUsers = SelectedUsers;
            ViewBag.Users = DB.SortedUsers();
            return View(groupEmail);
        }
        #endregion

        #region Login journal
        [AdminAccess]
        public ActionResult LoginsJournal()
        {
            return View();
        }
        [AdminAccess(false)] // RefreshTimout = false otherwise periodical refresh with lead to never timed out session
        public ActionResult GetLoginsList(bool forceRefresh = false)
        {
            if (forceRefresh || OnlineUsers.NeedUpdate())
            {
                ViewBag.LoggedUsersId = new List<int>(OnlineUsers.UsersId);
                return PartialView(DB.Logins.OrderByDescending(l => l.LoginDate));
            }
            return null;
        }
        [AdminAccess]
        public ActionResult DeleteJournalDay(string day)
        {
            try
            {
                DateTime date = DateTime.ParseExact(day, "dd-MM-yyyy", null);
                DB.DeleteLoginsJournalDay(date);
            }
            catch (Exception) { }
            return null;
        }
        #endregion
    }
}
