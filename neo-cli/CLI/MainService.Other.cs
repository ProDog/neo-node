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

namespace Neo.CLI
{
    partial class MainService
    {
        [ConsoleCommand("mint", Category = "Contract Commands")]
        private void OnMintCommand(UInt160 scriptHash, UInt160 asset, UInt160 to, string amount, UInt160 sender = null, UInt160[] signerAccounts = null)
        {
            Signer[] signers = Array.Empty<Signer>();
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
                signers = signerAccounts.Select(p => new Signer() { Account = p, Scopes = WitnessScope.CalledByEntry }).ToArray();
            }

            AssetDescriptor descriptor = new AssetDescriptor(asset);
            if (!BigDecimal.TryParse(amount, descriptor.Decimals, out BigDecimal decimalAmount) || decimalAmount.Sign <= 0)
            {
                Console.WriteLine("Incorrect Amount Format");
                return;
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                //scriptBuilder.EmitAppCall(NativeContract.NEO.Hash, "transfer", sender, to, (BigInteger)10);
                scriptBuilder.EmitAppCall(asset, "transfer", sender, to, (BigInteger)(decimal.Parse(amount) * (decimal)Math.Pow(10, descriptor.Decimals)));               
                scriptBuilder.EmitAppCall(scriptHash, "mint");
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
        /// Process "invoke" command
        /// </summary>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="operation">Operation</param>
        /// <param name="contractParameters">Contract parameters</param>
        /// <param name="sender">Transaction's sender</param>
        /// <param name="signerAccounts">Signer's accounts</param>
        //[ConsoleCommand("invoke", Category = "Contract Commands")]
        //private void OnInvokeCommand(UInt160 scriptHash, string operation, JArray contractParameters = null, UInt160 sender = null, UInt160[] signerAccounts = null)
        //{
        //    Signer[] signers = Array.Empty<Signer>();
        //    if (signerAccounts != null && !NoWallet())
        //    {
        //        if (sender != null)
        //        {
        //            if (signerAccounts.Contains(sender) && signerAccounts[0] != sender)
        //            {
        //                var signersList = signerAccounts.ToList();
        //                signersList.Remove(sender);
        //                signerAccounts = signersList.Prepend(sender).ToArray();
        //            }
        //            else if (!signerAccounts.Contains(sender))
        //            {
        //                signerAccounts = signerAccounts.Prepend(sender).ToArray();
        //            }
        //        }
        //        signers = signerAccounts.Select(p => new Signer() { Account = p, Scopes = WitnessScope.CalledByEntry }).ToArray();
        //    }

        //    Transaction tx = new Transaction
        //    {
        //        Signers = signers,
        //        Witnesses = Array.Empty<Witness>(),
        //    };

        //    _ = OnInvokeWithResult(scriptHash, operation, tx, contractParameters);

        //    if (NoWallet()) return;
        //    try
        //    {
        //        tx = CurrentWallet.MakeTransaction(tx.Script, sender, signers);
        //    }
        //    catch (InvalidOperationException e)
        //    {
        //        Console.WriteLine("Error: " + GetExceptionMessage(e));
        //        return;
        //    }
        //    if (!ReadUserInput("Relay tx(no|yes)").IsYes())
        //    {
        //        return;
        //    }
        //    SignAndSendTx(tx);
        //}
    }
}
