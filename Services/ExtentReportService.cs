namespace ReqnrollTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.IO;
    /// <summary>
    /// Professional test reporting service with enhanced styling.
    /// Generates comprehensive HTML reports without external dependencies.
    /// </summary>
    public class ExtentReportService
    {
        private static string? _reportPath;
        private static string? _currentTest;
        private static List<string> _logs = new();
        private static List<TestResult> _testResults = new();
        private static int _passCount = 0;
        private static int _failCount = 0;
        private static int _totalTests = 0;
        private static DateTime _reportStartTime;
        private static List<string>? _currentTestLogs;

        private class TestResult
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public bool Passed { get; set; }
            public List<string> Logs { get; set; } = new();
            public DateTime StartTime { get; set; }
        }

        public static void InitializeReport()
        {
            _reportStartTime = DateTime.Now;
            _passCount = 0;
            _failCount = 0;
            _totalTests = 0;

            // Create Reports directory in project root (outside of bin)
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName 
                ?? AppDomain.CurrentDomain.BaseDirectory;
            
            var reportDirectory = Path.Combine(projectRoot, "Reports");
            
            if (!Directory.Exists(reportDirectory))
            {
                Directory.CreateDirectory(reportDirectory);
            }

            // Use single fixed filename that gets overwritten each run
            _reportPath = Path.Combine(reportDirectory, "TestReport.html");

            // Start HTML report with enhanced styling
            _logs.Add("<!DOCTYPE html>");
            _logs.Add("<html lang='en'>");
            _logs.Add("<head>");
            _logs.Add("<meta charset='utf-8'/>");
            _logs.Add("<meta name='viewport' content='width=device-width, initial-scale=1.0'/>");
            _logs.Add("<title>Reqnroll Test Report - Professional Dashboard</title>");
            _logs.Add("<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            _logs.Add("<style>");
            
            // Advanced CSS Styling with Charts
            _logs.Add(@"
* { margin: 0; padding: 0; box-sizing: border-box; }
html { scroll-behavior: smooth; }
body { 
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; 
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
    padding: 20px;
    color: #333;
}
.container { max-width: 1400px; margin: 0 auto; }

/* Header Styles */
.header { 
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white; 
    padding: 25px 30px;
    border-radius: 12px;
    margin-bottom: 20px;
    box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
    position: relative;
    overflow: hidden;
}
.header::before {
    content: '';
    position: absolute;
    top: -50%;
    right: -50%;
    width: 200%;
    height: 200%;
    background: radial-gradient(circle, rgba(255,255,255,0.1) 1px, transparent 1px);
    background-size: 50px 50px;
    pointer-events: none;
}
.header-content { position: relative; z-index: 1; }
.header h1 { font-size: 1.8em; margin-bottom: 10px; display: flex; align-items: center; font-weight: 700; }
.header h1::before { content: '📊'; margin-right: 12px; font-size: 1.1em; animation: pulse 2s infinite; }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.7; } }
.header-subtext { font-size: 0.9em; opacity: 0.9; margin-bottom: 15px; }
.header-info { 
    display: grid; 
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); 
    gap: 12px;
    margin-top: 12px;
}
.info-box { 
    background: rgba(255, 255, 255, 0.15); 
    padding: 10px 12px; 
    border-radius: 8px;
    border-left: 4px solid rgba(255, 255, 255, 0.5);
    backdrop-filter: blur(10px);
}
.info-box strong { display: block; font-size: 0.7em; opacity: 0.85; margin-bottom: 4px; text-transform: uppercase; letter-spacing: 0.5px; }
.info-box span { display: block; font-size: 0.95em; font-weight: 600; }

/* Charts Section */
.charts-section {
    background: white;
    padding: 20px;
    border-radius: 12px;
    margin-bottom: 20px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}
.charts-section h2 {
    font-size: 1.2em;
    margin-bottom: 15px;
    color: #333;
    border-bottom: 2px solid #667eea;
    padding-bottom: 10px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: space-between;
    user-select: none;
}
.charts-section h2::after { content: '▼'; font-size: 0.8em; transition: transform 0.3s; }
.charts-section.collapsed h2::after { transform: rotate(-90deg); }
.charts-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 15px;
    margin-bottom: 0;
    max-height: 500px;
    transition: max-height 0.3s;
}
.charts-section.collapsed .charts-grid {
    display: none;
}
.chart-container {
    position: relative;
    background: #f8f9fa;
    padding: 15px;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}
.chart-container h3 {
    font-size: 0.95em;
    margin-bottom: 15px;
    color: #667eea;
    font-weight: 600;
}
.chart-wrapper {
    position: relative;
    height: 200px;
}

/* Summary Cards */
.summary { 
    display: grid; 
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); 
    gap: 12px; 
    margin-bottom: 20px;
}
.summary-card { 
    background: white; 
    padding: 15px 20px; 
    border-radius: 8px; 
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    text-align: center;
    border-top: 4px solid #2196F3;
    transition: transform 0.3s, box-shadow 0.3s;
}
.summary-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.12);
}
.summary-card.success { border-top-color: #4CAF50; }
.summary-card.danger { border-top-color: #F44336; }
.summary-card.warning { border-top-color: #FF9800; }
.summary-card .number { 
    font-size: 2em; 
    font-weight: 700;
    margin: 10px 0;
    background: linear-gradient(135deg, #667eea, #764ba2);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
}
.summary-card.success .number { 
    background: linear-gradient(135deg, #4CAF50, #45a049);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
}
.summary-card.danger .number { 
    background: linear-gradient(135deg, #F44336, #E53935);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
}
.summary-card .label { 
    color: #888; 
    font-size: 0.8em;
    text-transform: uppercase;
    letter-spacing: 1px;
    font-weight: 600;
}

/* Test Details Section */
.test-details-section {
    background: white;
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    margin-bottom: 20px;
}
.test-details-section h2 {
    font-size: 1.2em;
    margin-bottom: 15px;
    color: #333;
    border-bottom: 2px solid #667eea;
    padding-bottom: 10px;
}

.test-suite { 
    background: #f8f9fa;
    margin-bottom: 10px; 
    padding: 10px 12px; 
    border-left: 3px solid #667eea; 
    border-radius: 6px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
    transition: all 0.3s;
    display: none;
}
.test-suite:hover {
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
}
.test-suite h2 { 
    color: #667eea;
    margin-bottom: 6px;
    font-size: 0.95em;
    display: none;
    font-weight: 600;
}
.test-suite p { color: #555; margin-bottom: 6px; font-size: 0.8em; line-height: 1.4; }
.timestamp { color: #999; font-size: 0.75em; display: none; }

.test-pass { 
    color: #2e7d32; 
    padding: 6px 10px; 
    margin: 4px 0; 
    background: #e8f5e9; 
    border-left: 3px solid #4CAF50;
    border-radius: 3px;
    font-size: 0.85em;
}

.test-fail { 
    color: #c62828; 
    padding: 6px 10px; 
    margin: 4px 0; 
    background: #ffebee; 
    border-left: 3px solid #F44336;
    border-radius: 3px;
    font-size: 0.85em;
}

.test-info { 
    color: #1565c0; 
    padding: 6px 10px; 
    margin: 4px 0; 
    background: #e3f2fd; 
    border-left: 3px solid #2196F3;
    border-radius: 3px;
    font-size: 0.85em;
}

.test-warning { 
    color: #e65100; 
    padding: 6px 10px; 
    margin: 4px 0; 
    background: #fff3e0; 
    border-left: 3px solid #FF9800;
    border-radius: 3px;
    font-size: 0.85em;
}

.test-error { 
    color: #b71c1c; 
    padding: 6px 10px; 
    margin: 4px 0; 
    background: #ffcdd2; 
    border-left: 3px solid #F44336;
    border-radius: 3px;
}

/* HTTP Request/Response */
.http-section {
    background: #f5f5f5; 
    border: 1px solid #ddd; 
    border-radius: 4px; 
    padding: 8px 10px; 
    margin: 4px 0; 
    font-family: 'Courier New', monospace;
    font-size: 0.8em;
    display: none;
}
.http-request {
    background: #e3f2fd; 
    border: 1px solid #2196F3;
}
.http-response {
    background: #f5f5f5; 
    border: 1px solid #ddd;
}
.http-section strong { color: #333; display: inline; margin-right: 6px; }

/* Footer */
.footer {
    background: white;
    padding: 12px 20px;
    border-radius: 8px;
    text-align: center;
    color: #666;
    margin-top: 20px;
    font-size: 0.8em;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    border-top: 2px solid #667eea;
}
.footer strong { display: inline; font-size: 0.85em; color: #333; margin: 0 5px; }
.footer small { display: block; margin-top: 6px; opacity: 0.7; }

/* Responsive */
@media (max-width: 768px) {
    .header h1 { font-size: 2em; }
    .charts-grid { grid-template-columns: 1fr; }
    .header-info { grid-template-columns: 1fr 1fr; }
    .summary { grid-template-columns: 1fr 1fr; }
}
");
            
            _logs.Add("</style>");
            _logs.Add("</head>");
            _logs.Add("<body>");
            _logs.Add("<div class='container'>");
            
            // Header Section
            _logs.Add("<div class='header'>");
            _logs.Add("<div class='header-content'>");
            _logs.Add("<h1>Reqnroll Test Report</h1>");
            _logs.Add("<div class='header-subtext'>Professional API Test Automation Dashboard</div>");
            _logs.Add("<div class='header-info'>");
            _logs.Add("<div class='info-box'><strong>Generated:</strong><span>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</span></div>");
            _logs.Add("<div class='info-box'><strong>Machine:</strong><span>" + Environment.MachineName + "</span></div>");
            _logs.Add("<div class='info-box'><strong>OS:</strong><span>" + Environment.OSVersion + "</span></div>");
            _logs.Add("<div class='info-box'><strong>Framework:</strong><span>.NET 8.0</span></div>");
            _logs.Add("</div>");
            _logs.Add("</div>");
            _logs.Add("</div>");
            
            // Charts section placeholder
            _logs.Add("<div class='charts-section'>");
            _logs.Add("<h2>📈 Test Analytics</h2>");
            _logs.Add("<div class='charts-grid'>");
            _logs.Add("<div class='chart-container'><h3>Test Results Distribution</h3><div class='chart-wrapper'><canvas id='pieChart'></canvas></div></div>");
            _logs.Add("<div class='chart-container'><h3>Test Execution Timeline</h3><div class='chart-wrapper'><canvas id='lineChart'></canvas></div></div>");
            _logs.Add("</div>");
            _logs.Add("</div>");
            
            // Placeholder for summary (will be updated in FlushReport)
            _logs.Add("<div id='summary-placeholder'></div>");
            _logs.Add("<div id='test-details-placeholder'></div>");
        }

        public static void StartTest(string testName, string description)
        {
            _currentTest = testName;
            _totalTests++;
            _currentTestLogs = new List<string>();
            
            // Create new test result
            var testResult = new TestResult
            {
                Name = testName,
                Description = description,
                StartTime = DateTime.Now,
                Logs = _currentTestLogs
            };
            _testResults.Add(testResult);
            
            // Add to current test logs (not main logs)
            _currentTestLogs.Add($"<div class='test-suite'>");
            _currentTestLogs.Add($"<h2>{testName}</h2>");
            _currentTestLogs.Add($"<p><em>{description}</em></p>");
            _currentTestLogs.Add($"<p class='timestamp'>⏱ Started: {DateTime.Now:HH:mm:ss}</p>");
        }

        public static void LogPass(string message)
        {
            _passCount++;
            _currentTestLogs?.Add($"<div class='test-pass'>{message}</div>");
            _logs.Add($"<div class='test-pass'>{message}</div>");
            Console.WriteLine($"[PASS] {message}");
        }

        public static void LogFail(string message)
        {
            _failCount++;
            _currentTestLogs?.Add($"<div class='test-fail'>{message}</div>");
            _logs.Add($"<div class='test-fail'>{message}</div>");
            Console.WriteLine($"[FAIL] {message}");
        }

        public static void LogInfo(string message)
        {
            _currentTestLogs?.Add($"<div class='test-info'>{message}</div>");
            _logs.Add($"<div class='test-info'>{message}</div>");
            Console.WriteLine($"[INFO] {message}");
        }

        public static void LogWarning(string message)
        {
            _currentTestLogs?.Add($"<div class='test-warning'>{message}</div>");
            _logs.Add($"<div class='test-warning'>{message}</div>");
            Console.WriteLine($"[WARN] {message}");
        }

        public static void LogError(string message)
        {
            _currentTestLogs?.Add($"<div class='test-error'><strong>Error:</strong> {message}</div>");
            _logs.Add($"<div class='test-error'><strong>Error:</strong> {message}</div>");
            Console.WriteLine($"[ERROR] {message}");
        }

        public static void LogResponse(int statusCode, string method, string endpoint, string? responseBody = null)
        {
            var statusColor = statusCode >= 200 && statusCode < 300 ? "#4CAF50" : 
                             statusCode >= 400 && statusCode < 500 ? "#FF9800" : "#F44336";
            
            var responseHtml = $"<div style='background: #f5f5f5; border: 1px solid #ddd; border-radius: 4px; padding: 12px; margin: 10px 0; font-family: monospace;'>" +
                $"<strong style='color: #333;'>HTTP Response:</strong><br>" +
                $"<span style='color: #666;'><strong>Method:</strong> {method}</span><br>" +
                $"<span style='color: #666;'><strong>Endpoint:</strong> {endpoint}</span><br>" +
                $"<span style='color: {statusColor}; font-weight: bold;'><strong>Status Code:</strong> {statusCode}</span><br>";

            if (!string.IsNullOrEmpty(responseBody))
            {
                var truncatedBody = responseBody.Length > 500 ? responseBody.Substring(0, 500) + "..." : responseBody;
                responseHtml += $"<details style='margin-top: 10px;'><summary style='cursor: pointer; color: #2196F3; font-weight: bold;'>View Response Body</summary>" +
                    $"<pre style='background: #fff; border: 1px solid #ddd; padding: 10px; margin-top: 8px; overflow-x: auto; max-height: 300px; border-radius: 4px;'>{HtmlEncode(truncatedBody)}</pre>" +
                    $"</details>";
            }
            
            responseHtml += "</div>";

            _currentTestLogs?.Add(responseHtml);
            _logs.Add(responseHtml);
            Console.WriteLine($"[HTTP] {method} {endpoint} -> {statusCode}");
        }

        public static void LogResponseHeaders(string headerName, string headerValue)
        {
            var headerHtml = $"<div style='background: #e8f5e9; padding: 8px 12px; margin: 5px 0; border-left: 3px solid #4CAF50; border-radius: 2px; font-size: 0.9em;'>" +
                $"<strong>Header:</strong> {HtmlEncode(headerName)}: {HtmlEncode(headerValue)}" +
                $"</div>";

            _currentTestLogs?.Add(headerHtml);
            _logs.Add(headerHtml);
            Console.WriteLine($"[HEADER] {headerName}: {headerValue}");
        }

        public static void LogRequestDetails(string method, string url, string? requestBody = null)
        {
            var requestHtml = $"<div style='background: #e3f2fd; border: 1px solid #2196F3; border-radius: 4px; padding: 12px; margin: 10px 0; font-family: monospace;'>" +
                $"<strong style='color: #1565c0;'>HTTP Request:</strong><br>" +
                $"<span style='color: #1565c0;'><strong>Method:</strong> {method}</span><br>" +
                $"<span style='color: #1565c0;'><strong>URL:</strong> {HtmlEncode(url)}</span><br>";

            if (!string.IsNullOrEmpty(requestBody))
            {
                requestHtml += $"<details style='margin-top: 10px;'><summary style='cursor: pointer; color: #1565c0; font-weight: bold;'>View Request Body</summary>" +
                    $"<pre style='background: #fff; border: 1px solid #2196F3; padding: 10px; margin-top: 8px; overflow-x: auto; max-height: 300px; border-radius: 4px;'>{HtmlEncode(requestBody)}</pre>" +
                    $"</details>";
            }
            
            requestHtml += "</div>";

            _currentTestLogs?.Add(requestHtml);
            _logs.Add(requestHtml);
            Console.WriteLine($"[REQUEST] {method} {url}");
        }

        private static string HtmlEncode(string text)
        {
            return WebUtility.HtmlEncode(text);
        }

        public static void Reset()
        {
            if (_currentTestLogs != null)
            {
                _currentTestLogs.Add($"</div>");
                
                // Update the test result status
                if (_testResults.Count > 0)
                {
                    var lastTest = _testResults[_testResults.Count - 1];
                    lastTest.Passed = !_currentTestLogs.Any(x => x.Contains("test-fail"));
                }
            }
            _currentTestLogs = null;
        }

        public static void FlushReport()
        {
            if (_reportPath == null) return;

            var timespan = DateTime.Now - _reportStartTime;
            var successRate = _totalTests > 0 ? (_passCount * 100 / _totalTests) : 0;

            // Build summary section
            var summaryHtml = new List<string>
            {
                "<div class='summary'>",
                $"<div class='summary-card success'><div class='label'>Passed</div><div class='number'>{_passCount}</div></div>",
                $"<div class='summary-card danger'><div class='label'>Failed</div><div class='number'>{_failCount}</div></div>",
                $"<div class='summary-card warning'><div class='label'>Total Tests</div><div class='number'>{_totalTests}</div></div>",
                $"<div class='summary-card'><div class='label'>Success Rate</div><div class='number'>{successRate}%</div></div>",
                "</div>"
            };

            // Insert summary after placeholder
            _logs.InsertRange(
                _logs.FindIndex(x => x.Contains("id='summary-placeholder'")) + 1,
                summaryHtml
            );

            // Build test details section from organized _testResults
            var testDetailsHtml = new List<string>
            {
                "<div class='test-details-section'>",
                "<h2>🔍 Test Execution Details</h2>"
            };

            foreach (var test in _testResults)
            {
                var statusColor = test.Passed ? "#4CAF50" : "#F44336";
                var statusEmoji = test.Passed ? "✓" : "✗";
                
                testDetailsHtml.Add($"<div class='test-result-item' style='border-left: 5px solid {statusColor}; padding: 15px; margin: 10px 0; background: rgba({(test.Passed ? "76,175,80" : "244,67,54")}, 0.05); border-radius: 4px;'>");
                testDetailsHtml.Add($"<h3 style='color: {statusColor}; margin: 0 0 8px 0;'>{statusEmoji} {test.Name}</h3>");
                testDetailsHtml.Add($"<p style='margin: 5px 0; color: #666; font-size: 0.9em;'><em>{test.Description}</em></p>");
                testDetailsHtml.Add($"<p style='margin: 5px 0; color: #999; font-size: 0.85em;'>⏱ Duration: {(DateTime.Now - test.StartTime).TotalMilliseconds:F0}ms</p>");
                
                // Add test logs
                testDetailsHtml.Add("<div style='margin-top: 10px; padding-top: 10px; border-top: 1px solid #e0e0e0;'>");
                foreach (var log in test.Logs)
                {
                    testDetailsHtml.Add(log);
                }
                testDetailsHtml.Add("</div>");
                
                testDetailsHtml.Add("</div>");
            }

            testDetailsHtml.Add("</div>");

            // Insert test details
            var testPlaceholderIndex = _logs.FindIndex(x => x.Contains("id='test-details-placeholder'"));
            if (testPlaceholderIndex >= 0)
            {
                _logs.InsertRange(testPlaceholderIndex + 1, testDetailsHtml);
            }

            // Add Chart.js scripts
            var chartsScript = new List<string>
            {
                "<script>",
                "document.addEventListener('DOMContentLoaded', function() {",
                "  // Pie Chart - Test Results",
                "  const pieCtx = document.getElementById('pieChart');",
                "  if (pieCtx) {",
                "    new Chart(pieCtx, {",
                "      type: 'doughnut',",
                "      data: {",
                "        labels: ['Passed', 'Failed'],",
                $"        datasets: [{{",
                $"          data: [{_passCount}, {_failCount}],",
                $"          backgroundColor: ['#4CAF50', '#F44336'],",
                $"          borderColor: ['#388E3C', '#D32F2F'],",
                $"          borderWidth: 2",
                $"        }}]",
                "      },",
                "      options: {",
                "        responsive: true,",
                "        maintainAspectRatio: false,",
                "        plugins: {",
                "          legend: {",
                "            position: 'bottom',",
                "            labels: { font: { size: 12 }, padding: 15 }",
                "          }",
                "        }",
                "      }",
                "    });",
                "  }",
                "",
                "  // Line Chart - Pass/Fail Trend",
                "  const lineCtx = document.getElementById('lineChart');",
                "  if (lineCtx) {",
                "    new Chart(lineCtx, {",
                "      type: 'line',",
                "      data: {",
                "        labels: ['Execution Run'],",
                $"        datasets: [{{",
                $"          label: 'Passed Tests',",
                $"          data: [{_passCount}],",
                $"          borderColor: '#4CAF50',",
                $"          backgroundColor: 'rgba(76, 175, 80, 0.1)',",
                $"          tension: 0.4,",
                $"          fill: true,",
                $"          borderWidth: 3",
                $"        }},",
                $"        {{",
                $"          label: 'Failed Tests',",
                $"          data: [{_failCount}],",
                $"          borderColor: '#F44336',",
                $"          backgroundColor: 'rgba(244, 67, 54, 0.1)',",
                $"          tension: 0.4,",
                $"          fill: true,",
                $"          borderWidth: 3",
                $"        }}]",
                "      },",
                "      options: {",
                "        responsive: true,",
                "        maintainAspectRatio: false,",
                "        plugins: {",
                "          legend: {",
                "            position: 'bottom',",
                "            labels: { font: { size: 12 }, padding: 15 }",
                "          }",
                "        },",
                "        scales: {",
                "          y: {",
                "            beginAtZero: true,",
                "            grid: { color: 'rgba(0, 0, 0, 0.05)' }",
                "          },",
                "          x: {",
                "            grid: { color: 'rgba(0, 0, 0, 0.05)' }",
                "          }",
                "        }",
                "      }",
                "    });",
                "  }",
                "});",
                "</script>"
            };

            _logs.AddRange(chartsScript);

            _logs.Add("</div>"); // Close container
            _logs.Add("<div class='footer'>");
            _logs.Add($"<strong>Test Execution Summary Report</strong><br>");
            _logs.Add($"<strong>Duration:</strong> {timespan.TotalSeconds:F2}s | <strong>Passed:</strong> {_passCount} | <strong>Failed:</strong> {_failCount} | <strong>Total:</strong> {_totalTests}");
            _logs.Add($"<small>Report generated by Reqnroll Test Automation Framework v1.0 | .NET 8.0 | {DateTime.Now:F}</small>");
            _logs.Add("</div>");
            _logs.Add("</body>");
            _logs.Add("</html>");

            try
            {
                File.WriteAllLines(_reportPath, _logs);
                Console.WriteLine($"\n📊 Professional test report generated: {_reportPath}");
                Console.WriteLine($"✅ Passed: {_passCount} | ❌ Failed: {_failCount} | ⏱ Duration: {timespan.TotalSeconds:F2}s");
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
