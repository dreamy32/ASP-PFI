﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MySpace.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MySpaceDBEntities : DbContext
    {
        public MySpaceDBEntities()
            : base("name=MySpaceDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<FriendShip> FriendShips { get; set; }
        public virtual DbSet<Gender> Genders { get; set; }
        public virtual DbSet<Login> Logins { get; set; }
        public virtual DbSet<ResetPasswordCommand> ResetPasswordCommands { get; set; }
        public virtual DbSet<UnverifiedEmail> UnverifiedEmails { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
        public virtual DbSet<Artist> Artists { get; set; }
        public virtual DbSet<FanLike> FanLikes { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Video> Videos { get; set; }
    }
}
