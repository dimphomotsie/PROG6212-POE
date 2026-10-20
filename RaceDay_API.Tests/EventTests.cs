using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;


namespace RaceDay_API.Tests
{
    public class EventTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public EventTests(CustomWebApplicationFactory factory) => _factory = factory;

        private static object ValidEvent() => new
        {
            name = "Test Marathon",
            description = "A test event",
            eventDate = DateTime.UtcNow.AddDays(30),
            location = "Durban",
            distance = "42.2km",
            eventType = "Run"
        };

        [Fact]
        public async Task Organiser_CanCreateEvent()
        {
            var client = _factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                { HandleCookies = true });

            await TestHelpers.RegisterAsync(client, "org-create@test.co.za", "Pass123!", "Organiser");
            await client.PostAsJsonAsync("/api/auth/login",
                new { email = "org-create@test.co.za", password = "Pass123!" });

            var response = await client.PostAsJsonAsync("/api/events", ValidEvent());
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Participant_CannotCreateEvent_Returns403()
        {
            var client = _factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                { HandleCookies = true });

            await TestHelpers.RegisterAsync(client, "part-create@test.co.za", "Pass123!", "Participant");
            await client.PostAsJsonAsync("/api/auth/login",
                new { email = "part-create@test.co.za", password = "Pass123!" });

            var response = await client.PostAsJsonAsync("/api/events", ValidEvent());
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Unauthenticated_CannotViewEvents_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/events");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
