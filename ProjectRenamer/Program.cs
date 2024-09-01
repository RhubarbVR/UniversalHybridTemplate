﻿using System.Diagnostics;
using System.Formats.Tar;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ProjectRenamer;

internal class Program
{
	static string _mainPath;

	static async Task Main(string[] args) {
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
			await RunFullRename(newName);
		}
		catch (Exception ex) {
			Console.WriteLine($"Error: {ex.Message}");
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
		while (targetPath is not null) {
			if (File.Exists(Path.Combine(targetPath, "UniversalHybridTemplate.sln"))) {
				return targetPath;
			}
			targetPath = Path.GetDirectoryName(targetPath);
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
			if (Path.GetFileNameWithoutExtension(partFile).Contains(onlyWith)) {
				renameFiles.Add(partFile);
			}
		}
		foreach (var folder in Directory.GetDirectories(currentDir)) {
			LoadAllFilesFolders(baseDir, folder, onlyWith, allFiles, renameFiles, renameFolders);
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

	public static async Task RunRenamePart(string newName, string oldName) {
		var allFiles = new List<string>();
		var renameFiles = new List<string>();
		var renameFolders = new List<string>();
		LoadAllFilesFolders(_mainPath, _mainPath, oldName, allFiles, renameFiles, renameFolders);

		foreach (var item in allFiles) {
			try {
				var path = Path.Combine(_mainPath, item);
				if (IsBinary(path)) {
					continue;
				}
				await File.WriteAllTextAsync(path, (await File.ReadAllTextAsync(path)).Replace(oldName, newName));
			}
			catch (Exception ex) {
				Console.WriteLine($"Failed to Do rename for {item} {ex.Message}");
			}
		}
		renameFiles = [.. renameFiles.OrderByDescending(x => x.Length)];
		renameFolders = [.. renameFolders.OrderByDescending(x => x.Length)];
		foreach (var item in renameFiles) {
			var start = Path.GetDirectoryName(item);
			var end = item.Substring(start.Length + 1);
			var newPath = Path.Combine(start, end.Replace(oldName, newName));
			try {
				File.Copy(item, newPath);
			}
			catch { }
		}
		foreach (var item in renameFolders) {
			var start = Path.GetDirectoryName(item);
			var end = item.Substring(start.Length + 1);
			var newPath = Path.Combine(start, end.Replace(oldName, newName));
			try {
				Directory.Move(item, newPath);
			}
			catch { }
		}
	}

	public static async Task RunFullRename(string newName) {
		await Task.WhenAll(
			Task.Run(async () => await RunRenamePart(newName, "Universal_Hybrid_Template".ToUpper())),
			Task.Run(async () => await RunRenamePart(newName.ToLower(), "UniversalHybridTemplate".ToLower())),
			Task.Run(async () => await RunRenamePart(newName.ToUpper(), "UniversalHybridTemplate".ToUpper())),
			Task.Run(async () => await RunRenamePart(newName, "UniversalHybridTemplate"))
		);
	}
}
