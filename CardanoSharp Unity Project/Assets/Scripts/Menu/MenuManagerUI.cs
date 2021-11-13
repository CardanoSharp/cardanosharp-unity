using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuManagerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mnemonicText;
    [SerializeField] private TMP_Text addressText;

    [SerializeField] private Button playButton;

    private MenuManager menuManager;

    private void Awake()
    {
        menuManager = GetComponent<MenuManager>();
    }

    private void Start()
    {
        playButton.interactable = false;

        mnemonicText.text = "";
        addressText.text = "";
    }

    public void OnGenerateButtonPressed()
    {
        menuManager.GenerateMnemonic();

        //Change to correct strings
        SetMnemonicText("abandon ability able about above absent " +
            "absorb abstract absurd abuse access accident " +
            "account accuse achieve acid acoustic acquire " +
            "across act action actor actress actual");

        SetAddressText("addr1q80s6ggmlgzz3lzs6p6t57qz74llxwqrgcga5djk7ecfusg5e4ll25jy4zgje2tuuxzn0xjuuuc7qz0f3wqp09sdyjesw7llzf");

        //EnablePlayButton();
    }

    public void OnCopyButtonPressed()
    {
        menuManager.CopyAddress(addressText.text);
    }

    public void EnablePlayButton() { playButton.interactable = true; }

    void SetMnemonicText(string s) { mnemonicText.text = s; }
    void SetAddressText(string s) { addressText.text = s; }
}
