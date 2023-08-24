using System;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Sentry;
using System.Text.RegularExpressions;

public class ClientDevice
{
    public async static Task<DeviceMetadata> GetDeviceMetadata()
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

    private async static Task<string> GetIpAddress()
    {
        string url = "https://jsonip.com";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                SentrySdk.CaptureException(new Exception(webRequest.error));
                return "";
            }
            else
            {
                string response = webRequest.downloadHandler.text;
                JsonIpApiResponse responseObj = JsonUtility.FromJson<JsonIpApiResponse>(response);
                return responseObj.ip;
            }
        }
    }

    private static string GetUserAgent()
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

    private static BrowserInfo GetBrowserInfo()
    {
        string operatingSystem = SystemInfo.operatingSystem;
        
        DeviceType deviceType = SystemInfo.deviceType;

        return new BrowserInfo("Unity", operatingSystem, deviceType.ToString());
    }
}
