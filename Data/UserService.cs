using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
namespace BlazorApp1.Data
{
	public interface IUserService
	{
		Task<List<ApplicationUser>> GetAllUserAsync();
		Task<ApplicationUser> GetUserByIdAsync(string userId);
		Task<ApplicationUser> GetUserByEmail(string email);
		Task<IdentityResult> ChangeUsernameAsync(string userId, string newUsername);
		Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
		Task<string> GeneratePasswordResetTokenAsync(string userId);
		Task<IdentityResult> ResetPasswordWithTokenAsync(string userId, string Token, string newPassword);
		Task<IdentityResult> SetEmailConfirmedAsync(string userId, bool confirmed);
	}

	public class UserService : IUserService
	{		
		private readonly UserManager<ApplicationUser> _userManager;

		public UserService(UserManager<ApplicationUser> userManager)
		{ 
			_userManager = userManager;
		}

		public async Task<List<ApplicationUser>> GetAllUserAsync()
		{	
			return await _userManager.Users.ToListAsync();
		}

		public async Task<ApplicationUser> GetUserByIdAsync(string userId)
		{
			//return await _userManager.FindByIdAsync(userId);
			var user = await _userManager.FindByIdAsync(userId);
			return user ?? throw new ArgumentException($"User with ID {userId} not found.");
		}

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
			return user ?? throw new ArgumentException($"User with E-mail {email} not found.");
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new ArgumentException("User not found");
			}
			
			//_userManager.ResetPasswordAsync
			return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);	

		}

		public async Task<string> GeneratePasswordResetTokenAsync(string userId)
		{
			var token = "";
			var user = await _userManager.FindByIdAsync(userId);
		

			if (user != null) token = await _userManager.GeneratePasswordResetTokenAsync(user);
			return token;
		}

		public async Task<IdentityResult> ResetPasswordWithTokenAsync(string userId, string Token , string newPassword)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new ArgumentException("User not found");
			}

			var result = await _userManager.ResetPasswordAsync(user, Token, newPassword);
			return result;

		}

		public async Task<IdentityResult> ChangeUsernameAsync(string userId, string newUsername)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) 
			{
				throw new ArgumentException("User not found");
			}

			user.UserName = newUsername;
			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				// Update NormalizedUserName
				await _userManager.UpdateNormalizedUserNameAsync(user);
			}

			return result;

		}

		public async Task<IdentityResult> SetEmailConfirmedAsync(string userId, bool confirmed)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) 
			{
				return IdentityResult.Failed(new IdentityError { Description = "User not found" });
			}

			user.EmailConfirmed = confirmed;
			return await _userManager.UpdateAsync(user);
		}

       
    }
}
