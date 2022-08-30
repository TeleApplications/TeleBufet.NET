using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.Pages.LoginPage;

namespace TeleBufet.NET;

public partial class MainPage : ContentPage
{
	private readonly SwipeGestureRecognizer nextPageGesture = new SwipeGestureRecognizer() { Direction = SwipeDirection.Up};
	private readonly SwipeGestureRecognizer closePageGesture = new SwipeGestureRecognizer() { Direction = SwipeDirection.Down};

	private static LoginPage loginPage;


	public MainPage()
	{
		InitializeComponent();
		nextPageGesture.Swiped += async (object sender, SwipedEventArgs e) => 
		{
			loginPage = loginPage ?? new LoginPage();
			await Navigation.PushModalAsync(loginPage);
		};
		closePageGesture.Swiped += async (object sender, SwipedEventArgs e) => Navigation.RemovePage(loginPage);

		SlideOut.GestureRecognizers.Add(nextPageGesture);
		SlideOut.GestureRecognizers.Add(closePageGesture);

		using var tableCacheManager = new TableCacheHelper<ProductTable>();
		var tables = tableCacheManager.Deserialize();
	}
}

