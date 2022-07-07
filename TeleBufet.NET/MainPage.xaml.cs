using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.ClientAuthentication;

namespace TeleBufet.NET;


public partial class MainPage : ContentPage
{
	private ExtendedClient client;

	private AuthenticateHolder authenticateHolder;

	private  ConnectionData connectionData = new ConnectionData() { ClientId = "9992351a-ca4c-4f01-ac18-f74865c493ba", TenantId = "ea80bead-34b4-4c9b-9eee-cde4240e98ce" };

	public MainPage()
	{
		InitializeComponent();
		authenticateHolder = new AuthenticateHolder(connectionData);
	}

	private void OnClientConnect(object sender, EventArgs e)
	{
		var ipAddress = IPAddress.Any;
		client = new ExtendedClient("TestClient", IPAddress.Parse(this.ServerAddress.Text), ipAddress);
		this.PhoneAddress.Text = ipAddress.ToString();
		Task.Run(async () => await client.StartServer());
	}

	private async void OnLogin(object sender, EventArgs e) 
	{
		var authentificateResult = await authenticateHolder.LoginAsync();
		if (authentificateResult is not null || authentificateResult.IdToken != String.Empty) 
		{
			var account = new NormalAccount() {Username = authentificateResult.Account.Username, Token = authentificateResult.IdToken};
			var authenticatePacket = new AuthentificateAccountPacket() { Account = account };
            await DatagramHelper.SendDatagramAsync(async (byte[] data) => await ExtendedClient.ClientSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(authenticatePacket));
		}
	}
}

