using System.IO.Compression;

namespace ChromiumDownloader;

internal class Program
{
	public enum OS
	{
		Win,
		Mac,
		Linux
	}

	public enum Architecture
	{
		_Arm,
		_Arm64,
		_x64,
		_x86,
	}

	private static readonly HttpClient _client = new ();
	private const string CHROMIUM_BASE_URL = "https://www.googleapis.com/download/storage/v1/b/chromium-browser-snapshots/o/";

	static string ArcStringer(OS oS, Architecture architecture) {
		if (oS == OS.Win && architecture == Architecture._x86) {
			return "Win";
		}
		if (oS == OS.Linux && architecture == Architecture._x86) {
			return "Linux";
		}
		if (oS == OS.Mac && architecture == Architecture._x64) {
			return "Mac";
		}
		// Is just normal naming
		return oS.ToString() + architecture.ToString();
	}

	static async Task<string> GetLatestBuild(OS os, Architecture arc) {
		Console.WriteLine($"Fetching latest Chromium build number for {os}{arc}...");
		var LatestUrl = $"{CHROMIUM_BASE_URL}{ArcStringer(os, arc)}%2FLAST_CHANGE?alt=media";
		var latestBuild = await _client.GetStringAsync(LatestUrl);
		Console.WriteLine($"Latest build: {latestBuild} for {os}{arc}");
		return latestBuild;
	}

	static async Task<(string, string)> DownloadBuild(OS os, Architecture arc, string OutputFile = null) {
		OutputFile ??= $"chrome-{os}{arc}.zip";
		var latestBuild = await GetLatestBuild(os, arc);
		try {

			var downloadUrl = $"{CHROMIUM_BASE_URL}{ArcStringer(os, arc)}%2F{latestBuild}%2Fchrome-{os.ToString().ToLower()}.zip?alt=media";
			Console.WriteLine($"Downloading from: {downloadUrl}");

			using var response = await _client.GetAsync(downloadUrl);
			using var stream = await response.Content.ReadAsStreamAsync();
			using var fileStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.None);
			await stream.CopyToAsync(fileStream);
			Console.WriteLine("Download complete: " + OutputFile);
		}
		catch (Exception ex) {
			Console.WriteLine("Error: " + ex.Message);
		}
		return (OutputFile, latestBuild);
	}
	private static readonly char[] _spinner = ['|', '/', '-', '\\'];

	static async Task LoadingSpinner(List<Task> tasksToWaitOn, string Msg) {
		var i = 0;
		while (tasksToWaitOn.Count > 0) {
			Console.Write($"\r{Msg}... {_spinner[i]}");
			i = (i + 1) % _spinner.Length;
			var delayTask = Task.Delay(100);
			tasksToWaitOn.Add(delayTask);
			var done = await Task.WhenAny(tasksToWaitOn);
			tasksToWaitOn.Remove(delayTask);
			if (done == delayTask) {
				continue;
			}
			tasksToWaitOn.Remove(done);
		}
	}
	static async Task GetChromiums(string targetFolder) {
		if (!Directory.Exists(targetFolder)) {
			throw new Exception(targetFolder + " does not exist");
		}
		var supportedPlatforms = new Dictionary<string, (OS, Architecture)>();
		var builds = new Dictionary<string, string>();
		async Task Add(OS os, Architecture arc) {
			var (file, build) = await DownloadBuild(os, arc);
			builds.Add(file, build);
			supportedPlatforms.Add(file, (os, arc));
		}
		var downloadTasks = new List<Task>() {
		Task.Run(async () => await Add(OS.Win, Architecture._x64)),
		  Task.Run(async () => await Add(OS.Win, Architecture._x86)),
			Task.Run(async () => await Add(OS.Win, Architecture._Arm64)),
		  Task.Run(async () => await Add(OS.Mac, Architecture._x64)),
		  Task.Run(async () => await Add(OS.Mac, Architecture._Arm)),
		  Task.Run(async () => await Add(OS.Linux, Architecture._x64))
		};


		await LoadingSpinner(downloadTasks, "Downloading Chromium builds");

		Console.WriteLine("Builds are");
		foreach (var item in builds) {
			Console.WriteLine($"{item.Key} : {item.Value}");
		}
		var unzipTasks = new List<Task>();
		foreach (var item in supportedPlatforms) {
			var zip = item.Key;
			var target = Path.Combine(targetFolder, $"{item.Value.Item1}{item.Value.Item2}");
			unzipTasks.Add(ExtractZipAsync(zip, target));
		}

		await LoadingSpinner(unzipTasks, "Extracting Chromium builds");

		Console.WriteLine("\nAll done");

	}
	static Task ExtractZipAsync(string zipFilePath, string extractPath) {
		return Task.Run(() => ZipFile.ExtractToDirectory(zipFilePath, extractPath, true));
	}

	static string FindSLN() {
		var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
		while (dir != null) {
			var sln = dir.GetFiles("UniversalHybridTemplate.sln").FirstOrDefault();
			if (sln != null) {
				return sln.FullName;
			}
			dir = dir.Parent;
		}
		return null;
	}

	static async Task Main() {
		var sln =  FindSLN();
		if (sln == null) {
			Console.WriteLine("Could not find UniversalHybridTemplate.sln");
			return;
		}
		sln = Path.GetFullPath(sln);
		Console.WriteLine("Found UniversalHybridTemplate.sln at " + sln);
		var target = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sln), "UniversalHybridTemplate_UniversalPlatform", "Chromium"));
		Directory.CreateDirectory(target);
		await GetChromiums(target);
	}
}
