using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.ClientAuthentication;
using TeleBufet.NET.Pages.ProductPage;

namespace TeleBufet.NET.Pages.LoginPage;

public partial class LoginPage : ContentPage
{
	private static ExtendedClient client;
	private AuthenticateHolder authenticateHolder;
	private readonly ConnectionData connectionData = new ConnectionData() 
	{
		ClientId = "9992351a-ca4c-4f01-ac18-f74865c493ba",
		TenantId = "ea80bead-34b4-4c9b-9eee-cde4240e98ce"
	};

	public LoginPage()
	{
		InitializeComponent();
		authenticateHolder = new AuthenticateHolder(connectionData);
		if(client is null)
			ConnectClient();
	}

	private void ConnectClient()
	{
		var ipAddress = IPAddress.Any;
		//TODO: This hard coded ip address needs to be written by probably main school website
		try
		{
			client = new ExtendedClient("TestClient", IPAddress.Parse("10.0.0.11"), ipAddress);
		}
		catch
		{
			Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Error", "Server was no found", "Close"));
		}
		Task.Run(() => client.StartServerAsync());
	}

	private async void OnLogin(object sender, EventArgs e) 
	{
		var authentificateResult = await authenticateHolder.LoginAsync();
		if (authentificateResult is not null || authentificateResult.IdToken != String.Empty) 
		{
			var account = new NormalAccount() {Username = authentificateResult.Account.Username, Token = authentificateResult.IdToken};
			var authenticatePacket = new AuthentificateAccountPacket() { Account = account };
            await DatagramHelper.SendDatagramAsync(async (byte[] data) => await ExtendedClient.SendDataAsync(data), DatagramHelper.WriteDatagram(authenticatePacket));
			Navigation.PopAsync();
			await Navigation.PushModalAsync(new MainProductPage());
		}
	}
}