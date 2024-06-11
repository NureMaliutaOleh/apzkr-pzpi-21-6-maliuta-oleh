using SmartInlet.Server.Models;
using System.Text.Json.Serialization;

namespace SmartInlet.Server.Responses
{
    public class TempSensorResponse : BaseResponse
    {
        public TempSensorResponse(TempSensor data) : base(true, null, null)
        {
            Data = new View(data);
        }

        public TempSensorResponse(ICollection<TempSensor> user) : base(true, null, null)
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
            [JsonPropertyName("kelvins")]
            public short? Kelvins { get; set; }
            [JsonPropertyName("limitToOpen")]
            public short LimitToOpen { get; set; }
            [JsonPropertyName("limitToClose")]
            public short LimitToClose { get; set; }
            [JsonPropertyName("isBlocked")]
            public bool IsBlocked { get; set; }
            [JsonPropertyName("updatedAt")]
            public DateTime UpdatedAt { get; set; }

            public View(TempSensor obj)
            {
                Group = obj.Group?.Name;
                InletDeviceId = obj.InletDeviceId;
                InletDevice = obj.InletDevice?.Name;
                Name = obj.Name;
                Kelvins = obj.Kelvins;
                LimitToOpen = obj.KelvinLimitToOpen;
                LimitToClose = obj.KelvinLimitToClose;
                IsBlocked = obj.IsBlocked;
                UpdatedAt = obj.UpdatedAt;
            }
        }
    }
}
