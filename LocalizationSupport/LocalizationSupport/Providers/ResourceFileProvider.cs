using System.Globalization;
using System.Reflection;
using System.Resources;

namespace LocalizationSupport.Providers
{
    public class ResourceFileProvider : DatabaseResourceManager, ILocalizedResourceProvider
    {
        #region Constructors

        public ResourceFileProvider(string connectionString)
            : base(connectionString)
        {
        
        }

        #endregion

        #region ILocalizedResourceProvider Members

        public object GetValue(string key)
        {
            if (resourceSet != null)
            {
                try
                {
                    var debug =  resourceSet.GetObject(key);
                    return debug;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public void Load()
        {
            resourceSet = GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        }

        public void Unload()
        {
            //ReleaseAllResources();
        }

        #endregion

        #region Fields

        private ResourceSet resourceSet;

        #endregion
    }
}
