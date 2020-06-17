# C# Features and Best Practices

## Overview
The intent of this repository is to showcase best practices to follow when developing for .Net Core. This repository
will also highlight some of the key features that the framework offers to accelerate development, reduce the chance of bugs,
and ensure consistency across our various repositories.

The key features that will be highlighted are:

* Logging
* Configuration
* Middleware
* gRPC Interceptors
* Startup Filters
* Testing
* Common 3rd Party Libraries

To accomplish this, the repository will implement a simple Weather Forecasting API, based off the default Visual Studio 2019 template
for WebAPI applications. The API will utilize a backing gRPC service to retrieve weather information.

In some cases, it may be required to break with expected development best practices to showcase a given feature. In these instances,
it will be explicitly called out that the code is breaking with the normal approach, and comments will be provided to explain what
the best practice would actually be in a real environment.

## The Story
Some of the concepts described inside the repository are more easily described within the context of a real world business case.
Therefore, this repository should be thought of as the result of a story about a real world business wanting to enhance an existing
legacy system. In this fictional world, we have been tasked by the business with developing a new weather forecasting API built on top of
an existing legacy gRPC service which provides CRUD operations for weather forecasting data for locations all over the globe.

Unfortunately, the legacy system was only written to support Celsius temperature readings, but the business has several external partners who are requesting a public API that can accommodate Farhenheit and
Kelvin, as well. The legacy system is a shared service across many different organizations within the company, and the team responsible for that shared service doesn't have the bandwidth to accommodate
the change.

Instead, engineering decides to create a new publicly facing RESTful http API which acts an adapter on top of the existing gRPC service. The
API will act as a client, handling all conversions to and from Celsius and the other temperature scales.

## Requirements

* The legacy gRPC service cannot be changed to support additional temperature scales.
* The API must support Create, Read, Update, and Delete operations on weather forecasts for given locations.
* The API must properly validate all inputs and fail requests which provide invalid information (e.g. kelvin values below zero are not allowed).
* API must support temperature values provided in either Celsius, Fahrenheit, or Kelvin.
* API must convert any non-Celsius values to celsius before proxying the call to the legacy system.
* API must report any errors back to the caller with an error response.
* API must follow http best practices for status codes.
* API must display the an ASCII art image of a black cat in the logs at startup. Because the business said so, that's why.

## Design
This repository contains two executable applications: The Bcss.Reference.Web assembly which contains the API server, and
Bcss.Reference.Grpc.Server assembly, which contains the gRPC legacy system. The legacy system is entirely encapsulated
in the one assembly due to its legacy nature, but the rest of the API has been broken up into a simple 3-tier architecture.

```
Controller layer
      |
      V
 Business Layer
      |
      V
  Data Layer
```

The controller layer is the entrypoint to the API and is responsible for handling http requests, validating inputs, and mapping requests/responses to and from the appropriate domain objects.

The business layer is where all business logic is encapsulated. Any rules around how the conceptual business entities should interact should be enforced here.

The data layer is responsible for proxying requests to and from the legacy system. It implements a gRPC service client to provide the necessary CRUD operations.

#### Stairway Pattern

The new API utilizes the stairway pattern (from Gary McLean Hall's "Adaptive code via C#") to help promote separation of concerns and avoid classes being coupled to their dependencies implementations.

At a high level, the stairway pattern prescribes that:

1) All classes which perform functions (i.e. anything but models/POCOs) shall exist behind an interface.
2) For each layer, all interfaces for the classes of that layer shall be published in their own assembly, separate from the classes which implement them.
3) Any assemblies containing classes which depend on these interfaces MUST depend only on the interface assembly, not the assembly containing the implementation.

For example, class A depends on class C, which implements interface B. Both classes and the interface all exist within their own assembly, and the assemblies for class A and class C both depend on the assembly for interface B, but do NOT reference each others assemblies at all.

    class A ---> interface B              
                    |
                    |
                    |
                  class C

As the chain of dependencies grow, the layout resembles stairs, hence the name.

Note that this pattern applies to LAYERS of the application, not individual classes. Mapping our applications layers to the above example, class A = business, interface B = data interfaces, class C = Grpc Implementation of data interfaces.

