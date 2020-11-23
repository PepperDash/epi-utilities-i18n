using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash_Essentials_Core.DeviceTypeInterfaces;

namespace PepperDash.Utilities
{
    public class I18NUtility : EssentialsBridgeableDevice, ILanguageProvider
    {
        private const string FileDirectory = @"\user\";
        private const string FileName = @"*languages*.json";

        private readonly I18NUtilityConfiguration _config;

        public IntFeedback CurrentLanguageIntFeedback { get; private set; }
        public StringFeedback CurrentLanguageFeedback { get; private set; }
        private string _currentLanguageString;
        private int _currentLanguageInt;
        private I18NLanguagesConfiguration _languagesConfig;

        public I18NUtility(string key, string name, I18NUtilityConfiguration config)
            : base(key, name)
        {
            _config = config;

            CurrentLanguageFeedback = new StringFeedback(() => CurrentLanguageString);
            CurrentLanguageIntFeedback = new IntFeedback(() => CurrentLanguageIndex);

            CurrentLanguageString = _config.DefaultLocale;

            AddPreActivationAction(() =>
            {
                var fileNames = FindLanguagesConfigurationFiles();

                if (fileNames == null)
                {
                    return;
                }

                _languagesConfig = LoadLanguagesConfiguration(fileNames);

                if (_languagesConfig != null)
                {
                    return;
                }

                Debug.Console(0, this, Debug.ErrorLogLevel.Error,
                    "Languages not deserialized correctly. Please check configuration file");
            });
        }

        public string CurrentLanguageString
        {
            get { return _currentLanguageString; }
            set
            {
                if (value == _currentLanguageString)
                {
                    return;
                }

                _currentLanguageString = value;
                CurrentLanguageFeedback.FireUpdate();
            }
        }

        public int CurrentLanguageIndex
        {
            get { return _currentLanguageInt; }
            set
            {
                if (value == _currentLanguageInt)
                {
                    return;
                }

                _currentLanguageInt = value;

                CurrentLanguageIntFeedback.FireUpdate();
            }
        }

        private IEnumerable<string> FindLanguagesConfigurationFiles()
        {
            var files = Directory.GetFiles(FileDirectory, FileName);

            if (files.Length != 0)
            {
                return files;
            }
            Debug.Console(0, this, Debug.ErrorLogLevel.Error,
                "No Languages configuration file found. Please load languages configuration file to '\\user\\'");
            return null;
        }

        private I18NLanguagesConfiguration LoadLanguagesConfiguration(IEnumerable<string> fileNames)
        {
            var returnValue = new I18NLanguagesConfiguration();

            foreach (var fileName in fileNames)
            {
                Debug.Console(1, this, "Loading language definition from file {0}", fileName);
                using (var reader = new JsonTextReader(File.OpenText(fileName)))
                {
                    var jt = JToken.Load(reader);

                    try
                    {
                        JsonConvert.PopulateObject(jt.ToString(), returnValue.LanguageDefinitions);
                    }
                    catch (Exception ex)
                    {
                        Debug.Console(0, this, "Error deserializing languages file: {0}", ex.Message);
                        Debug.Console(0, this, "Stack Trace: {0}", ex.StackTrace);
                    }
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

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            //Set actions & Feedback for languages
            trilist.SetUShortSigAction(joinMap.CurrentLanguage.JoinNumber, i =>
            {
                if (i <= 0) return;

                CurrentLanguageIndex = i;

                CurrentLanguageString = _languagesConfig.LanguageDefinitions.Keys.ToList()[i - 1];

                CurrentLanguage = GetLanguageDefinitionByName(CurrentLanguageString);

                SetLanguageStrings(trilist, joinMap);
            });

            trilist.SetStringSigAction(joinMap.CurrentLanguage.JoinNumber, s =>
            {
                if (String.IsNullOrEmpty(s))
                {
                    return;
                }
                CurrentLanguageString = s;

                try
                {
                    CurrentLanguageIndex =
                        _languagesConfig.LanguageDefinitions.Keys.ToList().IndexOf(CurrentLanguageString);
                }
                catch (Exception ex)
                {
                    CurrentLanguageIndex = 0;
                    Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to get index for language {0}\r\n{1}",
                        CurrentLanguageString, ex.Message);
                }

                CurrentLanguage = GetLanguageDefinitionByName(CurrentLanguageString);

                SetLanguageStrings(trilist, joinMap);
            });

            CurrentLanguageFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentLanguage.JoinNumber]);
            CurrentLanguageIntFeedback.LinkInputSig(trilist.UShortInput[joinMap.CurrentLanguage.JoinNumber]);

            SetLanguageInfo(trilist, joinMap);

            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine)
                {
                    return;
                }

                SetLanguageInfo(trilist, joinMap);
                CurrentLanguageFeedback.FireUpdate();
                SetLanguageStrings(trilist, joinMap);
            };
        }

        private LanguageDefinition GetLanguageDefinitionByIndex(int index)
        {
            var languageNames = _languagesConfig.LanguageDefinitions.Keys.ToList();

            if (index - 1 > languageNames.Count)
            {
                Debug.Console(0, this, "Index out of range");
                return null;
            }

            var localeName = languageNames[index];

            return GetLanguageDefinitionByName(localeName);
        }

        private LanguageDefinition GetLanguageDefinitionByName(string localeName)
        {
            LanguageDefinition rv;

            if (_languagesConfig.LanguageDefinitions.TryGetValue(localeName, out rv))
            {
                return rv;
            }

            Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to find language definition for locale {0}",
                localeName);
            return null;
        }

        private void SetLanguageInfo(BasicTriList trilist, I18NUtilityJoinMap joinMap)
        {
            if (_languagesConfig == null)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "No Language configs loaded.");
                return;
            }
            Debug.Console(1, this, "Setting supported language information");
            ushort i = 0;

            if (_languagesConfig.LanguageDefinitions.Count == 0) return;

            foreach (var lang in _languagesConfig.LanguageDefinitions.Select(language => language.Value))
            {
                Debug.Console(1, this, "Setting supported language information for locale {0}", lang.LocaleName);

                trilist.SetBool(joinMap.SupportedLanguageEnable.JoinNumber + i, lang.Enable);
                trilist.SetString(joinMap.SupportedLanguagesStart.JoinNumber + i, lang.LocaleName);
                trilist.SetString(joinMap.SupportedLanguagesDescriptionStart.JoinNumber + i, lang.FriendlyName);

                i++;
            }
        }

        private void SetLanguageStrings(BasicTriList trilist, I18NUtilityJoinMap joinMap)
        {
            LanguageDefinition languageDefinition;

            if (!_languagesConfig.LanguageDefinitions.TryGetValue(_currentLanguageString, out languageDefinition))
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning,
                    "Language definition for selected locale {0} not found",
                    _currentLanguageString);
                return;
            }

            if (languageDefinition.UiLabels != null)
            {
                foreach (var label in languageDefinition.UiLabels)
                {

                    Debug.Console(2, this, "Setting join {0} to {1}",
                        (joinMap.UiLabelsStart.JoinNumber + label.JoinNumber) - 1,
                        label.DisplayText);

                    var stringInputSig =
                        trilist.StringInput[joinMap.UiLabelsStart.JoinNumber + label.JoinNumber - 1];

                    stringInputSig.StringEncoding = eStringEncoding.eEncodingUTF16;

                    trilist.SetString((joinMap.UiLabelsStart.JoinNumber + label.JoinNumber) - 1, label.DisplayText);
                }
            }

            if (languageDefinition.Sources != null)
            {
                foreach (var label in languageDefinition.Sources)
                {
                    var langLabel = label as LanguageLabel;

                    if (langLabel == null)
                    {
                        continue;
                    }

                    Debug.Console(2, this, "Setting join {0} to {1}",
                        (joinMap.SourceLabelsStart.JoinNumber + langLabel.JoinNumber) - 1,
                        label.DisplayText);

                    var stringInputSig =
                        trilist.StringInput[joinMap.SourceLabelsStart.JoinNumber + langLabel.JoinNumber - 1];

                    stringInputSig.StringEncoding = eStringEncoding.eEncodingUTF16;

                    trilist.SetString((joinMap.SourceLabelsStart.JoinNumber + langLabel.JoinNumber) - 1,
                        label.DisplayText);
                }
            }

            if (languageDefinition.Destinations == null)
            {
                return;
            }
            foreach (var label in languageDefinition.Destinations)
            {
                var langLabel = label as LanguageLabel;

                if (langLabel == null)
                {
                    continue;
                }

                Debug.Console(2, this, "Setting join {0} to {1}",
                    (joinMap.DestinationLabelsStart.JoinNumber + langLabel.JoinNumber) - 1,
                    label.DisplayText);

                var stringInputSig =
                    trilist.StringInput[joinMap.DestinationLabelsStart.JoinNumber + langLabel.JoinNumber - 1];

                stringInputSig.StringEncoding = eStringEncoding.eEncodingUTF16;

                trilist.SetString((joinMap.DestinationLabelsStart.JoinNumber + langLabel.JoinNumber) - 1,
                    label.DisplayText);
            }
        }

        #endregion

        #region Implementation of ILanguageProvider

        private ILanguageDefinition _currentLanguage;

        public ILanguageDefinition CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {
                if (value == _currentLanguage)
                {
                    return;
                }

                _currentLanguage = value;

                var handler = CurrentLanguageChanged;

                if (handler == null)
                {
                    return;
                }

                handler(this, new EventArgs());
            }
        }

        public event EventHandler CurrentLanguageChanged;

        #endregion
    }
}