using System.Net;

namespace BlazorApp1.Data
{
	public class ReturnServiceNote<T>
	{
		public bool Success { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string? Message { get; set; }
		public T? Data { get; set; }
	}
}
