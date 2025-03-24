using Foundation;

using UniversalSystemCalls;

namespace UniversalHybridTemplate.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	public AppDelegate() {
		SystemCaller.RegisterSystemCaller(new IOSSystemCaller(this));
	}

	protected override MauiApp CreateMauiApp() {
		return MauiProgram.CreateMauiApp();
	}
}
