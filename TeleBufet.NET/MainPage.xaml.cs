using System.Net;
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

	private void OnLogin(object sender, EventArgs e) 
	{
		var authentificateResult = Task.Run(async() => await authenticateHolder.LoginAsync());
	}
}

