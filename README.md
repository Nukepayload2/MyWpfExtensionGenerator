# MyWpfExtensionGenerator
VB "WPF My extension" generator.
Generates Visual Basic's My Namespace extension for Windows Presentation Foundation.
The generator writes the WPF specific part of My extension by default. For generating all members, please enable Windows Forms (Edit project file, add `<UseWindowsForms>true</UseWindowsForms>` to `<PropertyGroup>`), then reload the project.

https://www.nuget.org/packages/Nukepayload2.SourceGenerators.MyWpfExtension/

## Requirements
### Current version
Visual Studio 17.8 or later

### Version 1.x
Visual Studio 16.9 or later

## Features
- Generate My.Application
- Generate My.Windows
- Generate My.Computer (Windows Forms required)
- Generate My.User (Windows Forms required)
- Generate My.Log (Windows Forms required)
- Generate My.Application.Info (Windows Forms required)

## Project structure
https://github.com/Nukepayload2/MyWpfExtensionGenerator

### Nukepayload2.SourceGenerators.MyWpfExtension
The source generator project. Build this project before using the sample project.
### SampleNetWpfAndWinforms 
Sample project of WPF and Windows Forms. Requires to reopen Visual Studio after building the source generator project.
### SampleNetWpfOnly
Sample project of WPF. Requires to reopen Visual Studio after building the source generator project.

## Usage
### Package reference
- Install Nukepayload2.SourceGenerators.MyWpfExtension
- Version 1.x only: If you are using .NET 5 or earlier, edit your project file. In the `<ProjectReference>` element, add ` OutputItemType="Analyzer" ReferenceOutputAssembly="False"` . Add `<IncludePackageReferencesDuringMarkupCompilation>true</IncludePackageReferencesDuringMarkupCompilation>` to `<PropertyGroup>` element.

### Project reference
- Add the source generator project to your solution.
- Build the source generator project.
- Select your WPF project, and add project reference to the source generator.
- Version 1.x only: Edit your project file. In the `<ProjectReference>` element, add ` OutputItemType="Analyzer" ReferenceOutputAssembly="False"` . Add `<IncludePackageReferencesDuringMarkupCompilation>true</IncludePackageReferencesDuringMarkupCompilation>` to `<PropertyGroup>` element.
- Reload the solution.

## Known issues
- Version 1.x only: WPF windows with the same name and different namespace is unsupported.