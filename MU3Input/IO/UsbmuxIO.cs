using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MU3Input
{
    public class UsbmuxIO : IO
    {
        private ushort remotePort = 4354;
        iDeviceConnectionHandle connection;
        private IiDeviceApi iDevice = LibiMobileDevice.Instance.iDevice;
        private ILockdownApi lockdown = LibiMobileDevice.Instance.Lockdown;
        iDeviceEventCallBack deviceEventCallBack;
        public List<string> Devices = new List<string>();
        protected OutputData data;
        public override bool IsConnected => !(connection?.IsClosed ?? true);
        public override OutputData Data => data;
        static UsbmuxIO()
        {
            NativeLibraries.Load();
        }
        ~UsbmuxIO()
        {
            connection?.Close();
        }
        public UsbmuxIO(ushort remotePort)
        {
            this.remotePort = remotePort;
            deviceEventCallBack = new iDeviceEventCallBack(DeviceEventCallback);
            data = new OutputData() { Buttons = new byte[10], AimiId = new byte[10] };
            iDevice.idevice_event_subscribe(deviceEventCallBack, IntPtr.Zero);
            new Thread(PollThread).Start();
        }

        bool connecting = false;
        public override void Reconnect()
        {
            Disconnect();
            Connect();
        }
        public void Disconnect()
        {
            connection?.Close();
        }
        public void Connect()
        {
            if (connecting) return;
            if (IsConnected) return;
            connecting = true;
            string[] devices = Devices.ToArray();
            foreach (var device in devices)
            {
                if (!ConnectByUdid(device, out connection).IsError())
                {
                    SetLed(currentLedData);
                    break;
                }
            }
            connecting = false;
        }
        private byte[] _inBuffer = new byte[24];
        private void PollThread()
        {
            while (true)
            {
                if (!IsConnected)
                {
                    Reconnect();
                    continue;
                }
                uint len = 0;
                iDevice.idevice_connection_receive(connection, _inBuffer, (uint)_inBuffer.Length, ref len);
                if (len <= 0)
                {
                    Reconnect();
                    continue;
                }
                var temp = _inBuffer.ToStructure<OutputData>();
                if (temp.Scan == 1 && temp.AimiId.All(n => n == 255))
                {
                    temp.AimiId = Utils.ReadOrCreateAimeTxt();
                }
                data = temp;
            }
        }
        private uint currentLedData = 0;
        public override void SetLed(uint data)
        {
            try
            {
                // 缓存led数据将其设置到新连接的设备
                currentLedData = data;
                if (!IsConnected)
                    return;
                uint sendBytes = 0;
                iDevice.idevice_connection_send(connection, BitConverter.GetBytes(data), 4, ref sendBytes);
            }
            catch
            {
                return;
            }
        }
        private iDeviceError ConnectByUdid(string udid, out iDeviceConnectionHandle connection)
        {
            iDevice.idevice_new(out iDeviceHandle deviceHandle, udid);
            return iDevice.idevice_connect(deviceHandle, remotePort, out connection);
        }
        private void DeviceEventCallback(ref iDeviceEvent e, IntPtr userData)
        {
            var udid = e.udidString;
            switch (e.@event)
            {
                case iDeviceEventType.DeviceAdd:
                    if (!Devices.Any(d => d.Equals(udid)))
                    {
                        Devices.Add(udid);
                    }
                    break;
                case iDeviceEventType.DeviceRemove:
                    var value = Devices.First(d => d.Equals(udid));
                    Devices.Remove(value);
                    break;
                case iDeviceEventType.DevicePaired:
                    break;
                default:
                    break;
            }
        }
        private string GetDeviceName(string udid)
        {
            iDevice.idevice_new(out iDeviceHandle deviceHandle, udid).ThrowOnError();
            LockdownClientHandle lockdownHandle;
            lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "geki").ThrowOnError();
            string deviceName;
            lockdown.lockdownd_get_device_name(lockdownHandle, out deviceName).ThrowOnError();
            deviceHandle.Dispose();
            lockdownHandle.Dispose();
            return deviceName;
        }
    }
}
