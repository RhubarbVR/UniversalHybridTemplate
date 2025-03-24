
using System.Runtime.InteropServices;

namespace UniversalSystemCalls;

public sealed partial class UniversialWindowsSystemCall : ISystemCall, ISystemCall_GetUserAccentColor
{
	[LibraryImport("dwmapi.dll", EntryPoint = "DwmGetColorizationColor")]
	public static partial void DwmGetColorizationColor(out uint pcrColorization, [MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend);

	public string PlatformName => "UWindows";

	Task<SystemColor> ISystemCall_GetUserAccentColor.GetUserAccentColor() {
		DwmGetColorizationColor(out var color, out var opaque);

		// Extract RGB values
		var r = (byte)((color >> 16) & 0xFF);
		var g = (byte)((color >> 8) & 0xFF);
		var b = (byte)(color & 0xFF);

		return Task.FromResult(SystemColor.FromRGB(r, g, b));
	}
}
