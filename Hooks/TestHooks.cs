using System;
using System.Net.Http;
using ReqnrollTests.Services;

namespace ReqnrollTests.Hooks
{
    /// <summary>
    /// Test lifecycle hooks for setup and teardown operations.
    /// These hooks manage test initialization and cleanup.
    /// </summary>
    public class TestHooks : IDisposable
    {
        private HttpClient? _httpClient;
        private static TestHooks? _instance;

        private TestHooks()
        {
            InitializeHttpClient();
        }

        public static TestHooks Instance => _instance ??= new TestHooks();

        /// <summary>
        /// Initializes the HTTP client for API testing.
        /// Called before test suite execution.
        /// </summary>
        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://reqres.in"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Add default headers
            _httpClient.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ReqnrollTests/1.0");

            Console.WriteLine("✓ [BEFORE SUITE] HTTP Client initialized");
        }

        /// <summary>
        /// Gets the shared HTTP client instance.
        /// </summary>
        public HttpClient GetHttpClient()
        {
            if (_httpClient == null)
                throw new InvalidOperationException("HTTP Client not initialized");
            return _httpClient;
        }

        /// <summary>
        /// Called before each test scenario.
        /// </summary>
        public void BeforeScenario(string scenarioName)
        {
            Console.WriteLine($"\n▶ [BEFORE SCENARIO] Starting: {scenarioName}");
            ExtentReportService.StartTest(scenarioName, "API Test");
        }

        /// <summary>
        /// Called after each test scenario.
        /// </summary>
        public void AfterScenario(string scenarioName, bool passed)
        {
            if (passed)
            {
                Console.WriteLine($"✓ [AFTER SCENARIO] Passed: {scenarioName}");
                ExtentReportService.LogPass($"✓ {scenarioName} PASSED");
            }
            else
            {
                Console.WriteLine($"✗ [AFTER SCENARIO] Failed: {scenarioName}");
                ExtentReportService.LogFail($"✗ {scenarioName} FAILED");
            }
            
            ExtentReportService.Reset();
        }

        /// <summary>
        /// Cleanup resources.
        /// Called after all tests.
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("\n🧹 [AFTER SUITE] Cleaning up resources");
            _httpClient?.Dispose();
            ExtentReportService.FlushReport();
            Console.WriteLine("✓ [AFTER SUITE] Resources cleaned up");
        }

        public void BeforeSuite()
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║     ReqRes API Test Suite Started      ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Machine: {Environment.MachineName}");
            Console.WriteLine($".NET Version: {Environment.Version}");
            Console.WriteLine("───────────────────────────────────────────");
        }

        public void AfterSuite()
        {
            Console.WriteLine("───────────────────────────────────────────");
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║      ReqRes API Test Suite Ended       ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
        }
    }
}
