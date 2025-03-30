using System.Reflection;

using Microsoft.AspNetCore.Components.WebView.Maui;

namespace UniversalHybridTemplate
{
	public partial class MainPage : ContentPage
	{
		public readonly Guid WindowId = Guid.NewGuid();
		public BlazorWebView BlazorWebView => blazorWebView;
		private string MainString => $"?Window_ID={WindowId}";

		public string NavTo;

		public MainPage() {
			InitializeComponent();
			BlazorWebView.UrlLoading += BlazorWebView_UrlLoading;
			BlazorWebView.StartPath = MainString;
		}

		private void BlazorWebView_UrlLoading(object sender, Microsoft.AspNetCore.Components.WebView.UrlLoadingEventArgs e) {
			if (e.UrlLoadingStrategy != Microsoft.AspNetCore.Components.WebView.UrlLoadingStrategy.OpenInWebView) {
				return;
			}
			if (NavTo is not null) {
				Navigate(new Uri(NavTo + MainString, UriKind.Absolute));
				NavTo = null;
				e.UrlLoadingStrategy = Microsoft.AspNetCore.Components.WebView.UrlLoadingStrategy.CancelLoad;
			}
			if (!e.Url.ToString().Contains(MainString)) {
				Navigate(new Uri(e.Url.ToString() + MainString, UriKind.Absolute));
				e.UrlLoadingStrategy = Microsoft.AspNetCore.Components.WebView.UrlLoadingStrategy.CancelLoad;
			}
		}

		private void Navigate(Uri uri) {
#if WINDOWS
			var webview = BlazorWebView.Handler.PlatformView as Microsoft.UI.Xaml.Controls.WebView2;
			webview.CoreWebView2.Navigate(uri.ToString());
#elif ANDROID
			var webView = BlazorWebView.Handler.PlatformView as Android.Webkit.WebView;
			webView.LoadUrl(uri.ToString());
#elif IOS || MACCATALYST
			var webView = BlazorWebView.Handler.PlatformView as WebKit.WKWebView;
			webView.LoadRequest(new Foundation.NSUrlRequest(new Foundation.NSUrl(uri.ToString())));
#endif
		}
	}
}
