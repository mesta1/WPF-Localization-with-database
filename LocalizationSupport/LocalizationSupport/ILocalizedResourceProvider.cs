namespace LocalizationSupport
{
    /// <summary>
    /// An interface a class must provide to act as a localization provider.
    /// </summary>
    public interface ILocalizedResourceProvider
    {
        /// <summary>
        /// Gets a localized value for the specified resource key.
        /// </summary>
        /// <param name="key">Resource key that identifies the requested value</param>
        /// <returns>Localized value identified by the key or null if the key does not exist.</returns>
        object GetValue(string key);

        /// <summary>
        /// Loads the resources used by the localization provider.
        /// This method has to be called before a very first call to GetValue
        /// or before a first call to GetValue following a call to Unload method.
        /// </summary>
        void Load();

        /// <summary>
        /// Unloads the resources used by the localization provider.
        /// </summary>
        void Unload();
    }
}
