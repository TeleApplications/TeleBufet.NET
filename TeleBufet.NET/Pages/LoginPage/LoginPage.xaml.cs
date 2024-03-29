using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using TeleBufet.NET.AddressFinder;
using TeleBufet.NET.AddressFinder.Clients;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.ClientAuthentication;
using TeleBufet.NET.Pages.AccessRestrictionPage;
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
			Task.Run(() => ConnectClientAsync());
	}

	private async Task ConnectClientAsync()
	{
		//TODO: This hard coded ip address needs to be written by probably main school website
		var ipAddress = IPAddress.Parse("10.0.0.255");
		var broadcastFinder = new BroadcastFinder(ipAddress);

		await broadcastFinder.StartFindingAsync();
		if(await ConditionTask.WaitUntil(() => AddressClient.NewIPAddress is null, 10))
		{
			await broadcastFinder.ResponseClient.CloseConnectionAsync();

			client = new ExtendedClient(AddressClient.NewIPAddress);
			await client.StartServerAsync();
		}
		else
			Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Error", "Server was no found", "Close"));
	}

	private async void OnLogin(object sender, EventArgs e) 
	{
		var authentificateResult = await authenticateHolder.LoginAsync();
		if (authentificateResult is not null || authentificateResult.IdToken != String.Empty) 
		{
			var account = new NormalAccount() {Username = authentificateResult.Account.Username, Token = authentificateResult.IdToken};
			var authenticatePacket = new AuthentificateAccountPacket() { Account = account };
			await client.SendToDatagramAsync(authenticatePacket, client.DestinationEndPoint);

            await ExtendedClient.RequestCacheTablesPacketAsync(new ProductTable(), new CategoryTable(), new ImageTable());
			ContentPage nextPage = new();

			if (await ConditionTask.WaitUntil(new Func<bool>(() => MainProductPage.User is null), 10)) 
			{
				nextPage = MainProductPage.User.Karma <= 0 ? (ContentPage)new RestrictionPage() : (ContentPage)new MainProductPage();
				Navigation.PopAsync();
				await Navigation.PushModalAsync(nextPage);
			}
		}
	}
}