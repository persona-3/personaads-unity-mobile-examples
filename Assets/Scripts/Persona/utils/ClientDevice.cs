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

    public class ClientDevice: IClientDevice
    {
        private IAPIClient _apiclient;

        public ClientDevice(IAPIClient apiClient)
        {
            _apiclient = apiClient;
        }

        public async Task<DeviceMetadata> GetDeviceMetadata()
        {
            string ipAddress = await GetIpAddress();
            string userAgent = GetUserAgent();
            string deviceOrientation = Input.deviceOrientation.ToString();
            BrowserInfo browserInfo = GetBrowserInfo();
            string devicePlatform = "mobile";
            string deviceAdvertisingId = SystemInfo.deviceUniqueIdentifier;

            DeviceMetadata deviceMetadata = new DeviceMetadata(ipAddress, userAgent, deviceOrientation, browserInfo, devicePlatform, deviceAdvertisingId);
            return deviceMetadata;
        }

        private async Task<string> GetIpAddress()
        {
            try
            {
                string response = await _apiclient.MakeGetRequestAsync(Constants.JSON_IP_URL, null, null);
                JsonIpApiResponse responseObj = JsonUtility.FromJson<JsonIpApiResponse>(response);
                return responseObj.ip;
            }
            catch(Exception e)
            {
                SentrySdk.CaptureException(e);
                return "";
            }
        }

        private string GetUserAgent()
        {
            string platform;
            string browser = "Unity";
            string version = Application.unityVersion;

            // Get the platform information
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    platform = "Android";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platform = "iOS";
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platform = "Windows";
                    break;
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    platform = "Mac OS X";
                    break;
                // Add more cases for other platforms if needed
                default:
                    platform = "Unknown";
                    break;
            }

            // Combine the information into a user agent string
            string userAgent = $"{browser}/{version} ({platform}; {SystemInfo.deviceModel}; {SystemInfo.operatingSystem})";

            // Remove special characters from the userAgent
            userAgent = Regex.Replace(userAgent, @"[^\u0020-\u007E]", "");

            return userAgent;
        }

        private BrowserInfo GetBrowserInfo()
        {
            string operatingSystem = SystemInfo.operatingSystem;

            DeviceType deviceType = SystemInfo.deviceType;

            return new BrowserInfo("Unity", operatingSystem, deviceType.ToString());
        }
    }
}
