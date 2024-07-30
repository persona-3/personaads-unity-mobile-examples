using System;
namespace IO.Persona.MobileAds.Unity
{
    public static class Constants
    {
        public static readonly string DEVELOPMENT_API_BASE_URL = "https://dev.persona3.tech";
        public static readonly string STAGING_API_BASE_URL = "https://staging.persona3.tech";
        public static readonly string PRODUCTION_API_BASE_URL = "https://www.persona3.tech";
        public static readonly string FALLBACK_MEDIA_BASE_URL = "https://storage.googleapis.com/fallback-ad-inventory/";
        public static readonly string FALLBACK_MEDIA_CTA_URL = "https://www.persona3.io";

        public static readonly float VISIBILITY_THRESHOLD_PERCENTAGE = 80;
        public static readonly float VISIBILITY_DURATION_THRESHOLD = 1f;

        public static readonly string WATERMARK_PERSONA_LOGO = "https://cdn.persona3.tech/assets/logos/logo-white-small.png";
        public static readonly float REFERENCE_RESOLUTON_X = 1080;
        public static readonly float REFERENCE_RESOLUTON_Y = 2280;
        public static readonly float WATERMARK_IMAGE_WIDTH = 48;
        public static readonly float WATERMARK_IMAGE_HEIGHT = 48;
        public static readonly string WATERMARK_TEXT = "Ads By Persona";
        public static readonly float WATERMARK_TEXT_FONT_SIZE = 25;

        public static readonly string CONFIG_ASSET_RESOURCE_DIR_PATH = "Assets/Resources/Persona";
        public static readonly string SDK_RELEASE_VERSION = "0.0.10";
    }
}
