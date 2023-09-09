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
        BannerView bannerView = new BannerView(this, "5470eef5-da8a-40d8-8e0f-a50882844770");

        bannerView.SetWalletAddress("0xYourWalletAddress"); // Example wallet address - Optional field
        bannerView.SetUserEmail("sample.email@example.com"); // Example user email - Optional field
        bannerView.LoadAd();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
