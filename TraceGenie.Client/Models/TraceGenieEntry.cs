namespace TraceGenie.Client.Models
{
    public class TraceGenieEntry
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }

        public override string ToString()
        {
            return $"{FullName};{Address}:{PostCode}";
        }
        public string ToString(string separator)
        {
            return $"{FullName}{separator}{Address}{separator}{PostCode}";
        }
    }
}
