using Xunit;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ReqnrollTests.Services;

namespace ReqnrollTests.Tests
{
    public class ReqResAPITests : IAsyncLifetime
    {
        private static HttpClient? _httpClient;

        public async Task InitializeAsync()
        {
            ExtentReportService.InitializeReport();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://reqres.in")
            };
            _httpClient.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _httpClient?.Dispose();
            ExtentReportService.FlushReport();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetUsersFromPage2()
        {
            ExtentReportService.StartTest(nameof(GetUsersFromPage2), "API Test");
            
            try
            {
                var response = await _httpClient!.GetAsync("/api/users?page=2");
                
                Assert.True(response.IsSuccessStatusCode, "Status code should be 200 (OK)");
                ExtentReportService.LogInfo("✓ Response status: 200");

                var responseText = await response.Content.ReadAsStringAsync();
                var responseBody = JsonSerializer.Deserialize<JsonElement>(responseText);

                Assert.True(responseBody.TryGetProperty("page", out var pageValue), "Response should contain 'page' field");
                Assert.Equal(2, pageValue.GetInt32());
                ExtentReportService.LogInfo("✓ Page value: 2");

                Assert.True(responseBody.TryGetProperty("data", out var dataArray), "Response should contain 'data' array");
                Assert.Equal(JsonValueKind.Array, dataArray.ValueKind);
                ExtentReportService.LogInfo($"✓ Data array found with {dataArray.GetArrayLength()} items");
                
                ExtentReportService.LogPass($"✓ {nameof(GetUsersFromPage2)} PASSED");
            }
            catch (Exception ex)
            {
                ExtentReportService.LogFail($"✗ {nameof(GetUsersFromPage2)} FAILED");
                ExtentReportService.LogError(ex.Message);
                throw;
            }
            finally
            {
                ExtentReportService.Reset();
            }
        }

        [Fact]
        public async Task VerifyUserDataStructure()
        {
            ExtentReportService.StartTest(nameof(VerifyUserDataStructure), "API Test");
            
            try
            {
                var response = await _httpClient!.GetAsync("/api/users?page=2");
                Assert.True(response.IsSuccessStatusCode);

                var responseText = await response.Content.ReadAsStringAsync();
                var responseBody = JsonSerializer.Deserialize<JsonElement>(responseText);

                Assert.True(responseBody.TryGetProperty("data", out var dataArray), "Should have data array");
                Assert.True(dataArray.GetArrayLength() > 0, "Data array should not be empty");
                ExtentReportService.LogInfo("✓ Data array contains items");

                var firstUser = dataArray[0];
                var requiredFields = new[] { "id", "email", "first_name", "last_name", "avatar" };

                foreach (var field in requiredFields)
                {
                    Assert.True(firstUser.TryGetProperty(field, out _), $"User should have '{field}' field");
                    ExtentReportService.LogInfo($"✓ User has '{field}' field");
                }
                
                ExtentReportService.LogPass($"✓ {nameof(VerifyUserDataStructure)} PASSED");
            }
            catch (Exception ex)
            {
                ExtentReportService.LogFail($"✗ {nameof(VerifyUserDataStructure)} FAILED");
                ExtentReportService.LogError(ex.Message);
                throw;
            }
            finally
            {
                ExtentReportService.Reset();
            }
        }

        [Fact]
        public async Task VerifyPagination()
        {
            ExtentReportService.StartTest(nameof(VerifyPagination), "API Test");
            
            try
            {
                // Test page 2 with custom per_page parameter
                var response = await _httpClient!.GetAsync("/api/users?page=2");
                Assert.True(response.IsSuccessStatusCode);

                var responseText = await response.Content.ReadAsStringAsync();
                var responseBody = JsonSerializer.Deserialize<JsonElement>(responseText);

                // Verify pagination structure
                Assert.True(responseBody.TryGetProperty("page", out _), "Should have page field");
                Assert.True(responseBody.TryGetProperty("per_page", out _), "Should have per_page field");
                Assert.True(responseBody.TryGetProperty("total", out _), "Should have total field");
                Assert.True(responseBody.TryGetProperty("total_pages", out _), "Should have total_pages field");
                Assert.True(responseBody.TryGetProperty("data", out var dataArray), "Should have data array");
                Assert.True(dataArray.GetArrayLength() > 0, "Data array should not be empty");
                
                ExtentReportService.LogInfo($"✓ Pagination structure verified");
                ExtentReportService.LogInfo($"✓ Data array has {dataArray.GetArrayLength()} items");
                
                ExtentReportService.LogPass($"✓ {nameof(VerifyPagination)} PASSED");
            }
            catch (Exception ex)
            {
                ExtentReportService.LogFail($"✗ {nameof(VerifyPagination)} FAILED");
                ExtentReportService.LogError(ex.Message);
                throw;
            }
            finally
            {
                ExtentReportService.Reset();
            }
        }

        [Fact]
        public async Task FindUserById()
        {
            ExtentReportService.StartTest(nameof(FindUserById), "API Test");
            
            try
            {
                var response = await _httpClient!.GetAsync("/api/users?page=2");
                Assert.True(response.IsSuccessStatusCode);

                var responseText = await response.Content.ReadAsStringAsync();
                var responseBody = JsonSerializer.Deserialize<JsonElement>(responseText);

                Assert.True(responseBody.TryGetProperty("data", out var dataArray), "Should have data");
                
                bool userFound = false;
                foreach (var user in dataArray.EnumerateArray())
                {
                    if (user.TryGetProperty("id", out var id) && id.GetInt32() == 8)
                    {
                        userFound = true;
                        Assert.True(user.TryGetProperty("email", out var email), "User should have email");
                        var emailStr = email.GetString() ?? "";
                        Assert.Contains("@reqres.in", emailStr);
                        ExtentReportService.LogInfo($"✓ Found user with id=8, email={emailStr}");
                        break;
                    }
                }

                Assert.True(userFound, "User with id 8 should be found");
                ExtentReportService.LogPass($"✓ {nameof(FindUserById)} PASSED");
            }
            catch (Exception ex)
            {
                ExtentReportService.LogFail($"✗ {nameof(FindUserById)} FAILED");
                ExtentReportService.LogError(ex.Message);
                throw;
            }
            finally
            {
                ExtentReportService.Reset();
            }
        }
    }
}
