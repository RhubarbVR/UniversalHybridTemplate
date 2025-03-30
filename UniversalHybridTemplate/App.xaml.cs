
using Microsoft.AspNetCore.Components.WebView.Maui;

using UniversalSystemCalls;

namespace UniversalHybridTemplate
{
	public partial class App : Application, IMultiWindowHandler
	{
		public bool HasSupport { get; private set; }

		public Uri MainURI { get; private set; } = new Uri("https://0.0.0.1/");

		protected override Window CreateWindow(IActivationState activationState) {
			return new Window(new MainPage());
		}

		public App() {
			IMultiWindowHandler.RegisterMultiWindowHandler(this);
			if (DeviceInfo.Idiom == DeviceIdiom.Desktop) {
				HasSupport = true;
			}
			InitializeComponent();

			if (Windows.Count == 0) {
				return;
			}

			Windows[0].Page = new MainPage();
		}

		public void OpenWindowAtUri(Uri uri) {
			if (!HasSupport) {
				return;
			}
			var page = new MainPage();
			var window = new Window(page);
			page.NavTo = uri.ToString();
			OpenWindow(window);			
		}

		public Window GetWindowFromID(Guid windowID) {
			foreach (var window in Windows) {
				if (window.Page is MainPage mainPage && mainPage.WindowId == windowID) {
					return window;
				}
			}
			return null;
		}
	}
}
