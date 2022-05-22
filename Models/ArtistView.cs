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
    }
}