# FsCheck Setup for Unity

## Installation Instructions

FsCheck is a property-based testing library for .NET. To use it in Unity:

### Option 1: Download from NuGet (Recommended)

1. Download FsCheck from NuGet: https://www.nuget.org/packages/FsCheck/
   - Download version 2.16.6 (compatible with .NET Standard 2.0)

2. Extract the `.nupkg` file (rename to `.zip` if needed)

3. Copy the following DLLs to this folder (`Assets/_Project/Plugins/FsCheck/`):
   - `FsCheck.dll` from `lib/netstandard2.0/`

4. Also download and add FSharp.Core:
   - Download from: https://www.nuget.org/packages/FSharp.Core/
   - Copy `FSharp.Core.dll` from `lib/netstandard2.0/`

### Option 2: Using NuGetForUnity

1. Install NuGetForUnity from: https://github.com/GlitchEnzo/NuGetForUnity
2. Use the NuGet window in Unity to search and install "FsCheck"

## Required DLLs

Place these DLLs in this folder:
- FsCheck.dll
- FSharp.Core.dll

## Usage

```csharp
using FsCheck;

// Example property test
[Test]
public void MyPropertyTest()
{
    Prop.ForAll<int>(x => x + 0 == x).QuickCheckThrowOnFailure();
}
```

## Notes

- FsCheck requires .NET Standard 2.0 compatible DLLs
- Unity 2021+ supports .NET Standard 2.0
- Tests should be placed in the EtherDomes.Tests assembly
