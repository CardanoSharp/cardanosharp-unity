using Blockfrost.Api.Models;
using CardanoSharp.Wallet;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Models.Keys;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardanoManager : MonoBehaviour
{
    private readonly MnemonicService _mnemonicService;

    public CardanoManager()
    {
        _mnemonicService = new MnemonicService();  
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public Mnemonic GenerateMnemonic(int size = 24, WordLists wordLists = WordLists.English)
    {
        return _mnemonicService.Generate(size, wordLists);
    }

    public Mnemonic RestoreMnemonic(string words)
    {
        return _mnemonicService.Restore(words);
    }

    public Task<List<Amount>> GetFunds(string address)
    {
        return Task.FromResult(new List<Amount>());
    }
}
