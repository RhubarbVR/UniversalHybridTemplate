using UniversalSystemCalls;

namespace UniversalHybridTemplate.Platforms.Android;


public sealed class AndroidSystemCaller(MainActivity activity) : ISystemCall, ISystemCall_GetUserAccentColor
{
	private readonly MainActivity _activity = activity;

	public string PlatformName => "Maui_Android";

	public Task<SystemColor> GetUserAccentColor() {
		var color = _activity.GetColor(Resource.Attribute.colorAccent);

		var r = (byte)((color >> 16) & 0xFF);
		var g = (byte)((color >> 8) & 0xFF);
		var b = (byte)(color & 0xFF);

		return Task.FromResult(SystemColor.FromRGB(r, g, b));
	}

}
