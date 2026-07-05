namespace Selfaware.Shared.Models
{
    public class CustomResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static CustomResponse<T> SuccessResponse(T? data, string message = "Success")
        {
            return new CustomResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
            };
        }

        public static CustomResponse<T> ErrorResponse(
            string message,
            IEnumerable<string>? errors = null
        )
        {
            return new CustomResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
            };
        }
    }
}
