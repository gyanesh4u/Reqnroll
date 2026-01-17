namespace ReqnrollTests.Services
{
    /// <summary>
    /// Simplified test reporting service (file-based logging).
    /// Logs test results to an HTML report without external dependencies.
    /// </summary>
    public class ExtentReportService
    {
        private static string? _reportPath;
        private static string? _currentTest;
        private static List<string> _logs = new();

        public static void InitializeReport()
        {
            // Create Reports directory in project root (outside of bin)
            // Navigate from bin/Debug/net9.0 up to project root
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName 
                ?? AppDomain.CurrentDomain.BaseDirectory;
            
            var reportDirectory = Path.Combine(projectRoot, "Reports");
            
            if (!Directory.Exists(reportDirectory))
            {
                Directory.CreateDirectory(reportDirectory);
            }

            _reportPath = Path.Combine(
                reportDirectory,
                $"TestReport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.html"
            );

            // Start HTML report
            _logs.Add("<!DOCTYPE html>");
            _logs.Add("<html>");
            _logs.Add("<head>");
            _logs.Add("<meta charset='utf-8'/>");
            _logs.Add("<title>API Test Report</title>");
            _logs.Add("<style>");
            _logs.Add("body { font-family: Arial, sans-serif; margin: 20px; background: #f9f9f9; }");
            _logs.Add(".test-suite { margin-bottom: 30px; padding: 15px; background: #fff; border-left: 4px solid #2196F3; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            _logs.Add(".test-pass { color: green; padding: 10px; margin: 5px 0; background: #e8f5e9; border-left: 3px solid green; }");
            _logs.Add(".test-fail { color: red; padding: 10px; margin: 5px 0; background: #ffebee; border-left: 3px solid red; }");
            _logs.Add(".test-info { color: #333; padding: 10px; margin: 5px 0; background: #e3f2fd; border-left: 3px solid #2196F3; }");
            _logs.Add("h1 { color: #2196F3; border-bottom: 2px solid #2196F3; padding-bottom: 10px; }");
            _logs.Add("h2 { color: #555; margin-top: 20px; font-size: 18px; }");
            _logs.Add(".header { background: linear-gradient(to right, #2196F3, #1976D2); color: white; padding: 20px; border-radius: 4px; margin-bottom: 20px; }");
            _logs.Add("</style>");
            _logs.Add("</head>");
            _logs.Add("<body>");
            _logs.Add("<div class='header'>");
            _logs.Add($"<h1>API Test Report</h1>");
            _logs.Add($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            _logs.Add($"<p><strong>Machine:</strong> {Environment.MachineName}</p>");
            _logs.Add($"<p><strong>OS:</strong> {Environment.OSVersion}</p>");
            _logs.Add("</div>");
        }

        public static void StartTest(string testName, string description)
        {
            _currentTest = testName;
            _logs.Add($"<div class='test-suite'>");
            _logs.Add($"<h2>Test: {testName}</h2>");
            _logs.Add($"<p><em>{description}</em></p>");
            _logs.Add($"<p><small>Started: {DateTime.Now:HH:mm:ss}</small></p>");
        }

        public static void LogPass(string message)
        {
            _logs.Add($"<div class='test-pass'>✓ {message}</div>");
            Console.WriteLine($"[PASS] {message}");
        }

        public static void LogFail(string message)
        {
            _logs.Add($"<div class='test-fail'>✗ {message}</div>");
            Console.WriteLine($"[FAIL] {message}");
        }

        public static void LogInfo(string message)
        {
            _logs.Add($"<div class='test-info'>ℹ {message}</div>");
            Console.WriteLine($"[INFO] {message}");
        }

        public static void LogWarning(string message)
        {
            _logs.Add($"<div class='test-info' style='color: #ff9800;'>⚠ {message}</div>");
            Console.WriteLine($"[WARN] {message}");
        }

        public static void LogError(string message)
        {
            _logs.Add($"<div class='test-fail'><strong>Error:</strong> {message}</div>");
            Console.WriteLine($"[ERROR] {message}");
        }

        public static void Reset()
        {
            _logs.Add($"</div>");
        }

        public static void FlushReport()
        {
            if (_reportPath == null) return;

            _logs.Add("</body>");
            _logs.Add("</html>");

            try
            {
                File.WriteAllLines(_reportPath, _logs);
                Console.WriteLine($"\n📊 Test report generated: {_reportPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing report: {ex.Message}");
            }
        }

        public static string GetReportPath()
        {
            return _reportPath ?? "Unknown";
        }
    }
}
