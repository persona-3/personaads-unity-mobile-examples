using System;

namespace IO.Persona.MobileAds.Unity
{
    [Serializable]
    public class FetchCreativeApiResponse
    {
        public bool success;
        public string message;
        public ApiResponseData data;
    }

    [Serializable]
    public class ApiResponseData
    {
        public string? id;
        public string? ctaUrl;
        public string? mediaUrl;
        public ApiResponseDimensions dimensions;
    }


    [Serializable]
    public class ApiResponseDimensions
    {
        public int width;
        public int height;
    }
}
