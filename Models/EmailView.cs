using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MySpace.Models
{
    public class EmailView
    {
        [Display(Name = "Courriel"), EmailAddress(ErrorMessage = "Invalide"), Required(ErrorMessage = "Obligatoire")]
        [System.Web.Mvc.Remote("EmailExist", "Accounts", HttpMethod = "POST", ErrorMessage = "Ce courriel n'existe pas.")]
        public string Email { get; set; }
    }
}