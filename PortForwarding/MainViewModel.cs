using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;

using PropertyChanged;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PortForwarding
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class MainViewModel
    {
        static MainViewModel()
        {
            NativeLibraries.Load();
        }

        private IiDeviceApi iDevice = LibiMobileDevice.Instance.iDevice;
        private ILockdownApi lockdown = LibiMobileDevice.Instance.Lockdown;

        public ushort LocalPort { get; set; }
        public ushort RemotePort { get; set; }
        public int SelectedDeviceIndex { get; set; }
        private ObservableCollection<(string name, string udid)> Devices { get; set; } = new ObservableCollection<(string, string)>();

        public MainViewModel()
        {
            iDevice.idevice_event_subscribe(new iDeviceEventCallBack(DeviceEventCallback), IntPtr.Zero);
        }

        private void DeviceEventCallback(ref iDeviceEvent e, IntPtr userData)
        {
            var udid = e.udidString;
            switch (e.@event)
            {
                case iDeviceEventType.DeviceAdd:
                    string name = GetDeviceName(udid);
                    if (!Devices.Any(d => d.udid.Equals(udid))) Devices.Add((name, udid));
                    break;
                case iDeviceEventType.DeviceRemove:
                    var value = Devices.First(d => d.udid.Equals(udid));
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
            iDeviceHandle deviceHandle;
            iDevice.idevice_new(out deviceHandle, udid).ThrowOnError();
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
