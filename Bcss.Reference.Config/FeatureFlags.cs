namespace Bcss.Reference.Config
{
    /// <summary>
    /// Feature Flags enabling experimental new features.
    ///
    /// Note the names of the enumerations must match the names of the feature flag configuration entries
    /// from the Configuration system.
    /// </summary>
    public enum FeatureFlags
    {
        UseExperimentalRepository,
        AllowGetForecastByDate
    }
}