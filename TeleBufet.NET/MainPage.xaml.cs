using TeleBufet.NET.Pages.LoginPage;

namespace TeleBufet.NET;

public partial class MainPage : ContentPage
{
	private readonly SwipeGestureRecognizer nextPageGesture = new SwipeGestureRecognizer() { Direction = SwipeDirection.Up};

	public MainPage()
	{
		InitializeComponent();
		nextPageGesture.Swiped += async(object sender, SwipedEventArgs e) => await Navigation.PushModalAsync(new LoginPage());
		SlideOut.GestureRecognizers.Add(nextPageGesture);
	}
}

