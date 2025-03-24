using Foundation;

using UniversalSystemCalls;

namespace UniversalHybridTemplate.Platforms.MacCatalyst
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{

		public AppDelegate() {
			SystemCaller.RegisterSystemCaller(new MacOSSystemCaller(this));
		}
		protected override MauiApp CreateMauiApp() {
			return MauiProgram.CreateMauiApp();
		}
	}
}
