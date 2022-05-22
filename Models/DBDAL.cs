using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MySpace.Models
{
    public static partial class DBDAL
    {
        private static DbContextTransaction Transaction
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return (DbContextTransaction)HttpContext.Current.Session["Transaction"];
                }
                return null;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Session["Transaction"] = value;
                }
            }
        }
        private static void BeginTransaction(MySpaceDBEntities DB)
        {
            if (Transaction != null)
                throw new Exception("Transaction en cours! Impossible d'en démarrer une nouvelle!");
            Transaction = DB.Database.BeginTransaction();
        }
        private static void Commit()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;
            }
            else
                throw new Exception("Aucune transaction en cours! Impossible de mettre à jour la base de données!");
        }

        public static bool EmailAvailable(this MySpaceDBEntities DB, string email, int excludedId = 0)
        {
            User user = DB.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            if (user == null)
                return true;
            else
                if (user.Id != excludedId)
                return user.Email.ToLower() != email.ToLower();
            return true;
        }
        public static bool EmailExist(this MySpaceDBEntities DB, string email)
        {
            return DB.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault() != null;
        }
        public static bool EmailBlocked(this MySpaceDBEntities DB, string email)
        {
            User user = DB.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            if (user != null)
                return user.Blocked;
            return true;
        }
        public static bool EmailVerified(this MySpaceDBEntities DB, string email)
        {
            User user = DB.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            if (user != null)
                return user.Verified;
            return false;
        }

        public static User GetUser(this MySpaceDBEntities DB, LoginCredential loginCredential)
        {
            User user = DB.Users.Where(u => (u.Email.ToLower() == loginCredential.Email.ToLower()) &&
                                            (u.Password == loginCredential.Password))
                                .FirstOrDefault();
            return user;
        }
        public static User Add_User(this MySpaceDBEntities DB, User user)
        {
            user.SaveAvatar();
            user = DB.Users.Add(user);
            DB.SaveChanges();
            DB.Entry(user).Reference(u => u.Gender).Load();
            DB.Entry(user).Reference(u => u.UserType).Load();
            OnlineUsers.RenewSerialNumber();
            return user;
        }

        public static Artist Add_Artist(this MySpaceDBEntities DB, Artist artiste, User user)
        {
            artiste.Name = user.FirstName;
            artiste.MainPhotoGUID = "~/Content/UI-icons/defaultUser.png";
            artiste.Description = "Entrez une description";
            artiste.Approved = false;
            artiste.Likes = 0;
            artiste.Visits = 0;
            artiste.UserId = user.Id;
            artiste.Id = DB.Artists.Count() + 1;
            artiste = DB.Artists.Add(artiste);
            DB.SaveChanges();
            DB.Entry(artiste).Reference(u => u.User).Load();
            OnlineUsers.RenewSerialNumber();
            return artiste;
        }

        public static User Update_User(this MySpaceDBEntities DB, User user)
        {
            user.SaveAvatar();
            DB.Entry(user).State = EntityState.Modified;
            DB.SaveChanges();
            DB.Entry(user).Reference(u => u.Gender).Load();
            DB.Entry(user).Reference(u => u.UserType).Load();
            OnlineUsers.RenewSerialNumber();
            return user;
        }
        public static bool RemoveUser(this MySpaceDBEntities DB, int userId)
        {
            User userToDelete = DB.Users.Find(userId);
            if (userToDelete != null)
            {
                BeginTransaction(DB);
                OnlineUsers.RemoveUser(userToDelete.Id);

                DB.DeleteFriendShips(userId);
                DB.Logins.RemoveRange(DB.Logins.Where(l => l.UserId == userId));

                userToDelete.RemoveAvatar();
                DB.Users.Remove(userToDelete);
                DB.SaveChanges();

                Commit();
                return true;
            }
            return false;
        }
        public static User FindUser(this MySpaceDBEntities DB, int id)
        {
            User user = DB.Users.Find(id);
            if (user != null)
            {
                user.ConfirmEmail = user.Email;
                user.ConfirmPassword = user.Password;
                DB.Entry(user).Reference(u => u.Gender).Load();
                DB.Entry(user).Reference(u => u.UserType).Load();
            }
            return user;
        }
        public static IEnumerable<User> SortedUsers(this MySpaceDBEntities DB)
        {
            return DB.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);
        }
        public static bool Verify_User(this MySpaceDBEntities DB, int userId, int code)
        {
            User user = DB.FindUser(userId);
            if (user != null)
            {
                UnverifiedEmail unverifiedEmail = DB.UnverifiedEmails.Where(u => u.UserId == userId).FirstOrDefault();
                if (unverifiedEmail != null)
                {
                    if (unverifiedEmail.VerificationCode == code)
                    {
                        BeginTransaction(DB);
                        user.Email = user.ConfirmEmail = unverifiedEmail.Email;
                        user.Verified = true;
                        DB.Entry(user).State = EntityState.Modified;
                        DB.UnverifiedEmails.Remove(unverifiedEmail);
                        DB.SaveChanges();
                        Commit();
                        OnlineUsers.RenewSerialNumber();
                        return true;
                    }
                }
            }
            return false;
        }

        public static UnverifiedEmail Add_UnverifiedEmail(this MySpaceDBEntities DB, int userId, string email)
        {

            UnverifiedEmail unverifiedEmail = new UnverifiedEmail() { UserId = userId, Email = email, VerificationCode = DateTime.Now.Millisecond };
            unverifiedEmail = DB.UnverifiedEmails.Add(unverifiedEmail);
            DB.SaveChanges();
            return unverifiedEmail;
        }
        public static bool HaveUnverifiedEmail(this MySpaceDBEntities DB, int userId, int code)
        {
            return DB.UnverifiedEmails.Where(u => (u.UserId == userId && u.VerificationCode == code)).FirstOrDefault() != null;
        }
        public static ResetPasswordCommand Add_ResetPasswordCommand(this MySpaceDBEntities DB, string email)
        {
            User user = DB.Users.Where(u => u.Email == email).FirstOrDefault();
            if (user != null)
            {
                ResetPasswordCommand resetPasswordCommand = new ResetPasswordCommand() { UserId = user.Id, VerificationCode = DateTime.Now.Millisecond };
                resetPasswordCommand = DB.ResetPasswordCommands.Add(resetPasswordCommand);
                DB.SaveChanges();
                return resetPasswordCommand;
            }
            return null;
        }
        public static ResetPasswordCommand Find_ResetPasswordCommand(this MySpaceDBEntities DB, int userid, int verificationCode)
        {
            return DB.ResetPasswordCommands.Where(r => (r.UserId == userid && r.VerificationCode == verificationCode)).FirstOrDefault();
        }
        public static bool ResetPassword(this MySpaceDBEntities DB, int userId, string password)
        {
            User user = DB.FindUser(userId);
            if (user != null)
            {
                user.Password = user.ConfirmPassword = password;
                ResetPasswordCommand resetPasswordCommand = DB.ResetPasswordCommands.Where(r => r.UserId == userId).FirstOrDefault();
                if (resetPasswordCommand != null)
                {
                    BeginTransaction(DB);
                    DB.Entry(user).State = EntityState.Modified;
                    DB.ResetPasswordCommands.Remove(resetPasswordCommand);
                    DB.SaveChanges();
                    Commit();
                    return true;
                }
            }
            return false;
        }
        public static Login AddLogin(this MySpaceDBEntities DB, int userId)
        {
            Login login = new Login();
            login.LoginDate = login.LogoutDate = DateTime.Now;
            login.UserId = userId;
            login = DB.Logins.Add(login);
            DB.SaveChanges();
            return login;
        }
        public static bool UpdateLogout(this MySpaceDBEntities DB, int loginId)
        {
            Login login = DB.Logins.Find(loginId);
            if (login != null)
            {
                login.LogoutDate = DateTime.Now;
                DB.Entry(login).State = EntityState.Modified;
                DB.SaveChanges();
                return true;
            }
            return false;
        }
        public static bool UpdateLogoutByUserId(this MySpaceDBEntities DB, int userId)
        {
            Login login = DB.Logins.Where(l => l.UserId == userId).OrderByDescending(l => l.LoginDate).FirstOrDefault();
            if (login != null)
            {
                login.LogoutDate = DateTime.Now;
                DB.Entry(login).State = EntityState.Modified;
                DB.SaveChanges();
                return true;
            }
            return false;
        }
        public static bool DeleteLoginsJournalDay(this MySpaceDBEntities DB, DateTime day)
        {
            DateTime dayAfter = day.AddDays(1);
            DB.Logins.RemoveRange(DB.Logins.Where(l => l.LoginDate >= day && l.LoginDate < dayAfter));
            DB.SaveChanges();
            OnlineUsers.RenewSerialNumber();
            return true;
        }

        public static FriendShip Add_FiendShipRequest(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            User user = DB.Users.Find(userId);
            User targetUser = DB.FindUser(targetUserId);
            if (user != null && targetUser != null)
            {
                BeginTransaction(DB);
                DB.Remove_FiendShipRequest(userId, targetUserId);
                FriendShip friendShip = new FriendShip();
                friendShip.UserId = user.Id;
                friendShip.TargetUserId = targetUser.Id;
                friendShip.CreationDate = DateTime.Now;
                friendShip.Accepted = false;
                friendShip.Declined = false;
                friendShip = DB.FriendShips.Add(friendShip);
                DB.SaveChanges();
                Commit();
                return friendShip;
            }
            return null;
        }
        public static bool Remove_FiendShipRequest(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            User user = DB.Users.Find(userId);
            User targetUser = DB.FindUser(targetUserId);
            if (user != null && targetUser != null)
            {
                DB.FriendShips.RemoveRange(DB.FriendShips.Where(f => f.UserId == userId && f.TargetUserId == targetUserId));
                DB.FriendShips.RemoveRange(DB.FriendShips.Where(f => f.UserId == targetUserId && f.TargetUserId == userId));
                DB.SaveChanges();
            }
            return true;
        }
        public static bool Accept_FriendShip(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            FriendShip friendShip = DB.FriendShips.Where(f => (f.UserId == userId && f.TargetUserId == targetUserId)).FirstOrDefault();
            if (friendShip != null)
            {
                friendShip.Accepted = true;
                DB.Entry(friendShip).State = EntityState.Modified;
                DB.SaveChanges();
                return true;
            }
            return false;
        }
        public static bool Decline_FriendShip(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            FriendShip friendShip = DB.FriendShips.Where(f => (f.UserId == userId && f.TargetUserId == targetUserId)).FirstOrDefault();
            if (friendShip != null)
            {
                friendShip.Declined = true;
                DB.Entry(friendShip).State = EntityState.Modified;
                DB.SaveChanges();
                return true;
            }
            return false;
        }
        public static bool AreFriends(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            User targetUser = DB.Users.Find(targetUserId);
            if (targetUser != null)
            {
                if (targetUser.Blocked)
                    return false;
            }
            else
                return false;
            User user = DB.Users.Find(userId);
            if (user != null)
            {
                if (user.Blocked)
                    return false;
            }
            else
                return false;

            FriendShip friendShip = DB.FriendShips.Where(f => (f.UserId == userId && f.TargetUserId == targetUserId)).FirstOrDefault();
            if (friendShip != null)
            {
                return friendShip.Accepted;
            }
            friendShip = DB.FriendShips.Where(f => (f.UserId == targetUserId && f.TargetUserId == userId)).FirstOrDefault();
            if (friendShip != null)
            {
                return friendShip.Accepted;
            }
            return false;
        }
        public static bool FriendShipDeclined(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            FriendShip friendShip = DB.FriendShips.Where(f => (f.UserId == userId && f.TargetUserId == targetUserId)).FirstOrDefault();
            if (friendShip != null)
            {
                return friendShip.Declined;
            }
            friendShip = DB.FriendShips.Where(f => (f.UserId == targetUserId && f.TargetUserId == userId)).FirstOrDefault();
            if (friendShip != null)
            {
                return friendShip.Declined;
            }
            return false;
        }
        public static bool NotFriends(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            FriendShip friendShipOfUser = DB.FriendShips.Where(f => (f.UserId == userId && f.TargetUserId == targetUserId)).FirstOrDefault();
            FriendShip friendShipOfTargetUser = DB.FriendShips.Where(f => (f.UserId == targetUserId && f.TargetUserId == userId)).FirstOrDefault();
            return (friendShipOfUser == null && friendShipOfTargetUser == null);
        }

        private static int FriendShipStatus(this MySpaceDBEntities DB, int userId, int targetUserId)
        {
            FriendShip friendShipOfUser = DB.FriendShips.Where(f => (f.UserId == userId && f.TargetUserId == targetUserId)).FirstOrDefault();
            FriendShip friendShipOfTargetUser = DB.FriendShips.Where(f => (f.UserId == targetUserId && f.TargetUserId == userId)).FirstOrDefault();
            if (friendShipOfUser != null)
            {
                if (friendShipOfUser.Accepted)
                    return 1; // friend
                if (friendShipOfUser.Declined)
                    return 2; // targetUser declined
                return 3; // request friendship pending
            }
            if (friendShipOfTargetUser != null)
            {
                if (friendShipOfTargetUser.Accepted)
                    return 1; // friend
                if (friendShipOfTargetUser.Declined)
                    return 4; // user declined
                return 5; // request friendship offer
            }
            return 0; // not friend
        }
        public static List<FriendShipState> FriendShipsStatus(this MySpaceDBEntities DB, int userId)
        {
            List<FriendShipState> friendShipsStatus = new List<FriendShipState>();
            foreach (User targetUser in DB.SortedUsers())
            {
                if (targetUser.Id != userId)
                {
                    friendShipsStatus.Add(new FriendShipState(targetUser, DB.FriendShipStatus(userId, targetUser.Id)));
                }
            }
            return friendShipsStatus;
        }

        public static bool DeleteFriendShips(this MySpaceDBEntities DB, int userId)
        {
            DB.FriendShips.RemoveRange(DB.FriendShips.Where(f => f.UserId == userId));
            DB.FriendShips.RemoveRange(DB.FriendShips.Where(f => f.TargetUserId == userId));
            DB.SaveChanges();
            return true;
        }

    }
}