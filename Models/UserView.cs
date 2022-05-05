using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MySpace.Models
{
    [MetadataType(typeof(UserView))]
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            CreationDate = DateTime.Now;
            UserTypeId = 3; // User rights level
            Verified = false;
            Blocked = false;
            Logins = new HashSet<Login>();
        }

        [NotMapped]
        public static ImageGUIDReference AvatarReference =
            new ImageGUIDReference(@"/ImagesData/Avatars/", @"no_avatar.png", false);

        public String GetAvatarURL()
        {
            return AvatarReference.GetURL(Avatar, false);
        }
        public void SaveAvatar()
        {
            Avatar = AvatarReference.SaveImage(AvatarImageData, Avatar);
        }
        public void RemoveAvatar()
        {
            AvatarReference.Remove(Avatar);
        }

        [NotMapped]
        public string ConfirmEmail { get; set; }

        [NotMapped]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Avatar")]
        [NotMapped]
        public string AvatarImageData { get; set; }

        public bool IsAdmin
        {
            get { return UserTypeId == 1 /*Admin*/; }
        }
        public string GetFullName(bool showGender = false)
        {
            if (showGender)
            {
                if (Gender.Name != "Neutre")
                    return Gender.Name + " " + LastName;
            }
            return FirstName + " " + LastName;
        }

        public User Clone()
        {
            User user = new User();
            user.Id = this.Id;
            user.UserTypeId = this.UserTypeId;
            user.UserType = null;
            user.CreationDate = this.CreationDate;
            user.Email = this.Email;
            user.FirstName = this.FirstName;
            user.LastName = this.LastName;
            user.GenderId = this.GenderId;
            user.Gender = null;
            user.Avatar = this.Avatar;
            user.Password = this.Password;
            user.Blocked = this.Blocked;
            return user;
        }
    }

    public class UserView
    {
        [Display(Name = "Prenom"), Required(ErrorMessage = "Obligatoire")]
        public string FirstName { get; set; }

        [Display(Name = "Nom"), Required(ErrorMessage = "Obligatoire")]
        public string LastName { get; set; }

        [Display(Name = "Avatar")]
        public string Avatar { get; set; }

        [Display(Name = "Genre")]
        public int GenderId { get; set; }

        [Display(Name = "Courriel"), EmailAddress(ErrorMessage = "Invalide"), Required(ErrorMessage = "Obligatoire")]
        [System.Web.Mvc.Remote("EmailAvailable", "Accounts", HttpMethod = "POST", AdditionalFields = "Id", ErrorMessage = "Ce courriel n'est pas disponible.")]
        public string Email { get; set; }

        [Display(Name = "Confirmation")]
        [Compare("Email", ErrorMessage = "Le courriel et celui de confirmation ne correspondent pas.")]
        public string ConfirmEmail { get; set; }

        [Display(Name = "Mot de passe"), Required(ErrorMessage = "Obligatoire")]
        [StringLength(50, ErrorMessage = "Le mot de passe doit comporter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation")]
        [Compare("Password", ErrorMessage = "Le mot de passe et celui de confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Genre")]
        public virtual Gender Gender { get; set; }

        [Display(Name = "Date de création")]
        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; }
    }
    public partial class UserClone
    {
        public UserClone() { }
        public UserClone(User user)
        {
            CreationDate = user.CreationDate;
            UserTypeId = user.UserTypeId;
            GenderId = user.GenderId;
            Verified = user.Verified;
            Blocked = user.Blocked;
            Avatar = user.Avatar;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Password = user.Password;
            Email = user.Email;
            Id = user.Id;
            AvatarImageData = user.AvatarImageData;
        }

        public void CopyToUser(User user)
        {
            user.CreationDate = CreationDate;
            user.UserTypeId = UserTypeId;
            user.GenderId = GenderId;
            user.Verified = Verified;
            user.Blocked = Blocked;
            user.Avatar = Avatar;
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.Password = user.ConfirmPassword = Password;
            user.Email = user.ConfirmEmail = Email;
            user.Id = Id;
            user.AvatarImageData = AvatarImageData;
        }

        public int Id { get; set; }

        public int UserTypeId { get; set; }

        [Display(Name = "Prenom"), Required(ErrorMessage = "Obligatoire")]
        public string FirstName { get; set; }

        [Display(Name = "Nom"), Required(ErrorMessage = "Obligatoire")]
        public string LastName { get; set; }

        [Display(Name = "Courriel"), EmailAddress(ErrorMessage = "Invalide"), Required(ErrorMessage = "Obligatoire")]
        [System.Web.Mvc.Remote("EmailAvailable", "Accounts", HttpMethod = "POST", AdditionalFields = "Id", ErrorMessage = "Ce courriel n'est pas disponible.")]
        public string Email { get; set; }

        public string Avatar { get; set; }

        [Display(Name = "Genre")]
        public int GenderId { get; set; }

        public String Password { get; set; }


        [Display(Name = "Date de création")]
        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; }

        public bool Verified { get; set; }

        public bool Blocked { get; set; }

        public string AvatarImageData { get; set; }

        public String GetAvatarURL()
        {
            return User.AvatarReference.GetURL(Avatar, false);
        }
    }
}