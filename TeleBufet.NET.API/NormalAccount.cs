using System.Runtime.InteropServices;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NormalAccount
	{
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string Username;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string Token;
	}
}
