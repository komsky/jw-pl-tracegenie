using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TraceGenie.Client.Models;

namespace TraceGenie.Web.Models
{
    public class FoundModel
    {
        public FoundModel()
        {
            Records = new List<TraceGenieEntry>();
        }

        [Display(Name = "Post code")]
        public string PostCode { get; set; }
        [Display(Name ="Znaleziono wpisów")]
        public int RecordsFound { get; set; }

        public List<TraceGenieEntry> Records { get; set; }
    }
}
