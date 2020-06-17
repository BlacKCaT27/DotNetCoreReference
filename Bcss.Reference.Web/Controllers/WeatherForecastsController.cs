#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bcss.Reference.Business;
using Bcss.Reference.Config;
using Bcss.Reference.Domain;
using Bcss.Reference.Web.Requests;
using Bcss.Reference.Web.Responses;
using Bcss.Reference.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement.Mvc;

namespace Bcss.Reference.Web.Controllers
{
    // The constructs here over the class definition are called "Attributes".
    // These serve as markers and data containers to provide contextual information
    // about the class it marks. These markers are then later read at runtime
    // by code that uses reflection to find classes with the attributes they
    // care about and perform whatever action they need to take.

    // The ApiController attribute declares that the class should be
    // treated as an API controller that sends and receives data, as opposed to one which returns Views (views are out of scope of this repository).
    // Controllers, from the frameworks perspective, are classes which are responsible for handling incoming http requests.
    // We must also declare the route that this controller is responsible for. Route declarations are how the framework learns which requests
    // should be sent to which controllers. These attributes are scanned by the UseRouting() middleware from Startup.cs to
    // create an internal mapping in the framework.
    // Routes use a name-based convention strategy that can optionally be overridden. By using the word 'controller' in square brackets, we're
    // instructing the framework to use the name of the class (minus the special word "Controller") as the route.
    //
    // {{host}}:{{port}}/weatherforecasts
    //
    // Note that route paths are case-insensitive (but query parameters aren't!)
    //
    // We're designing our API to be REST compliant (or, to be a RESTful API). But since REST is not a true standard and more of
    // a set of design principles, we can adapt the guidelines to meet our needs. This will be discussed in the comments below.
    // For more information on REST, see https://restfulapi.net/.
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastsController : ControllerBase
    {
        private readonly IWeatherForecastService _weatherForecastService;
        private readonly IMapper _mapper;
        private readonly ILogger<WeatherForecastsController> _logger;

        /// <summary>
        /// Instantiates a new instance of the <see cref="WeatherForecastsController"/> class.
        ///
        /// The framework is solely responsible for the creation of controllers, which act as the entry point for specific requests.
        /// The framework recursively examines the constructors of every class that is required to instantiate the object, resolving
        /// each dependencies requested type with the concrete type registered to the DI framework until it has every object it needs
        /// to build the object graph and pass in the request.
        ///
        /// If required, you can also request concrete implementation classes, though this is considered poor practice as it can greatly reduce testability, so
        /// it should only be done when no alternative is available, such as 3rd party code. Whenever possible, if your implementation class CAN be behind an interface,
        /// it should be.
        /// </summary>
        /// <param name="logger">An instance of a logger. Log messages passed to this object will be sent to every registered logging provider.</param>
        /// <param name="weatherForecastService">An object that implements the IWeatherForecastService. This is a perfect example of the benefits
        /// of the stairway pattern. Even though the framework technically allows me to request a concrete implementation of the `WeatherForecastService`, I'm
        /// blocked from doing so by the compiler itself as the assembly this class is in has no direct reference to the Bcss.Reference.Business.Impl assembly
        /// in which that class is located. Thus, I am forced to use the interface, which my assembly can see. There is the potential workaround of adding a direct reference
        /// to the Impl assembly, but that would violate the stairway pattern and should be watched for in code review. There are additional measures that can be taken to avoid
        /// that pitfall, which will be highlighted elsewhere.
        /// </param>
        /// <param name="mapper"></param>
        public WeatherForecastsController(ILogger<WeatherForecastsController> logger, IWeatherForecastService weatherForecastService, IMapper mapper)
        {
            _logger = logger;
            _weatherForecastService = weatherForecastService;
            _mapper = mapper;
        }

        /// <summary>
        /// The HttpGet attribute instructs the framework to route all http GET requests that match this controller and actions route to this method. Methods that exist
        /// on controllers are referred to as "actions". So in our case, any GET requests sent to /weatherforecast/{id} will be routed to the GetByIdAsync() action.
        /// 
        /// In this scenario, the HttpGet attribute was passed a templated string. This template format, with { }, instructs the framework to handle the route dynamically,
        /// with the string inside the curly brackets becoming a parameter name for the action.
        ///
        /// Routes are built up incrementally in .Net Core, starting with the host url of your server, then whatever name is resolved from the attributes on the controller are added,
        /// and finally, any additional route information provided by attributes on actions is added.
        /// Since the string we provided to the attribute is a template, .Net Core knows not to expect a static value on the route, but to parametrize it.
        /// In this case, if we wanted to access the WeatherForecast resource with an ID of 1, this actions route would resolve to:
        ///
        /// {host}/weatherforecasts/1
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Because we're taking in a value type and not a request object, we have to do model validation ourselves.
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorMessage = "Weather Forecast ID's must be positive integers."
                });
            }

            var weatherForecast = await _weatherForecastService.GetWeatherForecastById(id);
            var viewModel = _mapper.Map<WeatherForecast, WeatherForecastViewModel>(weatherForecast);
            var response = new WeatherForecastResponse
            {
                Forecast = viewModel
            };

            return Ok(response);
        }

        /// <summary>
        /// Here we're implementing another GET but without any arguments passed in. This results in no additional data
        /// being appended to the route, so our route resolves to that of the controller (e.g. "/weatherforecasts").
        /// Following RESTful design best practices, endpoints for resources should return a collection of said resource.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListWeatherForecasts()
        {
            var forecasts = await _weatherForecastService.ListWeatherForecasts();
            var viewModels = forecasts.Select(f => _mapper.Map<WeatherForecast, WeatherForecastViewModel>(f)).ToList();
            var response = new ListWeatherForecastsResponse
            {
                WeatherForecasts = viewModels
            };

            return Ok(response);
        }

        /// <summary>
        /// Creates a new WeatherForecast resource.
        ///
        /// Whether you choose to use POST or PUT for object creation depends on how you intend to create the object.
        ///
        /// If the unique identification of a resource can be achieved by a caller-provided identifier, use PUT.
        /// If the unique identification of a resource is left up to the server, use POST.
        ///
        /// The idea here is that when creating an object where the ID is not yet known but will be assigned, you should POST
        /// to the resource collection endpoint. The response should be the resource you just created (along with its new ID).
        ///
        /// PUT should be considered an idempotent action which should be treated as either "replace" or "create or replace". In other words, if the caller
        /// knows the the identifier already it can replace the resource directly. In the event that the resource doesn't exist,
        /// the server can elect to either create the object (which according to HTTP spec means the server must return a 201 - Created response),
        /// or the server can elect to fail the request with 404 - Not Found. If the resource is found, the new values from the request
        /// replace the existing values on the resource, and an updated copy of the resource is returned in the body of a 200 response.
        ///
        /// In our case, since we want to maintain control over Weather Forecast ID generation, we will elect to use put for "replace" only,
        /// and will use Post for object creation.
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateWeatherForecast(CreateWeatherForecastRequest request)
        {
            var location = _mapper.Map<LocationViewModel, Location>(request.Location);
            var date = request.Date;
            var summary = request.Summary;
            var temperature = (decimal)request.Temperature;
            var scale = request.Scale;

            var result = await _weatherForecastService.CreateWeatherForecast(location, date, temperature, scale, summary);
            var viewModel = _mapper.Map<WeatherForecast, WeatherForecastViewModel>(result);

            var response = new WeatherForecastResponse
            {
                Forecast = viewModel
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeatherForecast(UpdateWeatherForecastRequest request)
        {
            // HACK: This try/catch should not be here. We only have to capture the ArgumentException because
            // the mapper is handling the validation for the three input temperature values in the WeatherForecastViewModel.
            // This is an example of technical debt; a quick solution in the short term to meet a deadline (say, a training
            // exercise that's occuring in T - 5 hours...), but one that needs to be tracked. The correct solution here would
            // be to write a custom model binder for the WeatherForecastViewModel object where it can validate that the three
            // input temperatures match and fail to bind if they don't.
            WeatherForecast newForecast;
            try
            {
                newForecast = _mapper.Map<WeatherForecastViewModel, WeatherForecast>(request.WeatherForecast);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            

            newForecast = await _weatherForecastService.UpdateWeatherForecast(newForecast);

            var result = _mapper.Map<WeatherForecast, WeatherForecastViewModel>(newForecast);

            var response = new WeatherForecastResponse
            {
                Forecast = result
            };

            return Ok(response);
        }

        /// <summary>
        /// HttpDelete, unsurprisingly, is used to mark an action that handles the deletion of resources.
        /// Returning objects for delete calls is considered optional according to the HTTP spec and REST guidelines,
        /// but we do it here for the sake of completeness.
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeatherForecast(int id)
        {
            // Because we're taking in a value type and not a request object, we have to do model validation ourselves.
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorMessage = "Weather Forecast ID's must be positive integers."
                });
            }

            var result = await _weatherForecastService.DeleteWeatherForecast(id);

            var response = new DeleteWeatherForecastResponse
            {
                Deleted = result
            };

            return Ok(response);
        }

        /// <summary>
        /// Here we implement an experimental new GET endpoint for retrieving a WeatherForecast based on location and date, rather than an identifier.
        /// Because this feature is still experimental, it is feature flagged. Also, the route has not yet been fully determined by the technical team, so "/weatherforecast/byLocation"
        /// is being used as a placeholder. This route path is declared by passing the string "byLocation" to the HttpGet attribute. Note that unlike the previous
        /// GET method, this string is static and not templated with { }, therefore the route will not change based on the request.
        ///
        /// Note that our method takes a fully defined C# object as its request. We don't have to worry about taking a reference to a raw HttpRequest object
        /// and unpacking the body ourselves; the framework does that for us. The framework also allows you to decorate request objects with Data Annotation
        /// attributes that let you provide validation rules in-line within the request. When unpacking an http request, the framework will deserialize the request
        /// body and apply any attribute rules. If any of the validation checks fail, the request is automatically denied with a 400 - Bad Request response before
        /// it even gets to our action.
        ///
        /// The FeatureGate attribute, provided by the Microsoft.FeatureManagement.Aspnetcore NuGet package, works with the Microsoft.FeatureManagement package to flag
        /// controllers and actions as being feature-gated. If a controller or action has been disabled via a feature flag, any calls to that controller or actions route
        /// will automatically return a 404, blocking callers from the feature.
        ///
        /// The [FromQuery] attributes on the parameters of this method indicate to the framework that the values should be pulled from the query string, using
        /// parameter/query parameter name matching. There are several other [From{x}] attributes, which are explained here:
        /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-3.1
        /// </summary>
        [HttpGet("byDate")]
        [FeatureGate(FeatureFlags.AllowGetForecastByDate)]
        public async Task<IActionResult> Get([FromQuery] int locationId, [FromQuery] DateTime date)
        {
            // Thanks to the Range attribute on the `GetWeatherForecastRequest` LocationId property,
            // we don't have to validate the location id here. If the range wasn't satisfied, the caller
            // will automatically be sent back a 400 - bad request.

            // Use the below format when logging to ensure the cuda logger can pick up on the individual parameters passed in during string formatting
            // so they can be captured and logged separately to be searchable later.
            _logger.LogInformation("Requesting weather forecast for location {locationId} on date {date}", locationId, date);

            var forecast = await _weatherForecastService.GetWeatherForecastForDate(locationId, date);

            // Since WeatherForecast and WeatherForecastViewModel don't have matching properties, we'll need to write a TypeConverter for AutoMapper to use
            // to do the mapping for us. We then need to configure AutoMapper to use our TypeConverter in WebMapperProfile.cs.
            var viewModel = _mapper.Map<WeatherForecast, WeatherForecastViewModel>(forecast);

            // It's a good practice to always return data from APIs in response 'envelopes'. Even if you're only returning a single entity,
            // the response envelope ensures that clients can always count on getting a single JSON object back. When designing public APIs, consistency
            // across endpoints is absolutely key to making an API easy and enjoyable to use.
            var response = new WeatherForecastResponse
            {
                Forecast = viewModel
            };

            // Since we inherit from the framework provided `ControllerBase` class, we're given access to several static helper methods which easily create
            // the appropriate IActionResult concrete response object. Here, `Ok(response)` takes our response object, serializes it to json, and returns
            // a 200 - OK http response with our response data in the response body. All in one line.
            return Ok(response);
        }
    }
}
