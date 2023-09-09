using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sentry;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

namespace IO.Persona.MobileAds.Unity
{
    public class Util
    {
        public static bool IsCredentialsValid(Environment? currentEnvironment, string adUnitId)
        {
            string apiKey = PersonaAdSDK.GetApiKey();

            if (apiKey == null || currentEnvironment == null)
            {
                if (apiKey == null) SentrySdk.CaptureMessage("apiKey is null", SentryLevel.Error);
                if (currentEnvironment == null) SentrySdk.CaptureMessage("currentEnvironment is null", SentryLevel.Error);
                if (currentEnvironment != Environment.PRODUCTION)
                {
                    Debug.LogError("API Key or Environment can't be empty!");
                }
                return false;
            }

            if (adUnitId == null || adUnitId.Trim().ToString().Length == 0)
            {
                SentrySdk.CaptureMessage("No Ad unit ID found", SentryLevel.Error);
                if (currentEnvironment != Environment.PRODUCTION)
                {
                    Debug.LogError("Ad Unit Id can't be empty!");
                }
                return false;
            }
            return true;
        }

        public static string GenerateRequestId()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetBaseUrl(Environment? currentEnvironment)
        {
            switch (currentEnvironment)
            {
                case Environment.DEVELOPMENT: return Constants.DEVELOPMENT_API_BASE_URL;
                case Environment.STAGING: return Constants.STAGING_API_BASE_URL;
                case Environment.PRODUCTION: return Constants.PRODUCTION_API_BASE_URL;
                default: return "";
            }
        }


        public static string GetFallbackImageUrl(int width, int height)
        {
            return ("https://storage.googleapis.com/fallback-ad-inventory/" + width.ToString() + "x" + height.ToString() + ".png");
        }

        public static async Task<Texture2D> DownloadImageFromUrl(string mediaUrl)
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

        public static async Task<byte[]> DownloadGifFromUrl(string url)
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

        public static List<(Texture2D texture, float delay)> SplitGifIntoFrames(byte[] gifBytes)
        {
            SentrySdk.AddBreadcrumb(message: "Beginning SplitGifIntoFrames", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            List<(Texture2D, float)> mFrames = new List<(Texture2D, float)>();

            using (var decoder = new MG.GIF.Decoder(gifBytes))
            {
                SentrySdk.AddBreadcrumb(message: $"decoder- {decoder}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                var img = decoder.NextImage();
                SentrySdk.AddBreadcrumb(message: $"img- {img}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                while (img != null)
                {
                    mFrames.Add((img.CreateTexture(), img.Delay / 1000.0f));
                    img = decoder.NextImage();
                }
            }
            SentrySdk.AddBreadcrumb(message: $"mFrames length- {mFrames.Count}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            return mFrames;
        }

        public static IEnumerator PlayGifAnimation(List<(Texture2D texture, float delay)> textures, RawImage rawImage)
        {
            SentrySdk.AddBreadcrumb(message: "Beginning PlayGifAnimation", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            int currentFrameIndex = 0;
            if (textures != null && textures.Count > 0) SentrySdk.AddBreadcrumb(message: $"textures[0]- {textures[0]}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            else SentrySdk.AddBreadcrumb(message: $"textures- {textures}");
            SentrySdk.AddBreadcrumb(message: $"rawImage- {rawImage}", category: "sdk.milestone", level: BreadcrumbLevel.Info);

            while (true)
            {
                var frameData = textures[currentFrameIndex];

                rawImage.texture = frameData.texture;

                currentFrameIndex = (currentFrameIndex + 1) % textures.Count;

                yield return new WaitForSeconds(frameData.delay);
            }
        }
    }
}
