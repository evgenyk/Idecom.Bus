﻿using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Idecom.Bus.SampleApp1")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Idecom.Bus.SampleApp1")]
[assembly: AssemblyCopyright("Copyright © Idecom 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("078c0356-1908-46fa-a476-cf452a60e2e5")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: AssemblyMetadata("idecom.endpoint.id", "61526A22-DEC7-4419-8C49-2F33FC7291B8")] //this is like an ip address (bus starts to operate on this address)
[assembly: AssemblyMetadata("idecom.endpoint.name", "idecom.sampleapp")] //this is a friendly name (aka dns name)
[assembly: AssemblyMetadata("idecom.endpoint.purpose", "Participates in a nice person conversation saga asking questions and answering some as well")] //this is just a description