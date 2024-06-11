using Microsoft.AspNetCore.Mvc;
using SmartInlet.Server.Responses;
using SmartInlet.Server.Models;
using Microsoft.EntityFrameworkCore;
using SmartInlet.Server.Services.DB;

namespace SmartInlet.Server.Controllers
{
    [ApiController]
    [Route("api/iot")]
    public class IotController : BaseController
    {
        private const string deviceValidationToken = "cb86a4fd606425caa3a468b149693f32665b114b601fb73a01fcf2d962d78ed76e225566001bc1708781351cabbbc88105404b6430c8acb941428241bd928889";

        public IotController(DbApp db) : base(db) { }

        [HttpGet("inlet/{deviceId}/try")]
        public async Task<IActionResult> TryInletDeviceConnection(
            [FromHeader(Name = "Device-Token")] string deviceToken, 
            [FromRoute] int deviceId)
        {
            if (deviceToken != deviceValidationToken)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Invalid device."));
            }

            InletDevice? device = await DB.InletDevices
                .SingleOrDefaultAsync(p => p.Id == deviceId);

            if (device == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Device not found."));
            }

            if (device.ControlType == "air")
            {
                AirSensor sensor = device.AirSensor!;

                if (sensor.Aqi > sensor.AqiLimitToOpen)
                {
                    device.IsOpened = true;
                }

                if (sensor.Aqi < sensor.AqiLimitToClose)
                {
                    device.IsOpened = false;
                }
            }

            if (device.ControlType == "temp")
            {
                TempSensor sensor = device.TempSensor!;

                if (sensor.Kelvins > sensor.KelvinLimitToOpen)
                {
                    device.IsOpened = true;
                }

                if (sensor.Kelvins < sensor.KelvinLimitToClose)
                {
                    device.IsOpened = false;
                }
            }

            await DB.SaveChangesAsync();
            return Ok(new InletDeviceResponse(device));
        }

        [HttpGet("air/{sensorId}/try")]
        public async Task<IActionResult> TryAirSensorConnection(
            [FromHeader(Name = "Device-Token")] string sensorToken,
            [FromRoute] int sensorId,
            [FromBody] short aqi)
        {
            if (sensorToken != deviceValidationToken)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Invalid sensor."));
            }

            AirSensor? sensor = await DB.AirSensors
                .SingleOrDefaultAsync(p => p.Id == sensorId);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Sensor not found."));
            }

            sensor.Aqi = aqi;
            await DB.SaveChangesAsync();
            return Ok(new AirSensorResponse(sensor));
        }

        [HttpGet("temp/{sensorId}/try")]
        public async Task<IActionResult> TryTempSensorConnection(
            [FromHeader(Name = "Device-Token")] string sensorToken,
            [FromRoute] int sensorId,
            [FromBody] short kelvins)
        {
            if (sensorToken != deviceValidationToken)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Invalid sensor."));
            }

            TempSensor? sensor = await DB.TempSensors
                .SingleOrDefaultAsync(p => p.Id == sensorId);

            if (sensor == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Sensor not found."));
            }

            sensor.Kelvins = kelvins;
            await DB.SaveChangesAsync();
            return Ok(new TempSensorResponse(sensor));
        }
    }
}
