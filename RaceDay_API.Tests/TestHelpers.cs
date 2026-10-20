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
