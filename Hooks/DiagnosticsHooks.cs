using System;
using System.Diagnostics;
using ReqnrollTests.Services;

namespace ReqnrollTests.Hooks
{
    /// <summary>
    /// Test execution hooks with timing and diagnostics.
    /// Tracks test performance and collects diagnostic information.
    /// </summary>
    public class DiagnosticsHooks
    {
        private Stopwatch? _testTimer;
        private readonly Stopwatch _suiteTimer = new();

        /// <summary>
        /// Called before test suite starts.
        /// Initializes timing and logging.
        /// </summary>
        public void BeforeSuiteStart()
        {
            _suiteTimer.Start();
            Console.WriteLine("\n📊 Test Diagnostics Enabled");
            Console.WriteLine($"   Memory before: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
        }

        /// <summary>
        /// Called before each scenario.
        /// Starts performance timer.
        /// </summary>
        public void BeforeScenarioStart(string scenarioName)
        {
            _testTimer = Stopwatch.StartNew();
            ExtentReportService.LogInfo($"⏱️ Test started at {DateTime.Now:HH:mm:ss}");
        }

        /// <summary>
        /// Called after each scenario.
        /// Logs execution time and resource usage.
        /// </summary>
        public void AfterScenarioComplete(string scenarioName)
        {
            if (_testTimer != null)
            {
                _testTimer.Stop();
                var executionTime = _testTimer.ElapsedMilliseconds;
                
                ExtentReportService.LogInfo($"⏱️ Execution time: {executionTime}ms");
                Console.WriteLine($"   └─ {scenarioName} completed in {executionTime}ms");

                if (executionTime > 5000)
                {
                    ExtentReportService.LogWarning($"⚠️ Test took longer than expected: {executionTime}ms");
                }
            }
        }

        /// <summary>
        /// Called after suite completes.
        /// Logs overall statistics.
        /// </summary>
        public void AfterSuiteComplete()
        {
            _suiteTimer.Stop();
            var totalTime = _suiteTimer.ElapsedMilliseconds;
            var memoryAfter = GC.GetTotalMemory(false) / 1024 / 1024;

            Console.WriteLine("\n📊 Test Suite Summary:");
            Console.WriteLine($"   Total execution time: {totalTime}ms");
            Console.WriteLine($"   Memory after: {memoryAfter} MB");
            Console.WriteLine($"   Completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        /// <summary>
        /// Handles test errors and logs diagnostics.
        /// </summary>
        public void OnTestError(string scenarioName, Exception ex)
        {
            ExtentReportService.LogError($"❌ Error in {scenarioName}: {ex.Message}");
            Console.WriteLine($"   Exception: {ex.GetType().Name}");
            Console.WriteLine($"   Message: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
    }
}
