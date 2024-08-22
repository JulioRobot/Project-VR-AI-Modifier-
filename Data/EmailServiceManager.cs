using BlazorApp1.Components.Account.Pages.Manage;
using System.Text.Json;

namespace BlazorApp1.Data
{
	public interface IEmailServiceManager
	{
		/// <summary>
		/// Called email service to send reset password reset to user's email.
		/// </summary>
		/// <param name="toEmail"></param>
		/// <param name="callBackUrl"></param>
		/// <returns></returns>
		Task<ReturnServiceNote<bool>> System_SendResetPassword(string toEmail, string callBackUrl);

		/// <summary>
		/// Called email service to send reset password reset to user's email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="resetUrl"></param>
		/// <returns></returns>
		Task<ReturnServiceNote<bool>> System_NotifyPasswordChanged(string email, string resetUrl);

		/// <summary>
		/// Called email service to send Confirmation user account in email.
		/// </summary>
		/// <param name="toEmail"></param>
		/// <param name="callBackUrl"></param>
		/// <returns></returns>
		Task<ReturnServiceNote<bool>> System_SendConfirmationUser(string toEmail, string callBackUrl, string newUserName, string newPassword);
	}

	public class EmailServiceManager : IEmailServiceManager
	{
		private readonly IJsInteropService _jsInteropService;
		private readonly IHttpClientFactory _clientFactory;

		private const string postmarkEmailFrom = "no-reply@petraverse.id";
		private const string api_sendWithTemplate = "https://api.postmarkapp.com/email/withTemplate";
		private const string key_postmark_sandbox = "2a698eb4-20f8-42fa-9314-3d066f80c6be";
		private int templateId_resetPassword = 37007506;
		private int templateId_notifyResetPassword = 37009703;
		private int templateId_notifyConfirmationUser = 37020291;

		private readonly PostmarkTemplate<TemplateResetPassword> template_ResetPassword;
		private readonly PostmarkTemplate<TemplateNotifyPasswordReset> template_NotifyPasswordReset;
		private readonly PostmarkTemplate<TemplateConfirmUser> template_ConfrimUser;

		public EmailServiceManager(IJsInteropService jsInteropService , IHttpClientFactory clientFactory)
		{
			_jsInteropService = jsInteropService;
			_clientFactory = clientFactory;

			template_ResetPassword = new PostmarkTemplate<TemplateResetPassword>()
			{
				From = postmarkEmailFrom,
				TemplateId = templateId_resetPassword,
				TemplateModel = new TemplateResetPassword()
			};

			template_NotifyPasswordReset = new PostmarkTemplate<TemplateNotifyPasswordReset>()
			{ 
				From = postmarkEmailFrom,
				TemplateId = templateId_notifyResetPassword,
				TemplateModel = new TemplateNotifyPasswordReset()
			};

			template_ConfrimUser = new PostmarkTemplate<TemplateConfirmUser>()
			{
				From = postmarkEmailFrom,
				TemplateId = templateId_notifyConfirmationUser,
				TemplateModel = new TemplateConfirmUser()
			};

		}

		public class PostmarkTemplate<T>
		{ 
			public string From { get; set; }
			public string To { get; set; }
			public int TemplateId { get; set; }
			public T TemplateModel { get; set; }

			public PostmarkTemplate<T> Duplicate()
			{
				var jsonCopy = JsonSerializer.Serialize(this);
				return JsonSerializer.Deserialize<PostmarkTemplate<T>>(jsonCopy);
			}

		}

		public class TemplateConfirmUser
		{
			public string product_name { get; set; } = "Test.Ai";
			public string action_url { get; set; } = "asdads";
			public string username { get; set; } = "asdasd";
			public string password { get; set; } = "asdas";
		}

		public class TemplateResetPassword
		{
			public string product_name { get; set; } = "Test.AI";
			public string action_url { get; set; } = "";
			public string company_name { get; set; } = "";
			public string company_address { get; set; } = "";
			public string support_url { get; set; } = "";
			public string product_url { get; set; } = "";
		}

		public class TemplateNotifyPasswordReset
		{
			public string product_url { get; set; } = "";
			public string product_name { get; set; } = "Test.AI";
			public string action_reset_url { get; set; } = "";
			public string support_email { get; set; } = "";
			public string company_name { get; set; } = "";
			public string company_address { get; set; } = "";
		}

		public async Task<ReturnServiceNote<bool>> System_SendResetPassword(string toEmail, string callBackUrl)
		{
			var postMarkTemplate = template_ResetPassword.Duplicate();
			postMarkTemplate.To = toEmail;
			postMarkTemplate.TemplateModel.action_url = callBackUrl;

			var client = _clientFactory.CreateClient();
			var request = new HttpRequestMessage(HttpMethod.Post, api_sendWithTemplate);
			request.Content = JsonContent.Create(postMarkTemplate);
			request.Headers.Add("Accept", "application/json");
			request.Headers.Add("X-Postmark-Server-Token", key_postmark_sandbox);

			var response = await client.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				await _jsInteropService.LogToConsole(
					string.Format("Reset Password - Request Email Service: FAILED. Emailing to: {0}. Code: {1}", toEmail, response.StatusCode),
					"error");
			}

			return new ReturnServiceNote<bool>()
			{
				Success = response.IsSuccessStatusCode,
				StatusCode = response.StatusCode,
				Message = response.IsSuccessStatusCode ? "Success sent notification to your E-mail" : "Failed to send to notification to your E-mail",
				Data = response.IsSuccessStatusCode
			};

		}

		public async Task<ReturnServiceNote<bool>> System_NotifyPasswordChanged(string email, string resetUrl)
		{
			var postMarkTemplate = template_NotifyPasswordReset.Duplicate();
			postMarkTemplate.To = email;
			postMarkTemplate.TemplateModel.action_reset_url = resetUrl;

			var client = _clientFactory.CreateClient();
			var request = new HttpRequestMessage(HttpMethod.Post, api_sendWithTemplate);
			request.Content = JsonContent.Create(postMarkTemplate);
			request.Headers.Add("Accept", "application/json");
			request.Headers.Add("X-Postmark-Server-Token", key_postmark_sandbox);

			var response = await client.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				await _jsInteropService.LogToConsole(
					string.Format("Notify Password Changed - Request Email Service: FAILED. Emailing to: {0}. Code: {1}", email, response.StatusCode),
					"error");
			}

			return new ReturnServiceNote<bool>()
			{
				Success = response.IsSuccessStatusCode,
				StatusCode = response.StatusCode,
				Message = response.IsSuccessStatusCode ? "Success sent notification to your E-mail" : "Failed to send to notification to your E-mail",
				Data = response.IsSuccessStatusCode
			};
		}

		public async Task<ReturnServiceNote<bool>> System_SendConfirmationUser(string toEmail, string callBackUrl, string newUserName, string newPassword)
		{
			var postMarkTemplate = template_ConfrimUser.Duplicate();
			postMarkTemplate.To = toEmail;
			postMarkTemplate.TemplateModel.username = newUserName;
			postMarkTemplate.TemplateModel.password = newPassword;
			postMarkTemplate.TemplateModel.action_url = callBackUrl;

			using (var httpClient = new HttpClient())
			{
				var request = new HttpRequestMessage(HttpMethod.Post, api_sendWithTemplate);
				request.Content = JsonContent.Create(postMarkTemplate);
				request.Headers.Add("Accept", "application/json");
				request.Headers.Add("X-Postmark-Server-Token", key_postmark_sandbox);

				var response = await httpClient.SendAsync(request);
				if (!response.IsSuccessStatusCode)
				{
					await _jsInteropService.LogToConsole(
						string.Format("Confirmation User - Request Email Service: FAILED. Emailing to: {0}. Code: {1} content: {2}", toEmail, response.StatusCode, response.Content),
						"error");

				}

				return new ReturnServiceNote<bool>()
				{
					Success = response.IsSuccessStatusCode,
					StatusCode = response.StatusCode,
					Message = response.IsSuccessStatusCode ? "Success sent notification to your E-mail" : "Failed to send to notification to your E-mail",
					Data = response.IsSuccessStatusCode
				};
			};
		}
	}
}
