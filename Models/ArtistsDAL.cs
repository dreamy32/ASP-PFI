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
        public static Message AddArtistMessage(this MySpaceDBEntities DB, Message message)
        {
            if (message != null)
            {
                message = DB.Messages.Add(message);
                DB.SaveChanges();
            }
            return message;
        }
    }
}