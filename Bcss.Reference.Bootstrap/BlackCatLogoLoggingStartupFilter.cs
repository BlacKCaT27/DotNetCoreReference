using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Bcss.Reference.Bootstrap
{
    /// <summary>
    /// StartupFilters are a handy tool for time when you have code that needs to be executed one time
    /// at application startup. All one must do is implement the IStartupFilter interface and register your
    /// implementation with the DI framework. As part of the app initialization, any registered IStartupFilters
    /// are executed in the order they're registered.
    ///
    /// StartupFilters can be used for all sorts of tasks, from seeding database data to checking on a backend services state
    /// to printing silly logos in your log files. Here, we go with the latter. :)
    /// </summary>
    public class BlackCatLogoLoggingStartupFilter : IStartupFilter
    {
        private const string Logo =
            @"                                                                                
                                    #@                                          
                 @@@@&     @@@@@@@@@@@@@                                        
                   @@@@    @@@@@@@@@@@@.                                        
                     @@@@    @@@@@@@@@@                                         
                @@@@@@@@@@@@@@@@@@@@@@@                                         
                  .,@@@@@@@@@@@@@@@@@@                                          
                     @@@@@@@@@@@@@@@,                                           
                     @@@@@@@@@@@@@@@                                            
                     @@@@@@@@@@@@@@@                                            
                     @@@@@@@@@@@@@@@@,                       @@@                
                     @@@@@@@@@@@@@@@@@&                      @@@                
                     @@@@@@@@@@@@@@@@@@@                     @@@                
                      @@@@@@@@@@@@@@@@@@(                    @@@                
                      @@@@@@@@@@@@@@@@@@@                   @@@@                
                       @@@@@@@@@@@@@@@@@@@                 @@@@                 
                        /@@@@@@@@@@@@@@@@@               @@@@@                  
                          @@@@@@@@@@@@@@@@            @@@@@@                    
                           @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                       
                         @@@@@@@@@@@@@@@@@@@@@@@@@@@                            
                       @@@@@@@@@@@@@@@,                                         
                                                                                ";
        private readonly ILogger<BlackCatLogoLoggingStartupFilter> _logger;

        public BlackCatLogoLoggingStartupFilter(ILogger<BlackCatLogoLoggingStartupFilter> logger)
        {
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            _logger.LogInformation(Logo);
            return next;
        }
    }
}