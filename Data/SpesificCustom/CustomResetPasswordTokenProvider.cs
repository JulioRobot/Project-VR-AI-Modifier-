using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlazorApp1.Data.SpesificCustom
{
	public class CustomResetPasswordTokenProvider<TUser> 
		: DataProtectorTokenProvider<TUser> where TUser : class
	{
		public CustomResetPasswordTokenProvider(
			IDataProtectionProvider dataProtectionProvider, 
			IOptions<DataProtectionTokenProviderOptions> options, 
			ILogger<DataProtectorTokenProvider<TUser>> logger) 
			: base(dataProtectionProvider, options, logger)
		{
		}
	}
}
