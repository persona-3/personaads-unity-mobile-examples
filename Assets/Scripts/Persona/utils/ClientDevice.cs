using System;
using UnityEngine;
using System.Threading.Tasks;
using Sentry;
using System.Text.RegularExpressions;

namespace IO.Persona.MobileAds.Unity
{
    public interface IClientDevice
    {
        Task<DeviceMetadata> GetDeviceMetadata();
    }

    public class ClientDevice : IClientDevice
    {
        private IAPIClient _apiclient;

        public ClientDevice(IAPIClient apiClient)
        {
            _apiclient = apiClient;
        }

        public async Task<DeviceMetadata> GetDeviceMetadata()
        {
            string deviceOrientation = Input.deviceOrientation.ToString();
            BrowserInfo browserInfo = GetBrowserInfo();
            string devicePlatform = "mobile";
            string deviceAdvertisingId = SystemInfo.deviceUniqueIdentifier;

            DeviceMetadata deviceMetadata = new DeviceMetadata(deviceOrientation, browserInfo, devicePlatform, deviceAdvertisingId);
            return deviceMetadata;
        }

        private BrowserInfo GetBrowserInfo()
        {
            string operatingSystem = SystemInfo.operatingSystem;

            DeviceType deviceType = SystemInfo.deviceType;

            return new BrowserInfo("Unity", operatingSystem, deviceType.ToString());
        }
    }
}
