using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInlet.Server.Attributes;
using SmartInlet.Server.Models;
using SmartInlet.Server.Requests;
using SmartInlet.Server.Responses;
using SmartInlet.Server.Services.DB;
using SmartInlet.Server.Tools;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace SmartInlet.Server.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DeviceController : BaseController
    {
        public DeviceController(DbApp db) : base(db) { }

        [Authorized]
        [HttpPost]
        public async Task<IActionResult> AddDeviceToGroup(
            [FromBody] AccessDeviceRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(p => 
                p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to add devices!"));
            }

            switch (request.DeviceType)
            {
                case "inlet":
                    InletDevice? device = await DB.InletDevices
                        .SingleOrDefaultAsync(p => p.Id == request.DeviceId);

                    if (device == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    if (device.IsBlocked)
                    {
                        return BadRequest(new BaseResponse.ErrorResponse(
                            "Device is blocked."));
                    }

                    if (!PasswordTool.Validate(request.AccessCode, device.AccessCode))
                    {
                        return BadRequest(new BaseResponse.ErrorResponse(
                            "Invalid access code."));
                    }

                    device.GroupId = group.Id;
                    device.Group = group;
                    break;
                case "air":
                    AirSensor? airSensor = await DB.AirSensors
                        .SingleOrDefaultAsync(p => p.Id == request.DeviceId);

                    if (airSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    if (airSensor.IsBlocked)
                    {
                        return BadRequest(new BaseResponse.ErrorResponse(
                            "Device is blocked."));
                    }

                    if (!PasswordTool.Validate(request.AccessCode, airSensor.AccessCode))
                    {
                        return BadRequest(new BaseResponse.ErrorResponse(
                            "Invalid access code."));
                    }

                    airSensor.GroupId = group.Id;
                    airSensor.Group = group;
                    break;
                case "temp":
                    TempSensor? tempSensor = await DB.TempSensors
                        .SingleOrDefaultAsync(p => p.Id == request.DeviceId);

                    if (tempSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    if (tempSensor.IsBlocked)
                    {
                        return BadRequest(new BaseResponse.ErrorResponse(
                            "Device is blocked."));
                    }

                    if (!PasswordTool.Validate(request.AccessCode, tempSensor.AccessCode))
                    {
                        return BadRequest(new BaseResponse.ErrorResponse(
                            "Invalid access code."));
                    }

                    tempSensor.GroupId = group.Id;
                    tempSensor.Group = group;
                    break;
                default:
                    return BadRequest(new BaseResponse.ErrorResponse(
                        "Unknown device type."));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("Device added!"));
        }

        [Authorized]
        [HttpGet("inlet/by-group/{groupName}")]
        public async Task<IActionResult> GetInletDevices(
            [FromRoute] string groupName,
            [FromQuery] PageRequest pageRequest,
            [FromQuery] SearchDevicesRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? ownMember = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (ownMember == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            request.Query = (request.Query ?? "").ToLower();
            IQueryable<InletDevice> query = DB.InletDevices
                .Include(p => p.Group)
                .Where(p => p.Group.Name == group.Name && p.Name.ToLower().Contains(request.Query))
                .OrderBy(p => p.Group!.Name);

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);
            List<InletDeviceResponse.View> result = await query
                .Select(p => new InletDeviceResponse.View(p))
                .ToListAsync();

            PageResponse<InletDeviceResponse.View> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [Authorized]
        [HttpGet("air/by-group/{groupName}")]
        public async Task<IActionResult> GetAirSensors(
            [FromRoute] string groupName,
            [FromQuery] PageRequest pageRequest,
            [FromQuery] SearchDevicesRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? ownMember = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (ownMember == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            request.Query = (request.Query ?? "").ToLower();
            IQueryable<AirSensor> query = DB.AirSensors
                .Include(p => p.Group)
                .Where(p => p.Group.Name == group.Name && p.Name.ToLower().Contains(request.Query))
                .OrderBy(p => p.Group!.Name);

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);
            List<AirSensorResponse.View> result = await query
                .Select(p => new AirSensorResponse.View(p))
                .ToListAsync();

            PageResponse<AirSensorResponse.View> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [Authorized]
        [HttpGet("temp/by-group/{groupName}")]
        public async Task<IActionResult> GetTempSensors(
            [FromRoute] string groupName,
            [FromQuery] PageRequest pageRequest,
            [FromQuery] SearchDevicesRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? ownMember = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (ownMember == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            request.Query = (request.Query ?? "").ToLower();
            IQueryable<TempSensor> query = DB.TempSensors
                .Include(p => p.Group)
                .Where(p => p.Group.Name == group.Name && p.Name.ToLower().Contains(request.Query))
                .OrderBy(p => p.Group!.Name);

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);
            List<TempSensorResponse.View> result = await query
                .Select(p => new TempSensorResponse.View(p))
                .ToListAsync();

            PageResponse<TempSensorResponse.View> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [Authorized]
        [HttpPut("{deviceId}")]
        public async Task<IActionResult> RenameDevice(
            [FromRoute] int deviceId,
            [FromBody] RenameDeviceRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            switch (request.DeviceType)
            {
                case "inlet":
                    InletDevice? device = await DB.InletDevices
                        .SingleOrDefaultAsync(
                        p => p.Id == deviceId &&
                        p.GroupId == group.Id);

                    if (device == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    device.Name = request.DeviceName;
                    break;
                case "air":
                    AirSensor? airSensor = await DB.AirSensors
                        .SingleOrDefaultAsync(
                        p => p.Id == deviceId &&
                        p.GroupId == group.Id);

                    if (airSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    airSensor.Name = request.DeviceName;
                    break;
                case "temp":
                    TempSensor? tempSensor = await DB.TempSensors
                        .SingleOrDefaultAsync(
                        p => p.Id == deviceId
                        && p.GroupId == group.Id);

                    if (tempSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    tempSensor.Name = request.DeviceName;
                    break;
                default:
                    return BadRequest(new BaseResponse.ErrorResponse(
                        "Unknown device type."));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(
                "Device renamed!"));
        }

        [Authorized]
        [HttpPut("inlet/{deviceId}/open")]
        public async Task<IActionResult> OpenInletDevice(
            [FromRoute] int deviceId,
            [Required][FromBody] string groupName)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == groupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            InletDevice? device = await DB.InletDevices
                .SingleOrDefaultAsync(
                p => p.Id == deviceId &&
                p.GroupId == group.Id);

            if (device == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device not found."));
            }

            if (device.ControlType != "manual")
            {
                return BadRequest(new BaseResponse.ErrorResponse(
                    "Device is not controlled manually."));
            }

            device.IsOpened = !device.IsOpened;
            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(
                "Device opened!"));
        }

        [Authorized]
        [HttpPut("inlet/{deviceId}/control-type/manual")]
        public async Task<IActionResult> ChangeDeviceControlTypeToManual(
            [FromRoute] int deviceId,
            [Required][FromBody] string groupName)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == groupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            InletDevice? device = await DB.InletDevices
                .Include(p => p.AirSensor)
                .SingleOrDefaultAsync(
                p => p.Id == deviceId &&
                p.GroupId == group.Id);

            if (device == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device not found."));
            }

            if (device.ControlType == "air")
            {
                AirSensor? sensor = device.AirSensor;
                if (sensor != null)
                {
                    sensor.InletDevice = null;
                    sensor.InletDeviceId = null;
                }

                device.AirSensor = null;
                device.AirSensorId = null;
            } else if (device.ControlType == "temp")
            {
                TempSensor? sensor = device.TempSensor;
                if (sensor != null)
                {
                    sensor.InletDevice = null;
                    sensor.InletDeviceId = null;
                }

                device.TempSensor = null;
                device.TempSensorId = null;
            }

            device.ControlType = "manual";

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(
                "Device control type changed."));
        }

        [Authorized]
        [HttpPut("inlet/{deviceId}/control-type/air")]
        public async Task<IActionResult> ChangeDeviceControlTypeToAir(
            [FromRoute] int deviceId,
            [FromBody] ChangeSensorRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            InletDevice? device = await DB.InletDevices
                        .SingleOrDefaultAsync(
                p => p.Id == deviceId &&
                p.GroupId == group.Id);

            if (device == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device not found."));
            }

            AirSensor? sensor = await DB.AirSensors
                .SingleOrDefaultAsync(
                p => p.Id == request.SensorId &&
                p.GroupId == group.Id);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Sensor not found."));
            }

            if (device.ControlType == "temp")
            {
                TempSensor? ts = device.TempSensor;

                if (ts == null)
                {
                    return NotFound(new BaseResponse.ErrorResponse(
                        "Temperature sensor not found."));
                }

                ts.InletDeviceId = null;
                ts.InletDevice = null;
                device.TempSensorId = null;
                device.TempSensor = null;
            }

            sensor.InletDeviceId = device.Id;
            sensor.InletDevice = device;
            device.AirSensorId = sensor.Id;
            device.AirSensor = sensor;
            device.ControlType = "air";

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(
                "Device control type changed."));
        }

        [Authorized]
        [HttpPut("inlet/{deviceId}/control-type/temp")]
        public async Task<IActionResult> ChangeDeviceControlTypeToTemp(
            [FromRoute] int deviceId,
            [FromBody] ChangeSensorRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            InletDevice? device = await DB.InletDevices
                        .SingleOrDefaultAsync(
                p => p.Id == deviceId &&
                p.GroupId == group.Id);

            if (device == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device not found."));
            }

            TempSensor? sensor = await DB.TempSensors
                .SingleOrDefaultAsync(
                p => p.Id == request.SensorId &&
                p.GroupId == group.Id);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Sensor not found."));
            }

            if (device.ControlType == "air")
            {
                AirSensor? @as = device.AirSensor;

                if (@as == null)
                {
                    return NotFound(new BaseResponse.ErrorResponse(
                        "Air sensor not found."));
                }

                @as.InletDeviceId = null;
                @as.InletDevice = null;
                device.AirSensorId = null;
                device.AirSensor = null;
            }

            sensor.InletDeviceId = device.Id;
            sensor.InletDevice = device;
            device.TempSensorId = sensor.Id;
            device.TempSensor = sensor;
            device.ControlType = "temp";

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(
                "Device control type changed."));
        }

        [Authorized]
        [HttpPut("inlet/{deviceId}/air-sensor")]
        public async Task<IActionResult> ChangeAirSensorForDevice(
            [FromRoute] int deviceId,
            [FromBody] ChangeSensorRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            InletDevice? device = await DB.InletDevices
                .Include(p => p.AirSensor)
                .SingleOrDefaultAsync(
                p => p.Id == deviceId &&
                p.GroupId == group.Id);

            if (device == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device not found."));
            }

            if (device.ControlType != "air")
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device is not controlled by an air sensor."));
            }

            AirSensor? oldSensor = device.AirSensor;

            if (oldSensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Old sensor not found."));
            }

            AirSensor? sensor = await DB.AirSensors
                .SingleOrDefaultAsync(
                p => p.Id == request.SensorId &&
                p.GroupId == group.Id);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Sensor not found."));
            }

            oldSensor.InletDeviceId = null;
            oldSensor.InletDevice = null;
            sensor.InletDeviceId = device.Id;
            sensor.InletDevice = device;
            device.AirSensorId = sensor.Id;
            device.AirSensor = sensor;

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(
                "Air sensor for the device is changed."));
        }

        [Authorized]
        [HttpPut("inlet/{deviceId}/temp-sensor")]
        public async Task<IActionResult> ChangeTempSensorForDevice(
            [FromRoute] int deviceId,
            [FromBody] ChangeSensorRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            InletDevice? device = await DB.InletDevices
                .Include(p => p.TempSensor)
                .SingleOrDefaultAsync(
                p => p.Id == deviceId &&
                p.GroupId == group.Id);

            if (device == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device not found."));
            }

            if (device.ControlType != "temp")
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Device is not controlled by an temperature sensor."));
            }

            TempSensor? oldSensor = device.TempSensor;

            if (oldSensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Old sensor not found."));
            }

            TempSensor? sensor = await DB.TempSensors
                .SingleOrDefaultAsync(
                p => p.Id == request.SensorId &&
                p.GroupId == group.Id);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Sensor not found."));
            }

            oldSensor.InletDeviceId = null;
            oldSensor.InletDevice = null;
            sensor.InletDeviceId = device.Id;
            sensor.InletDevice = device;
            device.TempSensorId = sensor.Id;
            device.TempSensor = sensor;

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(
                "Temperature sensor for the device is changed."));
        }

        [Authorized]
        [HttpPut("air/{sensorId}/limits")]
        public async Task<IActionResult> ChangeAirSensorLimits(
            [FromRoute] int sensorId,
            [FromBody] ChangeSensorLimitsRequest request)
        {
            if (request.ToOpen <= request.ToClose)
            {
                return BadRequest(new BaseResponse.ErrorResponse(
                    "To-open limit must be higher than to-close one."));
            }

            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            AirSensor? sensor = await DB.AirSensors
                .SingleOrDefaultAsync(
                p => p.Id == sensorId &&
                p.GroupId == group.Id);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Sensor not found."));
            }

            sensor.AqiLimitToOpen = request.ToOpen;
            sensor.AqiLimitToClose = request.ToClose;

            await DB.SaveChangesAsync();
            return Ok(new AirSensorResponse(sensor));
        }

        [Authorized]
        [HttpPut("temp/{sensorId}/limits")]
        public async Task<IActionResult> ChangeTempSensorLimits(
            [FromRoute] int sensorId,
            [FromBody] ChangeSensorLimitsRequest request)
        {
            if (request.ToOpen <= request.ToClose)
            {
                return BadRequest(new BaseResponse.ErrorResponse(
                    "To-open limit must be higher than to-close one."));
            }

            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            TempSensor? sensor = await DB.TempSensors
                .SingleOrDefaultAsync(
                p => p.Id == sensorId &&
                p.GroupId == group.Id);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Sensor not found."));
            }

            sensor.KelvinLimitToOpen = request.ToOpen;
            sensor.KelvinLimitToClose = request.ToClose;

            await DB.SaveChangesAsync();
            return Ok(new TempSensorResponse(sensor));
        }

        [Authorized]
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDevice(
            [FromRoute] int deviceId,
            [FromQuery] string groupName,
            [FromQuery] string deviceType)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(
                p => p.Name == WebUtility.UrlEncode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "Group not found."));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(
                p => p.GroupId == group.Id &&
                p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not a member of the group!"));
            }

            if (!member.CanEditDevices)
            {
                return NotFound(new BaseResponse.ErrorResponse(
                    "You are not allowed to rename devices!"));
            }

            switch (deviceType)
            {
                case "inlet":
                    InletDevice? device = await DB.InletDevices
                        .SingleOrDefaultAsync(
                        p => p.Id == deviceId &&
                        p.GroupId == group.Id);

                    if (device == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    device.GroupId = null;
                    device.Group = null;
                    device.AirSensorId = null;
                    device.AirSensor = null;
                    device.TempSensorId = null;
                    device.TempSensor = null;
                    device.ControlType = "manual";
                    break;
                case "air":
                    AirSensor? airSensor = await DB.AirSensors
                        .SingleOrDefaultAsync(
                        p => p.Id == deviceId &&
                        p.GroupId == group.Id);

                    if (airSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    airSensor.GroupId = null;
                    airSensor.Group = null;
                    airSensor.InletDeviceId = null;
                    airSensor.InletDevice = null;
                    break;
                case "temp":
                    TempSensor? tempSensor = await DB.TempSensors
                        .SingleOrDefaultAsync(
                        p => p.Id == deviceId &&
                        p.GroupId == group.Id);

                    if (tempSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse(
                            "Device not found."));
                    }

                    tempSensor.GroupId = null;
                    tempSensor.Group = null;
                    tempSensor.InletDeviceId = null;
                    tempSensor.InletDevice = null;
                    break;
                default:
                    return BadRequest(new BaseResponse.ErrorResponse(
                        "Unknown device type."));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("Device renamed!"));
        }
    }
}
