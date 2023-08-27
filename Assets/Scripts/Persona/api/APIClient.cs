using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System;

namespace IO.Persona.MobileAds.Unity
{
    public class APIClient
    {

        public APIClient()
        {

        }


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

                headers.Add("x-api-key", PersonaAdSDK.GetApiKey());
                headers.Add("Package-Name", Application.identifier);

                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }

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
                headers.Add("x-api-key", PersonaAdSDK.GetApiKey());
                headers.Add("Package-Name", Application.identifier);

                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }

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
    }
}
