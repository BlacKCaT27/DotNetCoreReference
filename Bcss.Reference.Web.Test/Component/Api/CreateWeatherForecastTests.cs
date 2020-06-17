using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bcss.Reference.Bootstrap;
using Bcss.Reference.Web.Requests;
using Bcss.Reference.Web.Responses;
using Bcss.Reference.Web.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Bcss.Reference.Web.Test.Component.Api
{
    [Category("e2e")]
    public class CreateWeatherForecastTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _factory = TestServerFactory.CreateWebApplicationFactory();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _factory.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task GivenValidRequestServiceCanCreateNewWeatherForecast()
        {
            const int year = 2020;
            const int month = 6;
            const int day = 1;
            const string summary = "Balmy";
            const string locationName = "Ann Arbor, MI";
            const double temperatureC = 20;
            const string scale = "c";

            var date = new DateTime(year, month, day);

            var expectedLocation = new LocationViewModel
            {
                Id = 1,
                Latitude = 40,
                Longitude = 80,
                Name = locationName
            };

            var createWeatherForecastRequest = new CreateWeatherForecastRequest
            {
                Summary = summary,
                Date = date,
                Temperature = temperatureC,
                Location = expectedLocation,
                Scale = scale
            };

            var expectedResult = new WeatherForecastViewModel
            {
                Id = 1,
                Summary = summary,
                Date = date,
                Temperature = (decimal) temperatureC,
                Scale = scale,
                Location = expectedLocation
            };

            var body = ToStringContent(createWeatherForecastRequest);

            var response = await _client.PostAsync("/weatherforecasts", body);

            response.StatusCode.Should().Be((int) HttpStatusCode.OK);
            var responseBody = await response.Content.ReadAsStringAsync();
            var createWeatherForecastResponse = JsonConvert.DeserializeObject<WeatherForecastResponse>(responseBody);

            createWeatherForecastResponse.Should().NotBeNull();
            createWeatherForecastResponse.Forecast.Should().NotBeNull();

            createWeatherForecastResponse.Forecast.Should().BeEquivalentTo(expectedResult);
        }

        private static StringContent ToStringContent(object body)
        {
            var json = JsonConvert.SerializeObject(body);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}