using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Json;

namespace RaceDay_API.Tests
{
    public static class TestHelpers
    {
        public static async Task<HttpClient> CreateLoggedInClientAsync(
            CustomWebApplicationFactory factory, string email, string password)
        {
            var client = factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                {
                    HandleCookies = true   
                });

            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email, password });
            response.EnsureSuccessStatusCode();
            return client;
        }

        public static async Task RegisterAsync(
            HttpClient client, string email, string password, string role)
        {
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                email,
                password,
                fullName = "Test User",
                role
            });
            response.EnsureSuccessStatusCode();
        }
    }
}

/*
1. async/await in Tests.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/>
Used in: Writing non-blocking helper methods (CreateLoggedInClientAsync, RegisterAsync) that mirror real API usage.
Date Accessed: 18 October 2026

2. HttpClient and PostAsJsonAsync (System.Net.Http.Json).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json.httpclientjsonextensions.postasjsonasync>
Used in: Sending JSON POST requests to /api/auth/register and /api/auth/login from tests.
Date Accessed: 18 October 2026

3. WebApplicationFactoryClientOptions (HandleCookies = true).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#handle-cookies>
Used in: Persisting the session cookie across requests so tests can call protected endpoints after login.
Date Accessed: 18 October 2026
*/