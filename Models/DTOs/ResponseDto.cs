namespace RestAPISystemKnowledge.Models.DTOs
{
    public class ResponseDto<T>
    {
        public int StatusCode { get; set; } = 200;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public bool Success => StatusCode >= 200 && StatusCode < 300;

        public static ResponseDto<T> Ok(T? data, string message = "Berhasil", int statusCode = 200)
            => new ResponseDto<T> { StatusCode = statusCode, Message = message, Data = data };

        public static ResponseDto<T> Fail(string message = "Gagal", int statusCode = 400, T? data = default)
            => new ResponseDto<T> { StatusCode = statusCode, Message = message, Data = data };
    }

    public class ResponseDto : ResponseDto<object?>
    {
        public static new ResponseDto Ok(object? data = null, string message = "Berhasil", int statusCode = 200)
            => new ResponseDto { StatusCode = statusCode, Message = message, Data = data };

        public static new ResponseDto Fail(string message = "Gagal", int statusCode = 400, object? data = null)
            => new ResponseDto { StatusCode = statusCode, Message = message, Data = data };
    }
}
