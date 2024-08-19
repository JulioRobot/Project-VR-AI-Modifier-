using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorApp1.Data
{
	public interface IJsInteropService
	{
		Task LogToConsole(string message, string type = "log");
	}

	public class JsInteropService : IJsInteropService
	{
		private readonly IJSRuntime _jsRuntime;

		public JsInteropService(IJSRuntime jsRuntime)
		{ 
			_jsRuntime = jsRuntime;
		}

		public async Task LogToConsole(string message, string type = "log")
		{
			await _jsRuntime.InvokeVoidAsync($"console.{type}", message);
		}
	}
}
