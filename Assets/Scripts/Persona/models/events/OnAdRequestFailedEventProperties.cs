using System;

namespace IO.Persona.MobileAds.Unity
{
    public class OnAdRequestFailedEventProperties
    {
        public string placementId;
        public string walletAddress;
        public string userEmail;
        public DeviceMetadata deviceMetadata;
        public long errorStatus;
        public string errorMessage;
        public string release;

        public OnAdRequestFailedEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata, long errorStatus, string errorMessage, string release)
        {
            this.placementId = placementId;
            this.walletAddress = walletAddress;
            this.userEmail = userEmail;
            this.deviceMetadata = deviceMetadata;
            this.errorStatus = errorStatus;
            this.errorMessage = errorMessage;
            this.release = release;
        }
    }
}

