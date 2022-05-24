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
        #region FanLikes
        public static FanLike Add_FanLike(this MySpaceDBEntities DB, FanLike fanLike, int artistId)
        {
            if (fanLike != null)
            {
                fanLike = DB.FanLikes.Add(fanLike);
                DB.Artists.Find(artistId).Likes++;
                DB.SaveChanges();
            }
            return fanLike;
        }
        public static bool Remove_FanLike(this MySpaceDBEntities DB, FanLike fanLike, int artistId)
        {
            if (fanLike != null)
            {
                BeginTransaction(DB);
                DB.FanLikes.Remove(fanLike);
                DB.Artists.Find(artistId).Likes--;
                DB.SaveChanges();
                Commit();
                return true;
            }
            return false;
        }
        #endregion

        public static Artist Update_Artist(this MySpaceDBEntities DB, Artist artist)
        {
            DB.Entry(artist).State = EntityState.Modified;
            DB.SaveChanges();
            DB.Entry(artist).Reference(u => u.User).Load();
            OnlineUsers.RenewSerialNumber();
            return artist;
        }

        public static Artist Add_Artist(this MySpaceDBEntities DB, Artist artist, User user)
        {
            artist.Name = user.FirstName;
            artist.MainPhotoGUID = "~/ImagesData/ArtistImages/No_Artist_Image.png";
            artist.Description = "Entrez une description";
            artist.Approved = false;
            artist.Likes = 0;
            artist.Visits = 0;
            artist.UserId = user.Id;
            artist = DB.Artists.Add(artist);
            DB.SaveChanges();
            DB.Entry(artist).Reference(u => u.User).Load();
            OnlineUsers.RenewSerialNumber();
            return artist;
        }

        public static void Add_Visit(this MySpaceDBEntities DB, int? artistId)
        {
            DB.Artists.Find(artistId).Visits++;
            DB.SaveChanges();
        }
    }
}