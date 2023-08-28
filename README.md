# Persona Mobile Ads Unity

A Unity package designed for Unity android mobile apps to facilitate smooth integration of diverse ad formats and networks, aimed at enhancing revenue streams and user engagement.

## Set up your app in your Persona publisher's account

Register your app in the Persona platform by completing the following steps:

1. Sign in or Sign up as a publisher on the Persona account.
2. Register your app and create ad units for the registered apps. You will get an API key which you will use in the later steps. You will also get adUnitId for each adUnit that you create.

## Download the Mobile Ads Unity package

Use this link to download the Unity package- [Download](https://github.com/persona-3/personaads-unity-mobile-examples/releases/download/v0.0.3/io.persona3.mobileads-v0.0.3.unitypackage)

## Import the Mobile Ads Unity package

To import the package, open your project in the Unity editor, select **Assets > Import Package > Custom Package**, and find the _io.persona3.mobileads-v0.0.3.unitypackage_ file you downloaded. Make sure all of the files are selected and click Import.

## Include external dependencies

We use Sentry as an external dependency for better logging and monitoring to improve our services.
Install the package via the [Unity Package Manager using a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html) to Sentry's SDK repository:
`https://github.com/getsentry/unity.git#1.5.1`

## Set your API Key and Environment

In the Unity editor, select **Assets > Persona Mobile Ads > Settings** from the menu.

![PersonaMobileAdsSettings](https://i.imgur.com/WZUWtlJl.png)

Add your API key and the environment in each field. Use environment as **staging** while integrating the SDK, change it to **production** before your final build and deployment.
Note- You will get your API Key in the Persona's dashboard UI for publishers after registering your app with us.

![PersonaMobileAdsSettingsWindow](https://i.imgur.com/RJGSIVgl.png)

## Initialize the Persona Mobile Ads SDK

Before loading the ads make sure to initialize the Persona Ad SDK. It’s highly recommended to do this as early as possible in the app’s lifecycle. Here’s an example-

### C#

```
using IO.Persona.MobileAds.Unity;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    void Start()
    {
        PersonaAdSDK.Initialize();
    }
}
```

## Display Banner Ad

**1. Add RawImage UI Element to your Scene-**

![AddRawImage](https://i.imgur.com/8uRmFVYl.png)

Make Sure you have the **RawImage** under **Canvas** along with **EventSystem** properly added to the scene as shown here-

![Scene](https://i.imgur.com/RNArcUtl.png)

**2. Load an ad-**
Here's an example that shows how to load an ad in the Start() method of a C# Script:

**C#**

```
using IO.Persona.MobileAds.Unity;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    void Start()
    {
        PersonaAdSDK.Initialize();

        // Here "9e643cf1-17f3-4de8-b28d-eb85eef60f91" is used as the sample adUnitId
        BannerView bannerView = new BannerView(this, "9e643cf1-17f3-4de8-b28d-eb85eef60f91");

        bannerView.SetWalletAddress("0xYourWalletAddress"); // Example wallet address - Optional field
        bannerView.SetUserEmail("sample.email@example.com"); // Example user email - Optional field

        bannerView.LoadAd();
    }
}
```

Note- You will find the adUnitId in the Persona's dashboard UI for publishers after registering your app with us.

**3. Attach your script to RawImage in the Inspector pane**

![InspectorPaneUnity](https://i.imgur.com/vC0gdOxl.png)

## Staging Ad Unit IDs

Following are the **supported Banner Ad Format sizes** and their corresponding AdUnit Ids for the staging environment:

1. 600x160 : ```5470eef5-da8a-40d8-8e0f-a50882844770```
2. 300x250 : ```9e643cf1-17f3-4de8-b28d-eb85eef60f91```
3. 970x90 : ```a6280861-fb15-466d-8a0d-9e82f0e4ffa0```

Note: You can use ```XXX_api_key_2_XXX``` as your API Key along with the above mentioned adUnit IDs for testing the ads in staging environment.
