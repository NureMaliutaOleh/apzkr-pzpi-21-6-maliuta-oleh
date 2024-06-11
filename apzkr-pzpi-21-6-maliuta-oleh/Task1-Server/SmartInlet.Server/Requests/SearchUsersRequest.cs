using Microsoft.AspNetCore.Mvc;

namespace SmartInlet.Server.Requests
{
    public class SearchUsersRequest
    {
        [FromQuery(Name = "q")]
        public string? Query { get; set; }
    }
}
