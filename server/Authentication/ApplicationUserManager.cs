using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

using GpEnerSaf.Models;

namespace GpEnerSaf.Authentication
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private readonly ApplicationUserManagerOptions options;

        public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger,
            IOptions<ApplicationUserManagerOptions> options)
          : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.options = options.Value;
        }

        public override bool SupportsUserSecurityStamp
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsUserRole
        {
            get
            {
                return false;
            }
        }

        public override Task<ApplicationUser> FindByNameAsync(string userName)
        {
            userName = userName.Replace($"@{options.Domain}", "", StringComparison.InvariantCultureIgnoreCase);
            return Task.FromResult(new ApplicationUser
            {
                UserName = userName
            });
        }

        public override async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var result = false;
            var connection = new LdapConnection();

            try
            {
                connection.Connect(options.Server, LdapConnection.DefaultPort);
                connection.Bind($"{user.UserName}@{options.Domain}", password);
                result = true;
            }
            catch (LdapException)
            {

            }
            finally
            {
                connection.Disconnect();
            }

            return await Task.FromResult(result);
        }

    }
}
