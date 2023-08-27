using System;

namespace IO.Persona.MobileAds.Unity
{
    [Serializable]
    public class BrowserInfo
    {
        public readonly string browser;
        public readonly string os;
        public readonly string deviceType;

        public BrowserInfo(string browser, string os, string deviceType)
        {
            this.browser = browser;
            this.os = os;
            this.deviceType = deviceType;
        }
    }
}
