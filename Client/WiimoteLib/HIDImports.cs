using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
namespace WiimoteLib
{
    internal class HIDImports
    {
        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 1u,
            Hidden = 2u,
            System = 4u,
            Directory = 16u,
            Archive = 32u,
            Device = 64u,
            Normal = 128u,
            Temporary = 256u,
            SparseFile = 512u,
            ReparsePoint = 1024u,
            Compressed = 2048u,
            Offline = 4096u,
            NotContentIndexed = 8192u,
            Encrypted = 16384u,
            Write_Through = 2147483648u,
            Overlapped = 1073741824u,
            NoBuffering = 536870912u,
            RandomAccess = 268435456u,
            SequentialScan = 134217728u,
            DeleteOnClose = 67108864u,
            BackupSemantics = 33554432u,
            PosixSemantics = 16777216u,
            OpenReparsePoint = 2097152u,
            OpenNoRecall = 1048576u,
            FirstPipeInstance = 524288u
        }
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr RESERVED;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public uint cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public short VendorID;
            public short ProductID;
            public short VersionNumber;
        }
        public const int DIGCF_DEFAULT = 1;
        public const int DIGCF_PRESENT = 2;
        public const int DIGCF_ALLCLASSES = 4;
        public const int DIGCF_PROFILE = 8;
        public const int DIGCF_DEVICEINTERFACE = 16;
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void HidD_GetHidGuid(out Guid gHid);
        [DllImport("hid.dll")]
        public static extern bool HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDImports.HIDD_ATTRIBUTES Attributes);
        [DllImport("hid.dll")]
        internal static extern bool HidD_SetOutputReport(IntPtr HidDeviceObject, byte[] lpReportBuffer, uint ReportBufferLength);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, [MarshalAs(UnmanagedType.LPTStr)] string Enumerator, IntPtr hwndParent, uint Flags);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInvo, ref Guid interfaceClassGuid, int memberIndex, ref HIDImports.SP_DEVICE_INTERFACE_DATA deviceInterfaceData);
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref HIDImports.SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref HIDImports.SP_DEVICE_INTERFACE_DATA deviceInterfaceData, ref HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ushort SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string fileName, [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess, [MarshalAs(UnmanagedType.U4)] FileShare fileShare, IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, [MarshalAs(UnmanagedType.U4)] HIDImports.EFileAttributes flags, IntPtr template);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}
