using System;
using System.Management;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // Start recursive search from the root namespace
        SearchNamespace("root");
    }

    static void SearchNamespace(string namespacePath)
    {
        try
        {
            // Create a ManagementScope and connect
            var scope = new ManagementScope($@"\\.\{namespacePath}");
            scope.Connect();

            // Search for classes in the current namespace
            var searcher = new ManagementObjectSearcher(scope, new WqlObjectQuery("SELECT * FROM meta_class"));

            foreach (ManagementClass wmiClass in searcher.Get())
            {
                var className = wmiClass["__CLASS"].ToString();

                // Exclusion list with partial names
                var excludedC = new List<string>
                {
                    "SecuritySettingOfLogicalFile", "COMApplicationSettings", "SystemProgramGroups",
                    "PnPEntity", "LogicalDisk", "CacheMemory", "Volume", "Process", "SoftwareElement",
                    "DataFile", "Directory", "ClassicCOMClassSettings", "ShortcutFile", "ShortcutAction",
                    "SubDirectory", "ProgramGroupContents", "SoftwareFeatureSoftwareElements",
                    "rectoryContainsFile", "ClassicCOMApplicationClasses", "VolumeQuota",
                    "VolumeQuotaSetting", "CDROMDrive", "StorageVolume", "PnPSignedDriver", "Service",
                    "SystemDriver", "Thread", "DCOMApplication", "SoftwareFeature", "ClassicCOMClass",
                    "DeviceMemoryAddress", "IRQResource", "PhysicalMedia", "QuotaSetting", "Property",
                    "Binary", "PNPAllocatedResource", "ClassicCOMClassSetting", "DiskDrivePhysicalMedia",
                    "AllocatedResource", "PnPDevice", "NetworkAdapterSetting", "SecuritySettingOfLogicalShare",
                    "SystemDevices", "VideoSettings", "ProcessExecutable", "NTLogEvent", "RegistryAction",
                    "ProductResource", "ActionCheck", "ImplementedCategory", "InstalledSoftwareElement",
                    "FileSpecification", "ReliabilityRecords", "DiskQuota", "ActionCheck", "EventLog",
                    "PnP", "Networking", "Printer", "SerialPort", "CreateFolderAction", "CheckCheck",
                    "MpThreatCatalog"
                };

                // Check if class name matches any exclusion pattern
                bool isExcluded = excludedC.Exists(partial => className.Contains(partial));
                if (isExcluded)
                {
                    continue;
                }

                // Start timer and line count
                var swatch = Stopwatch.StartNew();
                int lines = 0;
                PrintHeader($"{namespacePath}:{className}");
                Console.WriteLine("Properties:");

                try
                {
                    // Retrieve instances and properties
                    foreach (ManagementObject instance in wmiClass.GetInstances())
                    {
                        foreach (PropertyData property in instance.Properties)
                        {
                            // Display property
                            Console.WriteLine($"  {property.Name}: {property.Value}");
                            lines++;
                        }
                        Console.WriteLine(); // Separate instances for readability
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error accessing instances: {ex.Message}");
                }

                swatch.Stop();
                TimeSpan duration = swatch.Elapsed;

                Console.WriteLine(new string('-', 40)); // Separator between classes
                Console.WriteLine($"Lines printed for {className} : {lines}");
                Console.WriteLine($"Time Taken for {className} : {duration.TotalMilliseconds} ms");
            }

            // Recurse into sub-namespaces
            var nsSearcher = new ManagementObjectSearcher(scope, new WqlObjectQuery("SELECT * FROM __namespace"));
            foreach (ManagementObject ns in nsSearcher.Get())
            {
                var subNamespace = $"{namespacePath}\\{ns["Name"]}";
                SearchNamespace(subNamespace); // Recursive call
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in namespace '{namespacePath}': {ex.Message}");
        }
    }

    static void PrintHeader(string className)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(new string('=', 40)); // Separator between classes
        Console.WriteLine($"Class: {className}");
        Console.WriteLine(new string('=', 40));
    }
}
