using System;

namespace IO.Persona.MobileAds.Unity
{
    public class OnAdLoadFailedEventProperties
    {
        public string placementId;
        public string walletAddress;
        public string userEmail;
        public DeviceMetadata deviceMetadata;
        public string creativeId;
        public int errorStatus;
        public string errorMessage;
        public string release;

        public OnAdLoadFailedEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata, string creativeId, int errorStatus, string errorMessage, string release)
        {
            this.placementId = placementId;
            this.walletAddress = walletAddress;
            this.userEmail = userEmail;
            this.deviceMetadata = deviceMetadata;
            this.creativeId = creativeId;
            this.errorStatus = errorStatus;
            this.errorMessage = errorMessage;
            this.release = release;
        }
    }
}

