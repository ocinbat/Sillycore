using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sillycore;
using Sillycore.Web.Controllers;
using WebApplication.Domain;

namespace WebApplication.Controllers
{
    //[Authorization("defaultPolicy")]
    [Route("samples")]
    public class SamplesController : SillyController
    {
        private readonly ILogger<SamplesController> _logger;
        private readonly IConfiguration _configuration;

        public SamplesController(ILogger<SamplesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(List<Sample>), (int)HttpStatusCode.OK)]
        public IActionResult QuerySamples()
        {
            string dockerImageName = _configuration["Sillycore.DockerImageName"];
            string asdf = _configuration["asdf"];
            _logger.LogDebug("QuerySamples called.");
            List<Sample> samples = new List<Sample>();
            samples.Add(new Sample()
            {
                Id = Guid.NewGuid(),
                Name = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                CreatedOn = SillycoreApp.Instance.DateTimeProvider.Now
            });
            return Ok(samples);
        }
    }
}