using System;
using System.Net;
using System.Reflection;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Responses;
using Sillycore.Extensions;
using Sillycore.Web.Extensions;
using Sillycore.Web.Results;

namespace Sillycore.Web.Controllers
{
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
    public abstract class SillyController : Controller
    {
        private static readonly ILogger<SillyController> Logger = SillycoreApp.Instance?.LoggerFactory?.CreateLogger<SillyController>();

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

        protected IActionResult InvalidRequest(string errorMessage)
        {
            return InvalidRequest(errorMessage, String.Empty);
        }

        protected IActionResult InvalidRequest(ModelStateDictionary modelState)
        {
            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.ErrorCode = "BadRequest";

            if (modelState.IsValid)
            {
                throw new Exception("You cannot call InvalidRequest with a valid ModelState.");
            }

            foreach (var state in modelState.Values)
            {
                if (state.Errors.HasElements())
                {
                    foreach (ModelError modelStateError in state.Errors)
                    {
                        errorResponse.AddErrorMessage(modelStateError.GetErrorMessage());
                    }
                }
            }

            Logger?.LogInformation($"InvalidRequest: {errorResponse.GetFullMessage()}");

            return BadRequest(errorResponse);
        }

        protected IActionResult InvalidRequest(string errorMessage, string errorCode)
        {
            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.ErrorCode = errorCode;
            errorResponse.AddErrorMessage(errorMessage);

            Logger?.LogInformation($"InvalidRequest: {errorResponse.GetFullMessage()}");

            return BadRequest(errorResponse);
        }

        protected IActionResult Conflict(string errorMessage)
        {
            return Conflict(errorMessage, String.Empty);
        }

        protected IActionResult Conflict(string errorMessage, string errorCode)
        {
            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.ErrorCode = errorCode;
            errorResponse.AddErrorMessage(errorMessage);

            Logger?.LogWarning($"Conflict: {errorResponse.GetFullMessage()}");

            return StatusCode(HttpStatusCode.Conflict.ToInt(), errorResponse);
        }

        protected IActionResult Forbidden(string errorMessage)
        {
            return Forbidden(errorMessage, String.Empty);
        }

        protected IActionResult Forbidden(string errorMessage, string errorCode)
        {
            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.ErrorCode = errorCode;
            errorResponse.AddErrorMessage(errorMessage);

            Logger?.LogWarning($"Forbidden: {errorResponse.GetFullMessage()}");

            return StatusCode(HttpStatusCode.Forbidden.ToInt(), errorResponse);
        }

        protected IActionResult Created(object returnValue)
        {
            BaseResponse baseResponse = returnValue as BaseResponse;

            if (baseResponse != null && baseResponse.HasError)
            {
                return BadRequest(returnValue);
            }

            string id = GetIdFromReturnValue(returnValue);

            if (String.IsNullOrEmpty(id))
            {
                return Created(String.Empty, returnValue);
            }

            string url = Request.GetUri().AbsoluteUri;
            return Created($"{url.TrimEnd('/')}/{id}", returnValue);
        }

        protected IActionResult Deleted(object returnValue = null)
        {
            if (returnValue == null)
            {
                return NoContent();
            }

            return Ok(returnValue);
        }

        protected IActionResult InternalServerError(string errorMessage, string additionalInfo = null)
        {
            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.ErrorCode = "InternalServerError";
            errorResponse.AdditionalInfo = additionalInfo;
            errorResponse.AddErrorMessage(errorMessage);

            return InternalServerError(errorResponse);
        }

        protected IActionResult InternalServerError<T>(T errorResponse) where T : ErrorResponse
        {
            if (errorResponse != null)
            {
                Logger?.LogError($"InternalServerError: {errorResponse.GetFullMessage()}");
            }

            return StatusCode(HttpStatusCode.InternalServerError.ToInt(), errorResponse);
        }

        private string GetIdFromReturnValue(object returnValue)
        {
            if (returnValue != null)
            {
                PropertyInfo propertyInfo = returnValue.GetType().GetProperty("Id");

                if (propertyInfo != null)
                {
                    object idValue = propertyInfo.GetValue(returnValue, null);

                    if (idValue != null)
                    {
                        return idValue.ToString();
                    }
                }
            }

            return String.Empty;
        }
    }
}