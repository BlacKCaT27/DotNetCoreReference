using System.ComponentModel.DataAnnotations;

namespace Bcss.Reference.Web.ViewModels
{
    public class LocationViewModel
    {
        [Range(0, int.MaxValue)]
        public int Id { get; set; }

        [Range(-90.0f, 90.0f, ErrorMessage = "Latitude must be between -90 and 90 degrees.")]
        public float Latitude { get; set; }

        [Range(-180.0f, 180.0f, ErrorMessage = "Longitude must be between -180 and 180 degrees.")]
        public float Longitude { get; set; }

        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Location name cannot be empty")]
        public string Name { get; set; }
    }
}