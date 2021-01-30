using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace GeneratedApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManualController : ControllerBase
    {
        private ILogger<ManualController> _Logger;
        private readonly IServiceProvider _ServiceProvider;

        public ManualController(ILogger<ManualController> logger, IServiceProvider serviceProvider)
        {
            _Logger = logger;
            _ServiceProvider = serviceProvider;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _ServiceProvider.GetRequiredService<string>();

            return new StatusCodeResult(500);

            return NoContent();
            throw new NotImplementedException();
        }
    }
}
