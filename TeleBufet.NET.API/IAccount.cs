using System.Runtime.InteropServices;
using System.Security;

namespace DatagramsNet
{
    public interface IAccount
    {

        public SecureString Password { get; set; }
    }
}
