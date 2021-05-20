# DotX

This projects aims to be lightweight Linux-first library for creating GUI with markup (XAML), oppportunity to style widgets and redefine appearance with templates and Bindings. It his heavily inspired by [WPF](https://github.com/dotnet/wpf) and, less, by [Avalonia](https://github.com/AvaloniaUI/Avalonia). Though Avalonia provides opportunity to create applications for Linux, there are less options be integrated into the system (build desktop environment for example). This project was started as educational, so if you considering it's usage or even trying, think twice or more. It is in the very early stage of development and there are very limited amount of features is implemented. 

## Compliling

Firstly you have to compile ```src/Modules/DotX.Xaml.MsBuild/DotX.Xaml.MsBuild.csproj```. If you work with framework other than ```net5``` you should also fix path (AssemblyFile attribute) in ```src/DotX.targets```. After it you can build ```sample/DotX.Sample/DotX.Sample.csproj```. This will build sample application that might be executed.

Currently it will work only on Linux with XOrg installed (maybe Wayland session will also work with XWayland). It requires installed Cairo (rendering), Pango (font processing) and Xlib (communication with XOrg).