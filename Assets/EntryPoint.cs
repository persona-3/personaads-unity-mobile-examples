using System.Collections;
using System.Collections.Generic;
using IO.Persona.MobileAds.Unity;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PersonaAdSDK.Initialize();
        BannerView bannerView = new BannerView(this, "a6280861-fb15-466d-8a0d-9e82f0e4ffa0");

        bannerView.SetWalletAddress("0xYourWalletAddress"); // Example wallet address - Optional field
        bannerView.SetUserEmail("sample.email@example.com"); // Example user email - Optional field
        bannerView.LoadAd();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
