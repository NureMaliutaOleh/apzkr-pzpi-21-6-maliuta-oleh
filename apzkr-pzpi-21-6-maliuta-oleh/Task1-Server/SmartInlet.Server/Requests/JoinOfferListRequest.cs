using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class JoinOfferListRequest
    {
        [FromQuery(Name = "sort_parameter")]
        public string SortParameter { get; set; } = "date";
        [FromQuery(Name = "sort_direction")]
        [RegularExpression("^(asc|desc)$")]
        public string SortDirection { get; set; } = "asc";
    }
}
