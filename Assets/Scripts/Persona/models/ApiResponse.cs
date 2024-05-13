using System;

namespace IO.Persona.MobileAds.Unity
{
    [Serializable]
    public class ApiResponse
    {
        public long status;
        public string data;

        public ApiResponse(long status, string data)
        {
            this.status = status;
            this.data = data;
        }
    }
}