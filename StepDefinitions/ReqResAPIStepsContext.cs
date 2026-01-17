using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReqnrollTests.StepDefinitions
{
    /// <summary>
    /// Shared context for step definitions across scenarios.
    /// Allows state sharing between Given, When, Then steps.
    /// </summary>
    public class ScenarioContext
    {
        public HttpClient? HttpClient { get; set; }
        public HttpResponseMessage? Response { get; set; }
        public JsonElement ResponseBody { get; set; }
        public JsonElement CurrentUser { get; set; }
        public string? CurrentUserEmail { get; set; }
        public List<string> Errors { get; set; } = new();

        public void ClearErrors() => Errors.Clear();
        public void AddError(string error) => Errors.Add(error);
        public bool HasErrors => Errors.Count > 0;
    }

    /// <summary>
    /// Step implementations for ReqRes API feature tests.
    /// These map directly to your feature file scenarios.
    /// </summary>
    public class ReqResAPISteps
    {
        private readonly ScenarioContext _context;

        public ReqResAPISteps(ScenarioContext context)
        {
            _context = context;
        }

        #region Background Steps

        public async Task GivenReqResAPIIsAvailable()
        {
            _context.HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://reqres.in")
            };
        }

        public void GivenAPIKeyHeaderIsSet()
        {
            if (_context.HttpClient == null)
                throw new InvalidOperationException("HttpClient not initialized");
            
            _context.HttpClient.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
        }

        #endregion

        #region When Steps

        public async Task WhenRequestUsersListFromPage2()
        {
            if (_context.HttpClient == null)
                throw new InvalidOperationException("HttpClient not initialized");

            _context.Response = await _context.HttpClient.GetAsync("/api/users?page=2");
            var responseText = await _context.Response.Content.ReadAsStringAsync();
            _context.ResponseBody = JsonSerializer.Deserialize<JsonElement>(responseText);
        }

        #endregion

        #region Then Steps - Response Validation

        public void ThenResponseStatusIs200()
        {
            if (_context.Response?.StatusCode != System.Net.HttpStatusCode.OK)
                _context.AddError($"Expected status 200, got {_context.Response?.StatusCode}");
        }

        public void ThenResponseContainsPageFieldWithValue2()
        {
            if (!_context.ResponseBody.TryGetProperty("page", out var pageValue))
            {
                _context.AddError("Response missing 'page' field");
                return;
            }

            if (pageValue.GetInt32() != 2)
                _context.AddError($"Expected page value 2, got {pageValue.GetInt32()}");
        }

        public void ThenResponseContainsDataArrayWithUsers()
        {
            if (!_context.ResponseBody.TryGetProperty("data", out var dataArray))
            {
                _context.AddError("Response missing 'data' array");
                return;
            }

            if (dataArray.ValueKind != JsonValueKind.Array)
                _context.AddError($"Expected 'data' to be array, got {dataArray.ValueKind}");
        }

        public void ThenDataArrayHas6Items()
        {
            if (!_context.ResponseBody.TryGetProperty("data", out var dataArray))
            {
                _context.AddError("Response missing 'data' array");
                return;
            }

            if (dataArray.GetArrayLength() != 6)
                _context.AddError($"Expected 6 items, got {dataArray.GetArrayLength()}");
        }

        #endregion

        #region Then Steps - Data Structure Validation

        public void ThenEachUserHasRequiredFields(List<string> fields)
        {
            if (!_context.ResponseBody.TryGetProperty("data", out var dataArray))
            {
                _context.AddError("Response missing 'data' array");
                return;
            }

            var firstUser = dataArray[0];
            foreach (var field in fields)
            {
                if (!firstUser.TryGetProperty(field, out _))
                    _context.AddError($"User missing required field: {field}");
            }
        }

        public void ThenDataArrayNotEmpty()
        {
            if (!_context.ResponseBody.TryGetProperty("data", out var dataArray))
            {
                _context.AddError("Response missing 'data' array");
                return;
            }

            if (dataArray.GetArrayLength() == 0)
                _context.AddError("Data array is empty");
        }

        public void ThenResponseHasPaginationFields(List<string> fields)
        {
            var missingFields = fields.Where(f => !_context.ResponseBody.TryGetProperty(f, out _)).ToList();
            
            if (missingFields.Any())
                _context.AddError($"Missing pagination fields: {string.Join(", ", missingFields)}");
        }

        #endregion

        #region Then Steps - User Search

        public void ThenFoundUserWithId8()
        {
            if (!_context.ResponseBody.TryGetProperty("data", out var dataArray))
            {
                _context.AddError("Response missing 'data' array");
                return;
            }

            bool userFound = false;
            foreach (var user in dataArray.EnumerateArray())
            {
                if (user.TryGetProperty("id", out var id) && id.GetInt32() == 8)
                {
                    userFound = true;
                    _context.CurrentUser = user;
                    break;
                }
            }

            if (!userFound)
                _context.AddError("User with id 8 not found");
        }

        public void ThenUserHasEmailAddress()
        {
            if (!_context.CurrentUser.TryGetProperty("email", out var emailProp))
            {
                _context.AddError("User missing email field");
                return;
            }

            _context.CurrentUserEmail = emailProp.GetString();
            
            if (string.IsNullOrEmpty(_context.CurrentUserEmail))
                _context.AddError("User email is empty");
        }

        public void ThenEmailContainsReqresIn()
        {
            if (string.IsNullOrEmpty(_context.CurrentUserEmail))
            {
                _context.AddError("Email not available");
                return;
            }

            if (!_context.CurrentUserEmail.Contains("@reqres.in"))
                _context.AddError($"Email '{_context.CurrentUserEmail}' does not contain @reqres.in");
        }

        public void ThenEmailIsLindsayFerguson()
        {
            const string expectedEmail = "lindsay.ferguson@reqres.in";
            
            if (_context.CurrentUserEmail != expectedEmail)
                _context.AddError($"Expected email '{expectedEmail}', got '{_context.CurrentUserEmail}'");
        }

        #endregion

        #region Validation

        public void AssertNoErrors()
        {
            if (_context.HasErrors)
            {
                var errorMessage = "Validation errors:\n" + string.Join("\n", _context.Errors);
                throw new InvalidOperationException(errorMessage);
            }
        }

        #endregion
    }
}
