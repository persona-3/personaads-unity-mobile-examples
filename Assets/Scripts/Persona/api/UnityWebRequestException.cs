using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System;
using Sentry;

namespace IO.Persona.MobileAds.Unity
{
    public class UnityWebRequestException : Exception
    {
        public UnityWebRequestException(UnityWebRequest webRequest) : base($"Error: Status Code {webRequest.responseCode}, Message: {webRequest.error}")
        {
            StatusCode = webRequest.responseCode;
            Message = webRequest.error;
            Data = webRequest.downloadHandler.text;
        }

        public long StatusCode { get; }
        public string? Message { get; }
        public string? Data { get; }
    }
}
