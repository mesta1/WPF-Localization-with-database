using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace LocalizationSupport
{
    /// <summary>
    /// Provides a way to track the changes of the current culture and limit it to a set
    /// of supported cultures (existing localizations of the application).
    /// The current culture must only be set through this class if you want to use this library.
    /// </summary>
    public static class LocalizationManager
    {
        /// <summary>
        /// Occurs when the current culture changed.
        /// <see cref="CurrentCulture"/>
        /// </summary>
        public static EventHandler CultureChanged;

        /// <summary>
        /// Gets a reference to the resource provider that is currently being used
        /// for retrieving localized resources.
        /// </summary>
        public static ILocalizedResourceProvider LocalizationProvider { get; private set; }

        /// <summary>
        /// Collection of supported cultures (existing localizations of the application) 
        /// and the resource providers used for retrieving the localized values.
        /// </summary>
        public static IDictionary<CultureInfo, ILocalizedResourceProvider> SupportedCultures { get; private set; }

        /// <summary>
        /// Gets or sets the current culture.
        /// This property should be set to a valid value before the GetValue 
        /// method is called for the first time, otherwise it will return null to each call!
        /// </summary>
        /// <remarks>
        /// The Unload method is called on previous LocalizationProvider and the Load method
        /// is called on the new LocalizationProvider. If any of them causes an exception, it
        /// is propagated up through this method. Please refer to the documentatio
        /// of the localization providers you use in order to be able to handle them correctly.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Attempt to set the property to null.</exception>
        /// <exception cref="ArgumentException">
        /// Attempt to set the property to a culture that is not present in the <see cref="SupportedCultures"/>
        /// collection or has no provider assigned.
        /// </exception>
        public static CultureInfo CurrentCulture
        {
            get { return CultureInfo.CurrentUICulture; }
            set
            {
                if (!Thread.CurrentThread.CurrentUICulture.Equals(value) || LocalizationProvider == null)
                {
                    if (value == null)
                        throw new ArgumentNullException("value", "Current culture cannot be null.");

                    if (!IsSupported(value))
                        throw new ArgumentException(
                            String.Format("The culture {0} is not supported.", value.DisplayName), 
                            "value");

                    if (LocalizationProvider != null)
                        LocalizationProvider.Unload();

                    ILocalizedResourceProvider newProvider = null;
                    
                    if (SupportedCultures.TryGetValue(value, out newProvider) && newProvider != null)
                    {
                        Thread.CurrentThread.CurrentUICulture = value;

                        LocalizationProvider = newProvider;
                        LocalizationProvider.Load();

                        if (CultureChanged != null)
                            CultureChanged(null, EventArgs.Empty);
                    }
                    else
                        throw new ArgumentException(
                            String.Format("No provider has been provided for culture {0}.", value.DisplayName),
                            "value");
                }
            }
        }

        /// <summary>
        /// Checks if the given culture is present in the SupportedCultures collection.
        /// </summary>
        /// <param name="culture">Reference to the culture instance being checked</param>
        /// <returns>True if the culture is supported, false otherwise.</returns>
        private static bool IsSupported(CultureInfo culture)
        {   
            return SupportedCultures.ContainsKey(culture);
        }

        /// <summary>
        /// Gets a localized value for the specified resource key. 
        /// </summary>
        /// <param name="key">Resource key that identifies the requested value</param>
        /// <returns>
        /// Localized value identified by the key or null if the key does not exist 
        /// or the current localization provider is null (CurrentCulture was not yet set to a valid value).
        /// </returns>
        public static object GetValue(string key)
        {
            if (LocalizationProvider != null)
                return LocalizationProvider.GetValue(key);

            return null;
        }

        /// <summary>
        /// Initializes the manager.
        /// </summary>
        static LocalizationManager()
        {
            SupportedCultures = new Dictionary<CultureInfo, ILocalizedResourceProvider>();
        }
    }
}
