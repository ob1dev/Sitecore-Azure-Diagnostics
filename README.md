# Sitecore Azure Diagnostics

The module outputs Sitecore Log statements to [Microsoft Azure Storage](https://azure.microsoft.com/en-us/services/storage/) service using the Append Blobs.

[![NuGet version](https://img.shields.io/nuget/v/Sitecore.Azure.Diagnostics.svg)](https://www.nuget.org/packages/Sitecore.Azure.Diagnostics/)

[![Build status](https://ci.appveyor.com/api/projects/status/lcxyk2jftuie4w68?svg=true)](https://ci.appveyor.com/project/olegburov/sitecore-azure-diagnostics)

## Features

+ Append regular, WebDAV, Search, Crawling and Publishing Sitecore diagnostics
+ Based on [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/)
+ Append Blobs are used to store diagnostics as a text data
+ Text data is compatible with [Sitecore Log Analyzer](https://marketplace.sitecore.net/Modules/Sitecore_Log_Analyzer.aspx) tool
+ Support multiple Web Apps instances
+ Clean up out-of-date blobs using Sitecore Agent
+ Support Sitecore Log Viewer application to open, download and deleted blobs.

## Requirements

Sitecore XP 8.1 rev. 160519 (8.1 Update-3) or newer

## License  
  
This patch is licensed under the [MIT License](LICENSE).

## Download  
  
Downloads are available via [GitHub Releases](https://github.com/olegburov/Sitecore-Azure-Diagnostics/releases).  

## Instructions

The recommended approach to install Sitecore Azure Diagnostics extension is as follows:

1. In Visual Studio, create an empty project using the `ASP.NET Web Application (.NET Framework)` template.

2. Copy the generated files on top of Sitecore 8.1 installation. 

   > For example:
   > + `\sc81u3\Sitecore.sln`
   > + `\sc81u3\Website\Properties`
   > + `\sc81u3\Website\packages.config`
   > + `\sc81u3\Website\Sitecore.csproj`
   > + `\sc81u3\Website\Sitecore.csproj.user`
   > + `\sc81u3\Website\Web.Debug.config`
   > + `\sc81u3\Website\Web.Release.config`
  
3. Include all Sitecore's files in the project.

4. Install the NuGet package [Sitecore.Azure.Diagnostics](https://www.nuget.org/packages/Sitecore.Azure.Diagnostics/):

   ```PowerShell
   Install-Package Sitecore.Azure.Diagnostics
   ```
   
   > **Note:** The `Sitecore.Azure.Diagnostics` package depends on the Windows Azure Storage package, which will be installed automatically.
   
5. Modify both files the `Web.Debug.config` and `Web.Release.config`. Under the element `\configuration\appSettings`, replace the `{account-name}` with the name of your storage account, and the `{account-key}` with your account access key:

   ```XML
   <configuration>
   ...
     <appSettings>
     ...
       <add key="StorageConnectionString" value="DefaultEndpointsProtocol=https;AccountName={account-name};AccountKey={account-key}" />
     </appSettings>
   ...
   </configuration>
   ```

6. Now Sitecore instance is ready to log diagnostics to an Azure Blob Storage.