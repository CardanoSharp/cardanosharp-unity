using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Utils;

public class MenuManagerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mnemonicText;
    [SerializeField] private TMP_Text addressText;
    [SerializeField] private Button playButton;

    private MenuManager menuManager;
    private CooldownTimer _cooldownTimer;

    private void Awake()
    {
        menuManager = GetComponent<MenuManager>();
        _cooldownTimer = new CooldownTimer(3); // check every 3 seconds

        // Register handler that will trigger when timer is complete
        _cooldownTimer.TimerCompleteEvent += OnTimerComplete;
    }

    private void Update () 
    {
        // Update cooldown timer with Time.deltaTime 
        _cooldownTimer.Update(Time.deltaTime);
        if (_cooldownTimer.IsActive)
        {
            // update the ui or something
        }
    }

    private void Start()
    {
        playButton.interactable = false;

        mnemonicText.text = "";
        addressText.text = "";
    }

    public void OnGenerateButtonPressed()
    {
        var mnemonic = menuManager.GenerateMnemonic();
        SetMnemonicText(mnemonic);

        var address = menuManager.GetPaymentAddress(mnemonic);
        SetAddressText(address);
    }
    
    public void OnCopyButtonPressed() 
    {     
        menuManager.CopyAddress(addressText.text); 
        _cooldownTimer.Start();
        Debug.Log("Address copied! Waiting for funds to arrive...");
    }

    private void OnTimerComplete()
    {
        if(menuManager.HasFunds(addressText.text))
        {
            Debug.Log("Address funded! Enabling Play button.");
            EnablePlayButton();
        } 
        else
        {
            StartTimer();
        }
    }

    public void EnablePlayButton() { playButton.interactable = true; }
    public void StartTimer() { _cooldownTimer.Start(); }

    void SetMnemonicText(string s) { mnemonicText.text = s; }
    void SetAddressText(string s) { addressText.text = s; }

}
