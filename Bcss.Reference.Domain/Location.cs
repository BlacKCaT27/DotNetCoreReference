namespace Bcss.Reference.Domain
{
    public class Location
    {
        private const float MinLatitude = -90.0f;
        private const float MaxLatitude = 90.0f;

        private const float MinLongitude = -180.0f;
        private const float MaxLongitude = 180.0f;

        public int Id { get; }

        public float Latitude { get; }

        public float Longitude { get; }

        public string Name { get; }

        public Location(int id, float latitude, float longitude, string name)
        {
            Validate(id, latitude, longitude, name);

            Id = id;
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
        }

        private static void Validate(int id, float latitude, float longitude, string name)
        {
            if (id < 0)
            {
                throw new LocationCreationException($"ID must be positive integer, but found {id}.");
            }

            if (latitude < MinLatitude)
            {
                throw new LocationCreationException($"Latitude must be greater than {MinLatitude}, but found {latitude}.");
            }

            if (latitude > MaxLatitude)
            {
                throw new LocationCreationException($"Latitude must be less than {MaxLatitude}, but found {latitude}.");
            }

            if (longitude < MinLongitude)
            {
                throw new LocationCreationException($"Longitude must be greater than {MinLongitude}, but found {longitude}.");
            }

            if (longitude > MaxLongitude)
            {
                throw new LocationCreationException($"Longitude must be less than {MaxLongitude}, but found {longitude}.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new LocationCreationException("Location name cannot be null or empty.");
            }
        }
    }
}