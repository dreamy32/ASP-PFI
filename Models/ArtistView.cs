using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MySpace.Models
{
    public partial class Artist
    {
        public Artist()
        {
            FanLikes = new HashSet<FanLike>();
            Messages = new HashSet<Message>();
            Videos = new HashSet<Video>();
        }


        [NotMapped]
        public static ImageGUIDReference AvatarReference =
            new ImageGUIDReference(@"/ImagesData/ArtistImages/", @"No_Artist_Image.png", false);

        public String GetAvatarURL()
        {
            return AvatarReference.GetURL(MainPhotoGUID, false);
        }

    }

    public class ArtistView
    {
        [Display(Name = "Photo")]
        public string MainPhotoGUID { get; set; }
    }
}