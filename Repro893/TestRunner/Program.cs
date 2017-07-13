using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Mono.Cecil;

namespace TestRunner
{
	partial class Program
	{
		static int Main(string[] args)
			=> MainAsync(args).GetAwaiter().GetResult();

		static async Task<int> MainAsync(string[] args)
		{
			if (args.Length != 1)
			{
				Console.Error.WriteLine("Usage: TestRunner.exe <path to assembly to be tested>");
				return -1;
			}

			var pathToAssembly = Path.GetFullPath(args[0]);
			var targetFrameworkVersion = GetTargetFrameworkVersion(pathToAssembly);
			Console.WriteLine("Will test assembly that targets {0}", targetFrameworkVersion);

			var pathToCurrentAssembly = Path.GetDirectoryName(new Uri(typeof(Program).Assembly.CodeBase).LocalPath);
			var pathToVSTest = Path.GetFullPath(Path.Combine(pathToCurrentAssembly, "..", "TestPlatform", "Microsoft.TestPlatform.15.0.0", "tools", "net46", "vstest.console.exe"));

			if (!File.Exists(pathToVSTest))
			{
				Console.Error.WriteLine("Could not find vstest.console.exe. Make sure you have run build.cmd to restore it from NuGet.");
				return -2;
			}

			var logFilePath = Path.Combine(pathToCurrentAssembly, "..", FormattableString.Invariant($"VSTest_{Path.GetFileName(pathToAssembly)}_{DateTime.Now:yyyyMMdd-HHmmss.fff}.log"));

			var console = new VsTestConsoleWrapper(pathToVSTest, new ConsoleParameters { LogFilePath = logFilePath });
			console.StartSession();

			var pathToAssemblyDirectory = Path.GetDirectoryName(pathToAssembly);
			var extensions = Directory.GetFiles(pathToAssemblyDirectory, "*.testadapter.dll", SearchOption.TopDirectoryOnly);
			foreach (var extension in extensions)
			{
				Console.WriteLine("Found extension {0}", Path.GetFileName(extension));
			}
			console.InitializeExtensions(extensions);

			var tests = await console.DiscoverTestsAsync(pathToAssembly, GetRunSettingsXml(targetFrameworkVersion)).ConfigureAwait(false);
			Console.WriteLine("Discovered {0} tests", tests.Count());
			foreach (var test in tests)
			{
				Console.WriteLine("  - {0}", test.FullyQualifiedName);
			}

			console.EndSession();
			return 0;
		}

		static string GetTargetFrameworkVersion(string pathToAssembly)
		{
			var assembly = AssemblyDefinition.ReadAssembly(pathToAssembly);
			var targetFrameworkVersion = assembly.CustomAttributes
				.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
				?.ConstructorArguments[0]
				.Value;

			return targetFrameworkVersion as string;
		}

		static string GetRunSettingsXml(string targetFrameworkVersion)
		{
			if (targetFrameworkVersion == null)
			{
				return null;
			}

			var xml = new XElement(
				"RunSettings",
				new XElement(
					"RunConfiguration",
					new XElement(
						"TargetFrameworkVersion",
						targetFrameworkVersion)));

			return xml.ToString();
		}
	}
}