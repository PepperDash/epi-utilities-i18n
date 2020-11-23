using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Utilities
{
    public class I18NUtilityFactory : EssentialsPluginDeviceFactory<I18NUtility>
    {
        public I18NUtilityFactory()
        {
            // Set the minimum Essentials Framework Version
            MinimumEssentialsFrameworkVersion = "1.6.4";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            TypeNames = new List<string> { "languageutility", "i18nutility" };
        }

        // Builds and returns an instance of I18nUtility
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new device from type: {0}", dc.Type);

            var config = dc.Properties.ToObject<I18NUtilityConfiguration>();

            return new I18NUtility(dc.Key, dc.Name, config);
        }

    }
}