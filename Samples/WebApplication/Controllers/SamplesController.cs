using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Sillycore;
using Sillycore.Web.Controllers;
using WebApplication.Domain;

namespace WebApplication.Controllers
{
    [Route("samples")]
    public class SamplesController : SillyController
    {
        [HttpGet("")]
        [ProducesResponseType(typeof(List<Sample>), (int)HttpStatusCode.OK)]
        public IActionResult QuerySamples()
        {
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