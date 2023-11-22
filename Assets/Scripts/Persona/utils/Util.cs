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
        public static bool IsCredentialsValid(string apiKey, Environment? currentEnvironment, string adUnitId)
        {
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
            return (Constants.FALLBACK_MEDIA_BASE_URL + width.ToString() + "x" + height.ToString() + ".png");
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

        public static string AppendQueryParam(string baseUrl, string newQueryParam)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder(baseUrl);
                string existingQuery = uriBuilder.Query;

                if (existingQuery.StartsWith("?"))
                {
                    existingQuery = existingQuery.Substring(1);
                }

                if (!string.IsNullOrEmpty(existingQuery))
                {
                    existingQuery += "&" + newQueryParam;
                }
                else
                {
                    existingQuery = newQueryParam;
                }

                uriBuilder.Query = existingQuery;
                string updatedUrl = uriBuilder.Uri.ToString();
                return updatedUrl;
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                return baseUrl;
            }
        }
    }
}
