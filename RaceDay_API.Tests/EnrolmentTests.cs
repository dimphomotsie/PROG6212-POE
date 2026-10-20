using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace RaceDay_API.Tests
{
    public class EnrolmentTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public EnrolmentTests(CustomWebApplicationFactory factory) => _factory = factory;

        private async Task<(HttpClient organiserClient, int eventId, int categoryId)>
            SetupEventAsync(string suffix)
        {
            var orgClient = _factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                { HandleCookies = true });

            await TestHelpers.RegisterAsync(orgClient, $"org-{suffix}@test.co.za", "Pass123!", "Organiser");
            await orgClient.PostAsJsonAsync("/api/auth/login",
                new { email = $"org-{suffix}@test.co.za", password = "Pass123!" });

            var ev = await orgClient.PostAsJsonAsync("/api/events", new
            {
                name = $"Event {suffix}",
                description = "Desc",
                eventDate = DateTime.UtcNow.AddDays(10),
                location = "Durban",
                distance = "10km",
                eventType = "Run"
            });
            var evJson = await ev.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var eventId = int.Parse(evJson!["eventId"].ToString()!);

            var cat = await orgClient.PostAsJsonAsync($"/api/events/{eventId}/categories",
                new { name = "Senior", description = "Open" });
            var catJson = await cat.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var categoryId = int.Parse(catJson!["categoryId"].ToString()!);

            return (orgClient, eventId, categoryId);
        }

        [Fact]
        public async Task Participant_CanEnrol_Returns201()
        {
            var (_, eventId, categoryId) = await SetupEventAsync("enrol1");

            var partClient = _factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                { HandleCookies = true });
            await TestHelpers.RegisterAsync(partClient, "part-enrol1@test.co.za", "Pass123!", "Participant");
            await partClient.PostAsJsonAsync("/api/auth/login",
                new { email = "part-enrol1@test.co.za", password = "Pass123!" });

            var response = await partClient.PostAsJsonAsync(
                $"/api/events/{eventId}/enrolments", new { categoryId });
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Participant_CannotEnrolTwice_Returns400()
        {
            var (_, eventId, categoryId) = await SetupEventAsync("enrol2");

            var partClient = _factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                { HandleCookies = true });
            await TestHelpers.RegisterAsync(partClient, "part-enrol2@test.co.za", "Pass123!", "Participant");
            await partClient.PostAsJsonAsync("/api/auth/login",
                new { email = "part-enrol2@test.co.za", password = "Pass123!" });

            await partClient.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
            var second = await partClient.PostAsJsonAsync(
                $"/api/events/{eventId}/enrolments", new { categoryId });
            second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Organiser_CannotEnrol_Returns403()
        {
            var (orgClient, eventId, categoryId) = await SetupEventAsync("enrol3");
            var response = await orgClient.PostAsJsonAsync(
                $"/api/events/{eventId}/enrolments", new { categoryId });
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}

/*
1. FluentAssertions (.Should().Be()).
Author: Fluent Assertions
Link: <https://fluentassertions.com/introduction>
Used in: Asserting 201 Created on first enrol, 400 Bad Request on duplicate, and 403 Forbidden for Organisers.
Date Accessed: 18 October 2026

2. ReadFromJsonAsync<Dictionary<string, object>>.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json.httpcontentjsonextensions.readfromjsonasync>
Used in: Extracting eventId and categoryId from setup responses for use in subsequent calls.
Date Accessed: 18 October 2026

3. xUnit ([Fact], IClassFixture<T>).
Author: xUnit.net
Link: <https://xunit.net/>
Used in: Structuring enrolment tests.
Date Accessed: 18 October 2026
*/
