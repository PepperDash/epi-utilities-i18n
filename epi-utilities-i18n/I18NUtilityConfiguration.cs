using System.Collections.Generic;

namespace PepperDash.Utilities
{
	public class I18NUtilityConfiguration
	{
         Dictionary<string, LanguageDefinition> LanguageDefinitions { get; set; } 
	}

    public class LanguageDefinition
    {
        public List<Label> UiLabels { get; set; }
        public List<Label> Sources { get; set; }
        public List<Label> Destinations { get; set; } 
    }

    public class Label
    {
        public string Description { get; set; }
        public string DisplayText { get; set; }
        public uint JoinNumber { get; set; }
    }
}