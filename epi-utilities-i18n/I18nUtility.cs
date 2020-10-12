// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes

using System;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Core;

namespace PepperDash.Utilities 
{
	public class I18NUtility : EssentialsBridgeableDevice
	{
	    private const string FileDirectory = @"\user\";
	    private const string FileName = @"*languages*.json";
	    private I18NUtilityConfiguration _config;

		public I18NUtility(string key, string name)
			: base(key, name)
		{
            Debug.Console(0, this, "Constructing new I18nUtility instance");
		    AddPreActivationAction(() =>
		    {
		        var fileName = FindLanguagesConfigurationFile();

		        if (String.IsNullOrEmpty(fileName))
		        {
		            return;
		        }

		        _config = LoadLanguagesConfiguration(fileName);

		        if (_config == null)
		        {
		            Debug.Console(0, this, Debug.ErrorLogLevel.Error,
		                "Languages not deserialized correctly. Please check configuration file");
		            return;
		        }
		    });
		}

	    private string FindLanguagesConfigurationFile()
	    {
	        var files = Directory.GetFiles(FileDirectory, FileName);

	        if (files.Length == 0)
	        {
	            Debug.Console(0,this,Debug.ErrorLogLevel.Error, "No Languages configuration file found. Please load languages configuration file to '\\user\\'");
	            return String.Empty;
	        }

	        if (files.Length <= 1)
	        {
	            return files[0];
	        }

	        Debug.Console(0, this, Debug.ErrorLogLevel.Error,
	            "Multiple language configuration files found. Please delete unneeded files from `\\user\\`");
	        return String.Empty;
	    }

	    private I18NUtilityConfiguration LoadLanguagesConfiguration(string fileName)
	    {
	        I18NUtilityConfiguration returnValue = null;
	        using (var reader = new JsonTextReader(File.OpenText(fileName)))
	        {
	            var jt = JToken.Load(reader);

	            try
	            {
	                returnValue = jt.ToObject<I18NUtilityConfiguration>();
	            }
	            catch (Exception ex)
	            {
	                Debug.Console(0, this, Debug.ErrorLogLevel.Error,
	                    "Error deserializing languages file: {0}\r\nStack Trace: {1}", ex.Message, ex.StackTrace);
	            }
	        }

	        return returnValue;
	    }

	    #region Overrides of EssentialsBridgeableDevice

	    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        var joinMap = new I18NUtilityJoinMap(joinStart);

	        // This adds the join map to the collection on the bridge
	        if (bridge != null)
	        {
	            bridge.AddJoinMap(Key, joinMap);
	        }

	        var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

	        if (customJoins != null)
	        {
	            joinMap.SetCustomJoinData(customJoins);
	        }

	        Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
	        Debug.Console(0, "Linking to Bridge Type {0}", GetType().Name);


	        trilist.OnlineStatusChange += (o, a) =>
	        {
	            if (a.DeviceOnLine)
	            {
	                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
	            }
	        };
	    }


	    #endregion
	}
}

