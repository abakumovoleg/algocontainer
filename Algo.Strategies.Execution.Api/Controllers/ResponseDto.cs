namespace Algo.Strategies.Execution.Api.Controllers
{
    public class ResponseDto<T>
    {
        public ResponseDto()
        {
            
        }
        public ResponseDto(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
        public string Error { get; set; }
    }
}