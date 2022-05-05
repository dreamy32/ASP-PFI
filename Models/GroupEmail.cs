using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MySpace.Models
{
    public class GroupEmail
    {
        public List<int> SelectedUsers { get; set; }

        [Display(Name = "Sujet"), Required(ErrorMessage = "Obligatoire")]
        public string Subject { get; set; }

        [Display(Name = "Message"), Required(ErrorMessage = "Obligatoire")]
        public string Message { get; set; }

        public void Send(MySpaceDBEntities DB)
        {
            if (SelectedUsers != null)
            {
                foreach (int userId in SelectedUsers)
                {
                    User user = DB.Users.Find(userId);
                    string personalizedMessage = Message.Replace("[Nom]", user.GetFullName(true)).Replace("\r\n", @"<br>");
                    Gmail.SMTP.SendEmail(user.GetFullName(), user.Email, Subject, personalizedMessage);
                }
            }
        }
    }
}