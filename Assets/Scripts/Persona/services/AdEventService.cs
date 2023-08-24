using System;
using System.Collections.Generic;
using Sentry;
using UnityEngine;

public class AdEventService
{
    private readonly string requestId;
    private readonly string adUnitId;
    private readonly string walletAddress;
    private readonly string userEmail;
    private readonly APIClient _apiClient;
    private readonly Environment currentEnvironment;

    public AdEventService(string requestId, string adUnitId, string walletAddress, string userEmail, Environment currentEnvironment)
    {
        this.requestId = requestId;
        this.adUnitId = adUnitId;
        this.walletAddress = walletAddress;
        this.userEmail = userEmail;
        this._apiClient = new APIClient();
        this.currentEnvironment = currentEnvironment;
    }

    public async void SendAdRequestedEvent(ITransaction transaction)
    {
        string apiUrl = $"{Util.GetBaseUrl(this.currentEnvironment)}/events/ad/request";
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

        DeviceMetadata deviceMetadata = await ClientDevice.GetDeviceMetadata();

        transaction.SetTag("p3-device-ip-address", deviceMetadata.ipAddress);
        transaction.SetTag("p3-device-user-agent", deviceMetadata.userAgent);
        transaction.SetTag("p3-device-orientation", deviceMetadata.deviceOrientation);
        transaction.SetTag("p3-device-os", deviceMetadata.os);
        transaction.SetTag("p3-device-browser", deviceMetadata.browser);
        transaction.SetTag("p3-device-type", deviceMetadata.deviceType);
        transaction.SetTag("p3-device-platform", deviceMetadata.devicePlatform);
        transaction.SetTag("p3-device-advertising-id", deviceMetadata.deviceAdvertisingId);

        OnAdRequestedEventProperties reqBodyProperties = new OnAdRequestedEventProperties(this.adUnitId, this.walletAddress, this.userEmail, deviceMetadata);
        string reqBody = JsonUtility.ToJson(reqBodyProperties);

        this._apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
    }

    public async void SendAdRequestCompletedEvent(FetchCreativeApiResponse fetchedCreative)
    {
        string apiUrl = $"{Util.GetBaseUrl(this.currentEnvironment)}/events/ad/request/complete";
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

        DeviceMetadata deviceMetadata = await ClientDevice.GetDeviceMetadata();

        OnAdRequestCompletedEventProperties reqBodyProperties = new OnAdRequestCompletedEventProperties(this.adUnitId, this.walletAddress, this.userEmail, deviceMetadata, fetchedCreative.data.id);
        string reqBody = JsonUtility.ToJson(reqBodyProperties);

        this._apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
    }

    public async void SendAdRequestFailedEvent(Exception e)
    {
        string apiUrl = $"{Util.GetBaseUrl(this.currentEnvironment)}/events/ad/request/fail";
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

        DeviceMetadata deviceMetadata = await ClientDevice.GetDeviceMetadata();

        OnAdRequestFailedEventProperties reqBodyProperties = new OnAdRequestFailedEventProperties(this.adUnitId, this.walletAddress, this.userEmail, deviceMetadata, 400, e.Message);
        string reqBody = JsonUtility.ToJson(reqBodyProperties);

        this._apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
    }

    public async void SendAdLoadCompletedEvent(FetchCreativeApiResponse fetchedCreative)
    {
        string apiUrl = $"{Util.GetBaseUrl(this.currentEnvironment)}/events/ad/load/complete";
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

        DeviceMetadata deviceMetadata = await ClientDevice.GetDeviceMetadata();

        OnAdLoadCompletedEventProperties reqBodyProperties = new OnAdLoadCompletedEventProperties(this.adUnitId, this.walletAddress, this.userEmail, deviceMetadata, fetchedCreative.data.id);
        string reqBody = JsonUtility.ToJson(reqBodyProperties);

        this._apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
    }

    public async void SendAdLoadFailedEvent(FetchCreativeApiResponse fetchedCreative, Exception e)
    {
        string apiUrl = $"{Util.GetBaseUrl(this.currentEnvironment)}/events/ad/load/fail";
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

        DeviceMetadata deviceMetadata = await ClientDevice.GetDeviceMetadata();

        OnAdLoadFailedEventProperties reqBodyProperties = new OnAdLoadFailedEventProperties(this.adUnitId, this.walletAddress, this.userEmail, deviceMetadata, fetchedCreative.data.id, 400, e.Message);
        string reqBody = JsonUtility.ToJson(reqBodyProperties);

        this._apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
    }

    public async void SendAdImpressionEvent(FetchCreativeApiResponse fetchedCreative, float visiblePercentage, string triggeredAt)
    {
        string apiUrl = $"{Util.GetBaseUrl(this.currentEnvironment)}/events/ad/impression";
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

        DeviceMetadata deviceMetadata = await ClientDevice.GetDeviceMetadata();

        OnAdImpressionEventProperties reqBodyProperties = new OnAdImpressionEventProperties(this.adUnitId, this.walletAddress, this.userEmail, deviceMetadata, fetchedCreative.data.id, visiblePercentage, triggeredAt);
        string reqBody = JsonUtility.ToJson(reqBodyProperties);

        this._apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
    }

    public async void SendAdClickEvent(FetchCreativeApiResponse fetchedCreative)
    {
        string apiUrl = $"{Util.GetBaseUrl(this.currentEnvironment)}/events/ad/click";
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

        DeviceMetadata deviceMetadata = await ClientDevice.GetDeviceMetadata();

        OnAdClickEventProperties reqBodyProperties = new OnAdClickEventProperties(this.adUnitId, this.walletAddress, this.userEmail, deviceMetadata, fetchedCreative.data.id);
        string reqBody = JsonUtility.ToJson(reqBodyProperties);

        this._apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
    }
}
