using SmartInlet.Server.Models;
using System.Text.Json.Serialization;

namespace SmartInlet.Server.Responses
{
    public class AirSensorResponse : BaseResponse
    {
        public AirSensorResponse(AirSensor data) : base(true, null, null)
        {
            Data = new View(data);
        }

        public AirSensorResponse(ICollection<AirSensor> user) : base(true, null, null)
        {
            Data = user.Select(p => new View(p));
        }

        public class View
        {
            [JsonPropertyName("group")]
            public string? Group { get; set; }
            [JsonPropertyName("inletDeviceId")]
            public int? InletDeviceId { get; set; }
            [JsonPropertyName("inletDevice")]
            public string? InletDevice { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("controlType")]
            public string ControlType { get; set; }
            [JsonPropertyName("aqi")]
            public short? Aqi { get; set; }
            [JsonPropertyName("limitToOpen")]
            public short LimitToOpen { get; set; }
            [JsonPropertyName("limitToClose")]
            public short LimitToClose { get; set; }
            [JsonPropertyName("isBlocked")]
            public bool IsBlocked { get; set; }
            [JsonPropertyName("updatedAt")]
            public DateTime UpdatedAt { get; set; }

            public View(AirSensor obj)
            {
                Group = obj.Group?.Name;
                InletDeviceId = obj.InletDeviceId;
                InletDevice = obj.InletDevice?.Name;
                Name = obj.Name;
                Aqi = obj.Aqi;
                LimitToOpen = obj.AqiLimitToOpen;
                LimitToClose = obj.AqiLimitToClose;
                IsBlocked = obj.IsBlocked;
                UpdatedAt = obj.UpdatedAt;
            }
        }
    }
}
