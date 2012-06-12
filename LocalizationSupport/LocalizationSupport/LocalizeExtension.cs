using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LocalizationSupport
{
    /// <summary>
    /// The localized markup extension returns a localized 
    /// resource for a specified resource key.
    /// </summary>
    /// <remarks>
    /// If the target object and property are dependency object (property), this extension
    /// can update the target property when a culture is changed during runtime.
    /// Thus, the language of the application can be changed without the application restart.
    /// However, if this condition is not met, this functionality is unavailable and you should
    /// use <see cref="ReflectedLocalizeExtension"/> class instead.
    /// </remarks>
    [MarkupExtensionReturnType(typeof(string)), Localizability(LocalizationCategory.NeverLocalize)]
    public class LocalizeExtension : MarkupExtension
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizeExtension"/> class. 
        /// </summary>
        public LocalizeExtension()
        {
            LocalizationManager.CultureChanged += new EventHandler(LocalizationManager_CultureChanged);
        }

        /// <summary>
        /// Iniitializes a new instance of the <see cref="LocalizeExtension"/> class.
        /// </summary>
        /// <param name="key">Resource key identifying the localized resource</param>
        public LocalizeExtension(String key) : this()
        {
            Key = key;
        }

        #endregion

     /*   #region Destructor

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup before <see cref="LocalizeExtension"/>
        /// class is reclaimed by garbage collection.
        /// </summary>
        ~LocalizeExtension()
        {
            LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
        }

        #endregion*/

        #region Properties

        /// <summary>
        /// Gets or sets the resource key.
        /// </summary>
        /// <value>The key that identifies the localized resource value</value>
        [ConstructorArgument("key")]
        public String Key { get; set; }

        /// <summary>
        /// Gets or sets a format string that is used to format the value.
        /// </summary>
        /// <value>The format string</value>
        public String Format { get; set; }

        /// <summary>
        /// Gets or sets the default value that is used when the key was not found
        /// or the localized value is null.
        /// </summary>
        /// <value>The default value</value>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the value converter that is used to convert the value.
        /// </summary>
        /// <value>An IValueConverter instance</value>
        public IValueConverter Converter { get; set; }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the CultureChanged event of the LocalizationManager.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">A <see cref="System.EventArgs"/> instance containing the event data</param>
        private void LocalizationManager_CultureChanged(object sender, EventArgs e)
        {
            UpdateTarget();
        }

        #endregion

        #region Base Type Overrides

        /// <summary>
        /// Returns the localized value.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension</param>
        /// <returns>Object value to set on the property where the extension is applied</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Resolve the depending object and property
            if (targetType == null)
            {
                var targetHelper = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
                targetObject = targetHelper.TargetObject as DependencyObject;
                targetProperty = targetHelper.TargetProperty as DependencyProperty;
                if (targetProperty != null)
                {
                    targetType = targetProperty.PropertyType;
                    typeConverter = TypeDescriptor.GetConverter(targetType);
                }
                else if (targetHelper.TargetProperty != null)
                {
                    targetType = targetHelper.TargetProperty.GetType();
                    typeConverter = TypeDescriptor.GetConverter(targetType);
                }
            }

            return ProvideValueInternal();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Internal implementation of ProvideValue.
        /// </summary>
        /// <returns>The localized value</returns>
        protected object ProvideValueInternal()
        {
            // Get the localized value.
            object value = LocalizationManager.GetValue(Key);

            // Automatically convert the type if a matching converter is available.
            if (value != null && typeConverter != null && typeConverter.CanConvertFrom(value.GetType()))
                value = typeConverter.ConvertFrom(value);

            // If the value is null and we have a default value, use it.
            if (value == null && DefaultValue != null)
                value = DefaultValue;

            // If we have no fallback value, return the key.
            if (value == null)
            {
                if (targetType == typeof(string))
                    value = String.Concat("?", Key, "?");
                else if (targetProperty != null)
                    return DependencyProperty.UnsetValue;
                else
                    return null;
            }

            // If we have converter, use it
            if (Converter != null)
                value = Converter.Convert(
                    value, targetType, null, LocalizationManager.CurrentCulture);

            // Format the value if it is formattable and a format is provided.
            if (Format != null && value is IFormattable)
                ((IFormattable)value).ToString(Format, LocalizationManager.CurrentCulture);

            return value;
        }

        /// <summary>
        /// Updates the target.
        /// </summary>
        protected virtual void UpdateTarget()
        {
            if (targetObject != null && targetProperty != null)
            {
                targetObject.SetValue(targetProperty, ProvideValueInternal());
            }
        }
        
        #endregion

        #region Fields

        /// <summary>
        /// Caches the target object.
        /// </summary>
        protected DependencyObject targetObject;
        /// <summary>
        /// Caches the target property.
        /// </summary>
        protected DependencyProperty targetProperty;
        /// <summary>
        /// Caches the resolved default type converter.
        /// </summary>
        protected TypeConverter typeConverter;
        /// <summary>
        /// Caches the type of the target property.
        /// </summary>
        protected Type targetType;
        
        #endregion
    }
}
