using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using App.Metrics;
using App.Metrics.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sillycore;
using Sillycore.BackgroundProcessing;
using Sillycore.DynamicFiltering;
using Sillycore.Extensions;
using Sillycore.Web.Controllers;
using Sillycore.Web.Filters;
using WebApplication.BackgroundJobs;
using WebApplication.Configuration;
using WebApplication.Domain;
using WebApplication.HealthCheckers;
using WebApplication.Requests;

namespace WebApplication.Controllers
{
    //[Authorization("defaultPolicy")]
    [Route("samples")]
    public class SamplesController : SillyController
    {
        private readonly ILogger<SamplesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly BackgroundJobManager _backgroundJobManager;
        private readonly AppSettings _appSettings;

        public SamplesController(ILogger<SamplesController> logger, IConfiguration configuration, BackgroundJobManager backgroundJobManager, AppSettings appSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _backgroundJobManager = backgroundJobManager;
            _appSettings = appSettings;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(List<Sample>), (int)HttpStatusCode.OK)]
        [TransformException(typeof(NotImplementedException), HttpStatusCode.InternalServerError, "Patladık.", "TestErrorCode")]
        public IActionResult QuerySamples([FromQuery]QuerySamplesRequest request)
        {
            List<Sample> samples = new List<Sample>();
            samples.Add(CreateNewSample());
            samples.First().CreatedOn.AddWorkDays(1);
            return Page(samples.Select(request.Fields).ToPage(request));
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(Sample), (int)HttpStatusCode.Created)]
        public IActionResult CreateSample([FromBody]Sample request)
        {
            return Created(request);
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok();
        }

        private Sample CreateNewSample()
        {
            return new Sample()
            {
                Id = Guid.NewGuid(),
                Name = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                CreatedOn = SillycoreApp.Instance.DateTimeProvider.Now
            };
        }
    }
}