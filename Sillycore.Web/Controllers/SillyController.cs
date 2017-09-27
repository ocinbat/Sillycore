using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Responses;
using Sillycore.Web.Results;

namespace Sillycore.Web.Controllers
{
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
    public abstract class SillyController : Controller
    {
        protected IActionResult Page<T>(IPage<T> page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page), @"You need to provide a valid page object.");
            }

            if (page.Size == 0)
            {
                return Ok(page.Items);
            }

            return new PageResult<T>(page);
        }
    }
}