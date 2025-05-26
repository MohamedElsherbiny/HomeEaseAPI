using Newtonsoft.Json;

namespace HomeEase.Application.DTOs
{
    public class EntityError
    {
        public string ErrorCode { get; }
        public string ErrorMessage { get; }
        public dynamic ErrorContext { get; }

        public EntityError(string errorMessage) : this("-1", errorMessage)
        {
        }

        public EntityError(string errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public EntityError(string errorCode, string errorMessage, dynamic errorContext)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            ErrorContext = errorContext;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}