using Neo.ConsoleService;
using Neo.Wallets;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using System;
using System.Linq;
using Neo.VM;
using System.Numerics;
using Neo.Ledger;
using Neo.SmartContract.Native;
using System.IO;
using Neo.SmartContract.Manifest;
using System.Text;
using Neo.IO;
using System.Collections.Generic;
using Neo.Persistence;
using Akka.Actor;

namespace Neo.CLI
{
    partial class MainService
    {

        [ConsoleCommand("mint", Category = "Contract Commands")]
        private void OnMintCommand(UInt160 scriptHash, UInt160 asset, string amount, UInt160 sender = null, UInt160[] signerAccounts = null)
        {
            Signer[] signers = System.Array.Empty<Signer>();
            if (signerAccounts != null && !NoWallet())
            {
                if (sender != null)
                {
                    if (signerAccounts.Contains(sender) && signerAccounts[0] != sender)
                    {
                        var signersList = signerAccounts.ToList();
                        signersList.Remove(sender);
                        signerAccounts = signersList.Prepend(sender).ToArray();
                    }
                    else if (!signerAccounts.Contains(sender))
                    {
                        signerAccounts = signerAccounts.Prepend(sender).ToArray();
                    }
                }
                signers = signerAccounts.Select(p => new Signer() { Account = p, Scopes = WitnessScope.Global }).ToArray();
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(asset, "transfer", sender, scriptHash, BigInteger.Parse(amount));
                scriptBuilder.EmitAppCall(scriptHash, "mint");
                script = scriptBuilder.ToArray();
                Console.WriteLine($"Invoking script with: '{script.ToHexString()}'");
            }

            Transaction tx = new Transaction
            {
                Signers = signers,
                Witnesses = System.Array.Empty<Witness>(),
                Attributes = System.Array.Empty<TransactionAttribute>(),
                Version = 0,
                Script = script
            };

            using ApplicationEngine engine = ApplicationEngine.Run(script, Blockchain.Singleton.GetSnapshot(), container: tx);
            PrintExecutionOutput(engine, true);

            if (NoWallet()) return;
            try
            {
                tx = CurrentWallet.MakeTransaction(tx.Script, sender, signers);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Error: " + GetExceptionMessage(e));
                return;
            }
            if (!ReadUserInput("Relay tx(no|yes)").IsYes())
            {
                return;
            }
            SignAndSendTx(tx);
        }

        /// <summary>
        /// Process "migrate" command
        /// </summary>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="nefFilePath">nft and manifest path</param>
        /// <param name="signerAccounts">Witness address</param>
        [ConsoleCommand("update", Category = "Contract Commands")]
        private void OnUpdateCommand(UInt160 scriptHash, string nefFilePath, UInt160[] signerAccounts = null)
        {
            Signer[] signers = Array.Empty<Signer>();
            UInt160 sender = UInt160.Zero;
            if (signerAccounts != null && !NoWallet())
            {
                sender = signerAccounts[0];
                signers = signerAccounts.Select(p => new Signer() { Account = p, Scopes = WitnessScope.CalledByEntry }).ToArray();
            }

            string manifestFilePath = Path.ChangeExtension(nefFilePath, ".manifest.json");

            // Read manifest

            var info = new FileInfo(manifestFilePath);
            if (!info.Exists || info.Length >= Transaction.MaxTransactionSize)
            {
                throw new ArgumentException(nameof(manifestFilePath));
            }

            var manifest = ContractManifest.Parse(File.ReadAllBytes(manifestFilePath));

            // Read nef

            NefFile nef;
            info = new FileInfo(nefFilePath);
            if (!info.Exists || info.Length >= Transaction.MaxTransactionSize)
            {
                throw new ArgumentException(nameof(nefFilePath));
            }

            using (var stream = new BinaryReader(File.OpenRead(nefFilePath), Utility.StrictUTF8, false))
            {
                nef = stream.ReadSerializable<NefFile>();
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(scriptHash, "update", nef.ToArray(), manifest.ToJson().ToString());
                script = scriptBuilder.ToArray();
                Console.WriteLine($"Invoking script with: '{script.ToHexString()}'");
            }

            Transaction tx = new Transaction
            {
                Signers = signers,
                Witnesses = Array.Empty<Witness>(),
                Attributes = Array.Empty<TransactionAttribute>(),
                Version = 0,
                Script = script
            };

            using var sb = new ScriptBuilder();
            sb.Emit(OpCode.ABORT);
            sb.EmitPush(tx.Sender);
            sb.EmitPush(nef.Script);
            UInt160 hash = sb.ToArray().ToScriptHash();
            Console.WriteLine($"New contract hash: {hash}");

            using ApplicationEngine engine = ApplicationEngine.Run(script, Blockchain.Singleton.GetSnapshot(), container: tx);
            PrintExecutionOutput(engine, true);

            if (NoWallet()) return;
            try
            {
                tx = CurrentWallet.MakeTransaction(tx.Script, sender, signers);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Error: " + GetExceptionMessage(e));
                return;
            }
            if (!ReadUserInput("Relay tx(no|yes)").IsYes())
            {
                return;
            }
            SignAndSendTx(tx);
        }
    }
}
