using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PatoCup.Application.Wrappers
{
    public class Response<T>
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public T? Data { get; set; }

        public Response(){ }

        public Response(T data, string? message = null)
        {
            Succeeded = true;
            Message = message ?? "Petición exitosa";
            Data = data;
        }

        public Response(string message, List<string>? errors = null)
        {
            Succeeded = false;
            Message = message;
            Errors = errors ?? new List<string> { message };
        }



    }

}