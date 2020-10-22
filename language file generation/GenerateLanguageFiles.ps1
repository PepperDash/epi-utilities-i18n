    <#
        .SYNOPSIS
        Convert an Excel file to the correct format for Essentials to read for the indicated series
        .PARAMETER InputFile
        File to convert
        .PARAMETER OutputFilePrefix
        Prefix to attach to files. Files will be named as "OutputFilePrefix_LanguageLocale_languagesOutputFilesSuffix.json"
        .PARAMETER OutputFileSuffix
        Suffix to attach to files. Files will be named as "OutputFilePrefix_LanguageLocale_languagesOutputFilesSuffix.json"
        .PARAMETER Compress
        Remove all whitespace in output file    
    #>
    [CmdletBinding()]
    Param(
        [Parameter(ValueFromPipeline=$true,Mandatory=$true)]
        [Object]$InputFile,

        [Parameter(Mandatory=$false)]
        [string]$OutputFilePrefix,

        [Parameter(Mandatory=$false)]
        [string]$OutputFileSuffix,

        [Parameter(Mandatory=$false)]
        [switch]$Compress
    )

function Convert-ExcelSheetToObject
{
    <#
    .SYNOPSIS
    Converts an Excel sheet from a workbook to JSON
    .DESCRIPTION
    To allow for parsing of Excel Workbooks suitably in PowerShell, this script converts a sheet from a spreadsheet into a JSON file of the same structure as the sheet.
    .PARAMETER SheetName
    The sheet to convert to JSON
    #>
    [CmdletBinding()]
    Param(
    [Parameter(
        ValueFromPipeline=$true,
        Mandatory=$true
        )]
    [Object]$InputSheet
    )

    #region prep



    #endregion prep


    # Grab the sheet to work with
    $theSheet = $InputSheet

    #region headers
    # Get the row of headers
    $Headers = @{}
    $NumberOfColumns = 0
    $FoundHeaderValue = $true
    while ($FoundHeaderValue -eq $true) {
        $cellValue = $theSheet.Cells.Item(1, $NumberOfColumns+1).Text
        if ($cellValue.Trim().Length -eq 0) {
            $FoundHeaderValue = $false
        } else {
            $NumberOfColumns++
            $Headers.$NumberOfColumns = $cellValue
        }
    }
    #endregion headers

    # Count the number of rows in use, ignore the header row
    $rowsToIterate = $theSheet.UsedRange.Rows.Count

    #region rows
    $results = @()
    foreach ($rowNumber in 2..$rowsToIterate+1) {
        if ($rowNumber -gt 1) {
            $result = @{}
            foreach ($columnNumber in $Headers.GetEnumerator()) {
                $ColumnName = $columnNumber.Value
                $CellValue = $theSheet.Cells.Item($rowNumber, $columnNumber.Name).Value2
                $result.Add($ColumnName,$cellValue)
            }
            $results += $result
        }
    }
    #endregion rows

    $results

    # Close the Workbook

}

if($InputFile -is "System.IO.FileSystemInfo"){
    $InputFile = $InputFile.FullName.ToString()
}

$InputFile = [System.IO.Path]::GetFullPath($InputFile)
Write-Verbose "Converting '$InputFile' to JSON"

$metadataSheetName = "metadata"

$excelApplication = New-Object -ComObject Excel.Application
$excelApplication.DisplayAlerts = $false
$Workbook = $excelApplication.Workbooks.Open($InputFile)

$metadataSheet = $Workbook.Sheets | Where-Object {$_.Name -eq $metadataSheetName}

if($null -eq $metadataSheet) {
    $excelApplication.Workbooks.Close()
    # Close Excel
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($excelApplication)
    throw "Unable to find metadata sheet"
}

Write-Verbose "Getting Metadata for languages"
$metaDataHeaders = @{}
$metadataNumberOfColumns = 0
$FoundMetaDataHeaderValue = $true
while( $FoundMetaDataHeaderValue -eq $true) {
    $cellValue = $metadataSheet.Cells.Item(1, $metadataNumberOfColumns + 1).Text
    if($cellValue.Trim().Length -eq 0) {
        $FoundMetaDataHeaderValue = $false
    } else {
        $metadataNumberOfColumns++
        $metaDataHeaders.$metadataNumberOfColumns = $cellValue
    }
}

$rowsToIterate = $metadataSheet.UsedRange.Rows.Count

$languagesDict = @{}

foreach($rowNumber in 2..$rowsToIterate+1){
    if($rowNumber -gt 1) {
        $result = @{}
        foreach($columnNumber in $metaDataHeaders.GetEnumerator()) {
            $ColumnName = $columnNumber.Value
            $CellValue = $metadataSheet.Cells.Item($rowNumber, $columnNumber.Name).Value2
            $result.add($ColumnName, $cellValue)
        }
        Write-Verbose "Adding language $($result.localeName)"
        $languagesDict.Add($result.localeName, $result)
    }
}

foreach($languageLocale in $languagesDict.keys) {
    $language = $languagesDict.Item($languageLocale)

    $languageSheets = $Workbook.Sheets | Where-Object {$_.Name -like "$($languageLocale)*"}

    Write-Verbose "Getting text values for language $($languageLocale)"
    foreach($sheet in $languageSheets){ 
        $pos = $sheet.Name.IndexOf(">")
        $propertyName = $sheet.Name.Substring($pos+1)

        $results = Convert-ExcelSheetToObject -InputSheet $sheet
        
        $language.Add($propertyName, $results)
    }

    $output = @{}

    $output.Add($languageLocale, $language)

    $filename = "$($OutputFilePrefix)_$($languageLocale)_languages$($OutputFileSuffix).json"

    Write-Verbose "Outputting file for language $($languageLocale) to $($filename)"

    if($Compress.IsPresent){
        ConvertTo-Json -InputObject $output -Depth 5 -Compress | Out-File $filename
    } else {
        ConvertTo-Json -InputObject $output -Depth 5 | Out-File $filename
    }

    $output | ConvertTo-Json -Depth 5 -Compress:$Compress | Out-File $filename
}

$excelApplication.Workbooks.Close()
# Close Excel
[void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($excelApplication)

