using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using Microsoft.FluentUI.AspNetCore.Components;

namespace UniversalHybridTemplate_UniversalPlatform;

internal unsafe static partial class Windows
{
	[LibraryImport("kernel32.dll")]
	public static partial IntPtr CreateJobObjectA(IntPtr lpJobAttributes, IntPtr lpName);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobObjectInfo, uint cbJobObjectInfoLength);

	[StructLayout(LayoutKind.Sequential)]
	public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
	{
		public long PerProcessUserTimeLimit;
		public long PerJobUserTimeLimit;
		public int LimitFlags;
		public UIntPtr MinimumWorkingSetSize;
		public UIntPtr MaximumWorkingSetSize;
		public int ActiveProcessLimit;
		public IntPtr Affinity;
		public int PriorityClass;
		public int SchedulingClass;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IO_COUNTERS
	{
		public ulong ReadOperationCount;
		public ulong WriteOperationCount;
		public ulong OtherOperationCount;
		public ulong ReadTransferCount;
		public ulong WriteTransferCount;
		public ulong OtherTransferCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
	{
		public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
		public IO_COUNTERS IoInfo;
		public UIntPtr ProcessMemoryLimit;
		public UIntPtr JobMemoryLimit;
		public UIntPtr PeakProcessMemoryUsed;
		public UIntPtr PeakJobMemoryUsed;
	}

	private const int JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

	public static void KillOnCloseWindows(this Process process) {
		var jobHandle = CreateJobObjectA(IntPtr.Zero, IntPtr.Zero);
		var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
			BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION {
				LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
			}
		};
		var unused1 = SetInformationJobObject(jobHandle, 9, ref info, (uint)Marshal.SizeOf(info));
		var unused = AssignProcessToJobObject(jobHandle, process.Handle);
	}
}

public class Program
{
	public sealed class Server : IAsyncDisposable
	{
		public int Port { get; set; } = 5169;

		private async Task FindBestPort(CancellationToken cancellationToken) {
			var port = Port;
			while (!cancellationToken.IsCancellationRequested) {
				try {
					using var listener = new TcpListener(IPAddress.Loopback, port);
					listener.Start();
					listener.Stop();
					Port = port;
					return;
				}
				catch {
					port++;
				}
				await Task.Delay(1, cancellationToken);
			}
		}

		public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
		public Task Task { get; private set; }

		private TaskCompletionSource<bool> _start = new();
		public WebApplication WebApplication { get; private set; }
		public async Task WaitForStartAsync() {
			await WaitForStartAsync(CancellationToken.None);
		}

		public async Task WaitForStartAsync(CancellationToken cancellationToken) {
			if (IsStarted) {
				return;
			}
			var unused = await _start.Task.WaitAsync(cancellationToken);
		}

		public volatile bool IsStarted;

		public void Run() {
			if (Task != null) {
				throw new InvalidOperationException("Server is already running");
			}
			Task = Task.Run(async () => await Run(CancellationTokenSource.Token), CancellationTokenSource.Token);
		}

		private async Task Run(CancellationToken cancellationToken) {
			try {
				await FindBestPort(cancellationToken);

				var builder = WebApplication.CreateBuilder([]);

				// Add services to the container.
				var unused8 = builder.Services.AddRazorPages();
				var unused7 = builder.Services.AddServerSideBlazor();
				var unused6 = builder.Services.AddFluentUIComponents();

				var unused5 = builder.WebHost.ConfigureKestrel(configureApp => {
					configureApp.ListenLocalhost(Port);
				});

				WebApplication = builder.Build();

				// Configure the HTTP request pipeline.
				if (!WebApplication.Environment.IsDevelopment()) {
					var unused4 = WebApplication.UseExceptionHandler("/Error");
				}


				var unused3 = WebApplication.UseStaticFiles();

				var unused2 = WebApplication.UseRouting();

				var unused1 = WebApplication.MapBlazorHub();
				var unused = WebApplication.MapFallbackToPage("/_Host");

				await WebApplication.StartAsync(cancellationToken);
				IsStarted = true;
				_start.SetResult(true);
				await WebApplication.WaitForShutdownAsync(cancellationToken);
				await WebApplication.StopAsync(cancellationToken);
			}
			finally {
				IsStarted = false;
				Task = null;
			}
		}

		public void Dispose() {
			var unused = Task.Run(async () => await DisposeAsync());
		}

		public async ValueTask DisposeAsync() {
			await CancellationTokenSource.CancelAsync();
			while (IsStarted) {
				await Task.Delay(10);
			}
			CancellationTokenSource.Dispose();
			_start = null;
		}
	}

	public static Process OpenAppBrowser(Uri url, Action onClose) {
		if (url.Scheme is not "http" and not "https") {
			return null;
		}

		if (Debugger.IsAttached) {
			Console.WriteLine("Browser should be open at " + url);
			//return;
		}

		Environment.SetEnvironmentVariable("GOOGLE_API_KEY", "no");
		Environment.SetEnvironmentVariable("GOOGLE_DEFAULT_CLIENT_ID", "no");
		Environment.SetEnvironmentVariable("GOOGLE_DEFAULT_CLIENT_SECRET", "no");

		var exePath = GetChromiumPath();
		Console.WriteLine("Opening browser at " + url + " runner is " + exePath);
		var args = "--app=" + url;
		var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? Process.Start(new ProcessStartInfo {
				FileName = exePath,
				Arguments = args,
				UseShellExecute = true,

			})
			: throw new Exception("Unsupported OS");
		{
			var processe = Process.GetCurrentProcess();
			processe.EnableRaisingEvents = true;
			processe.Exited += (sender, e) => {
				process?.Kill();
			};
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			process.KillOnCloseWindows();
		}

		AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
			process?.Kill();
		};

		void Process_Exited(object sender, EventArgs e) {
			onClose();
		}

		process.EnableRaisingEvents = true;
		process.Exited += Process_Exited;

		if (process.HasExited) {
			Process_Exited(process, EventArgs.Empty);
		}
		return process;
	}


	private static string GetChromiumPath() {
		var rootPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "Chromium");
		var arc = RuntimeInformation.OSArchitecture;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			var pathNext = "Win_";
			if (arc == Architecture.X64) {
				pathNext += "x64";
			}
			else if (arc == Architecture.X86) {
				pathNext += "x86";
			}
			else if (arc == Architecture.Arm64) {
				pathNext += "Arm64";
			}
			else {
				throw new Exception("Unsupported architecture");
			}
			return Path.Combine(rootPath, pathNext, "chrome-win", "chrome.exe");
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			var pathNext = "Mac_";
			if (arc == Architecture.X64) {
				pathNext += "x64";
			}
			else if (arc == Architecture.X86) {
				pathNext += "x86";
			}
			else if (arc is Architecture.Arm64 or Architecture.Arm) {
				pathNext += "Arm";
			}
			else {
				throw new Exception("Unsupported architecture");
			}
			return Path.Combine(rootPath, pathNext, "chrome-mac", "Chromium.app");
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			var pathNext = "Linux_";
			if (arc == Architecture.X64) {
				pathNext += "x64";
			}
			else {
				throw new Exception("Unsupported architecture");
			}
			return Path.Combine(rootPath, pathNext, "chrome-linux", "chrome");
		}
		else {
			throw new Exception("Unsupported OS");
		}
	}


	public static async Task Main(string[] args) {
		var unused1= args ?? [];
		UniversalSystemCalls.SystemCaller.SetUpDefaultSystemCaller();
		var server = new Server();
		server.Run();
		await server.WaitForStartAsync();
		Console.WriteLine($"Server running on port {server.Port}");
		Console.WriteLine("Enter to stop the server");
		var process = OpenAppBrowser(new Uri("http://localhost:" + server.Port), server.CancellationTokenSource.Cancel);
		await process.WaitForExitAsync(server.CancellationTokenSource.Token);
		server.CancellationTokenSource.Cancel();
		await server.DisposeAsync();
		process?.Kill();
	}
}
