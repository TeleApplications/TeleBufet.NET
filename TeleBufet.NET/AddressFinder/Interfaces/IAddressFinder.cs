using DatagramsNet;
using System.Net;

namespace TeleBufet.NET.AddressFinder.Interfaces
{
    //We created this interface due to future expend of types of getting specify ip address
    internal interface IAddressFinder<T> where T : ServerManager
    {
        public T ResponseClient { get; set; }

        public ReadOnlyMemory<IPAddress> CurrentAddress { get; }
    }
}
