using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadSceneByIndex(int i) { SceneManager.LoadScene(i); }

    public void GenerateMnemonic()
    {
        Debug.Log("New mnemonic generated");
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
