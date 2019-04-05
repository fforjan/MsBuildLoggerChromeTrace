# What the hell is this ???

This repository contains a msbuild logger generating a Chrome Trace file.

_NOTE: format is available here : https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/edit_


## Dogfoung Demo

The idea is to compile it in debug and use it while compiling in release
```bash
dotnet build
dotnet build -c Release /logger:bin/Debug/netstandard2.0/MsBuildLoggerChromeTrace.dll
```