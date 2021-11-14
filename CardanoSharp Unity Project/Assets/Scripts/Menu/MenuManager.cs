using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils; 

public class MenuManager : MonoBehaviour
{
    private CardanoManager _cardanoManager;
    public void LoadSceneByIndex(int i) { SceneManager.LoadScene(i); }
    private void Awake()
    {
        _cardanoManager = GetComponent<CardanoManager>(); 
    }

    public string GenerateMnemonic()
    {
        var mnemonic = _cardanoManager.GenerateMnemonic();
        return mnemonic.Words;
    }

    public string GetPaymentAddress(string words)
    {
        var address = _cardanoManager.GetPaymentAddress(words);
        return address.ToString();
    }

    public bool HasFunds(string address)
    {
        Debug.Log("Checking funds...");
        // ill use asyncUtil to unwrap the state machine here because
        // 1: to see if it works
        // 2: to start a discussion about patterns we should advocate (avoid THIS, do THAT instead)
        var funds = AsyncUtil.RunSync<long>(() => _cardanoManager.GetFundsAsync(address));
        Debug.Log($"Balance: {funds}");
        return funds > 0;
    }

    public void CopyAddress(string s)
    {
        GUIUtility.systemCopyBuffer = s;
        Debug.Log("String: " + s + " copied to clipboard");
    }

    public void VisitInstructionsLink()
    {
        Application.OpenURL("https://testnets.cardano.org/en/testnets/cardano/tools/faucet/");
    }
}
