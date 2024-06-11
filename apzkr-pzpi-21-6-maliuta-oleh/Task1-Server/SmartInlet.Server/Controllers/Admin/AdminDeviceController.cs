using Microsoft.AspNetCore.Mvc;
using SmartInlet.Server.Attributes;
using SmartInlet.Server.Models;
using SmartInlet.Server.Responses;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartInlet.Server.Requests;
using SmartInlet.Server.Services.DB;
using SmartInlet.Server.Tools;

namespace SmartInlet.Server.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/devices")]
    public class AdminDevicesController : BaseController
    {
        public AdminDevicesController(DbApp db) : base(db) { }

        [Authorized, CanAdministrateDevices]
        [HttpPost]
        public async Task<IActionResult> RegisterDevice(
            [Required, FromHeader] string access,
            [Required, FromBody] string deviceType)
        {
            switch (deviceType)
            {
                case "inlet":
                    await DB.InletDevices.AddAsync(new InletDevice
                    {
                        AccessCode = PasswordTool.Hash(access),
                        Name = "new inlet device",
                        ControlType = "manual",
                        IsOpened = false,
                        IsBlocked = false,
                        UpdatedAt = DateTime.UtcNow
                    });
                    break;
                case "air":
                    await DB.AirSensors.AddAsync(new AirSensor
                    {
                        AccessCode = PasswordTool.Hash(access),
                        Name = "new air sensor",
                        Aqi = 10,
                        AqiLimitToOpen = 100,
                        AqiLimitToClose = 66,
                        IsBlocked = false,
                        UpdatedAt = DateTime.UtcNow
                    });
                    break;
                case "temp":
                    await DB.TempSensors.AddAsync(new TempSensor
                    {
                        AccessCode = PasswordTool.Hash(access),
                        Name = "new temp sensor",
                        Kelvins = 10,
                        KelvinLimitToOpen = 303,
                        KelvinLimitToClose = 273,
                        IsBlocked = false,
                        UpdatedAt = DateTime.UtcNow
                    });
                    break;
                default:
                    return BadRequest(new BaseResponse.ErrorResponse("Unknown device type."));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("Device registered!"));
        }

        [Authorized, CanAdministrateDevices]
        [HttpGet("inlet")]
        public async Task<IActionResult> GetInletDevices(
            [FromQuery] PageRequest pageRequest,
            [FromQuery] SearchDevicesRequest request)
        {
            request.Query = (request.Query ?? "").ToLower();
            IQueryable<InletDevice> query = DB.InletDevices
                .Include(p => p.Group)
                .Where(p => p.Group == null || p.Group.Name.ToLower().Contains(request.Query))
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

        [Authorized, CanAdministrateDevices]
        [HttpGet("air")]
        public async Task<IActionResult> GetAirSensors(
            [FromQuery] PageRequest pageRequest,
            [FromQuery] SearchDevicesRequest request)
        {
            request.Query = (request.Query ?? "").ToLower();
            IQueryable<AirSensor> query = DB.AirSensors
                .Include(p => p.Group)
                .Where(p => p.Group == null || p.Group.Name.ToLower().Contains(request.Query))
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

        [Authorized, CanAdministrateDevices]
        [HttpGet("temp")]
        public async Task<IActionResult> GetTempSensors(
            [FromQuery] PageRequest pageRequest,
            [FromQuery] SearchDevicesRequest request)
        {
            request.Query = (request.Query ?? "").ToLower();
            IQueryable<TempSensor> query = DB.TempSensors
                .Include(p => p.Group)
                .Where(p => p.Group == null || p.Group.Name.ToLower().Contains(request.Query))
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

        [Authorized, CanAdministrateDevices]
        [HttpPut("block")]
        public async Task<IActionResult> BlockDevice([FromBody] AdminDeviceRequest request)
        {
            switch (request.DeviceType)
            {
                case "inlet":
                    InletDevice? device = await DB.InletDevices
                        .SingleOrDefaultAsync(p => p.Id == request.DeviceId);

                    if (device == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse("Device not found."));
                    }

                    device.IsBlocked = !device.IsBlocked;

                    if (device.IsBlocked)
                    {
                        AirSensor? airS = device.AirSensor;
                        TempSensor? tempS = device.TempSensor;

                        if (airS != null)
                        {
                            airS.InletDeviceId = null;
                            airS.InletDevice = null;
                        }

                        if (tempS != null)
                        {
                            tempS.InletDeviceId = null;
                            tempS.InletDevice = null;
                        }

                        device.AirSensorId = null;
                        device.AirSensor = null;
                        device.TempSensorId = null;
                        device.TempSensor = null;
                        device.GroupId = null;
                        device.Group = null;
                    }
                    break;
                case "air":
                    AirSensor? airSensor = await DB.AirSensors
                        .SingleOrDefaultAsync(p => p.Id == request.DeviceId);

                    if (airSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse("Device not found."));
                    }

                    airSensor.IsBlocked = !airSensor.IsBlocked;

                    if (airSensor.IsBlocked)
                    {
                        InletDevice? inletD = airSensor.InletDevice;

                        if (inletD != null)
                        {
                            inletD.AirSensorId = null;
                            inletD.AirSensor = null;
                        }

                        airSensor.InletDeviceId = null;
                        airSensor.InletDevice = null;
                        airSensor.GroupId = null;
                        airSensor.Group = null;
                    }
                    break;
                case "temp":
                    TempSensor? tempSensor = await DB.TempSensors
                        .SingleOrDefaultAsync(p => p.Id == request.DeviceId);

                    if (tempSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse("Device not found."));
                    }

                    tempSensor.IsBlocked = !tempSensor.IsBlocked;

                    if (tempSensor.IsBlocked)
                    {
                        InletDevice? inletD = tempSensor.InletDevice;

                        if (inletD != null)
                        {
                            inletD.TempSensorId = null;
                            inletD.TempSensor = null;
                        }

                        tempSensor.InletDeviceId = null;
                        tempSensor.InletDevice = null;
                        tempSensor.GroupId = null;
                        tempSensor.Group = null;
                    }
                    break;
                default:
                    return BadRequest(new BaseResponse.ErrorResponse("Unknown device type."));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("Device blocking status changed!"));
        }

        [Authorized, CanAdministrateDevices]
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDevice(
            [FromRoute] int deviceId,
            [FromBody] AdminDeviceRequest request)
        {
            switch (request.DeviceType)
            {
                case "inlet":
                    InletDevice? device = await DB.InletDevices
                        .SingleOrDefaultAsync(p => p.Id == deviceId);

                    if (device == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse("Device not found."));
                    }

                    DB.InletDevices.Remove(device);
                    break;
                case "air":
                    AirSensor? airSensor = await DB.AirSensors
                        .SingleOrDefaultAsync(p => p.Id == deviceId);

                    if (airSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse("Device not found."));
                    }

                    DB.AirSensors.Remove(airSensor);
                    break;
                case "temp":
                    TempSensor? tempSensor = await DB.TempSensors
                        .SingleOrDefaultAsync(p => p.Id == deviceId);

                    if (tempSensor == null)
                    {
                        return NotFound(new BaseResponse.ErrorResponse("Device not found."));
                    }

                    DB.TempSensors.Remove(tempSensor);
                    break;
                default:
                    return BadRequest(new BaseResponse.ErrorResponse("Unknown device type."));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("Device deleted!"));
        }
    }
}
