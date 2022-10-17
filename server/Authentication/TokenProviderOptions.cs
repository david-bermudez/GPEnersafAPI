using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace GpEnerSaf.Authentication
{
    public class TokenProviderOptions
    {
        public static string Audience { get; } = "GpEnerSafAudience";
        public static string Issuer { get; } = "GpEnerSaf";
        public static SymmetricSecurityKey Key { get; } = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("GpEnerSafSecretSecurityKeyGcp2"));
        public static TimeSpan Expiration { get; } = TimeSpan.FromMinutes(480);
        public static SigningCredentials SigningCredentials { get; } = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
    }
}
