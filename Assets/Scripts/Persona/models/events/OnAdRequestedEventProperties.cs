using System;

namespace IO.Persona.MobileAds.Unity
{
    public class OnAdRequestedEventProperties
    {
        public string placementId;
        public string walletAddress;
        public string userEmail;
        public DeviceMetadata deviceMetadata;

        public OnAdRequestedEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata)
        {
            this.placementId = placementId;
            this.walletAddress = walletAddress;
            this.userEmail = userEmail;
            this.deviceMetadata = deviceMetadata;
        }
    }
}

