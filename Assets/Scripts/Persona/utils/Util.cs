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
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl))
            {
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    await Task.Yield(); // Yield to prevent blocking the main thread
                }

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Image download error: " + request.error);
                    return null;
                }
                else
                {
                    Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
                    return downloadedTexture;
                }
            }
        }

        public static async Task<List<(Texture2D, float)>> DownloadGifFromUrl(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))

            {
                UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    return null;
                }
                else
                {
                    byte[] gifBytes = webRequest.downloadHandler.data;
                    SimpleGif.Gif gif = SimpleGif.Gif.Decode(gifBytes);

                    // Convert the GIF frames into Unity sprites
                    List<(Texture2D, float)> textureFrames = new List<(Texture2D, float)>();
                    foreach (var frame in gif.Frames)
                    {
                        Texture2D texture = new Texture2D(frame.Texture.width, frame.Texture.height);
                        SimpleGif.Data.Color32[] frameColors = frame.Texture.GetPixels32();

                        // Convert color data to Unity format
                        Color32[] convertedColors = new Color32[frame.Texture.GetPixels32().Length];
                        for (int i = 0; i < frameColors.Length; i++)
                        {
                            convertedColors[i] = new Color32(
                                frameColors[i].r,
                                frameColors[i].g,
                                frameColors[i].b,
                                frameColors[i].a
                                );
                        }

                        texture.SetPixels32(convertedColors);
                        texture.Apply();

                        float delayInSeconds = frame.Delay;

                        //Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                        textureFrames.Add((texture, delayInSeconds));
                    }
                    return textureFrames;
                }
            }
        }

        public static IEnumerator PlayGifAnimation(List<(Texture2D texture, float delay)> textures, RawImage rawImage)
        {
            int currentFrameIndex = 0;

            while (true)
            {
                var frameData = textures[currentFrameIndex];

                rawImage.texture = frameData.texture;
                //rawImage.rectTransform.sizeDelta = new Vector2(frameData.sprite.rect.width, frameData.sprite.rect.height);

                currentFrameIndex = (currentFrameIndex + 1) % textures.Count;

                yield return new WaitForSeconds(frameData.delay);
            }
        }
    }
}
