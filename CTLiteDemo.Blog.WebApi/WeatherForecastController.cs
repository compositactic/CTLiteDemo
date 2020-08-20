using System;
using Microsoft.AspNetCore.Mvc;

namespace CTLiteDemo.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public WeatherForecastController()
        {
        }

        [HttpGet]
        public int Get()
        {
            return new Random().Next();
        }


    }
}
