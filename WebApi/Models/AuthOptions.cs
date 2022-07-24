using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApi.Models
{
    public class AuthOptions
    {
        public const string Issuer = "https://localhost:7062/";
        public const string Audience = "https://localhost:7062/";
        public const int LifeTime = 60; // minutes

        private const string Key = "DhftOS5uphK3vmCJQrexST1RsyjZBjXWRgJMFPU4";

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
