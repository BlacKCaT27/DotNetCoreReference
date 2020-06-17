using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Bcss.Reference.Grpc.Server.Test.Component
{

    public class CreateWeatherForecastTests
    {
        private TestClientManager<WeatherService.WeatherServiceClient> _client;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _client = new TestClientManager<WeatherService.WeatherServiceClient>();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _client.Dispose();
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task CanCreateWeatherForecast()
        {
            var request = new CreateWeatherForecastRequest
            {
                Summary = "test",
                Location = new Location
                {
                    Id = 1,
                    Latitude = 40,
                    Longitude = 80,
                    Name = "Test"
                },
                Date = "6/1/2020",
                Temperature = 20
            };

            var response = await _client.Instance.CreateWeatherForecastAsync(request);

            response.Should().BeOfType<CreateWeatherForecastResponse>();
            response.WeatherForecast.Summary.Should().Be("test");
        }
    }
}