using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Data
{
	public interface IUserService
	{
		Task<List<ApplicationUser>> GetAllUserAsync();
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
	}
}
