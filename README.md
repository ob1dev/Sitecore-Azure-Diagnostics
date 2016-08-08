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
