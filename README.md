# MyWpfExtensionGenerator
VB "WPF My extension" generator for .NET Core 3.1 and .NET 5 or later.
Generates Visual Basic's My Namespace extension for Windows Presentation Foundation.
The generator writes the WPF specific part of My extension by default. For generating all members, please upgrade to .NET 5 or later, and use Windows Forms (Edit project file, add `<UseWindowsForms>true</UseWindowsForms>` to `<PropertyGroup>`), then reload the project.

## Requirements
Visual Studio 16.9 or later

## Features
- Generate My.Application
- Generate My.Windows
- Generate My.Computer (.NET 5+ Windows Forms required)
- Generate My.User (.NET 5+ Windows Forms required)
- Generate My.Log (.NET 5+ Windows Forms required)
- Generate My.Application.Info (.NET 5+ Windows Forms required)

## Project structure
### Nukepayload2.SourceGenerators.MyWpfExtension
The source generator project. Build this project before using the sample project.
### SampleNet5WpfAndWinforms 
Sample project of .NET 5 WPF and Windows Forms. Requires to reopen Visual Studio after building the source generator project.
### SampleNetCore31Wpf
Sample project of .NET 5 WPF and Windows Forms. Requires to reopen Visual Studio after building the source generator project.

## Usage
This project haven't been published to Nuget yet. So, you can only use it as project reference.

- Add the source generator project to your solution.
- Build the source generator project.
- Select your WPF project, and add project reference to the source generator.
- Edit your project file. In the `<ProjectReference>` element, add ` OutputItemType="Analyzer" ReferenceOutputAssembly="False"` .
- Reload the solution.

## Known issues
- WPF windows with the same name and different namespace is temporarily unsupported.