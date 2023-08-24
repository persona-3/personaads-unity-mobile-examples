using System;

public class OnAdClickEventProperties
{
    public string placementId;
    public string walletAddress;
    public string userEmail;
    public DeviceMetadata deviceMetadata;
    public string creativeId;

    public OnAdClickEventProperties(string placementId, string walletAddress, string userEmail, DeviceMetadata deviceMetadata, string creativeId)
    {
        this.placementId = placementId;
        this.walletAddress = walletAddress;
        this.userEmail = userEmail;
        this.deviceMetadata = deviceMetadata;
        this.creativeId = creativeId;
    }
}

