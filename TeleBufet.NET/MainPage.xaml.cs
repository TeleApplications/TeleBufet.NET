using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.Pages.LoginPage;

namespace TeleBufet.NET;

public partial class MainPage : ContentPage
{
	private readonly SwipeGestureRecognizer closePageGesture = new SwipeGestureRecognizer() { Direction = SwipeDirection.Down};

	private LoginPage loginPage;

	public MainPage()
	{
		InitializeComponent();
		loginPage = new();
		closePageGesture.Swiped += async (object sender, SwipedEventArgs e) => await Navigation.PopAsync();
		mainStackLayout.GestureRecognizers.Add(closePageGesture);

		this.Loaded += async(object sender, EventArgs e) =>
		{
			loginPage.TranslationY = 250;
			await Navigation.PushModalAsync(loginPage);

			await loginPage.TranslateTo(0, 10);
		};
	}
}

