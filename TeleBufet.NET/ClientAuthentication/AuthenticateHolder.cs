using Microsoft.Identity.Client;
using TeleBufet.NET.ClientAuthentication.Interfaces;

namespace TeleBufet.NET.ClientAuthentication
{
    internal sealed class AuthenticateHolder : IAuthenticate
    {
        public PublicClientApplication ClientApplication { get; set; }

        public static object Platform { get; set; }

        public string[] Scopes => new string[] { "User.Read" };

        public AuthenticateHolder(ConnectionData data) 
        {
            ClientApplication = (PublicClientApplication)(PublicClientApplicationBuilder.Create(data.ClientId)
                .WithTenantId(data.TenantId)
                .WithRedirectUri($"msal{data.ClientId}://auth").WithAuthority("https://login.microsoftonline.com/common")
                .Build());
        }

        public async Task<AuthenticationResult> LoginAsync() 
        {
            var accounts = await ClientApplication.GetAccountsAsync();
            var authenticationResult = await ClientApplication.AcquireTokenInteractive(Scopes)
                .WithParentActivityOrWindow(AuthenticateHolder.Platform)
                .ExecuteAsync().ConfigureAwait(false);
            return authenticationResult;
        }

        private async Task<AuthenticationResult> CacheLoginAsync() 
        {
            var accounts = await ClientApplication.GetAccountsAsync();
            AuthenticationResult result;
            try
            {
                var firstAccount = accounts.FirstOrDefault();
                result = await ClientApplication.AcquireTokenSilent(Scopes, firstAccount).ExecuteAsync().ConfigureAwait(false);
            }
            catch(MsalThrottledUiRequiredException ex) 
            {
                result = null;
            }
            return result;
        }
    }
}
