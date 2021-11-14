using Blockfrost.Api;
using Blockfrost.Api.Extensions;
using Blockfrost.Api.Models;
using Blockfrost.Api.Services;
using CardanoSharp.Wallet;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Models.Keys;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.Extensions.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardanoManager : MonoBehaviour
{
    private readonly MnemonicService _mnemonicService;
    private readonly AddressService _addressService;

    private readonly ICardanoService _cardanoService;

    public CardanoManager()
    {
        _cardanoService = new ServiceCollection()
            .AddBlockfrost("testnet","kL2vAF27FpfuzrnhSofc1JawdlL0BNkh")
            .BuildServiceProvider()
            .GetRequiredService<ICardanoService>();        

        _mnemonicService = new MnemonicService();  
        _addressService = new AddressService();  
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public Mnemonic GenerateMnemonic(int size = 24, WordLists wordLists = WordLists.English)
    {
        return _mnemonicService.Generate(size, wordLists);
    }

    public Address GetPaymentAddress(string words)
    {
        Mnemonic mnemonic = _mnemonicService.Restore(words);
        
        // Fluent derivation API
        var account = mnemonic
            .GetMasterNode()      // IMasterNodeDerivation
            .Derive(PurposeType.Shelley)    // IPurposeNodeDerivation
            .Derive(CoinType.Ada)           // ICoinNodeDerivation
            .Derive(0);                     // IAccountNodeDerivation
        
        var payment = account.Derive(RoleType.ExternalChain).Derive(0);
        var staking = account.Derive(RoleType.Staking).Derive(0);

        Address baseAddr = _addressService.GetAddress(
            payment.PublicKey,
            staking.PublicKey,
            NetworkType.Testnet, 
            AddressType.Base);

        return baseAddr;
    }

    public Mnemonic RestoreMnemonic(string words)
    {
        return _mnemonicService.Restore(words);
    }

    public async Task<long> GetFundsAsync(string address)
    {
        try 
        {
            var response = await _cardanoService.Addresses.GetUtxosAsync(address);
            var amounts = response.SelectMany(m => m.Amount).Where(m => m.Unit == "lovelace").Sum(m => long.Parse(m.Quantity));
            return amounts;
        } 
        catch 
        {
            return 0;
        }
    }
}
