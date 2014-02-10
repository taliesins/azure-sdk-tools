using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	internal class GlobalMemoryStatusNativeMethods
	{
		private GlobalMemoryStatusNativeMethods.MEMORYSTATUSEX memStatus;

		public ulong AvailablePhysicalMemory
		{
			get;
			private set;
		}

		public ulong TotalPhysicalMemory
		{
			get;
			private set;
		}

		public GlobalMemoryStatusNativeMethods()
		{
			this.memStatus = new GlobalMemoryStatusNativeMethods.MEMORYSTATUSEX();
			if (GlobalMemoryStatusNativeMethods.GlobalMemoryStatusEx(this.memStatus))
			{
				this.TotalPhysicalMemory = this.memStatus.ullTotalPhys;
				this.AvailablePhysicalMemory = this.memStatus.ullAvailPhys;
			}
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern bool GlobalMemoryStatusEx([In][Out] GlobalMemoryStatusNativeMethods.MEMORYSTATUSEX lpBuffer);

		private class MEMORYSTATUSEX
		{
			public uint dwLength;

			public uint dwMemoryLoad;

			public ulong ullTotalPhys;

			public ulong ullAvailPhys;

			public ulong ullTotalPageFile;

			public ulong ullAvailPageFile;

			public ulong ullTotalVirtual;

			public ulong ullAvailVirtual;

			public ulong ullAvailExtendedVirtual;

			public MEMORYSTATUSEX()
			{
				this.dwLength = (uint)Marshal.SizeOf(typeof(GlobalMemoryStatusNativeMethods.MEMORYSTATUSEX));
			}
		}
	}
}