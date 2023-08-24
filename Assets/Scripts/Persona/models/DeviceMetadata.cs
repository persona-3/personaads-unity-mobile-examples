using System;


[Serializable]
public class DeviceMetadata
{
    public string ipAddress;
    public string userAgent;
    public string deviceOrientation;
    public string os;
    public string browser;
    public string deviceType;
    public string devicePlatform;
    public string deviceAdvertisingId;

    public DeviceMetadata(string ipAddress, string userAgent, string deviceOrientation, BrowserInfo browserInfo, string devicePlatform, string deviceAdvertisingId)
    {
        this.ipAddress = ipAddress;
        this.userAgent = userAgent;
        this.deviceOrientation = deviceOrientation;
        this.os = browserInfo?.os;
        this.browser = browserInfo?.browser;
        this.deviceType = browserInfo?.deviceType;
        this.devicePlatform = devicePlatform;
        this.deviceAdvertisingId = deviceAdvertisingId;
    }
}