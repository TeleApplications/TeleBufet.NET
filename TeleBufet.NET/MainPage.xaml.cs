using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.Pages.LoginPage;

namespace TeleBufet.NET;

public partial class MainPage : ContentPage
{
	private readonly SwipeGestureRecognizer closePageGesture = new SwipeGestureRecognizer() { Direction = SwipeDirection.Down};

	private LoginPage loginPage = new();

	public MainPage()
	{
		InitializeComponent();
		Navigation.PushModalAsync(loginPage);
		closePageGesture.Swiped += async (object sender, SwipedEventArgs e) => await Navigation.PopAsync();

		mainStackLayout.GestureRecognizers.Add(closePageGesture);
	}
}

