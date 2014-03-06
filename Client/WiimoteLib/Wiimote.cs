using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
namespace WiimoteLib
{
    public class Wiimote : IDisposable
    {
        private enum OutputReport : byte
        {
            LEDs = 17,
            Type,
            IR,
            Status = 21,
            WriteMemory,
            ReadMemory,
            IR2 = 26
        }
        internal delegate bool WiimoteFoundDelegate(string devicePath);
        private const int VID = 1406;
        private const int PID = 774;
        private const int REPORT_LENGTH = 22;
        private const int REGISTER_IR = 78643248;
        private const int REGISTER_IR_SENSITIVITY_1 = 78643200;
        private const int REGISTER_IR_SENSITIVITY_2 = 78643226;
        private const int REGISTER_IR_MODE = 78643251;
        private const int REGISTER_EXTENSION_INIT_1 = 77857008;
        private const int REGISTER_EXTENSION_INIT_2 = 77857019;
        private const int REGISTER_EXTENSION_TYPE = 77857018;
        private const int REGISTER_EXTENSION_CALIBRATION = 77856800;
        private const int BSL = 43;
        private const int BSW = 24;
        private const float KG2LB = 2.20462251f;
        private SafeFileHandle mHandle;
        private FileStream mStream;
        private readonly byte[] mBuff = new byte[22];
        private byte[] mReadBuff;
        private int mAddress;
        private short mSize;
        private readonly WiimoteState mWiimoteState = new WiimoteState();
        private readonly AutoResetEvent mReadDone = new AutoResetEvent(false);
        private readonly AutoResetEvent mWriteDone = new AutoResetEvent(false);
        private readonly AutoResetEvent mStatusDone = new AutoResetEvent(false);
        private bool mAltWriteMethod;
        private string mDevicePath = string.Empty;
        private readonly Guid mID = Guid.NewGuid();
        private event EventHandler<WiimoteChangedEventArgs> wiimoteChanged;
        public event EventHandler<WiimoteChangedEventArgs> WiimoteChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.wiimoteChanged += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.wiimoteChanged -= value;
            }
        }
        private event EventHandler<WiimoteExtensionChangedEventArgs> wiimoteExtensionChanged;
        public event EventHandler<WiimoteExtensionChangedEventArgs> WiimoteExtensionChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.wiimoteExtensionChanged += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.wiimoteExtensionChanged -= value;
            }
        }
        public WiimoteState WiimoteState
        {
            get
            {
                return this.mWiimoteState;
            }
        }
        public Guid ID
        {
            get
            {
                return this.mID;
            }
        }
        public string HIDDevicePath
        {
            get
            {
                return this.mDevicePath;
            }
        }
        public Wiimote()
        {
        }
        internal Wiimote(string devicePath)
        {
            this.mDevicePath = devicePath;
        }
        public void Connect()
        {
            if (string.IsNullOrEmpty(this.mDevicePath))
            {
                Wiimote.FindWiimote(new Wiimote.WiimoteFoundDelegate(this.WiimoteFound));
                return;
            }
            this.OpenWiimoteDeviceHandle(this.mDevicePath);
        }
        internal static void FindWiimote(Wiimote.WiimoteFoundDelegate wiimoteFound)
        {
            int num = 0;
            bool flag = false;
            Guid guid;
            HIDImports.HidD_GetHidGuid(out guid);
            IntPtr hDevInfo = HIDImports.SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, 16u);
            HIDImports.SP_DEVICE_INTERFACE_DATA sP_DEVICE_INTERFACE_DATA = default(HIDImports.SP_DEVICE_INTERFACE_DATA);
            sP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DATA);
            while (HIDImports.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guid, num, ref sP_DEVICE_INTERFACE_DATA))
            {
                uint deviceInterfaceDetailDataSize;
                HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref sP_DEVICE_INTERFACE_DATA, IntPtr.Zero, 0u, out deviceInterfaceDetailDataSize, IntPtr.Zero);
                HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA sP_DEVICE_INTERFACE_DETAIL_DATA = default(HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA);
                sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = ((IntPtr.Size == 8) ? 8u : 5u);
                if (!HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref sP_DEVICE_INTERFACE_DATA, ref sP_DEVICE_INTERFACE_DETAIL_DATA, deviceInterfaceDetailDataSize, out deviceInterfaceDetailDataSize, IntPtr.Zero))
                {
                    throw new WiimoteException("SetupDiGetDeviceInterfaceDetail failed on index " + num);
                }
                SafeFileHandle safeFileHandle = HIDImports.CreateFile(sP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);
                HIDImports.HIDD_ATTRIBUTES hIDD_ATTRIBUTES = default(HIDImports.HIDD_ATTRIBUTES);
                hIDD_ATTRIBUTES.Size = Marshal.SizeOf(hIDD_ATTRIBUTES);
                if (HIDImports.HidD_GetAttributes(safeFileHandle.DangerousGetHandle(), ref hIDD_ATTRIBUTES) && hIDD_ATTRIBUTES.VendorID == 1406 && hIDD_ATTRIBUTES.ProductID == 774)
                {
                    flag = true;
                    if (!wiimoteFound(sP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath))
                    {
                        break;
                    }
                }
                safeFileHandle.Close();
                num++;
            }
            HIDImports.SetupDiDestroyDeviceInfoList(hDevInfo);
            if (!flag)
            {
                throw new WiimoteNotFoundException("No Wiimotes found in HID device list.");
            }
        }
        private bool WiimoteFound(string devicePath)
        {
            this.mDevicePath = devicePath;
            this.OpenWiimoteDeviceHandle(this.mDevicePath);
            return false;
        }
        private void OpenWiimoteDeviceHandle(string devicePath)
        {
            this.mHandle = HIDImports.CreateFile(devicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);
            HIDImports.HIDD_ATTRIBUTES hIDD_ATTRIBUTES = default(HIDImports.HIDD_ATTRIBUTES);
            hIDD_ATTRIBUTES.Size = Marshal.SizeOf(hIDD_ATTRIBUTES);
            if (!HIDImports.HidD_GetAttributes(this.mHandle.DangerousGetHandle(), ref hIDD_ATTRIBUTES))
            {
                return;
            }
            if (hIDD_ATTRIBUTES.VendorID == 1406 && hIDD_ATTRIBUTES.ProductID == 774)
            {
                this.mStream = new FileStream(this.mHandle, FileAccess.ReadWrite, 22, true);
                this.BeginAsyncRead();
                try
                {
                    this.ReadWiimoteCalibration();
                }
                catch
                {
                    this.mAltWriteMethod = true;
                    this.ReadWiimoteCalibration();
                }
                this.GetStatus();
                return;
            }
            this.mHandle.Close();
            throw new WiimoteException("Attempted to open a non-Wiimote device.");
        }
        public void Disconnect()
        {
            if (this.mStream != null)
            {
                this.mStream.Close();
            }
            if (this.mHandle != null)
            {
                this.mHandle.Close();
            }
        }
        private void BeginAsyncRead()
        {
            if (this.mStream != null && this.mStream.CanRead)
            {
                byte[] array = new byte[22];
                this.mStream.BeginRead(array, 0, 22, new AsyncCallback(this.OnReadData), array);
            }
        }
        private void OnReadData(IAsyncResult ar)
        {
            byte[] buff = (byte[])ar.AsyncState;
            try
            {
                this.mStream.EndRead(ar);
                if (this.ParseInputReport(buff) && (this.wiimoteChanged != null))
                {
                    this.wiimoteChanged(this, new WiimoteChangedEventArgs(this.mWiimoteState));
                }
                this.BeginAsyncRead();
            }
            catch (OperationCanceledException)
            {
            }
        }
        private bool ParseInputReport(byte[] buff)
        {
            InputReport inputReport = (InputReport)buff[0];
            InputReport inputReport2 = inputReport;
            switch (inputReport2)
            {
                case InputReport.Status:
                    {
                        this.ParseButtons(buff);
                        this.mWiimoteState.BatteryRaw = buff[6];
                        this.mWiimoteState.Battery = 4800f * ((float)buff[6] / 48f) / 192f;
                        this.mWiimoteState.LEDState.LED1 = ((buff[3] & 16) != 0);
                        this.mWiimoteState.LEDState.LED2 = ((buff[3] & 32) != 0);
                        this.mWiimoteState.LEDState.LED3 = ((buff[3] & 64) != 0);
                        this.mWiimoteState.LEDState.LED4 = ((buff[3] & 128) != 0);
                        bool flag = (buff[3] & 2) != 0;
                        if (this.mWiimoteState.Extension != flag)
                        {
                            this.mWiimoteState.Extension = flag;
                            if (flag)
                            {
                                this.BeginAsyncRead();
                                this.InitializeExtension();
                            }
                            else
                            {
                                this.mWiimoteState.ExtensionType = ExtensionType.None;
                            }
                            long token1 = -1541405694;
                            if (this.wiimoteExtensionChanged != null && this.mWiimoteState.ExtensionType != (ExtensionType)((ulong)token1))
                            {
                                this.wiimoteExtensionChanged(this, new WiimoteExtensionChangedEventArgs(this.mWiimoteState.ExtensionType, this.mWiimoteState.Extension));
                            }
                        }
                        this.mStatusDone.Set();
                        break;
                    }
                case InputReport.ReadData:
                    this.ParseButtons(buff);
                    this.ParseReadData(buff);
                    break;
                case InputReport.OutputReportAck:
                    this.mWriteDone.Set();
                    break;
                default:
                    switch (inputReport2)
                    {
                        case InputReport.Buttons:
                            this.ParseButtons(buff);
                            return true;
                        case InputReport.ButtonsAccel:
                            this.ParseButtons(buff);
                            this.ParseAccel(buff);
                            return true;
                        case InputReport.IRAccel:
                            this.ParseButtons(buff);
                            this.ParseAccel(buff);
                            this.ParseIR(buff);
                            return true;
                        case InputReport.ButtonsExtension:
                            this.ParseButtons(buff);
                            this.ParseExtension(buff, 3);
                            return true;
                        case InputReport.ExtensionAccel:
                            this.ParseButtons(buff);
                            this.ParseAccel(buff);
                            this.ParseExtension(buff, 6);
                            return true;
                        case InputReport.IRExtensionAccel:
                            this.ParseButtons(buff);
                            this.ParseAccel(buff);
                            this.ParseIR(buff);
                            this.ParseExtension(buff, 16);
                            return true;
                    }
                    return false;
            }
            return true;
        }
        private void InitializeExtension()
        {
            // determines the extension type and updates the current state
            this.WriteData(77857008, 85);
            this.WriteData(77857019, 0);
            this.BeginAsyncRead();
            byte[] array = this.ReadData(77857018, 6);
            long num = (long)((ulong)array[0] << 40 | (ulong)array[1] << 32 | (ulong)array[2] << 24 | (ulong)array[3] << 16 | (ulong)array[4] << 8 | (ulong)array[5]);
            ExtensionType extensionType = (ExtensionType)num;


            if (extensionType != ExtensionType.None && extensionType != ExtensionType.ParitallyInserted)
            {
                this.mWiimoteState.ExtensionType = (ExtensionType)num;
                this.SetReportType(InputReport.ButtonsExtension, true);

                if (extensionType == ExtensionType.ClassicController)
                {
                    array = this.ReadData(77856800, 16);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXL = (byte)(array[0] >> 2);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinXL = (byte)(array[1] >> 2);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidXL = (byte)(array[2] >> 2);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL = (byte)(array[3] >> 2);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinYL = (byte)(array[4] >> 2);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidYL = (byte)(array[5] >> 2);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR = (byte)(array[6] >> 3);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinXR = (byte)(array[7] >> 3);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidXR = (byte)(array[8] >> 3);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR = (byte)(array[9] >> 3);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinYR = (byte)(array[10] >> 3);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidYR = (byte)(array[11] >> 3);
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerL = 0;
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL = 31;
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerR = 0;
                    this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR = 31;
                    return;
                }
                if(extensionType == ExtensionType.BalanceBoard)
                {
                    array = this.ReadData(77856800, 32);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopRight     = (short)((int)array[4] << 8 | (int)array[5]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomRight  = (short)((int)array[6] << 8 | (int)array[7]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopLeft      = (short)((int)array[8] << 8 | (int)array[9]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomLeft   = (short)((int)array[10] << 8 | (int)array[11]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopRight    = (short)((int)array[12] << 8 | (int)array[13]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomRight = (short)((int)array[14] << 8 | (int)array[15]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopLeft     = (short)((int)array[16] << 8 | (int)array[17]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomLeft  = (short)((int)array[18] << 8 | (int)array[19]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopRight    = (short)((int)array[20] << 8 | (int)array[21]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomRight = (short)((int)array[22] << 8 | (int)array[23]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopLeft     = (short)((int)array[24] << 8 | (int)array[25]);
                    this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomLeft  = (short)((int)array[26] << 8 | (int)array[27]);
                    return;
                }
                if (extensionType == ExtensionType.Nunchuk)
                {
                    array = this.ReadData(77856800, 16);
                    this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0 = array[0];
                    this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0 = array[1];
                    this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0 = array[2];
                    this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.XG = array[4];
                    this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.YG = array[5];
                    this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.ZG = array[6];
                    this.mWiimoteState.NunchukState.CalibrationInfo.MaxX = array[8];
                    this.mWiimoteState.NunchukState.CalibrationInfo.MinX = array[9];
                    this.mWiimoteState.NunchukState.CalibrationInfo.MidX = array[10];
                    this.mWiimoteState.NunchukState.CalibrationInfo.MaxY = array[11];
                    this.mWiimoteState.NunchukState.CalibrationInfo.MinY = array[12];
                    this.mWiimoteState.NunchukState.CalibrationInfo.MidY = array[13];
                    return;
                }
                return;
            }
            else if (extensionType == ExtensionType.None)
            {
                this.mWiimoteState.Extension = false;
                this.mWiimoteState.ExtensionType = ExtensionType.None;
                return;
            }
            else
            {
                throw new WiimoteException("Unknown extension controller found: " + num.ToString("x"));
            }
        }
        private byte[] DecryptBuffer(byte[] buff)
        {
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = (byte)((buff[i] ^ 23) + 23 & 255);
            }
            return buff;
        }
        private void ParseButtons(byte[] buff)
        {
            this.mWiimoteState.ButtonState.A     = ((buff[2] &   8) != 0);
            this.mWiimoteState.ButtonState.B     = ((buff[2] &   4) != 0);
            this.mWiimoteState.ButtonState.Minus = ((buff[2] &  16) != 0);
            this.mWiimoteState.ButtonState.Home  = ((buff[2] & 128) != 0);
            this.mWiimoteState.ButtonState.Plus  = ((buff[1] &  16) != 0);
            this.mWiimoteState.ButtonState.One   = ((buff[2] &   2) != 0);
            this.mWiimoteState.ButtonState.Two   = ((buff[2] &   1) != 0);
            this.mWiimoteState.ButtonState.Up    = ((buff[1] &   8) != 0);
            this.mWiimoteState.ButtonState.Down  = ((buff[1] &   4) != 0);
            this.mWiimoteState.ButtonState.Left  = ((buff[1] &   1) != 0);
            this.mWiimoteState.ButtonState.Right = ((buff[1] &   2) != 0);
        }
        private void ParseAccel(byte[] buff)
        {
            this.mWiimoteState.AccelState.RawValues.X = (int)buff[3];
            this.mWiimoteState.AccelState.RawValues.Y = (int)buff[4];
            this.mWiimoteState.AccelState.RawValues.Z = (int)buff[5];
            this.mWiimoteState.AccelState.Values.X = ((float)this.mWiimoteState.AccelState.RawValues.X - (float)this.mWiimoteState.AccelCalibrationInfo.X0) / ((float)this.mWiimoteState.AccelCalibrationInfo.XG - (float)this.mWiimoteState.AccelCalibrationInfo.X0);
            this.mWiimoteState.AccelState.Values.Y = ((float)this.mWiimoteState.AccelState.RawValues.Y - (float)this.mWiimoteState.AccelCalibrationInfo.Y0) / ((float)this.mWiimoteState.AccelCalibrationInfo.YG - (float)this.mWiimoteState.AccelCalibrationInfo.Y0);
            this.mWiimoteState.AccelState.Values.Z = ((float)this.mWiimoteState.AccelState.RawValues.Z - (float)this.mWiimoteState.AccelCalibrationInfo.Z0) / ((float)this.mWiimoteState.AccelCalibrationInfo.ZG - (float)this.mWiimoteState.AccelCalibrationInfo.Z0);
        }
        private void ParseIR(byte[] buff)
        {
            this.mWiimoteState.IRState.IRSensors[0].RawPosition.X = ((int)buff[6] | (buff[8] >> 4 & 3) << 8);
            this.mWiimoteState.IRState.IRSensors[0].RawPosition.Y = ((int)buff[7] | (buff[8] >> 6 & 3) << 8);
            switch (this.mWiimoteState.IRState.Mode)
            {
                case IRMode.Basic:
                    this.mWiimoteState.IRState.IRSensors[1].RawPosition.X = ((int)buff[9] | (int)(buff[8] & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[1].RawPosition.Y = ((int)buff[10] | (buff[8] >> 2 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[2].RawPosition.X = ((int)buff[11] | (buff[13] >> 4 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[2].RawPosition.Y = ((int)buff[12] | (buff[13] >> 6 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[3].RawPosition.X = ((int)buff[14] | (int)(buff[13] & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[3].RawPosition.Y = ((int)buff[15] | (buff[13] >> 2 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[0].Size = 0;
                    this.mWiimoteState.IRState.IRSensors[1].Size = 0;
                    this.mWiimoteState.IRState.IRSensors[2].Size = 0;
                    this.mWiimoteState.IRState.IRSensors[3].Size = 0;
                    this.mWiimoteState.IRState.IRSensors[0].Found = (buff[6] != 255 || buff[7] != 255);
                    this.mWiimoteState.IRState.IRSensors[1].Found = (buff[9] != 255 || buff[10] != 255);
                    this.mWiimoteState.IRState.IRSensors[2].Found = (buff[11] != 255 || buff[12] != 255);
                    this.mWiimoteState.IRState.IRSensors[3].Found = (buff[14] != 255 || buff[15] != 255);
                    break;
                case IRMode.Extended:
                    this.mWiimoteState.IRState.IRSensors[1].RawPosition.X = ((int)buff[9] | (buff[11] >> 4 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[1].RawPosition.Y = ((int)buff[10] | (buff[11] >> 6 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[2].RawPosition.X = ((int)buff[12] | (buff[14] >> 4 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[2].RawPosition.Y = ((int)buff[13] | (buff[14] >> 6 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[3].RawPosition.X = ((int)buff[15] | (buff[17] >> 4 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[3].RawPosition.Y = ((int)buff[16] | (buff[17] >> 6 & 3) << 8);
                    this.mWiimoteState.IRState.IRSensors[0].Size = (int)(buff[8] & 15);
                    this.mWiimoteState.IRState.IRSensors[1].Size = (int)(buff[11] & 15);
                    this.mWiimoteState.IRState.IRSensors[2].Size = (int)(buff[14] & 15);
                    this.mWiimoteState.IRState.IRSensors[3].Size = (int)(buff[17] & 15);
                    this.mWiimoteState.IRState.IRSensors[0].Found = (buff[6] != 255 || buff[7] != 255 || buff[8] != 255);
                    this.mWiimoteState.IRState.IRSensors[1].Found = (buff[9] != 255 || buff[10] != 255 || buff[11] != 255);
                    this.mWiimoteState.IRState.IRSensors[2].Found = (buff[12] != 255 || buff[13] != 255 || buff[14] != 255);
                    this.mWiimoteState.IRState.IRSensors[3].Found = (buff[15] != 255 || buff[16] != 255 || buff[17] != 255);
                    break;
            }
            this.mWiimoteState.IRState.IRSensors[0].Position.X = (float)this.mWiimoteState.IRState.IRSensors[0].RawPosition.X / 1023.5f;
            this.mWiimoteState.IRState.IRSensors[1].Position.X = (float)this.mWiimoteState.IRState.IRSensors[1].RawPosition.X / 1023.5f;
            this.mWiimoteState.IRState.IRSensors[2].Position.X = (float)this.mWiimoteState.IRState.IRSensors[2].RawPosition.X / 1023.5f;
            this.mWiimoteState.IRState.IRSensors[3].Position.X = (float)this.mWiimoteState.IRState.IRSensors[3].RawPosition.X / 1023.5f;
            this.mWiimoteState.IRState.IRSensors[0].Position.Y = (float)this.mWiimoteState.IRState.IRSensors[0].RawPosition.Y / 767.5f;
            this.mWiimoteState.IRState.IRSensors[1].Position.Y = (float)this.mWiimoteState.IRState.IRSensors[1].RawPosition.Y / 767.5f;
            this.mWiimoteState.IRState.IRSensors[2].Position.Y = (float)this.mWiimoteState.IRState.IRSensors[2].RawPosition.Y / 767.5f;
            this.mWiimoteState.IRState.IRSensors[3].Position.Y = (float)this.mWiimoteState.IRState.IRSensors[3].RawPosition.Y / 767.5f;
            if (this.mWiimoteState.IRState.IRSensors[0].Found && this.mWiimoteState.IRState.IRSensors[1].Found)
            {
                this.mWiimoteState.IRState.RawMidpoint.X = (this.mWiimoteState.IRState.IRSensors[1].RawPosition.X + this.mWiimoteState.IRState.IRSensors[0].RawPosition.X) / 2;
                this.mWiimoteState.IRState.RawMidpoint.Y = (this.mWiimoteState.IRState.IRSensors[1].RawPosition.Y + this.mWiimoteState.IRState.IRSensors[0].RawPosition.Y) / 2;
                this.mWiimoteState.IRState.Midpoint.X = (this.mWiimoteState.IRState.IRSensors[1].Position.X + this.mWiimoteState.IRState.IRSensors[0].Position.X) / 2f;
                this.mWiimoteState.IRState.Midpoint.Y = (this.mWiimoteState.IRState.IRSensors[1].Position.Y + this.mWiimoteState.IRState.IRSensors[0].Position.Y) / 2f;
                return;
            }
            this.mWiimoteState.IRState.Midpoint.X = (this.mWiimoteState.IRState.Midpoint.Y = 0f);
        }
        private void ParseExtension(byte[] buff, int offset)
        {
            ExtensionType extensionType = this.mWiimoteState.ExtensionType;

            switch (extensionType)
            {
                case ExtensionType.ClassicController:
                    this.mWiimoteState.ClassicControllerState.RawJoystickL.X       = (int)(buff[offset] & 63);
                    this.mWiimoteState.ClassicControllerState.RawJoystickL.Y       = (int)(buff[offset + 1] & 63);
                    this.mWiimoteState.ClassicControllerState.RawJoystickR.X       = (int)((byte)(buff[offset + 2] >> 7 | (buff[offset + 1] & 192) >> 5 | (buff[offset] & 192) >> 3));
                    this.mWiimoteState.ClassicControllerState.RawJoystickR.Y       = (int)(buff[offset + 2] & 31);
                    this.mWiimoteState.ClassicControllerState.RawTriggerL          = (byte)((buff[offset + 2] & 96) >> 2 | buff[offset + 3] >> 5);
                    this.mWiimoteState.ClassicControllerState.RawTriggerR          = (byte)(buff[offset + 3] & 31);
                    this.mWiimoteState.ClassicControllerState.ButtonState.TriggerR = ((buff[offset + 4] & 2) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Plus     = ((buff[offset + 4] & 4) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Home     = ((buff[offset + 4] & 8) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Minus    = ((buff[offset + 4] & 16) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.TriggerL = ((buff[offset + 4] & 32) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Down     = ((buff[offset + 4] & 64) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Right    = ((buff[offset + 4] & 128) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Up       = ((buff[offset + 5] & 1) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Left     = ((buff[offset + 5] & 2) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.ZR       = ((buff[offset + 5] & 4) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.X        = ((buff[offset + 5] & 8) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.A        = ((buff[offset + 5] & 16) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.Y        = ((buff[offset + 5] & 32) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.B        = ((buff[offset + 5] & 64) == 0);
                    this.mWiimoteState.ClassicControllerState.ButtonState.ZL       = ((buff[offset + 5] & 128) == 0);
                    if (this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXL != 0)
                    {
                        this.mWiimoteState.ClassicControllerState.JoystickL.X =
                            ((float)this.mWiimoteState.ClassicControllerState.RawJoystickL.X -
                            (float)this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidXL) /
                            (float)(this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXL -
                            this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinXL);
                    }
                    if (this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL != 0)
                    {
                        this.mWiimoteState.ClassicControllerState.JoystickL.Y =
                            ((float)this.mWiimoteState.ClassicControllerState.RawJoystickL.Y -
                            (float)this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidYL) /
                            (float)(this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL -
                            this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinYL);
                    }
                    if (this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR != 0)
                    {
                        this.mWiimoteState.ClassicControllerState.JoystickR.X =
                            ((float)this.mWiimoteState.ClassicControllerState.RawJoystickR.X -
                            (float)this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidXR) /
                            (float)(this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR -
                            this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinXR);
                    }
                    if (this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR != 0)
                    {
                        this.mWiimoteState.ClassicControllerState.JoystickR.Y =
                            ((float)this.mWiimoteState.ClassicControllerState.RawJoystickR.Y -
                            (float)this.mWiimoteState.ClassicControllerState.CalibrationInfo.MidYR) /
                            (float)(this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR -
                            this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinYR);
                    }
                    if (this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL != 0)
                    {
                        this.mWiimoteState.ClassicControllerState.TriggerL =
                            (float)this.mWiimoteState.ClassicControllerState.RawTriggerL /
                            (float)(this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL -
                            this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerL);
                    }
                    if (this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR != 0)
                    {
                        this.mWiimoteState.ClassicControllerState.TriggerR =
                            (float)this.mWiimoteState.ClassicControllerState.RawTriggerR /
                            (float)(this.mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR -
                            this.mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerR);
                        return;
                    }
                    break;





                case ExtensionType.Guitar:
                    this.mWiimoteState.GuitarState.GuitarType             = (((buff[offset] & 128) == 0) ? GuitarType.GuitarHeroWorldTour : GuitarType.GuitarHero3);
                    this.mWiimoteState.GuitarState.ButtonState.Plus       = ((buff[offset + 4] & 4) == 0);
                    this.mWiimoteState.GuitarState.ButtonState.Minus      = ((buff[offset + 4] & 16) == 0);
                    this.mWiimoteState.GuitarState.ButtonState.StrumDown  = ((buff[offset + 4] & 64) == 0);
                    this.mWiimoteState.GuitarState.ButtonState.StrumUp    = ((buff[offset + 5] & 1) == 0);
                    this.mWiimoteState.GuitarState.FretButtonState.Yellow = ((buff[offset + 5] & 8) == 0);
                    this.mWiimoteState.GuitarState.FretButtonState.Green  = ((buff[offset + 5] & 16) == 0);
                    this.mWiimoteState.GuitarState.FretButtonState.Blue   = ((buff[offset + 5] & 32) == 0);
                    this.mWiimoteState.GuitarState.FretButtonState.Red    = ((buff[offset + 5] & 64) == 0);
                    this.mWiimoteState.GuitarState.FretButtonState.Orange = ((buff[offset + 5] & 128) == 0);
                    this.mWiimoteState.GuitarState.RawJoystick.X          = (int)(buff[offset] & 63);
                    this.mWiimoteState.GuitarState.RawJoystick.Y          = (int)(buff[offset + 1] & 63);
                    this.mWiimoteState.GuitarState.RawWhammyBar           = (byte)(buff[offset + 3] & 31);
                    this.mWiimoteState.GuitarState.Joystick.X             = (float)(this.mWiimoteState.GuitarState.RawJoystick.X - 31) / 63f;
                    this.mWiimoteState.GuitarState.Joystick.Y             = (float)(this.mWiimoteState.GuitarState.RawJoystick.Y - 31) / 63f;
                    this.mWiimoteState.GuitarState.WhammyBar              = (float)this.mWiimoteState.GuitarState.RawWhammyBar / 10f;
                    this.mWiimoteState.GuitarState.TouchbarState.Yellow   = false;
                    this.mWiimoteState.GuitarState.TouchbarState.Green    = false;
                    this.mWiimoteState.GuitarState.TouchbarState.Blue     = false;
                    this.mWiimoteState.GuitarState.TouchbarState.Red      = false;
                    this.mWiimoteState.GuitarState.TouchbarState.Orange   = false;
                    int num = (int)(buff[offset + 2] & 31);
                    switch (num)
                    {
                        case 4:
                            this.mWiimoteState.GuitarState.TouchbarState.Green = true;
                            return;
                        case 5: case 6: case 8: case 9: case 11: case 14:
                        case 15: case 16: case 17: case 22: case 25:
                            break;
                        case 7:
                            this.mWiimoteState.GuitarState.TouchbarState.Green = true;
                            this.mWiimoteState.GuitarState.TouchbarState.Red = true;
                            return;
                        case 10:
                            this.mWiimoteState.GuitarState.TouchbarState.Red = true;
                            return;
                        case 12: case 13:
                            this.mWiimoteState.GuitarState.TouchbarState.Red = true;
                            this.mWiimoteState.GuitarState.TouchbarState.Yellow = true;
                            return;
                        case 18: case 19:
                            this.mWiimoteState.GuitarState.TouchbarState.Yellow = true;
                            return;
                        case 20: case 21:
                            this.mWiimoteState.GuitarState.TouchbarState.Yellow = true;
                            this.mWiimoteState.GuitarState.TouchbarState.Blue = true;
                            return;
                        case 23: case 24:
                            this.mWiimoteState.GuitarState.TouchbarState.Blue = true;
                            return;
                        case 26:
                            this.mWiimoteState.GuitarState.TouchbarState.Blue = true;
                            this.mWiimoteState.GuitarState.TouchbarState.Orange = true;
                            return;
                        default:
                            if (num != 31) return;
                            this.mWiimoteState.GuitarState.TouchbarState.Orange = true;
                            return;
                    }
                    break;




                case ExtensionType.Nunchuk:
                    this.mWiimoteState.NunchukState.RawJoystick.X          = (int)buff[offset];
                    this.mWiimoteState.NunchukState.RawJoystick.Y          = (int)buff[offset + 1];
                    this.mWiimoteState.NunchukState.AccelState.RawValues.X = (int)buff[offset + 2];
                    this.mWiimoteState.NunchukState.AccelState.RawValues.Y = (int)buff[offset + 3];
                    this.mWiimoteState.NunchukState.AccelState.RawValues.Z = (int)buff[offset + 4];
                    this.mWiimoteState.NunchukState.C                      = ((buff[offset + 5] & 2) == 0);
                    this.mWiimoteState.NunchukState.Z                      = ((buff[offset + 5] & 1) == 0);
                    this.mWiimoteState.NunchukState.AccelState.Values.X    = 
                        ((float)this.mWiimoteState.NunchukState.AccelState.RawValues.X - 
                        (float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0) / 
                        ((float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.XG - 
                        (float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0);
                    this.mWiimoteState.NunchukState.AccelState.Values.Y    = 
                        ((float)this.mWiimoteState.NunchukState.AccelState.RawValues.Y - 
                        (float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0) / 
                        ((float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.YG - 
                        (float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0);
                    this.mWiimoteState.NunchukState.AccelState.Values.Z    = 
                        ((float)this.mWiimoteState.NunchukState.AccelState.RawValues.Z - 
                        (float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0) / 
                        ((float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.ZG - 
                        (float)this.mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0);
                    if (this.mWiimoteState.NunchukState.CalibrationInfo.MaxX != 0)
                    {
                        this.mWiimoteState.NunchukState.Joystick.X = 
                            ((float)this.mWiimoteState.NunchukState.RawJoystick.X - 
                            (float)this.mWiimoteState.NunchukState.CalibrationInfo.MidX) / 
                            ((float)this.mWiimoteState.NunchukState.CalibrationInfo.MaxX - 
                            (float)this.mWiimoteState.NunchukState.CalibrationInfo.MinX);
                    }
                    if (this.mWiimoteState.NunchukState.CalibrationInfo.MaxY != 0)
                    {
                        this.mWiimoteState.NunchukState.Joystick.Y = 
                            ((float)this.mWiimoteState.NunchukState.RawJoystick.Y - 
                            (float)this.mWiimoteState.NunchukState.CalibrationInfo.MidY) / 
                            ((float)this.mWiimoteState.NunchukState.CalibrationInfo.MaxY - 
                            (float)this.mWiimoteState.NunchukState.CalibrationInfo.MinY);
                        return;
                    }
                    break;




                case ExtensionType.Drums:
                    this.mWiimoteState.DrumsState.RawJoystick.X = (int)(buff[offset] & 63);
                    this.mWiimoteState.DrumsState.RawJoystick.Y = (int)(buff[offset + 1] & 63);
                    this.mWiimoteState.DrumsState.Plus          = ((buff[offset + 4] & 4) == 0);
                    this.mWiimoteState.DrumsState.Minus         = ((buff[offset + 4] & 16) == 0);
                    this.mWiimoteState.DrumsState.Pedal         = ((buff[offset + 5] & 4) == 0);
                    this.mWiimoteState.DrumsState.Blue          = ((buff[offset + 5] & 8) == 0);
                    this.mWiimoteState.DrumsState.Green         = ((buff[offset + 5] & 16) == 0);
                    this.mWiimoteState.DrumsState.Yellow        = ((buff[offset + 5] & 32) == 0);
                    this.mWiimoteState.DrumsState.Red           = ((buff[offset + 5] & 64) == 0);
                    this.mWiimoteState.DrumsState.Orange        = ((buff[offset + 5] & 128) == 0);
                    this.mWiimoteState.DrumsState.Joystick.X    = (float)(this.mWiimoteState.DrumsState.RawJoystick.X - 31) / 63f;
                    this.mWiimoteState.DrumsState.Joystick.Y    = (float)(this.mWiimoteState.DrumsState.RawJoystick.Y - 31) / 63f;
                    if ((buff[offset + 2] & 64) == 0)
                    {
                        int num2 = buff[offset + 2] >> 1 & 31;
                        int num3 = buff[offset + 3] >> 5;
                        if (num3 != 7)
                        {
                            int num4 = num2;
                            switch (num4)
                            {
                                case 14: this.mWiimoteState.DrumsState.OrangeVelocity = num3; return;
                                case 15: this.mWiimoteState.DrumsState.BlueVelocity = num3; return;
                                case 16: break;
                                case 17: this.mWiimoteState.DrumsState.YellowVelocity = num3; return;
                                case 18: this.mWiimoteState.DrumsState.GreenVelocity = num3; return;
                                default:
                                    switch (num4)
                                    {
                                        case 25: this.mWiimoteState.DrumsState.RedVelocity = num3; return;
                                        case 26: break;
                                        case 27: this.mWiimoteState.DrumsState.PedalVelocity = num3; return;
                                        default: return;
                                    }
                                    break;
                            }
                        }
                    }
                    break;



                
                case ExtensionType.BalanceBoard:
                    this.mWiimoteState.BalanceBoardState.SensorValuesRaw.TopRight = (short)((int)buff[offset] << 8 | (int)buff[offset + 1]);
                    this.mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomRight = (short)((int)buff[offset + 2] << 8 | (int)buff[offset + 3]);
                    this.mWiimoteState.BalanceBoardState.SensorValuesRaw.TopLeft = (short)((int)buff[offset + 4] << 8 | (int)buff[offset + 5]);
                    this.mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomLeft = (short)((int)buff[offset + 6] << 8 | (int)buff[offset + 7]);
                    this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft = 
                        this.GetBalanceBoardSensorValue(this.mWiimoteState.BalanceBoardState.SensorValuesRaw.TopLeft, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopLeft, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopLeft, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopLeft);
                    this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight = 
                        this.GetBalanceBoardSensorValue(this.mWiimoteState.BalanceBoardState.SensorValuesRaw.TopRight, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopRight, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopRight, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopRight);
                    this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft = 
                        this.GetBalanceBoardSensorValue(this.mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomLeft, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomLeft, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomLeft, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomLeft);
                    this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight = 
                        this.GetBalanceBoardSensorValue(this.mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomRight, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomRight, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomRight, 
                        this.mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomRight);
                    this.mWiimoteState.BalanceBoardState.SensorValuesLb.TopLeft = 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft * 2.20462251f;
                    this.mWiimoteState.BalanceBoardState.SensorValuesLb.TopRight = 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight * 2.20462251f;
                    this.mWiimoteState.BalanceBoardState.SensorValuesLb.BottomLeft = 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft * 2.20462251f;
                    this.mWiimoteState.BalanceBoardState.SensorValuesLb.BottomRight = 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight * 2.20462251f;
                    this.mWiimoteState.BalanceBoardState.WeightKg = 
                        (this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight) / 4f;
                    this.mWiimoteState.BalanceBoardState.WeightLb = 
                        (this.mWiimoteState.BalanceBoardState.SensorValuesLb.TopLeft + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesLb.TopRight + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesLb.BottomLeft + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesLb.BottomRight) / 4f;
                    float num5 = (this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft) / 
                        (this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight);
                    float num6 = (this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight) / 
                        (this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft + 
                        this.mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight);
                    this.mWiimoteState.BalanceBoardState.CenterOfGravity.X = (num5 - 1f) / (num5 + 1f) * -21f;
                    this.mWiimoteState.BalanceBoardState.CenterOfGravity.Y = (num6 - 1f) / (num6 + 1f) * -12f;
                    break;





                default:
                    return;
            }
        }
        private float GetBalanceBoardSensorValue(short sensor, short min, short mid, short max)
        {
            if (max == mid || mid == min)
            {
                return 0f;
            }
            if (sensor < mid)
            {
                return 68f * ((float)(sensor - min) / (float)(mid - min));
            }
            return 68f * ((float)(sensor - mid) / (float)(max - mid)) + 68f;
        }
        private void ParseReadData(byte[] buff)
        {
            if ((buff[3] & 8) != 0)
            {
                throw new WiimoteException("Error reading data from Wiimote: Bytes do not exist.");
            }
            if ((buff[3] & 7) != 0)
            {
                throw new WiimoteException("Error reading data from Wiimote: Attempt to read from write-only registers.");
            }
            int num = (buff[3] >> 4) + 1;
            int num2 = (int)buff[4] << 8 | (int)buff[5];
            Array.Copy(buff, 6, this.mReadBuff, num2 - this.mAddress, num);
            if (this.mAddress + (int)this.mSize == num2 + num)
            {
                this.mReadDone.Set();
            }
        }
        private byte GetRumbleBit()
        {
            return this.mWiimoteState.Rumble ? (byte)1 : (byte)0;
        }
        private void ReadWiimoteCalibration()
        {
            byte[] array = this.ReadData(22, 7);
            this.mWiimoteState.AccelCalibrationInfo.X0 = array[0];
            this.mWiimoteState.AccelCalibrationInfo.Y0 = array[1];
            this.mWiimoteState.AccelCalibrationInfo.Z0 = array[2];
            this.mWiimoteState.AccelCalibrationInfo.XG = array[4];
            this.mWiimoteState.AccelCalibrationInfo.YG = array[5];
            this.mWiimoteState.AccelCalibrationInfo.ZG = array[6];
        }
        public void SetReportType(InputReport type, bool continuous)
        {
            this.SetReportType(type, IRSensitivity.Maximum, continuous);
        }
        public void SetReportType(InputReport type, IRSensitivity irSensitivity, bool continuous)
        {
            long
                token1 = -1541405694;
            if (this.mWiimoteState.ExtensionType == (ExtensionType)((ulong)token1))
            {
                type = InputReport.ButtonsExtension;
            }
            InputReport inputReport = type;
            if (inputReport != InputReport.IRAccel)
            {
                if (inputReport != InputReport.IRExtensionAccel)
                {
                    this.DisableIR();
                }
                else
                {
                    this.EnableIR(IRMode.Basic, irSensitivity);
                }
            }
            else
            {
                this.EnableIR(IRMode.Extended, irSensitivity);
            }
            this.ClearReport();
            this.mBuff[0] = 18;
            this.mBuff[1] = (byte)((continuous ? 4 : 0) | (this.mWiimoteState.Rumble ? 1 : 0));
            this.mBuff[2] = (byte)type;
            this.WriteReport();
        }
        public void SetLEDs(bool led1, bool led2, bool led3, bool led4)
        {
            this.mWiimoteState.LEDState.LED1 = led1;
            this.mWiimoteState.LEDState.LED2 = led2;
            this.mWiimoteState.LEDState.LED3 = led3;
            this.mWiimoteState.LEDState.LED4 = led4;
            this.ClearReport();
            this.mBuff[0] = 17;
            this.mBuff[1] = (byte)((led1 ? 16 : 0) | (led2 ? 32 : 0) | (led3 ? 64 : 0) | (led4 ? 128 : 0) | this.GetRumbleBit());
            this.WriteReport();
        }
        public void SetLEDs(int leds)
        {
            this.mWiimoteState.LEDState.LED1 = ((leds & 1) > 0);
            this.mWiimoteState.LEDState.LED2 = ((leds & 2) > 0);
            this.mWiimoteState.LEDState.LED3 = ((leds & 4) > 0);
            this.mWiimoteState.LEDState.LED4 = ((leds & 8) > 0);
            this.ClearReport();
            this.mBuff[0] = 17;
            this.mBuff[1] = (byte)((((leds & 1) > 0) ? 16 : 0) | (((leds & 2) > 0) ? 32 : 0) | (((leds & 4) > 0) ? 64 : 0) | (((leds & 8) > 0) ? 128 : 0) | this.GetRumbleBit());
            this.WriteReport();
        }
        public void SetRumble(bool on)
        {
            this.mWiimoteState.Rumble = on;
            this.SetLEDs(
                this.mWiimoteState.LEDState.LED1, 
                this.mWiimoteState.LEDState.LED2, 
                this.mWiimoteState.LEDState.LED3, 
                this.mWiimoteState.LEDState.LED4);
        }
        public void GetStatus()
        {
            this.ClearReport();
            this.mBuff[0] = 21;
            this.mBuff[1] = this.GetRumbleBit();
            this.WriteReport();
            if (!this.mStatusDone.WaitOne(3000, false))
            {
                throw new WiimoteException("Timed out waiting for status report");
            }
        }
        private void EnableIR(IRMode mode, IRSensitivity irSensitivity)
        {
            this.mWiimoteState.IRState.Mode = mode;
            this.ClearReport();
            this.mBuff[0] = 19;
            this.mBuff[1] = (byte)(4 | this.GetRumbleBit());
            this.WriteReport();
            this.ClearReport();
            this.mBuff[0] = 26;
            this.mBuff[1] = (byte)(4 | this.GetRumbleBit());
            this.WriteReport();
            this.WriteData(78643248, 8);
            switch (irSensitivity)
            {
                case IRSensitivity.WiiLevel1:
                    this.WriteData(78643200, 9, new byte[]
				{
					2,
					0,
					0,
					113,
					1,
					0,
					100,
					0,
					254
				});
                    this.WriteData(78643226, 2, new byte[]
				{
					253,
					5
				});
                    break;
                case IRSensitivity.WiiLevel2:
                    this.WriteData(78643200, 9, new byte[]
				{
					2,
					0,
					0,
					113,
					1,
					0,
					150,
					0,
					180
				});
                    this.WriteData(78643226, 2, new byte[]
				{
					179,
					4
				});
                    break;
                case IRSensitivity.WiiLevel3:
                    this.WriteData(78643200, 9, new byte[]
				{
					2,
					0,
					0,
					113,
					1,
					0,
					170,
					0,
					100
				});
                    this.WriteData(78643226, 2, new byte[]
				{
					99,
					3
				});
                    break;
                case IRSensitivity.WiiLevel4:
                    this.WriteData(78643200, 9, new byte[]
				{
					2,
					0,
					0,
					113,
					1,
					0,
					200,
					0,
					54
				});
                    this.WriteData(78643226, 2, new byte[]
				{
					53,
					3
				});
                    break;
                case IRSensitivity.WiiLevel5:
                    this.WriteData(78643200, 9, new byte[]
				{
					7,
					0,
					0,
					113,
					1,
					0,
					114,
					0,
					32
				});
                    this.WriteData(78643226, 2, new byte[]
				{
					1,
					3
				});
                    break;
                case IRSensitivity.Maximum:
                    {
                        this.WriteData(78643200, 9, new byte[]
				{
					2,
					0,
					0,
					113,
					1,
					0,
					144,
					0,
					65
				});
                        int arg_215_1 = 78643226;
                        byte arg_215_2 = 2;
                        byte[] array = new byte[2];
                        array[0] = 64;
                        this.WriteData(arg_215_1, arg_215_2, array);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("irSensitivity");
            }
            this.WriteData(78643251, (byte)mode);
            this.WriteData(78643248, 8);
        }
        private void DisableIR()
        {
            this.mWiimoteState.IRState.Mode = IRMode.Off;
            this.ClearReport();
            this.mBuff[0] = 19;
            this.mBuff[1] = this.GetRumbleBit();
            this.WriteReport();
            this.ClearReport();
            this.mBuff[0] = 26;
            this.mBuff[1] = this.GetRumbleBit();
            this.WriteReport();
        }
        private void ClearReport()
        {
            Array.Clear(this.mBuff, 0, 22);
        }
        private void WriteReport()
        {
            if (this.mAltWriteMethod)
            {
                HIDImports.HidD_SetOutputReport(this.mHandle.DangerousGetHandle(), this.mBuff, (uint)this.mBuff.Length);
            }
            else
            {
                if (this.mStream != null)
                {
                    this.mStream.Write(this.mBuff, 0, 22);
                }
            }
            if (this.mBuff[0] == 22)
            {
                this.mWriteDone.WaitOne(1000, false);
            }
        }
        public byte[] ReadData(int address, short size)
        {
            long
                token1 = -16777216;
            this.ClearReport();
            this.mReadBuff = new byte[(int)size];
            this.mAddress = (address & 65535);
            this.mSize = size;
            this.mBuff[0] = 23;
            this.mBuff[1] = (byte)(((long)address & (long)((ulong)token1)) >> 24 | (long)((ulong)this.GetRumbleBit()));
            this.mBuff[2] = (byte)((address & 16711680) >> 16);
            this.mBuff[3] = (byte)((address & 65280) >> 8);
            this.mBuff[4] = (byte)(address & 255);
            this.mBuff[5] = (byte)(((int)size & 65280) >> 8);
            this.mBuff[6] = (byte)(size & 255);
            this.WriteReport();
            if (!this.mReadDone.WaitOne(1000, false))
            {
                throw new WiimoteException("Error reading data from Wiimote...is it connected?");
            }
            return this.mReadBuff;
        }
        public void WriteData(int address, byte data)
        {
            this.WriteData(address, 1, new byte[]
			{
				data
			});
        }
        public void WriteData(int address, byte size, byte[] buff)
        {
            long token1 = -16777216;
            this.ClearReport();
            this.mBuff[0] = 22;
            this.mBuff[1] = (byte)(((long)address & (long)((ulong)token1)) >> 24 | (long)((ulong)this.GetRumbleBit()));
            this.mBuff[2] = (byte)((address & 16711680) >> 16);
            this.mBuff[3] = (byte)((address & 65280) >> 8);
            this.mBuff[4] = (byte)(address & 255);
            this.mBuff[5] = size;
            Array.Copy(buff, 0, this.mBuff, 6, (int)size);
            this.WriteReport();
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Disconnect();
            }
        }
    }
}
