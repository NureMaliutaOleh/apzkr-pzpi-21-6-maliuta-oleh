using SmartInlet.Server.Models;
using System.Text.Json.Serialization;

namespace SmartInlet.Server.Responses
{
    public class UserResponse : BaseResponse
    {
        public UserResponse(User data) : base(true, null, null)
        {
            Data = new View(data);
        }

        public UserResponse(ICollection<User> user) : base(true, null, null)
        {
            Data = user.Select(p => new View(p));
        }

        public class View
        {
            [JsonPropertyName("username")]
            public string Username { get; set; }
            [JsonPropertyName("firstName")]
            public string? FirstName { get; set; }
            [JsonPropertyName("lastName")]
            public string? LastName { get; set; }
            [JsonPropertyName("email")]
            public string Email { get; set; }
            [JsonPropertyName("canAdministrateDevices")]
            public bool CanAdministrateDevices { get; set; }
            [JsonPropertyName("canAdministrateUsers")]
            public bool CanAdministrateUsers { get; set; }
            [JsonPropertyName("isActivated")]
            public bool IsActivated { get; set; }
            [JsonPropertyName("registeredAt")]
            public DateTime RegisteredAt { get; set; }

            public View(User obj)
            {
                Username = obj.Username;
                FirstName = obj.FirstName;
                LastName = obj.LastName;
                Email = obj.Email;
                CanAdministrateDevices = obj.CanAdministrateDevices;
                CanAdministrateUsers = obj.CanAdministrateUsers;
                IsActivated = obj.IsActivated;
                RegisteredAt = obj.RegisteredAt;
            }
        }

        public class ShortView
        {
            [JsonPropertyName("username")]
            public string Username { get; set; }
            [JsonPropertyName("firstName")]
            public string? FirstName { get; set; }
            [JsonPropertyName("lastName")]
            public string? LastName { get; set; }
            [JsonPropertyName("canAdministrateDevices")]
            public bool CanAdministrateDevices { get; set; }
            [JsonPropertyName("canAdministrateUsers")]
            public bool CanAdministrateUsers { get; set; }

            public ShortView(User obj)
            {
                Username = obj.Username;
                FirstName = obj.FirstName;
                LastName = obj.LastName;
                CanAdministrateDevices = obj.CanAdministrateDevices;
                CanAdministrateUsers = obj.CanAdministrateUsers;
            }
        }
    }
}
