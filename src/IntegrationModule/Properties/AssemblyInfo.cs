using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCopyright("Copyright ©  2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8e3ef316-5e3a-40bd-88e0-d93423b1165d")]

// ---------------------------------------------------------------------------
// VERSIONING - READ BEFORE CHANGING
//
// This project sets <GenerateAssemblyInfo>false</GenerateAssemblyInfo>, so the
// <AssemblyVersion>/<FileVersion> properties in IntegrationModule.csproj have
// NO EFFECT. The attributes below are the only source of truth.
//
// AssemblyVersion is FROZEN at 1.1.0.0 and must NOT be bumped per release.
// It is part of the assembly's strong-name identity: every consumer compiles a
// hard reference to this exact value. Changing it makes the CLR throw
// FileLoadException ("manifest definition does not match the assembly
// reference") for every integration built against an older SDK, and hosts that
// load us as a plugin - CCURE 9000 in particular - swallow that failure and
// silently drop the plugin instead of reporting it. Consumers cannot work
// around it either: binding redirects only apply from the host process's
// .exe.config, which third-party hosts own.
//
// This has already bitten us once: packages 1.0.x shipped AssemblyVersion
// 0.0.0.0, then 1.1.0 changed it to 1.1.0.0, silently breaking everything
// compiled against 1.0.x. 1.1.0.0 is the de-facto identity of the whole
// 1.1.x-1.2.x line, so it stays.
//
// Release numbers belong in AssemblyFileVersion and AssemblyInformationalVersion
// below, and in <Version> (the NuGet package version) in the .csproj. Bump those.
// ---------------------------------------------------------------------------
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.2.7.0")]
[assembly: AssemblyInformationalVersion("1.2.7")]
