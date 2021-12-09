using Blockfrost.Api;
using Blockfrost.Api.Extensions;
using Blockfrost.Api.Models;
using Blockfrost.Api.Services;
using CardanoSharp.Wallet;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Models.Keys;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.Extensions;
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
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using System.IO;

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
    public void CreatePlayerWallet(string walletName)
    {
        walletName = getWalletDataName(walletName);

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

        _dataManager.SaveData(walletName, JsonSerializer.Serialize(baseAddr));
    }
    public void CreateDeveloperWallet(string walletName)
    {
        walletName = getWalletDataName(walletName);

        if (_dataManager.Exists(walletName))
            _dataManager.DeleteData(walletName);

        var mnemonic = new MnemonicService().Generate(24);

        var accountNode = mnemonic.GetMasterNode()
            .Derive(PurposeType.Shelley)
            .Derive(CoinType.Ada)
            .Derive(0);
        accountNode.SetPublicKey();

        //DEMO ONLY
        //Never save private key unencrypted
        _dataManager.SaveData(walletName, JsonSerializer.Serialize((accountNode.PrivateKey, accountNode.PublicKey)));
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
    public void CreateNativeScript(string name, uint? after = null, uint? before = null)
    {
        var keypair = KeyPair.GenerateKeyPair();

        var scriptBuilder = ScriptAllBuilder.Create;
        var policyKeyHash = HashUtility.Blake2b244(keypair.PublicKey.Key);
        scriptBuilder.SetScript(NativeScriptBuilder.Create.SetKeyHash(policyKeyHash));

        _dataManager.SaveData(getPolicyScriptDataName(name), JsonSerializer.Serialize(scriptBuilder));

        var nativeScript = scriptBuilder.Build();
        var policyId = nativeScript.GetPolicyId();

        _dataManager.SaveData(getPolicyDataName(name), JsonSerializer.Serialize((keypair.PrivateKey, keypair.PublicKey)));
        _dataManager.SaveData(getPolicyIdDataName(name), policyId.ToStringHex());
    }
    #endregion

    #region Transactions
    public async void MintNFT(string fromWalletName, string toWalletName, string tokenName, uint quantity, object metadata)
    {
        //DEMO
        //we know "from" is game dev wallet
        //          "to" is player address
        var fromPolicyIdString = getPolicyIdDataName(fromWalletName);
        var fromPolicyKeysSerialized = getPolicyDataName(fromWalletName);
        var fromPolicyScriptSerialized = getPolicyScriptDataName(fromWalletName);
        fromWalletName = getWalletDataName(fromWalletName);
        toWalletName = getWalletDataName(toWalletName);

        var fromPolicyId = _dataManager.GetData(fromPolicyIdString).HexToByteArray();
        var fromPolicyScript = JsonSerializer.Deserialize<IScriptAllBuilder>(_dataManager.GetData(fromPolicyScriptSerialized));
        var fromPolicyKeys = JsonSerializer.Deserialize<(PrivateKey, PublicKey)>(_dataManager.GetData(fromPolicyKeysSerialized));
        var fromWallet = JsonSerializer.Deserialize<(PrivateKey, PublicKey)>(_dataManager.GetData(fromWalletName));
        var toAddress = JsonSerializer.Deserialize<Address>(_dataManager.GetData(toWalletName));

        var fromAccount = new AccountNodeDerivation(fromWallet.Item1, 0);
        var paymentIx = fromAccount
            .Derive(RoleType.ExternalChain)
            .Derive(0);
        paymentIx.SetPublicKey();

        var stakingIx = fromAccount
            .Derive(RoleType.Staking)
            .Derive(0);
        paymentIx.SetPublicKey();

        var baseAddr = new AddressService()
            .GetAddress(paymentIx.PublicKey,
                stakingIx.PublicKey,
                NetworkType.Testnet,
                AddressType.Base);

        var utxos = await GetUtxos(baseAddr.ToString());
        var feeParams = await GetFeeParameters();
        var latestSlot = await GetLatestSlot();

        var transactionBody = TransactionBodyBuilder.Create;

        foreach (var utxo in utxos)
        {
            transactionBody.AddInput(utxo.TxHash.HexToByteArray(), (uint)utxo.TxIndex);
        }

        long totalBalance = utxos.SelectMany(m => m.Amount).Where(m => m.Unit == "lovelace").Sum(m => long.Parse(m.Quantity));
        ulong sendingAdaQuantity = 2000000;
        ulong totalChange = (ulong)totalBalance - sendingAdaQuantity;

        var mintAsset = TokenBundleBuilder.Create
            .AddToken(fromPolicyId, tokenName.ToBytes(), quantity);

        TransactionOutput changeOutput = new TransactionOutput()
        {
            Address = baseAddr.GetBytes(),
            Value = new TransactionOutputValue()
            {
                Coin = totalChange
            }
        };

        transactionBody
            .AddOutput(toAddress.GetBytes(), sendingAdaQuantity, mintAsset)
            .AddOutput(changeOutput.Address, changeOutput.Value.Coin)
            .SetFee(0)
            .SetTtl(0);

        var witnesses = TransactionWitnessSetBuilder.Create
            .AddVKeyWitness(fromWallet.Item2, fromWallet.Item1)
            .AddVKeyWitness(fromPolicyKeys.Item2, fromPolicyKeys.Item1)
            .SetNativeScript(fromPolicyScript);

        var auxData = AuxiliaryDataBuilder.Create
            .AddMetadata(1337, metadata);

        var transactionBuilder = TransactionBuilder.Create
            .SetBody(transactionBody)
            .SetWitnesses(witnesses)
            .SetAuxData(auxData);

        var transaction = transactionBuilder.Build();

        var fee = transaction.CalculateFee((uint)feeParams.Item1, (uint)feeParams.Item2);

        transactionBody.SetFee(fee);
        changeOutput.Value.Coin = changeOutput.Value.Coin - fee;
        transaction = transactionBuilder.Build();

        //serialize the transaction
        var signedTx = transaction.Serialize();

        var txHash = await SubmitTx(signedTx);
        Debug.Log($"Minting Transaction: {txHash}");
    }
    #endregion

    private string getWalletDataName(string name)
    {
        return $"Wallet:{name}";
    }

    private string getPolicyIdDataName(string name)
    {
        return $"PolicyId:{name}";
    }

    private string getPolicyScriptDataName(string name)
    {
        return $"PolicyScript:{name}";
    }

    private string getPolicyDataName(string name)
    {
        return $"Policy:{name}";
    }

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

    public async Task<AddressUtxoContentResponseCollection> GetUtxos(string address)
    {
        try
        {
            return await _cardanoService.Addresses.GetUtxosAsync(address);
        }catch
        {
            return null;
        }
    }

    public async Task<(long, long)> GetFeeParameters()
    {
        try
        {
            var pps = await _cardanoService.Epochs.GetLatestParametersAsync();
            return (pps.MinFeeA, pps.MinFeeB);
        }
        catch
        {
            return (-1, -1);
        }
    }

    public async Task<long> GetLatestSlot()
    {
        try
        {
            var block = await _cardanoService.Blocks.GetLatestAsync();
            return block.Slot;
        }
        catch
        {
            return -1;
        }
    }
    public async Task<string> SubmitTx(byte[] signedTx)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream(signedTx))
                return await _cardanoService.Transactions.PostTxSubmitAsync(stream);
        }
        catch 
        {
            return null;
        }
    }

    #endregion
}
