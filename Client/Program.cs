using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WiimoteLib;

namespace Sockets
{
    class Program
    {
        static Wiimote wm = new Wiimote();
        static Socket sender;
        static bool flag = true;
        static byte btnOld = 0x00;

        public static void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.
                //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = IPAddress.Parse("10.0.0.7");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8421);

                // Create a TCP/IP  socket.
                sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);
                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
                }
                catch (ArgumentNullException ane)   { Console.WriteLine("ArgumentNullException : {0}", ane.ToString()); }
                catch (SocketException se)          { Console.WriteLine("SocketException : {0}", se.ToString());        }
                catch (Exception e)                 { Console.WriteLine("Unexpected exception : {0}", e.ToString());    }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public static int Main(String[] args)
        {
            wm.WiimoteChanged += wm_WiimoteChanged;
            wm.WiimoteExtensionChanged += wm_WiimoteExtensionChanged;
            wm.Connect();
            wm.SetReportType(InputReport.IRAccel, true);
            wm.SetLEDs(true, false, false, true);

            StartClient();
            Console.ReadLine();
            return 0;
        }

        private static void wm_WiimoteExtensionChanged(object obj, WiimoteExtensionChangedEventArgs e)
        {
            //Console.Write("{0}\t", wm.WiimoteState.ButtonState.A);

            if (e.Inserted) wm.SetReportType(InputReport.IRExtensionAccel,  true);
            else            wm.SetReportType(InputReport.IRAccel,           true);
        }

        private static void wm_WiimoteChanged(object obj, WiimoteChangedEventArgs e)
        {
            // Data buffer for incoming data. // 1024
            byte[] dIN = new byte[1024];

            // Outgoing data
            float x = ((int)(e.WiimoteState.NunchukState.Joystick.X * 200)) / 100F;
            float y = ((int)(e.WiimoteState.NunchukState.Joystick.Y * 200)) / 100F;

            byte btn2 = 0;
            btn2 |= (byte)((e.WiimoteState.ButtonState.A   ? 1 : 0) << 0);
            btn2 |= (byte)((e.WiimoteState.ButtonState.B   ? 1 : 0) << 1);
            btn2 |= (byte)((e.WiimoteState.ButtonState.One ? 1 : 0) << 2);
            btn2 |= (byte)((e.WiimoteState.ButtonState.Two ? 1 : 0) << 3);
            btn2 |= (byte)((e.WiimoteState.NunchukState.C  ? 1 : 0) << 4);
            btn2 |= (byte)((e.WiimoteState.NunchukState.Z  ? 1 : 0) << 5);

            if (e.WiimoteState.ButtonState.Home)
            {
                // send disconnect signal
                btn2 |= 1 << 7;
            }

            List<byte> msg = new List<byte>();
            msg.Add(btn2);
            msg.AddRange(BitConverter.GetBytes(x));
            msg.AddRange(BitConverter.GetBytes(y));


            // Send the data through the socket.
            if (sender != null && (btn2 != btnOld || e.WiimoteState.NunchukState.Z))
            {
                try
                {
                    int bytesSent = sender.Send(msg.ToArray());
                    Console.WriteLine("{2}\tx: {0}\ty: {1}", x, y, e.WiimoteState.ButtonState.B);
                    if (e.WiimoteState.ButtonState.Home) { flag = false; return; }

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(dIN);

                    btnOld = btn2;
                    Console.WriteLine("Echoed test = {0}", Convert.ToString(msg[0], 2));
                }
                catch { }
            }

            if (e.WiimoteState.ButtonState.Home) EndConnection();
        }

        private static void EndConnection()
        {
            // Release the socket.
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}
