using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TestRunner
{
	static class IVsTestConsoleWrapperExtensions
	{
		public static async Task<IEnumerable<TestCase>> DiscoverTestsAsync(this IVsTestConsoleWrapper wrapper, string assemblyPath, string runSettings)
		{
			var handler = new TestDiscoveryEventsHandler();
			wrapper.DiscoverTests(new[] { assemblyPath }, runSettings, handler);
			return await handler.Task.ConfigureAwait(false);
		}

		class TestDiscoveryEventsHandler : ITestDiscoveryEventsHandler
		{
			readonly TaskCompletionSource<IEnumerable<TestCase>> tcs = new TaskCompletionSource<IEnumerable<TestCase>>();
			readonly List<TestCase> tests = new List<TestCase>();

			public Task<IEnumerable<TestCase>> Task => tcs.Task;

			public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
			{
				Console.WriteLine("HandleDiscoveredTests([{0} items]", discoveredTestCases.Count());
				tests.AddRange(discoveredTestCases);
			}

			public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase> lastChunk, bool isAborted)
			{
				if (lastChunk != null)
				{
					Console.WriteLine("HandleDiscoveryCompleted(totalTests: {0}, lastChunk: [{1} items], isAborted: {2}", totalTests, lastChunk.Count(), isAborted);
				}
				else
				{
					Console.WriteLine("HandleDiscoveryCompleted(totalTests: {0}, lastChunk: null, isAborted: {1}", totalTests, isAborted);
				}

				if (!isAborted)
				{
					tcs.TrySetResult(tests);
				}
				else
				{
					tcs.TrySetCanceled();
				}
			}

			public void HandleLogMessage(TestMessageLevel level, string message)
			{
			}

			public void HandleRawMessage(string rawMessage)
			{
			}
		}
	}
}