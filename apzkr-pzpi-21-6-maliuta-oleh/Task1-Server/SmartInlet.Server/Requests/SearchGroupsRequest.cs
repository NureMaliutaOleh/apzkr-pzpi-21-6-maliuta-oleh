using Microsoft.AspNetCore.Mvc;

namespace SmartInlet.Server.Requests
{
    public class SearchGroupsRequest
    {
        [FromQuery(Name = "q")]
        public string? Query { get; set; }
    }
}
