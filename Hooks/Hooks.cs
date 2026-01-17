using Reqnroll;
using AventStack.ExtentReports;
using ReqnrollTests.Services;

namespace ReqnrollTests.Hooks
{
    [Binding]
    public class Hooks
    {
        private readonly ScenarioContext _scenarioContext;

        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Initialize the Extent Report before all tests run
            ExtentReportService.InitializeReport();
            Console.WriteLine("✓ Extent Report initialized");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            var scenarioName = _scenarioContext.ScenarioInfo.Title;
            var scenarioTags = string.Join(", ", _scenarioContext.ScenarioInfo.Tags);
            
            // Start test in Extent Report
            ExtentReportService.StartTest(
                scenarioName,
                $"Tags: {scenarioTags}"
            );

            Console.WriteLine($"Starting scenario: {scenarioName}");
            ExtentReportService.LogInfo($"Scenario Started: {scenarioName}");
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var scenarioName = _scenarioContext.ScenarioInfo.Title;
            var status = _scenarioContext.ScenarioExecutionStatus;

            Console.WriteLine($"Finished scenario: {scenarioName}");
            
            if (status == ScenarioExecutionStatus.OK)
            {
                ExtentReportService.LogPass($"Scenario Passed: {scenarioName}");
                Console.WriteLine("✓ Scenario passed!");
            }
            else if (status == ScenarioExecutionStatus.TestError)
            {
                ExtentReportService.LogFail($"Scenario Failed: {scenarioName}");
                
                if (_scenarioContext.TestError != null)
                {
                    ExtentReportService.LogError(
                        _scenarioContext.TestError.Message,
                        _scenarioContext.TestError
                    );
                }
                
                Console.WriteLine("✗ Scenario failed with error!");
            }
            else if (status == ScenarioExecutionStatus.BindingError)
            {
                ExtentReportService.LogFail($"Scenario Binding Error: {scenarioName}");
                Console.WriteLine("✗ Scenario failed with binding error!");
            }
            else if (status == ScenarioExecutionStatus.StepDefinitionPending)
            {
                ExtentReportService.LogWarning($"Scenario Pending: {scenarioName}");
                Console.WriteLine("⊘ Scenario has pending steps!");
            }

            // Reset for next scenario
            ExtentReportService.Reset();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Flush the report after all tests complete
            ExtentReportService.FlushReport();
            Console.WriteLine("✓ Extent Report finalized and saved");
        }
    }
}

