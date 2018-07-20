using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sillycore.BackgroundProcessing;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Enums;
using Sillycore.Domain.Objects;
using Sillycore.Extensions;
using Sillycore.Web.Controllers;
using Sillycore.Web.Filters;
using WebApplication.Configuration;
using WebApplication.Domain;
using WebApplication.Requests;

namespace WebApplication.Controllers
{
    [Route("samples")]
    public class SamplesController : SillyController
    {
        private readonly ILogger<SamplesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly BackgroundJobManager _backgroundJobManager;
        private readonly AppSettings _appSettings;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SamplesController(ILogger<SamplesController> logger, IConfiguration configuration, BackgroundJobManager backgroundJobManager, AppSettings appSettings, IDateTimeProvider dateTimeProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _backgroundJobManager = backgroundJobManager;
            _appSettings = appSettings;
            _dateTimeProvider = dateTimeProvider;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(List<Sample>), (int)HttpStatusCode.OK)]
        [TransformException(typeof(NotImplementedException), HttpStatusCode.InternalServerError, "Patladık.", "TestErrorCode")]
        [RetryOnException(RetryCount = 3, ExceptionType = typeof(DbUpdateException))]
        public IActionResult QuerySamples([FromQuery]QuerySamplesRequest request)
        {
            List<Sample> samples = new List<Sample>();

            for (int i = 0; i < 100; i++)
            {
                samples.Add(CreateNewSample());
            }

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
                CreatedOn = _dateTimeProvider.Now
            };
        }
    }
}