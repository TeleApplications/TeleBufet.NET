using Microsoft.Identity.Client;

namespace TeleBufet.NET.ClientAuthentication.Interfaces
{
    internal interface IAuthenticate
    {
        public PublicClientApplication ClientApplication { get; set; }

        public string[] Scopes { get; }
    }
}
