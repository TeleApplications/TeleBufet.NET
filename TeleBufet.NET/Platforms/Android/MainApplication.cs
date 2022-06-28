using Android.App;
using Android.Runtime;
using Plugin.CurrentActivity;
using TeleBufet.NET.ClientAuthentication;

namespace TeleBufet.NET;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp()
	{
		CrossCurrentActivity.Current.Init(this);
		AuthenticateHolder.Platform = this;
		return MauiProgram.CreateMauiApp();
	}
}
