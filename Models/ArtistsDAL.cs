using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
namespace MySpace.Models
{
    //Séparation du DAL dans un autre fichier pour Artiste
    public static partial class DBDAL
    {
        public static Message Add_Message(this MySpaceDBEntities DB, Message message)
        {
            if (message != null)
            {
                message = DB.Messages.Add(message);
                DB.SaveChanges();
            }
            return message;
        }
        public static Video Add_Video(this MySpaceDBEntities DB, Video video)
        {
            Video existingVideo = DB.Videos.Where(v => v.YoutubeId == video.YoutubeId).FirstOrDefault();
            if (existingVideo == null)
            {
                video = DB.Videos.Add(video);
                DB.SaveChanges();
            }
            return video;
        }

        public static Artist Update_Artist(this MySpaceDBEntities DB, Artist artist)
        {
            DB.Entry(artist).State = EntityState.Modified;
            DB.SaveChanges();
            DB.Entry(artist).Reference(u => u.User).Load();
            OnlineUsers.RenewSerialNumber();
            return artist;
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
    }
}