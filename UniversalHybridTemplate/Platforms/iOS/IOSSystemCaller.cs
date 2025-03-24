
using Microsoft.Maui.Graphics.Platform;

using UIKit;

using UniversalSystemCalls;

namespace UniversalHybridTemplate.Platforms.iOS;


public sealed class IOSSystemCaller(AppDelegate appDelegate) : ISystemCall, ISystemCall_GetUserAccentColor
{
	private readonly AppDelegate _appDelegate = appDelegate;

	public string PlatformName => "Maui_IOS";

	public Task<SystemColor> GetUserAccentColor() {
		var scene = UIApplication.SharedApplication
					.ConnectedScenes
					.OfType<UIWindowScene>()
					.FirstOrDefault();
		var window = scene?.Windows.FirstOrDefault(w => w.IsKeyWindow);
		var accentUIColor = window?.TintColor ?? UIColor.SystemBlue;
		accentUIColor.AsColor().ToRgba(out var r, out var g, out var b, out _);
		return Task.FromResult(SystemColor.FromRGB(r, g, b));
	}

}
