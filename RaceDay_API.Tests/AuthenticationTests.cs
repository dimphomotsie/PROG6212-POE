using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace RaceDay_API.Tests
{
    public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthenticationTests(CustomWebApplicationFactory factory) => _factory = factory;

        [Fact]
        public async Task Register_WithValidData_Returns201()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                email = "neworg@test.co.za",
                password = "Password123!",
                fullName = "New Organiser",
                role = "Organiser",
                organisationName = "Test Org"
            });
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_Returns400()
        {
            var client = _factory.CreateClient();
            var body = new
            {
                email = "dup@test.co.za",
                password = "Pass123!",
                fullName = "Dup",
                role = "Participant"
            };
            await client.PostAsJsonAsync("/api/auth/register", body);
            var second = await client.PostAsJsonAsync("/api/auth/register", body);
            second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            var client = _factory.CreateClient();
            await TestHelpers.RegisterAsync(client, "login1@test.co.za", "Correct123!", "Participant");
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email = "login1@test.co.za", password = "Wrong123!" });
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithValidCredentials_Returns200()
        {
            var client = _factory.CreateClient();
            await TestHelpers.RegisterAsync(client, "login2@test.co.za", "Correct123!", "Participant");
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email = "login2@test.co.za", password = "Correct123!" });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

/*
1. FluentAssertions (.Should().Be()).
Author: Fluent Assertions
Link: <https://fluentassertions.com/introduction>
Used in: Writing readable assertions on HTTP status codes (201, 200, 400, 401).
Date Accessed: 18 October 2026

2. HttpStatusCode Enum.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode>
Used in: Comparing responses against named status codes instead of magic numbers.
Date Accessed: 18 October 2026

3. xUnit ([Fact], IClassFixture<T>).
Author: xUnit.net
Link: <https://xunit.net/>
Used in: Structuring authentication tests and sharing a single WebApplicationFactory across the test class.
Date Accessed: 18 October 2026
*/