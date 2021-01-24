# MyWpfExtensionGenerator
`MyWpfExtension.vb` source generator for .NET Core 3.1 and .NET

## Requirements
Visual Studio 16.9 preview 3 or later

## Features
- Generate My.Application
- Generate My.Windows
- Generate My.Computer (.NET 5 Windows Forms required)
- Generate My.User (.NET 5 Windows Forms required)
- Generate My.Log (.NET 5 Windows Forms required)
- Generate My.Application.Info (.NET 5 Windows Forms required)

## Project structure
### Nukepayload2.SourceGenerators.MyWpfExtension
The source generator project
### SampleNet5WpfAndWinforms 
Sample project of .NET 5 WPF and Windows Forms
### SampleNetCore31Wpf
Sample project of .NET 5 WPF and Windows Forms

## Usage
This project haven't been published to Nuget yet. So, you can only use it as project reference.

## Known issues
- Generated code can't be used in early binding. This is probably a bug of Visual Studio.
- WPF windows with the same name and different namespace is temporarily unsupported.