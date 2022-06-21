using System.Net;

namespace TeleBufet.NET;

public partial class MainPage : ContentPage
{
	private ExtendedClient client;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnClientConnect(object sender, EventArgs e)
	{
		var ipAddress = IPAddress.Any;
		client = new ExtendedClient("TestClient", IPAddress.Parse(this.ServerAddress.Text), ipAddress);
		this.PhoneAddress.Text = ipAddress.ToString();
		Task.Run(async () => await client.StartServer());
	}
}

