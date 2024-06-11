using Microsoft.AspNetCore.Mvc;

namespace SmartInlet.Server.Requests
{
    public class SearchDevicesRequest
    {
        [FromQuery(Name = "q")]
        public string? Query { get; set; }
    }
}
