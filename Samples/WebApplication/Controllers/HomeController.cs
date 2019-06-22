using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sillycore.Web.Controllers;

namespace WebApplication.Controllers
{
    [Route("Home")]
    public class HomeController : SillyController
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("Error")]
        public IActionResult Error()
        {
            return Ok();
        }
    }
}
