# BinaryInput
Proof of concept for writing text using binary search.

## Showcase

![BinaryInput showcase](Images/Showcase.gif)

## Improvements

It's possible to improve this by providing multiple rows to choose from. For example separate rows for lowercase and uppercase.

## Builds

Find the builds for desktop platforms on [https://apos.itch.io/binaryinput](https://apos.itch.io/binaryinput).

## Restore

```
dotnet restore Platforms/DesktopGL
dotnet restore Platforms/WindowsDX
```

## Run

```
dotnet run --project Platforms/DesktopGL
dotnet run --project Platforms/WindowsDX
```

## Debug

In vscode, you can debug by pressing F5.

## Publish

```
dotnet publish Platforms/DesktopGL -c Release -r win-x64 --self-contained --output artifacts/windows
dotnet publish Platforms/DesktopGL -c Release -r osx-x64 --self-contained --output artifacts/osx
dotnet publish Platforms/DesktopGL -c Release -r linux-x64 --self-contained --output artifacts/linux
```

```
dotnet publish Platforms/WindowsDX -c Release -r win-x64 --output artifacts/windowsdx
```
