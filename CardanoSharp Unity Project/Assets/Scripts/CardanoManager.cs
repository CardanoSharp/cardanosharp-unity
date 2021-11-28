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
using CardanoSharp.Wallet.Models.Derivations;
using System.Text.Json;
using CardanoSharp.Wallet.Models.Transactions;
using CardanoSharp.Wallet.TransactionBuilding;
using CardanoSharp.Wallet.Utilities;
using CardanoSharp.Wallet.Models.Transactions.Scripts;
using System;

public class CardanoManager : MonoBehaviour
{
    private DataManager _dataManager;

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

    private void Awake()
    {
        _dataManager = GetComponent<DataManager>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    #region Wallet Creation
    public void CreateWallet(string walletName)
    {
        walletName = $"Wallet:{walletName}";

        if (_dataManager.Exists(walletName))
            _dataManager.DeleteData(walletName);

        var mnemonic = new MnemonicService().Generate(24);

        var accountNode = mnemonic.GetMasterNode()
            .Derive(PurposeType.Shelley)
            .Derive(CoinType.Ada)
            .Derive(0);

        var paymentIx = accountNode
            .Derive(RoleType.ExternalChain)
            .Derive(0);
        paymentIx.SetPublicKey();

        var stakingIx = accountNode
            .Derive(RoleType.Staking)
            .Derive(0);
        paymentIx.SetPublicKey();

        var baseAddr = new AddressService()
            .GetAddress(paymentIx.PublicKey,
                stakingIx.PublicKey,
                NetworkType.Testnet,
                AddressType.Base);

        _dataManager.SaveData(walletName, baseAddr.ToString());
    }

    public Mnemonic GenerateMnemonic(int size = 24, WordLists wordLists = WordLists.English)
    {
        return _mnemonicService.Generate(size, wordLists);
    }

    public Mnemonic RestoreMnemonic(string words)
    {
        return _mnemonicService.Restore(words);
    }

    public IAccountNodeDerivation GetAccountNode(string words)
    {
        Mnemonic mnemonic = _mnemonicService.Restore(words);

        // Fluent derivation API
        return mnemonic
            .GetMasterNode()                // IMasterNodeDerivation
            .Derive(PurposeType.Shelley)    // IPurposeNodeDerivation
            .Derive(CoinType.Ada)           // ICoinNodeDerivation
            .Derive(0);                     // IAccountNodeDerivation
    }

    public KeyPair GenerateKeyPair()
    {
        return KeyPair.GenerateKeyPair();
    }
    #endregion

    #region Encryption
    public string EncryptKeys(PrivateKey accountPrivateKey, PublicKey accountPublicKey)
    {
        return JsonSerializer.Serialize((accountPrivateKey.Encrypt("spending_password"), accountPublicKey));
    }

    public (PrivateKey, PublicKey) DecryptKeys(string accountNode)
    {
        var load = JsonSerializer.Deserialize<(PrivateKey, PublicKey)>(accountNode);
        return (load.Item1.Decrypt("spending_password"), load.Item2);
    }
    #endregion

    #region Addresses
    public Address GetPaymentAddress(string words)
    {
        Mnemonic mnemonic = _mnemonicService.Restore(words);

        // Fluent derivation API
        var account = mnemonic
            .GetMasterNode()                // IMasterNodeDerivation
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
    #endregion

    #region Minting
    public void CreateNativeScript(string name, List<PublicKey> publicKeys, uint? after = null, uint? before = null)
    {
        var scriptBuilder = ScriptAllBuilder.Create;
        foreach (var pubKey in publicKeys)
        {
            var policyKeyHash = HashUtility.Blake2b244(pubKey.Key);
            scriptBuilder.SetScript(NativeScriptBuilder.Create.SetKeyHash(policyKeyHash));
        }
        var nativeScript = scriptBuilder.Build();
        var policyId = nativeScript.GetPolicyId();

        _dataManager.SaveData($"PolicyId:{name}", JsonSerializer.Serialize(policyId));
    }
    #endregion

    #region Transactions
    public async void MintNFT(string nft)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Blockfrost
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


    #endregion
}
