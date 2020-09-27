using System.ComponentModel.DataAnnotations;

namespace TraceGenie.Web.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Twój login")]
        public string Login { get; set; }
        [Required]
        [Display(Name = "Twoje hasło")]
        public string Password { get; set; }

        public bool LoginError { get; set; }
    }
}
