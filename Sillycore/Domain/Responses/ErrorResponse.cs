using System;
using Sillycore.Domain.Dtos;
using Sillycore.Extensions;

namespace Sillycore.Domain.Responses
{
    public class ErrorResponse : BaseResponse
    {
        public string ErrorCode { get; set; }

        public string AdditionalInfo { get; set; }

        public string Exception { get; set; }

        public string GetFullMessage()
        {
            string message = ErrorCode;

            if (!String.IsNullOrWhiteSpace(AdditionalInfo))
            {
                message += $" - {AdditionalInfo}";
            }

            if (Messages.HasElements())
            {
                foreach (MessageDto messageDto in Messages)
                {
                    message += $" - {messageDto.Type}:{messageDto.Content}";
                }
            }

            return message;
        }
    }
}