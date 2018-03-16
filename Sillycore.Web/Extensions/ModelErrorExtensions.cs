using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Sillycore.Web.Extensions
{
    public static class ModelErrorExtensions
    {
        public static string GetErrorMessage(this ModelError modelError)
        {
            if (modelError == null)
            {
                throw new ArgumentNullException(nameof(modelError));
            }

            string message = modelError.ErrorMessage;

            if (modelError.Exception != null)
            {
                if (!String.IsNullOrWhiteSpace(message))
                {
                    message += " - ExceptionMessage:";
                }

                var exception = modelError.Exception;
                message += exception.Message;

                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                    message += " ";
                    message += exception.Message;
                }
            }

            return message;
        }
    }
}