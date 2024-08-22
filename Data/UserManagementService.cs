using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.Threading.Tasks;

namespace BlazorApp1.Data
{
	public interface IUserManagementService
	{
		Task<ReturnServiceNote<bool>> AdminChangeUsernameAndPassword(string userId, string newUsername, string newPassword, string email , string baseUrl);
		Task<ReturnServiceNote<bool>> SetEmailConfirmed(string userId, bool confirmed);
		Task<ReturnServiceNote<bool>> Create_ForgotPassword(string email, string baseUrl);	
	}

	public class UserManagementService : IUserManagementService
	{
		private readonly IUserService _userService;
		private readonly IJsInteropService _jsInterop;
		private readonly IEmailServiceManager _emailServiceManager;

		public UserManagementService(IUserService userService, IJsInteropService jsInterop, IEmailServiceManager emailServiceManager)
		{
			_userService = userService;
			_jsInterop = jsInterop;
			_emailServiceManager = emailServiceManager;
		}

		public async Task<ReturnServiceNote<bool>> AdminChangeUsernameAndPassword(string userId, string newUsername, string newPassword, string email,string baseUrl)
		{
		
			try
			{
				var result = await _userService.ChangeUsernameAsync(userId, newUsername);
				if (!result.Succeeded)
				{
					await LogErrors(result.Errors , "ChangeUsernameAsync");
					return new ReturnServiceNote<bool>()
					{
						Success = false,
						Data = false,
						StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
						Message = "Invalid Data Cannot Change Username"
					};
				}

				await _jsInterop.LogToConsole("Username changed succesfully");

				var token = await _userService.GeneratePasswordResetTokenAsync(userId);

				if (token == null)
				{
					await _jsInterop.LogToConsole("Function: GeneratePasswordResetTokenAsync - Error: token is null", "error");
					return new ReturnServiceNote<bool>()
					{
						Success = false,
						Data = false,
						StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
						Message = "Failed Generate Token"
					};
				}

				var resultPassword = await _userService.ResetPasswordWithTokenAsync(userId, token, newPassword);
				if (!resultPassword.Succeeded)
				{
					await LogErrors(resultPassword.Errors , "ResetPasswordWithTokenAsync");
					return new ReturnServiceNote<bool>()
					{
						Success = false,
						Data = false,
						StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
						Message = "Invalid Data Cannot Change"
					};
				}

				await _jsInterop.LogToConsole("Password changed succesfully");

				var notifyEmail = await _emailServiceManager.System_SendConfirmationUser(email, baseUrl , newUsername ,newPassword);

				if (!notifyEmail.Success) 
				{
					await _jsInterop.LogToConsole($"Function : System_SendConfirmationUser - Error: {notifyEmail.Message}", "error");
					return new ReturnServiceNote<bool>()
					{
						Success = false,
						Data = false,
						StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
						Message = notifyEmail.Message
					};
				}

				var emaiConfirmed = await SetEmailConfirmed(userId, true);

				if (!emaiConfirmed.Success)
				{
					return emaiConfirmed;
				}

				return new ReturnServiceNote<bool>()
				{
					Success = true,
					Data = true,
					StatusCode = System.Net.HttpStatusCode.OK,
					Message = "Username and Password has been changed ,E-mail Confrimation ok and user successfuly sent E-mail to user"
				};

			}
			catch (Exception e)
			{
				await _jsInterop.LogToConsole($"Error Function AdminChangeUsernameAndPassword : {e.Message}" , "error");
				return new ReturnServiceNote<bool>()
				{
					Success = false,
					Data = false,
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = string.Format("Error Function AdminChangeUsernameAndPassword Internal Service message = {0}", e)
				};
				
			}

		}

        public async Task<ReturnServiceNote<bool>> SetEmailConfirmed(string userId, bool confirmed)
		{
			try
			{
				var result = await _userService.SetEmailConfirmedAsync(userId, confirmed);
				if (!result.Succeeded) 
				{
					await LogErrors(result.Errors, "SetEmailConfirmedAsync");

					return new ReturnServiceNote<bool>()
					{
						Success = false,
						Data = false,
						StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
						Message = "Invalid Data Cannot Change Confirmed Email"
					};
				}

				await _jsInterop.LogToConsole($"Success Confiremed Email");

				return new ReturnServiceNote<bool>()
				{
					Success = true,
					Data = true,
					StatusCode = System.Net.HttpStatusCode.OK,
					Message = "Success Confirmed Email"
				};

			}
			catch (Exception e)
			{
				await _jsInterop.LogToConsole($"Error Function SetEmailConfirmed : {e.Message}", "error");
				return new ReturnServiceNote<bool>()
				{
					Success = false,
					Data = false,
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = string.Format("Error Function SetEmailConfirmed Internal Service message = {0}", e)
				};
			}
		}

		public async Task<ReturnServiceNote<bool>> Create_ForgotPassword(string email, string baseUrl)
		{
			var appUser = await _userService.GetUserByEmail(email);

			if (appUser == null)
			{
                await _jsInterop.LogToConsole($"Function : Create_ForgotPassword - Finding Email: FAILED, {email}" , "error");
				return new ReturnServiceNote<bool>()
				{
					Success = false,
					Data = false,
					StatusCode= System.Net.HttpStatusCode.NotFound,
					Message = "Failed Find Email"
				};
            }

			try
			{
				var token = await _userService.GeneratePasswordResetTokenAsync(appUser.Id);
				token = Base64UrlEncoder.Encode(token);

				baseUrl = baseUrl.Trim().Trim('/');
				string callBack = $"{baseUrl}/reset-password/next?email={email}&token={token}";

				var result = await _emailServiceManager.System_SendResetPassword(email, callBack);

				if (!result.Success)
				{
					await _jsInterop.LogToConsole($"Function : System_SendResetPassword - Sending Email Failed. Requested Email: {email}. Code: {result.StatusCode}", "error");

					return new ReturnServiceNote<bool>()
					{
						Success = false,
						Data = false,
						StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
						Message = result.Message
					};
				}

                await _jsInterop.LogToConsole($"Function : System_SendResetPassword - Sending Email Success. Requested Email: {email}.");

				return new ReturnServiceNote<bool>()
				{ 
					Success = true,
					Data = true,
					StatusCode = System.Net.HttpStatusCode.OK,
					Message = result.Message
				};

            }
			catch (Exception e)
			{
                await _jsInterop.LogToConsole($"Function : System_SendResetPassword - Sending Email Failed. Requested Email: {email}. Message {e.Message}");

                return new ReturnServiceNote<bool>()
                {
                    Success = true,
                    Data = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = e.Message
                };
            }

			
		}

		private async Task LogErrors(IEnumerable<IdentityError> errors , string function)
		{
			foreach (var error in errors)
			{
				await _jsInterop.LogToConsole($"Function : {function} - Error : {error.Description}", "error");
			}
		}

	}
}
