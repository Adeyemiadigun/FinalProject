namespace Application.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public object? Error { get; set; }
        public string ErrorCode { get; set; }

        public ApiException(string message, int statusCode, string errorCode, object? error = null) : base(message)
        {
            StatusCode = statusCode;
            Error = error;
            ErrorCode = errorCode;
        }
    }
}
