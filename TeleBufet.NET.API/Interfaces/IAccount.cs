using System.Security;

namespace TeleBufet.NET.API.Interfaces
{
    public interface IAccount
    {
        public SecureString Password { get; set; }
    }
}
