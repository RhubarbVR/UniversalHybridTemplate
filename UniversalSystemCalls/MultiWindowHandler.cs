using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalSystemCalls;

public interface IWindowHandle
{
	public WindowHandle WindowHandle { get; }

	public void SetURI(Uri uri);
	public Uri GetUri();

	public Task Close();
}

public sealed class WindowHandle
{
	private readonly IWindowHandle _windowContextHandle;

	internal WindowHandle(IWindowHandle windowContextHandle) {
		_windowContextHandle = windowContextHandle;
	}

	public event Action<WindowHandle> WindowClosed;

	public Uri URL
	{
		get => GetUri();
		set => SetURI(value);
	}

	public void SetURI(Uri uri) {
		_windowContextHandle.SetURI(uri);
	}

	public Uri GetUri() {
		return _windowContextHandle.GetUri();
	}

	public Task Close() {
		return _windowContextHandle.Close();
	}

	internal void HandleClose() {
		WindowClosed?.Invoke(this);
	}
}

public interface IMultiWindowHandler
{
	public bool HasSupport { get; }

	public Uri MainURI { get; }

	public void OpenWindowAtUri(Uri uri);

	public static void RegisterMultiWindowHandler(IMultiWindowHandler multiWindowHandler) {
		MultiWindowHandler.RegisterMultiWindowHandler(multiWindowHandler);
	}

	public static void RegisterWindowHandle(WindowHandle windowHandle) {
		MultiWindowHandler.RegisterWindowHandle(windowHandle);
	}

	public static WindowHandle CreateWindowHandle(IWindowHandle windowContextHandle) {
		return new WindowHandle(windowContextHandle);
	}

	public static void HandleClose(WindowHandle windowHandle) {
		windowHandle.HandleClose();
	}
}

public static class MultiWindowHandler
{
	private static readonly List<WindowHandle> _windowHandles = [];

	internal static void RegisterWindowHandle(WindowHandle windowHandle) {
		lock (_windowHandles) {
			_windowHandles.Add(windowHandle);
			windowHandle.WindowClosed += UnregisterWindowHandle;
		}
	}

	private static void UnregisterWindowHandle(WindowHandle windowHandle) {
		lock (_windowHandles) {
			_windowHandles.Remove(windowHandle);
		}
	}

	public static IReadOnlyList<WindowHandle> WindowHandles
	{
		get {
			lock (_windowHandles) {
				return [.._windowHandles];
			}
		}
	}

	private static IMultiWindowHandler _multiWindowHandler;
	public static Uri MainURI => _multiWindowHandler?.MainURI;

	public static bool HasMultiWindowSupport => _multiWindowHandler?.HasSupport ?? false;

	internal static void RegisterMultiWindowHandler(IMultiWindowHandler multiWindowHandler) {
		if (_multiWindowHandler != null) {
			throw new InvalidOperationException("Already has multi window handler registered");
		}
		_multiWindowHandler = multiWindowHandler;
	}

	public static void OpenWindowAtUri(Uri uri) {
		_multiWindowHandler?.OpenWindowAtUri(uri);
	}

	public static void OpenWindowRelitive(Uri uri) {
		_multiWindowHandler?.OpenWindowAtUri(new Uri(MainURI, uri));
	}
}
