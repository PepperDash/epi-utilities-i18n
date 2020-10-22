using PepperDash.Essentials.Core;

namespace PepperDash.Utilities
{
	public class I18NUtilityJoinMap : JoinMapBaseAdvanced
	{
        [JoinName("CurrentLanguage")]
	    public JoinDataComplete CurrentLanguage = new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
	        new JoinMetadata
	        {
	            Description = "Select Current Language & feedback",
	            JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
	            JoinType = eJoinType.AnalogSerial
	        });

        [JoinName("SupportedLanguageEnable")]
	    public JoinDataComplete SupportedLanguageEnable =
	        new JoinDataComplete(new JoinData {JoinSpan = 150, JoinNumber = 11},
	            new JoinMetadata
	            {
	                Description = "Language is enabled",
	                JoinCapabilities = eJoinCapabilities.ToSIMPL,
	                JoinType = eJoinType.Digital
	            });

        [JoinName("SupportedLanguagesStart")]
	    public JoinDataComplete SupportedLanguagesStart =
	        new JoinDataComplete(new JoinData {JoinNumber = 11, JoinSpan = 150},
	            new JoinMetadata
	            {
	                Description = "Start for list of supported languages",
	                JoinCapabilities = eJoinCapabilities.ToSIMPL,
	                JoinType = eJoinType.Serial
	            });

        [JoinName("SupportedLanguagesDescriptionStart")]
        public JoinDataComplete SupportedLanguagesDescriptionStart =
            new JoinDataComplete(new JoinData { JoinNumber = 161, JoinSpan = 150 },
                new JoinMetadata
                {
                    Description = "Start for list of supported languages",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("UiLabelsStart")]
	    public JoinDataComplete UiLabelsStart = new JoinDataComplete(new JoinData {JoinNumber = 401, JoinSpan = 1024},
	        new JoinMetadata
	        {
	            Description = "Start Join for UI Labels",
	            JoinCapabilities = eJoinCapabilities.ToSIMPL,
	            JoinType = eJoinType.Serial
	        });

        [JoinName("SourceLabelsStart")]
        public JoinDataComplete SourceLabelsStart = new JoinDataComplete(new JoinData { JoinNumber = 1501, JoinSpan = 1024 },
            new JoinMetadata
            {
                Description = "Start Join for Source Labels",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("DestinationLabelsStart")]
        public JoinDataComplete DestinationLabelsStart = new JoinDataComplete(new JoinData { JoinNumber = 3001, JoinSpan = 1024 },
            new JoinMetadata
            {
                Description = "Start Join for Destination Labels",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		public I18NUtilityJoinMap(uint joinStart) 
            :base(joinStart, typeof(I18NUtilityJoinMap))
		{
		}
	}
}