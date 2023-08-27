using System;
using Sentry;
using UnityEngine;

namespace IO.Persona.MobileAds.Unity
{
    public class PersonaAdSDK
    {
        private static string apiKey = null;
        private static Environment? environment = null;
        private static PersonaAdSDKConfig config;

        public static string GetApiKey()
        {
            return apiKey;
        }

        public static Environment? GetEnvironment()
        {
            return environment;
        }

        public static void Initialize()
        {
            try
            {
                config = Resources.Load<PersonaAdSDKConfig>("Persona/PersonaAdSDKConfig");
                if (config == null)
                {
                    config = PersonaAdSDKConfig.CreateConfig();

                    if (config == null)
                    {
                        Debug.LogError("PersonaAdSDKConfig not found. Make sure the config exists.");
                    }
                }

                string providedApiKey = config.apiKey;
                string providedEnvironment = config.environment;

                if ((providedApiKey != null && providedApiKey.Length != 0) && (providedEnvironment != null && providedEnvironment.Length != 0))
                {
                    apiKey = providedApiKey;
                    switch (providedEnvironment)
                    {
                        case "development":
                            environment = Environment.DEVELOPMENT;
                            break;
                        case "staging":
                            environment = Environment.STAGING;
                            break;
                        case "production":
                            environment = Environment.PRODUCTION;
                            break;
                        default:
                            environment = null;
                            break;
                    }
                }
                else return;
                SentrySdk.AddBreadcrumb(message: "SDK Initialized", category: "sdk.milestone", level: BreadcrumbLevel.Info);
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                if (environment != Environment.PRODUCTION)
                {
                    Debug.LogError("Initialization error - " + e);
                }
            }

        }
    }
}
