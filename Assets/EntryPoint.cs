using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PersonaAdSDK.Initialize();
        BannerView bannerView = new BannerView(this, "9e643cf1-17f3-4de8-b28d-eb85eef60f91", 0, 100);

        bannerView.SetWalletAddress("0xYourWalletAddress"); // Example wallet address - Optional field
        bannerView.SetUserEmail("sample.email@example.com"); // Example user email - Optional field
        bannerView.LoadAd();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
