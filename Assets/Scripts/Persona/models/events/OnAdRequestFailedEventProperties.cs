using System;

namespace IO.Persona.MobileAds.Unity
{
    public class OnAdRequestFailedEventProperties
    {
        public string placementId;
        public string walletAddress;
        public string userEmail;
        public DeviceMetadata deviceMetadata;
        public int errorStatus;
        public string errorMessage;

        public OnAdRequestFailedEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata, int errorStatus, string errorMessage)
        {
            this.placementId = placementId;
            this.walletAddress = walletAddress;
            this.userEmail = userEmail;
            this.deviceMetadata = deviceMetadata;
            this.errorStatus = errorStatus;
            this.errorMessage = errorMessage;
        }
    }
}

