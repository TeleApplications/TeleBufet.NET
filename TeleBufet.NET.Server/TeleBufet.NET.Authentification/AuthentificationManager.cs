using Azure.Identity;
using DatagramsNet;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Security;

namespace TeleBufet.NET.Server.TeleBufet.NET.Authentification
{

    internal class AuthentificationManager
    {
        protected virtual string[] scopes => new string[] {"User.Read", "User.ReadBasic.All"};

        public AuthenticationResult? AuthenticationResult { get; protected set; }

        public IConfidentialClientApplication ClientApplication { get; init; }

        public NormalAccount Account { get; private set; }

        public AuthentificationManager(NormalAccount account) 
        {
            Account = account;
        }

        public async Task ValidateAccountAsync() 
        {

            var provider = new ClientSecretCredential("ea80bead-34b4-4c9b-9eee-cde4240e98ce", "9992351a-ca4c-4f01-ac18-f74865c493ba", "d61ad155-e8c1-49f5-8673-bc5073463102");
            var graphClient = new GraphServiceClient(provider, scopes);
            var accounts = Task.Run(async() => await graphClient.Users.Request().GetAsync()).Result;
            string username = new string(Account.Username.ToArray());
            string password = Account.Password;
        }

        private SecureString GetSecureString(char[] passwordChars) 
        {
            var secureString = new SecureString();
            for (int i = 0; i < passwordChars.Length; i++)
            {
                secureString.AppendChar(passwordChars[i]);
            }
            return secureString;
        }
    }
}

