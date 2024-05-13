using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sentry;
using System.Collections;

namespace IO.Persona.MobileAds.Unity
{
    public interface IClientIdentity
    {
        void SetUserEmail(string userEmail);
        void SetWalletAddress(string walletAddress);
        string GetUserEmail();
        string GetWalletAddress();
    }
    public class ClientIdentity : IClientIdentity
    {
        private string userEmail = null;
        private string walletAddress = null;

        public ClientIdentity() { }

        public void SetUserEmail(string userEmail)
        {
            this.userEmail = userEmail;
        }

        public void SetWalletAddress(string walletAddress)
        {
            this.walletAddress = walletAddress;
        }

        public string GetUserEmail()
        {
            return userEmail;
        }

        public string GetWalletAddress()
        {
            return walletAddress;
        }
    }
}