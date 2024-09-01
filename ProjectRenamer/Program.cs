using System.Diagnostics;
using System.Formats.Tar;
using System.Management;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ProjectRenamer;

internal class Program
{
	static string _mainPath;
	static ConsoleColor _color;

	static void Main(string[] args) {
		_color = Console.ForegroundColor;
	Start:
		Console.Clear();
		Console.WriteLine("Rename UniversalHybridTemplate Project and all sub data!");
	Error:
		var newName = Console.ReadLine().Replace(' ', '_');
		if (string.IsNullOrEmpty(newName)) {
			Console.WriteLine($"Need to have a name");
			goto Error;
		}
		if (char.IsLower(newName[0])) {
			Console.WriteLine($"Can not do name {newName} needs to start with compatible");
			goto Error;
		}
		Console.WriteLine($"Do you want to rename to {newName} y or n");
		if (Console.ReadKey().Key != ConsoleKey.Y) {
			goto Start;
		}
		try {
			_mainPath = FindUniversalHybridTemplateSLN();
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				if (IsVisualStudioOpen()) {
					Console.WriteLine($"Visual Studio is open");
					goto Error;
				}
			}
			RunFullRename(newName);
		}
		catch (Exception ex) {
			Console.WriteLine($"Error: {ex.Message}");
			Console.WriteLine(ex.ToString());
			Console.ReadLine();
			throw;
		}
	}

	[SupportedOSPlatform("Windows")]
	static bool IsVisualStudioOpen() {
		var processes = Process.GetProcessesByName("devenv");
		foreach (var process in processes) {
			return true;
		}
		return false;
	}



	public static string FindUniversalHybridTemplateSLN() {
		var targetPath = Path.GetFullPath("./");
		var max = 1000;
		while (targetPath is not null) {
			if (File.Exists(Path.Combine(targetPath, "UniversalHybridTemplate.sln"))) {
				return targetPath;
			}
			targetPath = Path.GetFullPath(Path.Combine(targetPath, ".."));
			max--;
			if (max <= 0) {
				throw new Exception("Failed to find UniversalHybridTemplate.sln");
			}
		}
		throw new Exception("Failed to find UniversalHybridTemplate.sln");
	}

	public static void LoadAllFilesFolders(string baseDir, string currentDir, string onlyWith, List<string> allFiles, List<string> renameFiles, List<string> renameFolders) {
		if (currentDir.EndsWith(".git")) {
			return;
		}
		if (Path.GetFileNameWithoutExtension(currentDir) == "obj") {
			return;
		}
		if (Path.GetFileNameWithoutExtension(currentDir) == "bin") {
			return;
		}
		foreach (var file in Directory.GetFiles(currentDir)) {
			var partFile = Path.GetRelativePath(Path.GetFullPath(baseDir), Path.GetFullPath(file));
			allFiles.Add(partFile);
			if (Path.GetFileName(partFile).Contains(onlyWith)) {
				renameFiles.Add(partFile);
			}
		}
		foreach (var folder in Directory.GetDirectories(currentDir)) {
			LoadAllFilesFolders(baseDir, Path.GetFullPath(folder), onlyWith, allFiles, renameFiles, renameFolders);
			var partFolder = Path.GetRelativePath(Path.GetFullPath(baseDir), Path.GetFullPath(folder));
			if (Path.GetFileName(partFolder).Contains(onlyWith)) {
				renameFolders.Add(partFolder);
			}
		}
	}
	public static bool IsBinary(string filePath, int requiredConsecutiveNul = 1) {
		const int CHARS_TO_CHECK = 8000;
		const char NUL_CHAR = '\0';
		var nulCount = 0;
		using var streamReader = new StreamReader(filePath);
		for (var i = 0; i < CHARS_TO_CHECK; i++) {
			if (streamReader.EndOfStream) {
				return false;
			}

			if ((char)streamReader.Read() == NUL_CHAR) {
				nulCount++;
				if (nulCount >= requiredConsecutiveNul) {
					return true;
				}
			}
			else {
				nulCount = 0;
			}
		}
		return false;
	}

	public static void RunRenamePart(string newName, string oldName) {
		var allFiles = new List<string>();
		var renameFiles = new List<string>();
		var renameFolders = new List<string>();
		LoadAllFilesFolders(Path.GetFullPath(_mainPath), Path.GetFullPath(_mainPath), oldName, allFiles, renameFiles, renameFolders);

		renameFiles = [.. renameFiles.OrderByDescending(x => x.Length)];
		renameFolders = [.. renameFolders.OrderByDescending(x => x.Length)];

		foreach (var file in renameFiles) {
			Console.WriteLine("RenameFile:" + file);
		}
		foreach (var folder in renameFolders) {
			Console.WriteLine("RenameFolder:" + folder);
		}

		foreach (var item in allFiles) {
			try {
				var path = Path.Combine(_mainPath, item);
				if (IsBinary(path)) {
					continue;
				}
				File.WriteAllText(path, File.ReadAllText(path).Replace(oldName, newName));
			}
			catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Failed to Do rename for {item} {ex.Message}");
				Console.ForegroundColor = _color;
			}
		}

		foreach (var item in renameFiles) {
			var start = Path.GetFullPath(Path.Combine(_mainPath, item, ".."));
			var end = Path.GetFullPath(item).Substring(start.Length);
			var newPath = Path.GetFullPath(Path.Combine(_mainPath, start, end.Replace(oldName, newName)));
			try {
				if (File.Exists(newPath)) {
					newPath = Path.GetFullPath(newPath);
					if (newPath.StartsWith(_mainPath)) {
						File.Delete(newPath);
					}
				}
				Console.Write($"File Rename {Path.GetFullPath(Path.Combine(_mainPath, item))} {newPath}  start:{start} end:{end}");
				if (!newPath.StartsWith(Path.GetFullPath(_mainPath))) {
					throw new Exception("Tried to put data where it shouldn't go");
				}
				File.Move(Path.GetFullPath(Path.Combine(_mainPath, item)), newPath);
			}
			catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Failed to Do file move for {item} {ex.Message}");
				Console.ForegroundColor = _color;

			}
		}

		foreach (var item in renameFolders) {
			var start = Path.GetFullPath(Path.Combine(_mainPath, item, ".."));
			var end = Path.GetFullPath(item).Substring(start.Length);
			var newPath = Path.GetFullPath(Path.Combine(_mainPath, start, end.Replace(oldName, newName)));
			try {
				if (Directory.Exists(newPath)) {
					newPath = Path.GetFullPath(newPath);
					if (newPath.StartsWith(_mainPath)) {
						Directory.Delete(newPath, true);
					}
				}
				Console.Write($"Dir Rename {Path.GetFullPath(Path.Combine(_mainPath, item))} {newPath} start:{start} end:{end}");
				if (!newPath.StartsWith(Path.GetFullPath(_mainPath))) {
					throw new Exception("Tried to put data where it shouldn't go");
				}
				Directory.Move(Path.GetFullPath(Path.Combine(_mainPath, item)), newPath);
			}
			catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Failed to Do Dir Move for {item} {ex.Message}");
				Console.ForegroundColor = _color;
			}
		}
	}

	public static void RunFullRename(string newName) {
		RunRenamePart(newName, "Universal_Hybrid_Template".ToUpper());
		RunRenamePart(newName.ToLower(), "UniversalHybridTemplate".ToLower());
		RunRenamePart(newName.ToUpper(), "UniversalHybridTemplate".ToUpper());
		RunRenamePart(newName, "UniversalHybridTemplate");
	}
}
