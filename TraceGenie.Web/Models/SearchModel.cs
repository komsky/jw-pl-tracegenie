using System.ComponentModel.DataAnnotations;

namespace TraceGenie.Web.Models
{
    public class SearchModel
    {
        public SearchModel()
        {
            FilterPolishNames = true;
        }
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name ="Post code")]
        public string PostCode { get; set; }
        [Display(Name = "Filtruj tylko polskie imiona")]
        public bool FilterPolishNames { get; set; }
    }
}
