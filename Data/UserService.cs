using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Data
{
	public interface IUserService
	{
		Task<List<ApplicationUser>> GetAllUserAsync();
		Task<ApplicationUser> GetUserByIdAsync(string userId);
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
	}
}
