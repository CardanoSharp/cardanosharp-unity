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
using CardanoSharp.Wallet.Models.Transactions;
using CardanoSharp.Wallet.TransactionBuilding;
using CardanoSharp.Wallet.Utilities;
using CardanoSharp.Wallet.Models.Transactions.Scripts;
using System;
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Utils;

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

        _dataManager.SaveData(walletName, baseAddr.ToString());
    }
    public Mnemonic CreateDeveloperWallet(string identifier)
    {
        var walletName = getWalletDataName(identifier);

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
        var walletData = JsonConvert.SerializeObject((accountNode.PrivateKey, accountNode.PublicKey));
        _dataManager.SaveData(walletName, walletData);
        CreateMintingKeyPair(identifier);
        
        return mnemonic;
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
        return JsonConvert.SerializeObject((accountPrivateKey.Encrypt("spending_password"), accountPublicKey));
    }

    public (PrivateKey, PublicKey) DecryptKeys(string accountNode)
    {
        var load = JsonConvert.DeserializeObject<(PrivateKey, PublicKey)>(accountNode);
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
    public void CreateMintingKeyPair(string name, uint? after = null, uint? before = null)
    {
        if (_dataManager.Exists(getPolicyDataName(name)))
            _dataManager.DeleteData(getPolicyDataName(name));
        
        var keypair = GenerateKeyPair();

        _dataManager.SaveData(getPolicyDataName(name), JsonConvert.SerializeObject((keypair.PrivateKey, keypair.PublicKey)));
    }
    #endregion

    #region Transactions
    public void MintNFT(string fromWalletName, string toWalletName, string tokenName, uint quantity, object metadata)
    {
        //DEMO
        //we know "from" is game dev wallet
        //          "to" is player address
        var fromPolicyKeysSerialized = getPolicyDataName(fromWalletName);
        fromWalletName = getWalletDataName(fromWalletName);
        toWalletName = getWalletDataName(toWalletName);

        var fromPolicyKeys = JsonConvert.DeserializeObject<(PrivateKey, PublicKey)>(_dataManager.GetData(fromPolicyKeysSerialized));
        var fromWallet = JsonConvert.DeserializeObject<(PrivateKey, PublicKey)>(_dataManager.GetData(fromWalletName));
        var toAddress = new Address(_dataManager.GetData(toWalletName));

        //minting native script construction
        var keypair = new KeyPair(fromPolicyKeys.Item1, fromPolicyKeys.Item2);
        var scriptBuilder = ScriptAllBuilder.Create;
        var policyKeyHash = HashUtility.Blake2b244(keypair.PublicKey.Key);
        scriptBuilder.SetScript(NativeScriptBuilder.Create.SetKeyHash(policyKeyHash));
        var nativeScript = scriptBuilder.Build();
        var policyId = nativeScript.GetPolicyId();
        
        //get the developers account node
        var accountPrivateKey = fromWallet.Item1;
        
        var paymentIx = accountPrivateKey
            .Derive(RoleType.ExternalChain)
            .Derive(0);
        paymentIx.SetPublicKey();

        var stakingIx = accountPrivateKey
            .Derive(RoleType.Staking)
            .Derive(0);
        stakingIx.SetPublicKey();

        //generate the address we sent ada to
        var baseAddr = new AddressService()
            .GetAddress(paymentIx.PublicKey,
                stakingIx.PublicKey,
                NetworkType.Testnet,
                AddressType.Base);

        //get utxos of address
        var utxos = AsyncUtil.RunSync<AddressUtxoContentResponseCollection>(() => GetUtxos(baseAddr.ToString()));
        
        //get network parameters
        var feeParams = AsyncUtil.RunSync<(long, long)>(() => GetFeeParameters());
        var latestSlot = AsyncUtil.RunSync<long>(() => GetLatestSlot());

        //start a tx body
        var transactionBody = TransactionBodyBuilder.Create;

        //add utxo to body as inputs
        foreach (var utxo in utxos)
        {
            transactionBody.AddInput(utxo.TxHash.HexToByteArray(), (uint)utxo.TxIndex);
        }

        //calculate values
        long totalBalance = utxos.SelectMany(m => m.Amount).Where(m => m.Unit == "lovelace").Sum(m => long.Parse(m.Quantity));
        ulong sendingAdaQuantity = 2000000;
        ulong totalChange = (ulong)totalBalance - sendingAdaQuantity;

        //add minting asset
        var mintAsset = TokenBundleBuilder.Create
            .AddToken(policyId, tokenName.ToBytes(), quantity);

        //declare a change output to send developer back change
        TransactionOutput changeOutput = new TransactionOutput()
        {
            Address = baseAddr.GetBytes(),
            Value = new TransactionOutputValue()
            {
                Coin = totalChange
            }
        };

        //construct the body pieces
        transactionBody
            .AddOutput(toAddress.GetBytes(), sendingAdaQuantity, mintAsset)
            .AddOutput(changeOutput.Address, changeOutput.Value.Coin)
            .SetMint(mintAsset)
            .SetFee(0)
            .SetTtl((uint)latestSlot + 1000);

        //add our witnesses and native script to mint
        var witnesses = TransactionWitnessSetBuilder.Create
            .AddVKeyWitness(paymentIx.PublicKey, paymentIx.PrivateKey)
            .AddVKeyWitness(fromPolicyKeys.Item2, fromPolicyKeys.Item1)
            .SetNativeScript(scriptBuilder);
        
        //start a tx builder
        var transactionBuilder = TransactionBuilder.Create
            .SetBody(transactionBody)
            .SetWitnesses(witnesses);
            
        //add metadata if we have any
        if (metadata != null)
        {
            var auxData = AuxiliaryDataBuilder.Create
                .AddMetadata(1337, metadata);
            transactionBuilder.SetAuxData(auxData);
        }

        //build out the tx
        var transaction = transactionBuilder.Build();

        //calculate fee of tx
        var fee = transaction.CalculateFee((uint)feeParams.Item1, (uint)feeParams.Item2);

        //update fee, change output and rebuild
        transactionBody.SetFee(fee);
        transaction = transactionBuilder.Build();
        transaction.TransactionBody.TransactionOutputs.Last().Value.Coin = changeOutput.Value.Coin - fee;

        //serialize the transaction aka sign the transaction
        var signedTx = transaction.Serialize();
        var signedTxString = signedTx.ToStringHex();

        //submit tx
        var txHash = AsyncUtil.RunSync<string>(() => SubmitTx(signedTx));
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
        }catch(Exception e)
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
        catch(Exception e)
        {
            return e.Message;
        }
    }

    #endregion
}
