using System.Collections.Generic;
using PepperDash_Essentials_Core.DeviceTypeInterfaces;

namespace PepperDash.Utilities
{
    public class I18NUtilityConfiguration
    {
        public string DefaultLocale { get; set; }
        public bool SortAlphabetically { get; set; }
    }

	public class I18NLanguagesConfiguration
	{
        public Dictionary<string, LanguageDefinition> LanguageDefinitions { get; set; }

	    public I18NLanguagesConfiguration()
	    {
	        LanguageDefinitions = new Dictionary<string, LanguageDefinition>();
            
	    }
	}

    public class LanguageDefinition:ILanguageDefinition
    {
        public string LocaleName { get; set; }
        public string FriendlyName { get; set; }
        public bool Enable { get; set; }
        public List<LanguageLabel> UiLabels { get; set; }
        public List<LanguageLabel> Sources { get; set; }
        public List<LanguageLabel> Destinations { get; set; }
        public List<LanguageLabel> SourceGroupNames { get; set; }
        public List<LanguageLabel> DestinationGroupNames { get; set; }
        public List<LanguageLabel> RoomNames { get; set; }
    }
}