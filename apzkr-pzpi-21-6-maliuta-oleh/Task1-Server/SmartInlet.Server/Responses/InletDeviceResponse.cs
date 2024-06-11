using SmartInlet.Server.Models;
using System.Text.Json.Serialization;

namespace SmartInlet.Server.Responses
{
    public class InletDeviceResponse : BaseResponse
    {
        public InletDeviceResponse(InletDevice data) : base(true, null, null)
        {
            Data = new View(data);
        }

        public InletDeviceResponse(ICollection<InletDevice> user) : base(true, null, null)
        {
            Data = user.Select(p => new View(p));
        }

        public class View
        {
            [JsonPropertyName("group")]
            public string? Group { get; set; }
            [JsonPropertyName("airSensorId")]
            public int? AirSensorId { get; set; }
            [JsonPropertyName("airSensor")]
            public string? AirSensor { get; set; }
            [JsonPropertyName("tempSensorId")]
            public int? TempSensorId { get; set; }
            [JsonPropertyName("tempSensor")]
            public string? TempSensor { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("controlType")]
            public string ControlType { get; set; }
            [JsonPropertyName("isOpened")]
            public bool IsOpened { get; set; }
            [JsonPropertyName("isBlocked")]
            public bool IsBlocked { get; set; }
            [JsonPropertyName("updatedAt")]
            public DateTime UpdatedAt { get; set; }

            public View(InletDevice obj)
            {
                Group = obj.Group?.Name;
                AirSensorId = obj.AirSensorId;
                AirSensor = obj.AirSensor?.Name;
                TempSensorId = obj.TempSensorId;
                TempSensor = obj.TempSensor?.Name;
                Name = obj.Name;
                ControlType = obj.ControlType;
                IsOpened = obj.IsOpened;
                IsBlocked = obj.IsBlocked;
                UpdatedAt = obj.UpdatedAt;
            }
        }
    }
}
