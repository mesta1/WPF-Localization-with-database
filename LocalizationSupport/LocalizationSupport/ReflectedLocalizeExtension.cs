using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace LocalizationSupport
{
    /// <summary>
    /// The reflected localize markup extension simulates the funcitonality
    /// of the localize markup extension for non-dependency objects / properties.
    /// </summary>
    [MarkupExtensionReturnType(typeof(string)), Localizability(LocalizationCategory.NeverLocalize)]
    public class ReflectedLocalizeExtension : LocalizeExtension
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedLocalizeExtension"/> class.
        /// </summary>
        /// <param name="key">Resource key identifying the localized resource</param>
        /// <param name="propertyName">Name of the target property (for reflection)</param>
        public ReflectedLocalizeExtension(string key, string propertyName) : base(key)
        {
            PropertyName = propertyName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the target property.
        /// The name is used for updating the target via reflection.
        /// </summary>
        [ConstructorArgument("propertyName")]
        public String PropertyName
        {
            get { return propertyName; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "The PropertyName cannot be null.");

                propertyName = value;
            }
        }

        #endregion

        #region Base Type Overrides

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Resolve the target object and property
            if (targetObject == null && targetObjectND == null)
            {
                var targetHelper = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
                targetObject = targetHelper.TargetObject as DependencyObject;

                if (targetObject == null)
                    targetObjectND = targetHelper.TargetObject;

                targetProperty = targetHelper.TargetProperty as DependencyProperty;
                if (targetProperty != null)
                {
                    targetType = targetProperty.PropertyType;
                    typeConverter = TypeDescriptor.GetConverter(targetType);
                }
                else if (targetHelper.TargetProperty != null)
                {
                    Type targetObjectType = targetObjectND.GetType();
                    try
                    {
                        PropertyInfo property = targetObjectType.GetProperty(PropertyName);
                        if (property == null)
                            throw new ArgumentException("PropertyName", "Wrong property name!");
                        targetType = property.PropertyType;
                    }
                    catch (AmbiguousMatchException)
                    {
                        targetType = targetHelper.TargetProperty.GetType();
                    }

                    typeConverter = TypeDescriptor.GetConverter(targetType);
                }
            }

            return ProvideValueInternal();
        }

        #endregion

        #region Protected Methods

        protected override void UpdateTarget()
        {
            if (targetObject != null)
                base.UpdateTarget();
            else if (targetObjectND != null)
            {
                targetObjectND.GetType().InvokeMember(
                    PropertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | 
                    BindingFlags.SetProperty,
                    null,
                    targetObjectND,
                    new Object[] { ProvideValueInternal() });
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Caches the reference to the target object.
        /// </summary>
        protected object targetObjectND;
        /// <summary>
        /// Holds the name of the target property.
        /// </summary>
        protected string propertyName;

        #endregion
    }
}
