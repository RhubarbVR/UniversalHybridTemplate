using System.Runtime.InteropServices;

namespace UniversalSystemCalls;

public static class SystemCaller
{
	public enum SystemApis
	{
		HasSystemCall,
		Notification,
		UserAccentColor,
	}

	private static ISystemCall _systemCall;

	public static string SystemCallName => _systemCall?.PlatformName ?? "None";

	public static IEnumerable<SystemApis> GetSupportedApis() {
		if (_systemCall != null) {
			yield return SystemApis.HasSystemCall;
		}
		if (_systemCall is ISystemCall_Notification) {
			yield return SystemApis.Notification;
		}
		if (_systemCall is ISystemCall_GetUserAccentColor) {
			yield return SystemApis.UserAccentColor;
		}
	}

	public static void RegisterSystemCaller<T>() where T : class, ISystemCall, new() {
		if (_systemCall != null) {
			throw new InvalidOperationException("System caller already registered");
		}
		RegisterSystemCaller(new T());
	}

	public static void RegisterSystemCaller(ISystemCall systemCall) {
		if (_systemCall != null) {
			throw new InvalidOperationException("System caller already registered");
		}
		_systemCall = systemCall;
	}

	/// <summary>
	/// OS like android and iOS will be started through <see cref="RegisterSystemCaller"/>
	/// only windows, mac and linux will be started through this method
	/// </summary>
	public static void SetUpDefaultSystemCaller() {
		if (_systemCall != null) {
			return;
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			RegisterSystemCaller<UniversialWindowsSystemCall>();
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			RegisterSystemCaller<UniversialOSXSystemCall>();
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			RegisterSystemCaller<UniversialLinuxSystemCall>();
		}
	}

	private static T RunTask<T>(Func<Task<T>> task) {
		return Task.Run(task).GetAwaiter().GetResult();
	}
	private static void RunTask(Func<Task> task) {
		Task.Run(task).GetAwaiter().GetResult();
	}

	public static SystemColor GetUserAccentColor() {
		return RunTask(GetUserAccentColorAsync);
	}

	public static void Notifcation(SystemNotification systemNotification) {
		RunTask(async () => await NotifcationAsync(systemNotification));
	}

	public static async Task<SystemColor> GetUserAccentColorAsync() {
		return _systemCall is ISystemCall_GetUserAccentColor systemCall ? await systemCall.GetUserAccentColor() : SystemColor.White;
	}

	public static async Task NotifcationAsync(SystemNotification systemNotification) {
		if (_systemCall is ISystemCall_Notification systemCall) {
			await systemCall.Notifcation(systemNotification);
		}
	}

	public static void UnloadSystemCall() {
		if (_systemCall is IDisposable disposable) {
			disposable.Dispose();
		}
		_systemCall = null;
	}
}
