using Microsoft.AspNetCore.Mvc;
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

        public ManualController(ILogger<ManualController> logger)
        {
            _Logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            throw new NotImplementedException();
        }
    }
}
