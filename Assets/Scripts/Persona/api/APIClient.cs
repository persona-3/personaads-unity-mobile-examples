using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System;
using Sentry;

namespace IO.Persona.MobileAds.Unity
{
    public interface IAPIClient
    {
        Task<string> MakeGetRequestAsync(string url, Dictionary<string, string> queryParams, Dictionary<string, string> headers);
        Task<string> MakePostRequestAsync(string url, string postData, Dictionary<string, string> queryParams, Dictionary<string, string> headers);
        Task<Texture2D> DownloadImageFromUrl(string mediaUrl);
        Task<byte[]> DownloadGifFromUrl(string url);

    }

    public class APIClient : IAPIClient
    {
        public APIClient() { }


        public async Task<string> MakeGetRequestAsync(string url, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            // Construct the query parameters
            if (queryParams != null && queryParams.Count > 0)
            {
                StringBuilder queryString = new StringBuilder("?");
                foreach (var param in queryParams)
                {
                    queryString.Append($"{UnityWebRequest.EscapeURL(param.Key)}={UnityWebRequest.EscapeURL(param.Value)}&");
                }
                url += queryString.ToString().TrimEnd('&');
            }

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))

            {
                headers ??= new Dictionary<string, string>();

                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }

                SentrySdk.AddBreadcrumb(message: $"MakeGetRequestAsync url - {url}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    throw new UnityWebRequestException(webRequest);
                }
                else
                {
                    string response = webRequest.downloadHandler.text;
                    long statusCode = webRequest.responseCode;
                    ApiResponse apiResponse = new ApiResponse(statusCode, response);
                    string apiResponseJson = JsonUtility.ToJson(apiResponse);
                    return apiResponseJson;
                }
            }
        }

        public async Task<string> MakePostRequestAsync(string url, string postData, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            // Construct the query parameters
            if (queryParams != null && queryParams.Count > 0)
            {
                StringBuilder queryString = new StringBuilder("?");
                foreach (var param in queryParams)
                {
                    queryString.Append($"{UnityWebRequest.EscapeURL(param.Key)}={UnityWebRequest.EscapeURL(param.Value)}&");
                }
                url += queryString.ToString().TrimEnd('&');
            }

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] postDataBytes = Encoding.UTF8.GetBytes(postData);
                webRequest.uploadHandler = new UploadHandlerRaw(postDataBytes);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                headers ??= new Dictionary<string, string>();

                headers.Add("Content-Type", "application/json");

                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
                SentrySdk.AddBreadcrumb(message: $"MakePostRequestAsync url - {url}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(webRequest.error);
                }
                else
                {
                    string response = webRequest.downloadHandler.text;
                    return response;
                }
            }
        }

        public async Task<Texture2D> DownloadImageFromUrl(string mediaUrl)
        {
            SentrySdk.AddBreadcrumb(message: "Beginning DownloadImageFromUrl", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl))
            {
                SentrySdk.AddBreadcrumb(message: $"DownloadImageFromUrl url - {mediaUrl}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    await Task.Yield(); // Yield to prevent blocking the main thread
                }

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    throw new Exception(request.error);
                    //Debug.LogError("Image download error: " + request.error);
                    //return null;
                }
                else
                {
                    Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
                    return downloadedTexture;
                }
            }
        }

        public async Task<byte[]> DownloadGifFromUrl(string url)
        {
            SentrySdk.AddBreadcrumb(message: "Beginning DownloadGifFromUrl", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))

            {
                SentrySdk.AddBreadcrumb(message: $"DownloadGifFromUrl url - {url}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(webRequest.error);
                    //SentrySdk.CaptureException(new Exception(webRequest.error));
                    //Debug.LogError("Error: " + webRequest.error);
                    //return null;
                }
                else
                {
                    byte[] gifBytes = webRequest.downloadHandler.data;
                    SentrySdk.AddBreadcrumb(message: $"gifBytes- {gifBytes}", category: "sdk.milestone", level: BreadcrumbLevel.Info);

                    return gifBytes;
                }
            }
        }
    }
}
