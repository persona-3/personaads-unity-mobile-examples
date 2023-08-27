using System;

namespace IO.Persona.MobileAds.Unity
{
    public class OnAdLoadCompletedEventProperties
    {
        public string placementId;
        public string walletAddress;
        public string userEmail;
        public DeviceMetadata deviceMetadata;
        public string creativeId;

        public OnAdLoadCompletedEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata, string creativeId)
        {
            this.placementId = placementId;
            this.walletAddress = walletAddress;
            this.userEmail = userEmail;
            this.deviceMetadata = deviceMetadata;
            this.creativeId = creativeId;
        }
    }
}

