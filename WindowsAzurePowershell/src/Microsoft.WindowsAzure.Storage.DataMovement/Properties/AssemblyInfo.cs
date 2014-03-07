using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyInformationalVersion("1.0.8698.17 (rd_data_stable.130626-2036)")]
[assembly: AssemblyFileVersion("1.0.8698.17")]
[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.MainAssembly)]
[assembly: ComVisible(false)]
[assembly: AssemblyProduct("Windows Azure SDK")]
[assembly: AssemblyCopyright("Copyright © Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyVersion("2.1.0.0")]

#if SIGN
[assembly: InternalsVisibleTo("Microsoft.WindowsAzure.Storage.DataMovement.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#else
[assembly: InternalsVisibleTo("Microsoft.WindowsAzure.Storage.DataMovement.Tests")]
#endif