
using UniversalSystemCalls;

using Windows.UI.ViewManagement;

namespace UniversalHybridTemplate.Platforms.Windows;


public sealed class WindowsSystemCaller(WinUI.App app) : ISystemCall, ISystemCall_GetUserAccentColor
{
	private readonly WinUI.App _app = app;

	public string PlatformName => "Maui_Windows";

	public Task<SystemColor> GetUserAccentColor() {
		var settings = new UISettings();
		var color = settings.GetColorValue(UIColorType.Accent);
		return Task.FromResult(SystemColor.FromRGB(color.R, color.G, color.B));
	}

}
