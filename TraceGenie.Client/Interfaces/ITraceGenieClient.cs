using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TraceGenie.Client.Models;

namespace TraceGenie.Client.Interfaces
{
    public interface ITraceGenieClient : IDisposable
    {
        Task<bool> Login(string username, string password);
        Task<List<TraceGenieEntry>> SearchAllYears(string postcode);
        Task<List<TraceGenieEntry>> SearchSingleYear(string postcode, string year);
    }
}
