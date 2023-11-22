﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sentry;
using System.Collections;

namespace IO.Persona.MobileAds.Unity
{
    public class BannerView
    {
        private readonly string adUnitId = "";
        private string userEmail = null;
        private string walletAddress = null;
        private string requestId = "";
        private readonly MonoBehaviour parent;
        Dictionary<string, string> customizedTags = new Dictionary<string, string>();
        private IAPIClient _apiClient;
        private ClientDevice _clientDevice;
        private IAdEventService _adEventService;
        private IPersonaAdSDK _personaAdSDK;

        public BannerView(MonoBehaviour parent, string adUnitId)
        {
            this.adUnitId = adUnitId;
            this.parent = parent;
            _apiClient = new APIClient();
            _clientDevice = new ClientDevice(_apiClient);
            _personaAdSDK = new PersonaAdSDK();
            requestId = Util.GenerateRequestId();
            _adEventService = new AdEventService(requestId, this.adUnitId, walletAddress, userEmail, _apiClient, _clientDevice, _personaAdSDK);
        }

        public BannerView(MonoBehaviour parent, string adUnitId, IPersonaAdSDK personaAdSDK, IAPIClient apiClient, IAdEventService adEventService)
        {
            this.adUnitId = adUnitId;
            this.parent = parent;
            _personaAdSDK = personaAdSDK;
            _apiClient = apiClient;
            _clientDevice = new ClientDevice(_apiClient);
            requestId = Util.GenerateRequestId();
            _adEventService = adEventService;
        }

        public void SetUserEmail(string userEmail)
        {
            this.userEmail = userEmail;
        }

        public void SetWalletAddress(string walletAddress)
        {
            this.walletAddress = walletAddress;
        }

        public string GetUserEmail()
        {
            return userEmail;
        }

        public string GetWalletAddress()
        {
            return walletAddress;
        }

        public async void LoadAd()
        {
            SentrySdk.AddBreadcrumb(message: "loadAd Called", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            ITransaction transaction = SentrySdk.StartTransaction("LoadBannerAd", "banner_ad.load");

            Environment? currentEnvironment = _personaAdSDK.GetEnvironment();
            string apiKey = _personaAdSDK.GetApiKey();
            if (!Util.IsCredentialsValid(apiKey, currentEnvironment, adUnitId)) return;

            SentrySdk.AddBreadcrumb(message: "null check completed", category: "sdk.milestone", level: BreadcrumbLevel.Info);

            transaction.SetTag("p3-request-id", requestId);
            transaction.SetTag("p3-api-key", apiKey);
            transaction.SetTag("p3-current-environment", currentEnvironment.ToString());
            transaction.SetTag("p3-ad-unit-id", adUnitId);
            transaction.SetTag("p3-user-email", userEmail);
            transaction.SetTag("p3-wallet-address", walletAddress);
            transaction.SetTag("p3-app-package-name", Application.identifier);
            customizedTags.Add("p3-request-id", requestId);
            customizedTags.Add("p3-api-key", apiKey);
            customizedTags.Add("p3-current-environment", currentEnvironment.ToString());
            customizedTags.Add("p3-ad-unit-id", adUnitId);
            customizedTags.Add("p3-user-email", userEmail);
            customizedTags.Add("p3-wallet-address", walletAddress);
            customizedTags.Add("p3-app-package-name", Application.identifier);

            if (currentEnvironment != Environment.STAGING) _adEventService.SendAdRequestedEvent(transaction);

            try
            {
                ISpan fetchIpAddressSpan = transaction.StartChild("fetch_ip_address", "Fetch IP Address");
                //FetchIpAddress();
                string ipAddress = "";
                SentrySdk.AddBreadcrumb(message: "Calling getIpAddress", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                fetchIpAddressSpan.Finish();
                ISpan fetchCreativeSpan = transaction.StartChild("fetch_creative", "Fetch Creative");
                SentrySdk.AddBreadcrumb(message: "Fetching Creative", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                FetchCreativeApiResponse fetchedCreative = await TriggerFetchCreative(ipAddress);

                transaction.SetTag("p3-creative-id", fetchedCreative.data?.id);
                transaction.SetTag("p3-creative-media-url", fetchedCreative.data?.mediaUrl);
                transaction.SetTag("p3-creative-cta-url", fetchedCreative.data?.ctaUrl);
                transaction.SetTag("p3-creative-width", fetchedCreative.data?.dimensions?.width.ToString());
                transaction.SetTag("p3-creative-height", fetchedCreative.data?.dimensions?.height.ToString());
                customizedTags.Add("p3-creative-id", fetchedCreative.data?.id);
                customizedTags.Add("p3-creative-media-url", fetchedCreative.data?.mediaUrl);
                customizedTags.Add("p3-creative-cta-url", fetchedCreative.data?.ctaUrl);
                customizedTags.Add("p3-creative-width", fetchedCreative.data?.dimensions?.width.ToString());
                customizedTags.Add("p3-creative-height", fetchedCreative.data?.dimensions?.height.ToString());

                fetchCreativeSpan.Finish();

                TriggerFetchCreativeResponseHandler(fetchedCreative, (Environment)currentEnvironment, transaction);
            }
            catch (Exception e)
            {
                SendExceptionToSentry(e, transaction, customizedTags, "LoadAd function");
                transaction.Finish();
                TriggerFetchCreativeErrorHandler(e);
            }


        }

        private async Task<FetchCreativeApiResponse> TriggerFetchCreative(string ipAddress)
        {
            try
            {
                string apiUrl = $"{Util.GetBaseUrl(_personaAdSDK.GetEnvironment())}/creatives";
                Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", requestId },
            { "x-api-key", _personaAdSDK.GetApiKey() },
            { "Package-Name", Application.identifier }
        };

                Dictionary<string, string> queryParams = new Dictionary<string, string>
        {
            { "placementId", adUnitId },
            { "ipAddress", ipAddress },
            { "walletAddress", walletAddress },
            { "userEmail", userEmail }
        };

                string response = await _apiClient.MakeGetRequestAsync(apiUrl, queryParams, headers);
                FetchCreativeApiResponse apiResponse = JsonUtility.FromJson<FetchCreativeApiResponse>(response);
                return apiResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async void TriggerFetchCreativeResponseHandler(FetchCreativeApiResponse fetchedCreative, Environment currentEnvironment, ITransaction transaction)
        {
            ISpan renderBannerAdSpan = transaction.StartChild("render_banner_ad", "Render Banner Ad");
            try
            {
                SentrySdk.AddBreadcrumb(message: "Beginning handleResponse", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                SentrySdk.AddBreadcrumb(message: JsonUtility.ToJson(fetchedCreative), category: "sdk.milestone", level: BreadcrumbLevel.Info);
                if (currentEnvironment == Environment.STAGING)
                {
                    await RenderFallbackMedia(fetchedCreative, renderBannerAdSpan);
                    renderBannerAdSpan.Finish();
                }
                else
                {
                    _adEventService.SendAdRequestCompletedEvent(fetchedCreative);
                    ISpan renderActualImageSpan = renderBannerAdSpan.StartChild("render_actual_image", "Render Actual Image");
                    await RenderFetchedCreative(fetchedCreative, transaction);
                    renderActualImageSpan.Finish();
                    renderBannerAdSpan.Finish();
                    _adEventService.SendAdLoadCompletedEvent(fetchedCreative);
                }
                transaction.Finish();
            }
            catch (Exception e)
            {
                SendExceptionToSentry(e, transaction, customizedTags, "TriggerFetchCreativeResponseHandler function - Outer Exception");
                //SentrySdk.CaptureException(e, (scope) => scope.Transaction = transaction);

                if (currentEnvironment != Environment.STAGING)
                {
                    try
                    {
                        _adEventService.SendAdLoadFailedEvent(fetchedCreative, e);
                        await RenderFallbackMedia(fetchedCreative, renderBannerAdSpan);
                        renderBannerAdSpan.Finish();
                        transaction.Finish();
                    }
                    catch (Exception e2)
                    {
                        SendExceptionToSentry(e2, transaction, customizedTags, "TriggerFetchCreativeResponseHandler function - Inner Exception");
                        //SentrySdk.CaptureException(e2, (scope) => scope.Transaction = transaction);
                        renderBannerAdSpan.Finish();
                        transaction.Finish();
                    }
                }
                else
                {
                    renderBannerAdSpan.Finish();
                    transaction.Finish();
                }
            }
        }

        private void TriggerFetchCreativeErrorHandler(Exception e)
        {
            _adEventService.SendAdRequestFailedEvent(e);
        }


        private async Task<RawImage> RenderFetchedCreative(FetchCreativeApiResponse fetchedCreative, ITransaction transaction)
        {
            try
            {
                SentrySdk.AddBreadcrumb(message: "Beginning RenderFetchedCreative", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                Canvas canvas = parent.GetComponentInParent<Canvas>();
                SentrySdk.AddBreadcrumb(message: $"canvas- {canvas}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                GameObject rawImageGO = parent.gameObject;
                SentrySdk.AddBreadcrumb(message: $"rawImageGO- {rawImageGO}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                RawImage rawImageComponent = rawImageGO.GetComponent<RawImage>();
                SentrySdk.AddBreadcrumb(message: $"rawImageComponent- {rawImageComponent}", category: "sdk.milestone", level: BreadcrumbLevel.Info);

                string mediaUrl = fetchedCreative.data.mediaUrl;

                string[] mediaUrlParts = mediaUrl.Split(".");

                if (mediaUrlParts[mediaUrlParts.Length - 1].Contains("gif"))
                {
                    byte[] gifBytes = await _apiClient.DownloadGifFromUrl(mediaUrl);

                    List<(Texture2D, float)> textureImages = Util.SplitGifIntoFrames(gifBytes);
                    parent.StartCoroutine(Util.PlayGifAnimation(textureImages, rawImageComponent));
                }
                else
                {
                    Texture2D imageTexture = await _apiClient.DownloadImageFromUrl(mediaUrl);

                    rawImageComponent.texture = imageTexture;
                }
                DisplayPersonaTag(rawImageComponent);
                try
                {
                    TrackVisibilityOnScreen(fetchedCreative, rawImageComponent, canvas);

                    Button button = rawImageGO.AddComponent<Button>();
                    button.onClick.AddListener(() =>
                    {
                        _adEventService.SendAdClickEvent(fetchedCreative);
                        string targetUrl = Util.AppendQueryParam(fetchedCreative.data.ctaUrl, $"prsna_id={requestId}");
                        Application.OpenURL(targetUrl);
                    });
                }
                catch (Exception e2)
                {
                    SendExceptionToSentry(e2, transaction, customizedTags, "RenderFetchedCreative function - Inner Exception");
                    //SentrySdk.CaptureException(e2, (scope) => scope.Transaction = transaction);
                }

                return rawImageComponent;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private async void DisplayPersonaTag(RawImage rawImageComponent)
        {
            try
            {
                GameObject watermarkImageGO = new GameObject("WatermarkImage");
                watermarkImageGO.transform.SetParent(rawImageComponent.gameObject.transform);
                RawImage watermarkImageComponent = watermarkImageGO.AddComponent<RawImage>();

                watermarkImageComponent.texture = await _apiClient.DownloadImageFromUrl(Constants.WATERMARK_PERSONA_LOGO);
                CanvasScaler canvasScaler = rawImageComponent.canvas.GetComponent<CanvasScaler>();
                float referenceResolutionScaleFactor = 1f;
                if (canvasScaler != null && canvasScaler.referenceResolution != null)
                {
                    referenceResolutionScaleFactor = (canvasScaler.referenceResolution.x / Constants.REFERENCE_RESOLUTON_X + canvasScaler.referenceResolution.y / Constants.REFERENCE_RESOLUTON_Y) / 2;
                }
                watermarkImageComponent.rectTransform.sizeDelta = new Vector2(Constants.WATERMARK_IMAGE_WIDTH * referenceResolutionScaleFactor * rawImageComponent.canvas.scaleFactor, Constants.WATERMARK_IMAGE_HEIGHT * referenceResolutionScaleFactor * rawImageComponent.canvas.scaleFactor);
                watermarkImageComponent.rectTransform.anchorMin = new Vector2(1f, 1f);
                watermarkImageComponent.rectTransform.anchorMax = new Vector2(1f, 1f);
                watermarkImageComponent.rectTransform.pivot = new Vector2(1f, 1f);
                watermarkImageComponent.rectTransform.anchoredPosition = new Vector2(0, 0);

                GameObject watermarkBackgroundGO = new GameObject("WatermarkBackground");
                watermarkBackgroundGO.transform.SetParent(rawImageComponent.gameObject.transform);
                Image watermarkBackground = watermarkBackgroundGO.AddComponent<Image>();
                watermarkBackground.color = new Color(0, 0, 0, 0.4f);
                watermarkBackground.rectTransform.anchorMin = watermarkImageComponent.rectTransform.anchorMin;
                watermarkBackground.rectTransform.anchorMax = watermarkImageComponent.rectTransform.anchorMax;
                watermarkBackground.rectTransform.pivot = watermarkImageComponent.rectTransform.pivot;
                watermarkBackground.rectTransform.sizeDelta = watermarkImageComponent.rectTransform.sizeDelta;
                watermarkBackground.rectTransform.anchoredPosition = watermarkImageComponent.rectTransform.anchoredPosition;

                GameObject watermarkTextGO = new GameObject("WatermarkText");
                watermarkTextGO.transform.SetParent(rawImageComponent.gameObject.transform);
                Text watermarkTextComponent = watermarkTextGO.AddComponent<Text>();
                watermarkTextComponent.text = Constants.WATERMARK_TEXT;
                // watermarkTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                watermarkTextComponent.font = Resources.Load<Font>("PersonaFonts/Roboto-Regular");
                watermarkTextComponent.fontSize = (int)(Constants.WATERMARK_TEXT_FONT_SIZE * referenceResolutionScaleFactor * rawImageComponent.canvas.scaleFactor);
                watermarkTextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
                watermarkTextComponent.verticalOverflow = VerticalWrapMode.Overflow;
                ContentSizeFitter contentSizeFitter = watermarkTextGO.AddComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                watermarkTextComponent.rectTransform.sizeDelta = new Vector2(watermarkTextComponent.preferredWidth, watermarkTextComponent.preferredHeight);
                watermarkTextComponent.rectTransform.anchorMin = new Vector2(1f, 1f);
                watermarkTextComponent.rectTransform.anchorMax = new Vector2(1f, 1f);
                watermarkTextComponent.rectTransform.pivot = new Vector2(1f, 1f);
                watermarkTextComponent.rectTransform.anchoredPosition = new Vector2(0, 0);
                watermarkTextGO.SetActive(false);

                Button watermarkButton = watermarkBackgroundGO.AddComponent<Button>();
                watermarkButton.onClick.AddListener(() =>
                {
                    watermarkImageGO.SetActive(false);
                    watermarkTextGO.SetActive(true);
                    watermarkBackground.rectTransform.sizeDelta = watermarkTextComponent.rectTransform.sizeDelta;
                    watermarkBackground.rectTransform.anchoredPosition = watermarkTextComponent.rectTransform.anchoredPosition;
                    watermarkBackground.rectTransform.anchorMin = watermarkTextComponent.rectTransform.anchorMin;
                    watermarkBackground.rectTransform.anchorMax = watermarkTextComponent.rectTransform.anchorMax;
                    watermarkBackground.rectTransform.pivot = watermarkTextComponent.rectTransform.pivot = new Vector2(1f, 1f);
                });
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
            }
        }

        private async Task<RawImage> RenderFallbackMedia(FetchCreativeApiResponse fetchedCreative, ISpan renderBannerAdSpan)
        {
            ISpan renderFallbackImageSpan = renderBannerAdSpan.StartChild("render_fallback_image", "Render Fallback Image");
            try
            {
                GameObject rawImageGO = parent.gameObject;
                RawImage rawImage = rawImageGO.GetComponent<RawImage>();

                int creativeWidth = fetchedCreative.data.dimensions.width;
                int creativeHeight = fetchedCreative.data.dimensions.height;

                Texture2D imageTexture = await _apiClient.DownloadImageFromUrl(Util.GetFallbackImageUrl(creativeWidth, creativeHeight));

                rawImage.texture = imageTexture;

                renderFallbackImageSpan.Finish();

                Button button = rawImageGO.AddComponent<Button>();
                button.onClick.AddListener(() => Application.OpenURL(Constants.FALLBACK_MEDIA_CTA_URL));
                return rawImage;
            }
            catch (Exception e)
            {
                renderFallbackImageSpan.Finish();
                throw e;
            }
        }

        private void TrackVisibilityOnScreen(FetchCreativeApiResponse fetchedCreative, RawImage rawImage, Canvas canvas)
        {
            SentrySdk.AddBreadcrumb(message: "Beginning TrackVisibilityOnScreen", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            float visiblePercentage = ViewVisibilityOnScreen.GetVisiblePercentage(rawImage.rectTransform, canvas.GetComponent<RectTransform>());
            bool isInstantImpressionTriggered = false;
            bool isDelayedImpressionTriggered = false;
            Coroutine delayedCoroutine = null;

            IEnumerator VisibilityTimer()
            {
                yield return new WaitForSeconds(Constants.VISIBILITY_DURATION_THRESHOLD);
                string triggeredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                _adEventService.SendAdImpressionEvent(fetchedCreative, visiblePercentage, triggeredAt);
                _adEventService.SendAdChargeableImpressionEvent(fetchedCreative, visiblePercentage, triggeredAt);
                isDelayedImpressionTriggered = true;
            }

            if (visiblePercentage > 0 && !isInstantImpressionTriggered)
            {
                string triggeredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                _adEventService.SendAdImpressionEvent(fetchedCreative, visiblePercentage, triggeredAt);
                isInstantImpressionTriggered = true;
            }

            if (visiblePercentage > Constants.VISIBILITY_THRESHOLD_PERCENTAGE && !isDelayedImpressionTriggered)
            {
                if (delayedCoroutine != null)
                {
                    parent.StopCoroutine(delayedCoroutine);
                    delayedCoroutine = null;
                }
                delayedCoroutine = parent.StartCoroutine(VisibilityTimer());
            }

            ScrollRect scrollRect = rawImage.GetComponentInParent<ScrollRect>();
            if (scrollRect == null) return;
            scrollRect.onValueChanged.AddListener((Vector2 scrollPosition) =>
            {
                if (isDelayedImpressionTriggered)
                {
                    //Remove event listener here
                    return;
                }

                visiblePercentage = ViewVisibilityOnScreen.GetVisiblePercentage(rawImage.rectTransform, canvas.GetComponent<RectTransform>());
                if (visiblePercentage > 0 && !isInstantImpressionTriggered)
                {
                    string triggeredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    _adEventService.SendAdImpressionEvent(fetchedCreative, visiblePercentage, triggeredAt);
                    isInstantImpressionTriggered = true;
                }
                if (visiblePercentage > Constants.VISIBILITY_THRESHOLD_PERCENTAGE && !isDelayedImpressionTriggered)
                {
                    if (delayedCoroutine == null)
                    {
                        delayedCoroutine = parent.StartCoroutine(VisibilityTimer());
                    }

                }
                else
                {
                    if (delayedCoroutine != null)
                    {
                        parent.StopCoroutine(delayedCoroutine);
                        delayedCoroutine = null;
                    }
                }
            });
        }

        private void SendExceptionToSentry(Exception error, ITransaction transaction, Dictionary<string, string> tags, string eventName)
        {
            SentrySdk.CaptureException(error, (scope) =>
            {
                scope.Transaction = transaction;
                scope.AddBreadcrumb(new Breadcrumb(message: $"Exception triggered by- {eventName}", type: "error", level: BreadcrumbLevel.Error));
                scope.SetTags(tags);
            });
        }
    }
}
