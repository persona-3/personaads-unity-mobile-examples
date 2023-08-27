using System;

namespace IO.Persona.MobileAds.Unity
{
    public class OnAdImpressionEventProperties
    {
        public string placementId;
        public string walletAddress;
        public string userEmail;
        public DeviceMetadata deviceMetadata;
        public string creativeId;
        public float visiblePercentage;
        public string triggeredAt;

        public OnAdImpressionEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata, string creativeId, float visiblePercentage, string triggeredAt)
        {
            this.placementId = placementId;
            this.walletAddress = walletAddress;
            this.userEmail = userEmail;
            this.deviceMetadata = deviceMetadata;
            this.creativeId = creativeId;
            this.visiblePercentage = visiblePercentage;
            this.triggeredAt = triggeredAt;
        }
    }
}

