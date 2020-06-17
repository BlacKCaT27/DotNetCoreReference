using AutoMapper;
using Bcss.Reference.Domain;
using Bcss.Reference.Web.ViewModels;

namespace Bcss.Reference.Web.TypeConverters
{
    public class LocationViewModelToLocationTypeConverter : ITypeConverter<LocationViewModel, Location>
    {
        public Location Convert(LocationViewModel source, Location destination, ResolutionContext context)
        {
            return new Location(source.Id, source.Latitude, source.Longitude, source.Name);
        }
    }
}