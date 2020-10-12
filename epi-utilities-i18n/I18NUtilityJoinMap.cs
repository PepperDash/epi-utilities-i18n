using PepperDash.Essentials.Core;

namespace PepperDash.Utilities
{
	public class I18NUtilityJoinMap : JoinMapBaseAdvanced
	{
	    public JoinDataComplete DeviceName = new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
	        new JoinMetadata
	        {
	            Description = "Device Name",
	            JoinCapabilities = eJoinCapabilities.ToSIMPL,
	            JoinType = eJoinType.Serial
	        });

		public I18NUtilityJoinMap(uint joinStart) 
            :base(joinStart)
		{
		}
	}
}