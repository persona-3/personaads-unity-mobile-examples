using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sentry;
using System.Collections;

public class BannerView
{
    private readonly string adUnitId = "";
    private string userEmail = null;
    private string walletAddress = null;
    private string requestId = "";
    private readonly float xCoord;
    private readonly float yCoord;
    private readonly MonoBehaviour parent;

    AdEventService _adEventService;

    public BannerView(MonoBehaviour parent, string adUnitId, float xCoord, float yCoord)
    {
        this.adUnitId = adUnitId;
        this.xCoord = xCoord;
        this.yCoord = yCoord;
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

        Environment ? currentEnvironment = PersonaAdSDK.GetEnvironment();
        if (!Util.IsCredentialsValid(currentEnvironment, this.adUnitId)) return;

        SentrySdk.AddBreadcrumb(message: "null check completed", category: "sdk.milestone", level: BreadcrumbLevel.Info);

        this.requestId = Util.GenerateRequestId();
        this._adEventService = new AdEventService(this.requestId, this.adUnitId, this.walletAddress, this.userEmail, (Environment) currentEnvironment);

        transaction.SetTag("p3-request-id", this.requestId);
        transaction.SetTag("p3-api-key", PersonaAdSDK.GetApiKey());
        transaction.SetTag("p3-current-environment", currentEnvironment.ToString());
        transaction.SetTag("p3-ad-unit-id", this.adUnitId);
        transaction.SetTag("p3-x-coord", this.xCoord.ToString());
        transaction.SetTag("p3-y-coord", this.yCoord.ToString());
        transaction.SetTag("p3-user-email", this.userEmail);
        transaction.SetTag("p3-wallet-address", this.walletAddress);
        transaction.SetTag("p3-app-package-name", Application.identifier);

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
            fetchCreativeSpan.Finish();

            TriggerFetchCreativeResponseHandler(fetchedCreative, (Environment)currentEnvironment, transaction);
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e, (scope) => scope.Transaction = transaction);
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
        }catch(Exception e)
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
            SentrySdk.CaptureException(e, (scope) => scope.Transaction = transaction);

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
                    SentrySdk.CaptureException(e2, (scope) => scope.Transaction = transaction);
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
            Canvas canvas = this.parent.GetComponentInParent<Canvas>();
            GameObject rawImageGO = this.parent.gameObject;
            RawImage rawImage = rawImageGO.GetComponent<RawImage>();

            string mediaUrl = fetchedCreative.data.mediaUrl;

            string[] mediaUrlParts = mediaUrl.Split(".");

            if (mediaUrlParts[mediaUrlParts.Length - 1].Contains("gif"))
            {
                rawImage.rectTransform.anchoredPosition = new Vector2(this.xCoord, this.yCoord);

                List<(Sprite, float)> spriteImages = await Util.DownloadGifFromUrl(mediaUrl);
                parent.StartCoroutine(Util.PlayGifAnimation(spriteImages, rawImage));
            }
            else
            {
                Sprite imageSprite = await Util.DownloadImageFromUrl(mediaUrl);

                rawImage.texture = imageSprite.texture;
                rawImage.rectTransform.sizeDelta = new Vector2(imageSprite.rect.width, imageSprite.rect.height);
                rawImage.rectTransform.anchoredPosition = new Vector2(this.xCoord, this.yCoord);
            }
            try
            {
                TrackVisibilityOnScreen(fetchedCreative, rawImage, canvas);

                Button button = rawImageGO.AddComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    this._adEventService.SendAdClickEvent(fetchedCreative);
                    Application.OpenURL(fetchedCreative.data.ctaUrl);
                });
            }catch(Exception e2)
            {
                SentrySdk.CaptureException(e2, (scope) => scope.Transaction = transaction);
            }
            
            return rawImage;
        }catch(Exception e)
        {
            throw e;
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

            Sprite imageSprite = await Util.DownloadImageFromUrl(Util.GetFallbackImageUrl(creativeWidth, creativeHeight));

            rawImage.texture = imageSprite.texture;
            rawImage.rectTransform.sizeDelta = new Vector2(imageSprite.rect.width, imageSprite.rect.height);
            rawImage.rectTransform.anchoredPosition = new Vector2(this.xCoord, this.yCoord);

            renderFallbackImageSpan.Finish();

            Button button = rawImageGO.AddComponent<Button>();
            button.onClick.AddListener(() => Application.OpenURL(Constants.FALLBACK_MEDIA_CTA_URL));
            return rawImage;
        }
        catch(Exception e)
        {
            renderFallbackImageSpan.Finish();
            throw e;
        }
    }

    private void TrackVisibilityOnScreen(FetchCreativeApiResponse fetchedCreative, RawImage rawImage, Canvas canvas)
    {
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
            if(visiblePercentage > Constants.VISIBILITY_THRESHOLD_PERCENTAGE && !isDelayedImpressionTriggered)
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
}
