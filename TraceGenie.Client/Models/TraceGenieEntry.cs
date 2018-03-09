namespace TraceGenie.Client.Models
{
    public class TraceGenieEntry
    {
        public string FullName { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"{FullName};{Address}";
        }
    }
}
