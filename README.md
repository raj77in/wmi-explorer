# WMI Explorer with Exclusions

This repository provides a C# script for exploring Windows Management Instrumentation (WMI) classes and their properties across all namespaces. The script recursively navigates through WMI namespaces, retrieving properties of each class while applying an exclusion list to prevent excessive output from certain classes known for generating large data sets.

## Table of Contents
* [Overview](#overview)
* [Features](#features)
* [Usage](#usage)
* [Exclusions](#exclusions)
* [Examples](#examples)
* [Notes](#notes)
* [License](#license)

## Overview

The WMI Explorer script connects to the Windows Management Instrumentation (WMI) service and queries all classes within each namespace, retrieving instances and properties. This script is useful for investigating system information, configuration, and diagnostics by exploring a wide range of WMI classes, including those used in hardware, software, and system settings.

This script has an **exclusion list** that prevents classes with large outputs from being processed. The exclusion list was created based on known classes that tend to have extensive or repetitive outputs, which can significantly slow down or clutter the output.

## Features

* Recursively explores all WMI namespaces, starting from the `root` namespace.
* Retrieves properties for each WMI class that is not on the exclusion list.
* Displays the number of lines and time taken for each class, making it easier to identify heavy classes.
* Includes a function to format each class name as a header for clearer output.

## Usage

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/wmi-explorer.git
   cd wmi-explorer
   ```

2. **Build the Project**:
   Open the project in Visual Studio or another compatible C# editor, or use the .NET CLI to build:
   ```bash
   dotnet build
   ```

3. **Run the Program**:
   Run the compiled executable:
   ```bash
   dotnet run
   ```

### Output

For each WMI class found, the script will:
* Print a header with the class name.
* Display each property and its value, unless the class is in the exclusion list.
* Output the total lines printed and time taken for each class.

## Exclusions

The script has an **exclusion list** of classes known to produce large volumes of output. These classes are either repetitive or verbose, and including them would clutter the results and slow down processing.

The exclusion list includes partial matches (e.g., `PnP`, `Process`, `NetworkAdapterSetting`) to ensure that any WMI class containing these substrings is excluded from the output. This approach keeps the output manageable while still providing access to most classes.

To adjust the exclusion list, modify the `excludedC` list in the code:
```csharp
var excludedC = new List<string>
{
    "SecuritySettingOfLogicalFile", "COMApplicationSettings", "SystemProgramGroups",
    "PnPEntity", "LogicalDisk", "CacheMemory", "Volume", "Process", "SoftwareElement",
    // Add or remove exclusions here as needed
};
```

## Examples

### Sample Output

The output for each WMI class looks like this:

```
========================================
Class: Win32_BIOS
========================================
Properties:
  Name: Value
  Version: Value
  Manufacturer: Value

----------------------------------------
Lines printed for Win32_BIOS: 3
Time Taken for Win32_BIOS: 5 ms
```

### Exclusion Behavior
Classes with names like `PnPEntity`, `Process`, or `LogicalDisk` will be excluded from the results by default to avoid extensive output. 

## Notes

* **Permissions**: Some WMI classes or properties require administrative privileges. Run the program as Administrator to avoid access issues.
* **Performance**: Exclusions are applied to improve performance by skipping classes known to produce excessive output.
* **Direct Queries**: If you are looking for a specific property (e.g., `OA3xOriginalProductKey` from `SoftwareLicensingService`), use a direct query as the property may not be accessible with general queries.

