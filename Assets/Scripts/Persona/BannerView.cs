using System;
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

        AdEventService _adEventService;

        public BannerView(MonoBehaviour parent, string adUnitId)
        {
            this.adUnitId = adUnitId;
            this.parent = parent;
        }

        public void SetUserEmail(string userEmail)
        {
            this.userEmail = userEmail;
        }

        public void SetWalletAddress(string walletAddress)
        {
            this.walletAddress = walletAddress;
        }

        public async void LoadAd()
        {
            SentrySdk.AddBreadcrumb(message: "loadAd Called", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            ITransaction transaction = SentrySdk.StartTransaction("LoadBannerAd", "banner_ad.load");

            Environment? currentEnvironment = PersonaAdSDK.GetEnvironment();
            if (!Util.IsCredentialsValid(currentEnvironment, this.adUnitId)) return;

            SentrySdk.AddBreadcrumb(message: "null check completed", category: "sdk.milestone", level: BreadcrumbLevel.Info);

            this.requestId = Util.GenerateRequestId();
            this._adEventService = new AdEventService(this.requestId, this.adUnitId, this.walletAddress, this.userEmail, (Environment)currentEnvironment);

            transaction.SetTag("p3-request-id", this.requestId);
            transaction.SetTag("p3-api-key", PersonaAdSDK.GetApiKey());
            transaction.SetTag("p3-current-environment", currentEnvironment.ToString());
            transaction.SetTag("p3-ad-unit-id", this.adUnitId);
            transaction.SetTag("p3-user-email", this.userEmail);
            transaction.SetTag("p3-wallet-address", this.walletAddress);
            transaction.SetTag("p3-app-package-name", Application.identifier);
            this.customizedTags.Add("p3-request-id", this.requestId);
            this.customizedTags.Add("p3-api-key", PersonaAdSDK.GetApiKey());
            this.customizedTags.Add("p3-current-environment", currentEnvironment.ToString());
            this.customizedTags.Add("p3-ad-unit-id", this.adUnitId);
            this.customizedTags.Add("p3-user-email", this.userEmail);
            this.customizedTags.Add("p3-wallet-address", this.walletAddress);
            this.customizedTags.Add("p3-app-package-name", Application.identifier);

            if (currentEnvironment != Environment.STAGING) this._adEventService.SendAdRequestedEvent(transaction);

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
                this.customizedTags.Add("p3-creative-id", fetchedCreative.data?.id);
                this.customizedTags.Add("p3-creative-media-url", fetchedCreative.data?.mediaUrl);
                this.customizedTags.Add("p3-creative-cta-url", fetchedCreative.data?.ctaUrl);
                this.customizedTags.Add("p3-creative-width", fetchedCreative.data?.dimensions?.width.ToString());
                this.customizedTags.Add("p3-creative-height", fetchedCreative.data?.dimensions?.height.ToString());

                fetchCreativeSpan.Finish();

                TriggerFetchCreativeResponseHandler(fetchedCreative, (Environment)currentEnvironment, transaction);
            }
            catch (Exception e)
            {
                this.SendExceptionToSentry(e, transaction, this.customizedTags, "LoadAd function");
                transaction.Finish();
                TriggerFetchCreativeErrorHandler(e);
            }


        }

        private async Task<FetchCreativeApiResponse> TriggerFetchCreative(string ipAddress)
        {
            try
            {
                string apiUrl = $"{Util.GetBaseUrl(PersonaAdSDK.GetEnvironment())}/creatives";
                Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-request-id", this.requestId }
        };

                Dictionary<string, string> queryParams = new Dictionary<string, string>
        {
            { "placementId", this.adUnitId },
            { "ipAddress", ipAddress },
            { "walletAddress", this.walletAddress },
            { "userEmail", this.userEmail }
        };

                APIClient _apiClient = new APIClient();

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
                    this._adEventService.SendAdRequestCompletedEvent(fetchedCreative);
                    ISpan renderActualImageSpan = renderBannerAdSpan.StartChild("render_actual_image", "Render Actual Image");
                    await RenderFetchedCreative(fetchedCreative, transaction);
                    renderActualImageSpan.Finish();
                    renderBannerAdSpan.Finish();
                    this._adEventService.SendAdLoadCompletedEvent(fetchedCreative);
                }
                transaction.Finish();
            }
            catch (Exception e)
            {
                this.SendExceptionToSentry(e, transaction, this.customizedTags, "TriggerFetchCreativeResponseHandler function - Outer Exception");
                //SentrySdk.CaptureException(e, (scope) => scope.Transaction = transaction);

                if (currentEnvironment != Environment.STAGING)
                {
                    try
                    {
                        this._adEventService.SendAdLoadFailedEvent(fetchedCreative, e);
                        await RenderFallbackMedia(fetchedCreative, renderBannerAdSpan);
                        renderBannerAdSpan.Finish();
                        transaction.Finish();
                    }
                    catch (Exception e2)
                    {
                        this.SendExceptionToSentry(e2, transaction, this.customizedTags, "TriggerFetchCreativeResponseHandler function - Inner Exception");
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
            this._adEventService.SendAdRequestFailedEvent(e);
        }


        private async Task<RawImage> RenderFetchedCreative(FetchCreativeApiResponse fetchedCreative, ITransaction transaction)
        {
            try
            {
                SentrySdk.AddBreadcrumb(message: "Beginning RenderFetchedCreative", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                Canvas canvas = this.parent.GetComponentInParent<Canvas>();
                SentrySdk.AddBreadcrumb(message: $"canvas- {canvas}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                GameObject rawImageGO = this.parent.gameObject;
                SentrySdk.AddBreadcrumb(message: $"rawImageGO- {rawImageGO}", category: "sdk.milestone", level: BreadcrumbLevel.Info);
                RawImage rawImageComponent = rawImageGO.GetComponent<RawImage>();
                SentrySdk.AddBreadcrumb(message: $"rawImageComponent- {rawImageComponent}", category: "sdk.milestone", level: BreadcrumbLevel.Info);

                string mediaUrl = fetchedCreative.data.mediaUrl;

                string[] mediaUrlParts = mediaUrl.Split(".");

                if (mediaUrlParts[mediaUrlParts.Length - 1].Contains("gif"))
                {
                    byte[] gifBytes = await Util.DownloadGifFromUrl(mediaUrl);

                    List<(Texture2D, float)> textureImages = Util.SplitGifIntoFrames(gifBytes);
                    parent.StartCoroutine(Util.PlayGifAnimation(textureImages, rawImageComponent));
                }
                else
                {
                    Texture2D imageTexture = await Util.DownloadImageFromUrl(mediaUrl);

                    rawImageComponent.texture = imageTexture;
                }
                DisplayPersonaTag(rawImageComponent);
                try
                {
                    TrackVisibilityOnScreen(fetchedCreative, rawImageComponent, canvas);

                    Button button = rawImageGO.AddComponent<Button>();
                    button.onClick.AddListener(() =>
                    {
                        this._adEventService.SendAdClickEvent(fetchedCreative);
                        Application.OpenURL(fetchedCreative.data.ctaUrl);
                    });
                }
                catch (Exception e2)
                {
                    this.SendExceptionToSentry(e2, transaction, this.customizedTags, "RenderFetchedCreative function - Inner Exception");
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
                
                watermarkImageComponent.texture = await Util.DownloadImageFromUrl(Constants.WATERMARK_PERSONA_LOGO);
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
                watermarkTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                watermarkTextComponent.fontSize = (int) (Constants.WATERMARK_TEXT_FONT_SIZE * referenceResolutionScaleFactor * rawImageComponent.canvas.scaleFactor);
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
                GameObject rawImageGO = this.parent.gameObject;
                RawImage rawImage = rawImageGO.GetComponent<RawImage>();

                int creativeWidth = fetchedCreative.data.dimensions.width;
                int creativeHeight = fetchedCreative.data.dimensions.height;

                Texture2D imageTexture = await Util.DownloadImageFromUrl(Util.GetFallbackImageUrl(creativeWidth, creativeHeight));

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
            float visiblePercentage = ViewVisibilityOnScreen.GetVisiblePercentage(rawImage.rectTransform, canvas);
            bool isInstantImpressionTriggered = false;
            bool isDelayedImpressionTriggered = false;
            Coroutine delayedCoroutine = null;

            IEnumerator VisibilityTimer()
            {
                yield return new WaitForSeconds(Constants.VISIBILITY_DURATION_THRESHOLD);
                string triggeredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                this._adEventService.SendAdImpressionEvent(fetchedCreative, visiblePercentage, triggeredAt);
                isDelayedImpressionTriggered = true;
            }

            if (visiblePercentage > 0 && !isInstantImpressionTriggered)
            {
                string triggeredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                this._adEventService.SendAdImpressionEvent(fetchedCreative, visiblePercentage, triggeredAt);
                isInstantImpressionTriggered = true;
            }

            if (visiblePercentage > Constants.VISIBILITY_THRESHOLD_PERCENTAGE && !isDelayedImpressionTriggered)
            {
                if (delayedCoroutine != null)
                {
                    this.parent.StopCoroutine(delayedCoroutine);
                    delayedCoroutine = null;
                }
                delayedCoroutine = this.parent.StartCoroutine(VisibilityTimer());
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

                visiblePercentage = ViewVisibilityOnScreen.GetVisiblePercentage(rawImage.rectTransform, canvas);
                if (visiblePercentage > 0 && !isInstantImpressionTriggered)
                {
                    string triggeredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    this._adEventService.SendAdImpressionEvent(fetchedCreative, visiblePercentage, triggeredAt);
                    isInstantImpressionTriggered = true;
                }
                if (visiblePercentage > Constants.VISIBILITY_THRESHOLD_PERCENTAGE && !isDelayedImpressionTriggered)
                {
                    if (delayedCoroutine == null)
                    {
                        delayedCoroutine = this.parent.StartCoroutine(VisibilityTimer());
                    }

                }
                else
                {
                    if (delayedCoroutine != null)
                    {
                        this.parent.StopCoroutine(delayedCoroutine);
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
