using System;
using System.Collections.Generic;
using Sentry;
using UnityEngine;

namespace IO.Persona.MobileAds.Unity
{
    public interface IAdEventService
    {
        void SendAdRequestedEvent(ITransaction transaction);
        void SendAdRequestCompletedEvent(FetchCreativeApiResponse fetchedCreative);
        void SendAdRequestFailedEvent(string? errorMessage, long? errorStatus);
        void SendAdLoadCompletedEvent(FetchCreativeApiResponse fetchedCreative);
        void SendAdLoadFailedEvent(FetchCreativeApiResponse fetchedCreative, Exception e);
        void SendAdImpressionEvent(FetchCreativeApiResponse fetchedCreative, float visiblePercentage, string triggeredAt);
        void SendAdChargeableImpressionEvent(FetchCreativeApiResponse fetchedCreative, float visiblePercentage, string triggeredAt);
        void SendAdClickEvent(FetchCreativeApiResponse fetchedCreative);
    }
    public class AdEventService : IAdEventService
    {
        private readonly string requestId;
        private readonly string adUnitId;
        private readonly IAPIClient _apiClient;
        private readonly IClientIdentity _clientIdentity;
        private readonly IClientDevice _clientDevice;
        private readonly IPersonaAdSDK _personaAdSDK;

        public AdEventService(string requestId, string adUnitId, IClientIdentity clientIdentity, IAPIClient apiClient, IClientDevice clientDevice, IPersonaAdSDK personaAdSDK)
        {
            this.requestId = requestId;
            this.adUnitId = adUnitId;
            _clientIdentity = clientIdentity;
            _apiClient = apiClient;
            _clientDevice = clientDevice;
            _personaAdSDK = personaAdSDK;
        }

        public async void SendAdRequestedEvent(ITransaction transaction)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/request";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            transaction.SetTag("p3-device-orientation", deviceMetadata.deviceOrientation);
            transaction.SetTag("p3-device-os", deviceMetadata.os);
            transaction.SetTag("p3-device-browser", deviceMetadata.browser);
            transaction.SetTag("p3-device-type", deviceMetadata.deviceType);
            transaction.SetTag("p3-device-platform", deviceMetadata.devicePlatform);
            transaction.SetTag("p3-device-advertising-id", deviceMetadata.deviceAdvertisingId);

            OnAdRequestedEventProperties reqBodyProperties = new OnAdRequestedEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }

        public async void SendAdRequestCompletedEvent(FetchCreativeApiResponse fetchedCreative)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/request/complete";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            OnAdRequestCompletedEventProperties reqBodyProperties = new OnAdRequestCompletedEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, fetchedCreative.data.id, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }

        public async void SendAdRequestFailedEvent(string errorMessage, long? errorStatus)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/request/fail";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            OnAdRequestFailedEventProperties reqBodyProperties = new OnAdRequestFailedEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, errorStatus ?? -1, errorMessage, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }

        public async void SendAdLoadCompletedEvent(FetchCreativeApiResponse fetchedCreative)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/load/complete";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            OnAdLoadCompletedEventProperties reqBodyProperties = new OnAdLoadCompletedEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, fetchedCreative.data.id, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }

        public async void SendAdLoadFailedEvent(FetchCreativeApiResponse fetchedCreative, Exception e)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/load/fail";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            OnAdLoadFailedEventProperties reqBodyProperties = new OnAdLoadFailedEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, fetchedCreative.data.id, 400, e.Message, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }

        public async void SendAdImpressionEvent(FetchCreativeApiResponse fetchedCreative, float visiblePercentage, string triggeredAt)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/impression";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            OnAdImpressionEventProperties reqBodyProperties = new OnAdImpressionEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, fetchedCreative.data.id, visiblePercentage, triggeredAt, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }

        public async void SendAdChargeableImpressionEvent(FetchCreativeApiResponse fetchedCreative, float visiblePercentage, string triggeredAt)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/valid-impression";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            OnAdChargeableImpressionEventProperties reqBodyProperties = new OnAdChargeableImpressionEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, fetchedCreative.data.id, visiblePercentage, triggeredAt, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }

        public async void SendAdClickEvent(FetchCreativeApiResponse fetchedCreative)
        {
            string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/events/ad/click";
            Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

            DeviceMetadata deviceMetadata = await _clientDevice.GetDeviceMetadata();
            string userEmail = _clientIdentity.GetUserEmail();
            string walletAddress = _clientIdentity.GetWalletAddress();

            OnAdClickEventProperties reqBodyProperties = new OnAdClickEventProperties(adUnitId, walletAddress, userEmail, deviceMetadata, fetchedCreative.data.id, Constants.SDK_RELEASE_VERSION);
            string reqBody = JsonUtility.ToJson(reqBodyProperties);

            _apiClient.MakePostRequestAsync(apiUrl, reqBody, null, headers);
        }
    }
}
