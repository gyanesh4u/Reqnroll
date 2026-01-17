using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ReqnrollTests.StepDefinitions
{
    /// <summary>
    /// Step definitions for ReqRes API feature scenarios.
    /// Maps Gherkin steps to actual test implementation.
    /// </summary>
    public class ReqResAPIStepDefinitions
    {
        private HttpClient? _httpClient;
        private HttpResponseMessage? _response;
        private JsonElement _responseBody;
        private JsonElement _currentUser;
        private string? _currentUserEmail;

        #region Background Steps

        [Given(@"the ReqRes API is available at https://reqres\.in")]
        public void GivenTheReqResAPIIsAvailable()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://reqres.in")
            };
            Assert.NotNull(_httpClient);
        }

        [Given(@"the API key header is set to ""x-api-key: reqres-free-v1""")]
        public void GivenTheAPIKeyHeaderIsSet()
        {
            if (_httpClient == null)
                throw new InvalidOperationException("HttpClient not initialized");
            
            _httpClient.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
        }

        #endregion

        #region When Steps

        [When(@"I request the users list from page 2")]
        public async Task WhenIRequestTheUsersListFromPage2()
        {
            if (_httpClient == null)
                throw new InvalidOperationException("HttpClient not initialized");

            _response = await _httpClient.GetAsync("/api/users?page=2");
            Assert.NotNull(_response);

            var responseText = await _response.Content.ReadAsStringAsync();
            _responseBody = JsonSerializer.Deserialize<JsonElement>(responseText);
        }

        #endregion

        #region Then Steps

        [Then(@"the response status should be 200")]
        public void ThenTheResponseStatusShouldBe200()
        {
            Assert.NotNull(_response);
            Assert.Equal(System.Net.HttpStatusCode.OK, _response.StatusCode);
        }

        [Then(@"the response should contain a page field with value 2")]
        public void ThenTheResponseShouldContainAPageFieldWithValue2()
        {
            Assert.True(_responseBody.TryGetProperty("page", out var pageValue), 
                "Response should contain 'page' field");
            Assert.Equal(2, pageValue.GetInt32());
        }

        [Then(@"the response should contain a data array with users")]
        public void ThenTheResponseShouldContainADataArrayWithUsers()
        {
            Assert.True(_responseBody.TryGetProperty("data", out var dataArray), 
                "Response should contain 'data' array");
            Assert.Equal(JsonValueKind.Array, dataArray.ValueKind);
        }

        [Then(@"the data array should have 6 items")]
        public void ThenTheDataArrayShouldHave6Items()
        {
            Assert.True(_responseBody.TryGetProperty("data", out var dataArray));
            Assert.Equal(6, dataArray.GetArrayLength());
        }

        [Then(@"each user in the data array should have the following fields:")]
        public void ThenEachUserShouldHaveTheFollowingFields(Table table)
        {
            Assert.True(_responseBody.TryGetProperty("data", out var dataArray));
            
            var firstUser = dataArray[0];
            var requiredFields = table.Rows.Select(row => row["Field"]).ToList();

            foreach (var field in requiredFields)
            {
                Assert.True(firstUser.TryGetProperty(field, out _), 
                    $"User should have '{field}' field");
            }
        }

        [Then(@"the data array should contain at least one user")]
        public void ThenTheDataArrayShouldContainAtLeastOneUser()
        {
            Assert.True(_responseBody.TryGetProperty("data", out var dataArray));
            Assert.True(dataArray.GetArrayLength() > 0, "Data array should not be empty");
        }

        [Then(@"the response should have pagination fields:")]
        public void ThenTheResponseShouldHavePaginationFields(Table table)
        {
            var requiredFields = table.Rows.Select(row => row["Field"]).ToList();

            foreach (var field in requiredFields)
            {
                Assert.True(_responseBody.TryGetProperty(field, out _), 
                    $"Response should contain '{field}' field");
            }
        }

        [Then(@"the data array should not be empty")]
        public void ThenTheDataArrayShouldNotBeEmpty()
        {
            Assert.True(_responseBody.TryGetProperty("data", out var dataArray));
            Assert.True(dataArray.GetArrayLength() > 0);
        }

        [Then(@"I should find a user with id 8 in the data array")]
        public void ThenIShouldFindAUserWithId8InTheDataArray()
        {
            Assert.True(_responseBody.TryGetProperty("data", out var dataArray));
            
            bool userFound = false;
            foreach (var user in dataArray.EnumerateArray())
            {
                if (user.TryGetProperty("id", out var id) && id.GetInt32() == 8)
                {
                    userFound = true;
                    _currentUser = user;
                    break;
                }
            }

            Assert.True(userFound, "User with id 8 should be found");
        }

        [Then(@"that user should have an email address")]
        public void ThenThatUserShouldHaveAnEmailAddress()
        {
            Assert.True(_currentUser.TryGetProperty("email", out var emailProp), 
                "User should have email");
            _currentUserEmail = emailProp.GetString();
            Assert.NotNull(_currentUserEmail);
        }

        [Then(@"the email should contain ""@reqres\.in""")]
        public void ThenTheEmailShouldContainReqresIn()
        {
            Assert.NotNull(_currentUserEmail);
            Assert.Contains("@reqres.in", _currentUserEmail);
        }

        [Then(@"the user email should be ""lindsay\.ferguson@reqres\.in""")]
        public void ThenTheUserEmailShouldBeLindayFerguson()
        {
            Assert.NotNull(_currentUserEmail);
            Assert.Equal("lindsay.ferguson@reqres.in", _currentUserEmail);
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            _httpClient?.Dispose();
            _response?.Dispose();
        }

        #endregion
    }
}
