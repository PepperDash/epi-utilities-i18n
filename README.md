# Internationalization Essentials Plugin (c) 2021

## Overview

This plugin allows for configuration of languages using configuration files. The languages/translations are available to SIMPL Windows via an EISC Bridge, as well as internally to Essentials.

## Configuration

```json
{
  "key": "i18n",
  "uid": 18,
  "name": "I18N",
  "group": "utility",
  "type": "languageutility",
  "properties": {
    "defaultLocale": "en-us"
  }
}
```

## Join Map

```
Digitals:
Join Number: 11   | JoinSpan: '150'  | Description: 'Language is enabled'                   | Type: 'Digital'      | Capabilities: 'ToSIMPL'
Analogs:
Join Number: 1    | JoinSpan: '1'    | Description: 'Select Current Language & feedback'    | Type: 'AnalogSerial' | Capabilities: 'ToFromSIMPL'
Serials:
Join Number: 1    | JoinSpan: '1'    | Description: 'Select Current Language & feedback'    | Type: 'AnalogSerial' | Capabilities: 'ToFromSIMPL'
Join Number: 11   | JoinSpan: '150'  | Description: 'Start for list of supported languages' | Type: 'Serial'       | Capabilities: 'ToSIMPL'
Join Number: 161  | JoinSpan: '150'  | Description: 'Start for list of supported languages' | Type: 'Serial'       | Capabilities: 'ToSIMPL'
Join Number: 401  | JoinSpan: '1024' | Description: 'Start Join for UI Labels'              | Type: 'Serial'       | Capabilities: 'ToSIMPL'
Join Number: 1501 | JoinSpan: '1024' | Description: 'Start Join for Source Labels'          | Type: 'Serial'       | Capabilities: 'ToSIMPL'
Join Number: 3001 | JoinSpan: '1024' | Description: 'Start Join for Destination Labels'     | Type: 'Serial'       | Capabilities: 'ToSIMPL'

```

## Language Configuration

Currently, language configuration is done using the spreadsheet and Powershell script located in the `language file generation` folder.

### Editing the Excel file

#### Metadata

The `localeName` value for a given language should match standard locale names. Locale names can be found in t[this github repo](https://github.com/ladjs/i18n-locales). The `friendlyName` value can be displayed on a UI to allow for easier language selection. Setting the `enable` value true for a given language will allow that language to be used. All 3 of these values will be bridged to SIMPL via EISC.

#### Language tab

Each language should have a corresponding tab named as `{localeName}>uiLabels`, as in the example file. The PS script looks for these tabs to generate the language configuration files

The `Join` value for an entry corresponds to the Join Number on the Touchpanel where the value will be displayed. This makes it easy to translate the VT-Pro/CH5 file to the configuration files. The `key` value should be unique for each entry in the list. The `joinNumber` column can be set using an offset from the `Join` value in a formula, or starting at 1 and incrementing. The `joinNumber` value is used to determine the placement of the text value on the bridge. The `description` value is not shown on the UI anywhere, and is used to make configuration easier, especially when using languages that don't use the western alphabet. The `displayText` value is the string that will be shown when this language is selected. UTF-16 is supported, allowing for languages such as Japanese to be displayed correctly.

#### Sections

Each language can have sections for `uiLabels`, `sources`, and `destinations`.

### Generating the language files

To generate the langauge configuration files, enter the following command in a terminal or powershell window

`.\GenerateLanguageFiles.ps1 -InputFile 'C:\Users\awelker\Documents\PDEngineering\epi-utilities-i18n\language file generation\languageDefinitions.xlsx'`

#### Optional flags

`-OutputFilePrefix` - Add a prefix to the files output by the script
`-OutputFileSuffix` - Add a suffix to the files output by the script
`-Compress` - Generate the files with no whitespace. This will generate smaller files.

#### Uploading Files

The files can be uploaded to either the `user` folder or the `user\languages` folder.

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They are referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

To install dependencies once nuget.exe is installed, run the following command from the root directory of your repository:
`nuget install .\packages.config -OutputDirectory .\packages -excludeVersion`.
To verify that the packages installed correctly, open the plugin solution in your repo and make sure that all references are found, then try and build it.

### Installing Different versions of PepperDash Core

If you need a different version of PepperDash Core, use the command `nuget install .\packages.config -OutputDirectory .\packages -excludeVersion -Version {versionToGet}`. Omitting the `-Version` option will pull the version indicated in the packages.config file.

## License

Provided under [MIT License](LICENSE.md)
