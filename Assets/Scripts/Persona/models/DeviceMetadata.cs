using System;

namespace IO.Persona.MobileAds.Unity
{
    [Serializable]
    public class DeviceMetadata
    {
        public string deviceOrientation;
        public string os;
        public string browser;
        public string deviceType;
        public string devicePlatform;
        public string deviceAdvertisingId;

        public DeviceMetadata(string deviceOrientation, BrowserInfo browserInfo, string devicePlatform, string deviceAdvertisingId)
        {
            this.deviceOrientation = deviceOrientation;
            this.os = browserInfo?.os;
            this.browser = browserInfo?.browser;
            this.deviceType = browserInfo?.deviceType;
            this.devicePlatform = devicePlatform;
            this.deviceAdvertisingId = deviceAdvertisingId;
        }
    }
}