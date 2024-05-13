using System;

namespace IO.Persona.MobileAds.Unity
{
    public class OnAdRequestCompletedEventProperties
    {
        public string placementId;
        public string walletAddress;
        public string userEmail;
        public DeviceMetadata deviceMetadata;
        public string creativeId;
        public string release;

        public OnAdRequestCompletedEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata, string creativeId, string release)
        {
            this.placementId = placementId;
            this.walletAddress = walletAddress;
            this.userEmail = userEmail;
            this.deviceMetadata = deviceMetadata;
            this.creativeId = creativeId;
            this.release = release;
        }
    }
}

