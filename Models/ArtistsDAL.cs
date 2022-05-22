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
    }
}