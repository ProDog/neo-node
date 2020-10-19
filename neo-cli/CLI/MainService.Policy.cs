using Neo.ConsoleService;
using Neo.SmartContract.Native;
using Neo.VM;
using System;
using Neo.Cryptography.ECC;
using Neo.VM.Types;
using Neo.IO.Json;
using System.Collections.Generic;
using Neo.SmartContract;
using System.Linq;
using Akka.Util.Internal;
using Neo.Network.P2P.Payloads;

namespace Neo.CLI
{
    partial class MainService
    {
        /// <summary>
        /// Process "set max block size" command
        /// </summary>
        /// <param name="value">size value</param>
        [ConsoleCommand("set max block", Category = "Policy Commands")]
        private void OnSetMaxBlockSizeCommand(uint value, UInt160 senderAccount)
        {
            if (NoWallet())
            {
                Console.WriteLine("Need open wallet!");
                return;
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(NativeContract.Policy.Hash, "setMaxBlockSize", value);
                script = scriptBuilder.ToArray();
            }

            SendTransaction(script, senderAccount);
        }

        /// <summary>
        /// Process "set max block transactions" command
        /// </summary>
        /// <param name="value">transactions value</param>
        [ConsoleCommand("set max transactions", Category = "Policy Commands")]
        private void OnSetMaxTransactionsPerBlockCommand(uint value, UInt160 senderAccount)
        {
            if (NoWallet())
            {
                Console.WriteLine("Need open wallet!");
                return;
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(NativeContract.Policy.Hash, "setMaxTransactionsPerBlock", value);
                script = scriptBuilder.ToArray();
            }

            SendTransaction(script, senderAccount);
        }

        /// <summary>
        /// Process "set fee per byte" command
        /// </summary>
        /// <param name="value">fee value</param>
        [ConsoleCommand("set byte fee", Category = "Policy Commands")]
        private void OnSetFeePerBytePerBlockCommand(uint value, UInt160 senderAccount)
        {
            if (NoWallet())
            {
                Console.WriteLine("Need open wallet!");
                return;
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(NativeContract.Policy.Hash, "setFeePerByte", value);
                script = scriptBuilder.ToArray();
            }

            SendTransaction(script, senderAccount);
        }

        /// <summary>
        /// Process "block account" command
        /// </summary>
        /// <param name="account">block account</param>
        [ConsoleCommand("block account", Category = "Policy Commands")]
        private void OnBlockAccountCommand(UInt160 account, UInt160 senderAccount)
        {
            if (NoWallet())
            {
                Console.WriteLine("Need open wallet!");
                return;
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(NativeContract.Policy.Hash, "blockAccount", account);
                script = scriptBuilder.ToArray();
            }

            SendTransaction(script, senderAccount);
        }

        /// <summary>
        /// Process "unblock account" command
        /// </summary>
        /// <param name="account">block account</param>
        [ConsoleCommand("unblock account", Category = "Policy Commands")]
        private void OnUnBlockAccountCommand(UInt160 account, UInt160 senderAccount)
        {
            if (NoWallet())
            {
                Console.WriteLine("Need open wallet!");
                return;
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(NativeContract.Policy.Hash, "unblockAccount", account);
                script = scriptBuilder.ToArray();
            }

            SendTransaction(script, senderAccount);
        }

        /// <summary>
        /// Process "get blocked accounts"
        /// </summary>
        [ConsoleCommand("get blocked accounts", Category = "Policy Commands")]
        private void OnGetBlockedAccountsCommand()
        {
            if (!OnInvokeWithResult(NativeContract.Policy.Hash, "getBlockedAccounts", out StackItem result, null, null, false)) return;

            var resJArray = (VM.Types.Array)result;

            if (resJArray.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Blocked accounts:");

                foreach (var item in resJArray)
                {
                    Console.WriteLine(((ByteString)item)?.GetSpan().ToHexString());
                }
            }
        }

        /// <summary>
        /// Process "get fee per byte"
        /// </summary>
        [ConsoleCommand("get byte fee", Category = "Policy Commands")]
        private void OnGetFeePerByteCommand()
        {
            if (!OnInvokeWithResult(NativeContract.Policy.Hash, "getFeePerByte", out StackItem result, null, null, false)) return;

            Console.WriteLine("Fee per byte: " + result.GetInteger());
        }

        /// <summary>
        /// Process "get max block sys fee"
        /// </summary>
        [ConsoleCommand("get max fee", Category = "Policy Commands")]
        private void OnGetMaxBlockFeeCommand()
        {
            if (!OnInvokeWithResult(NativeContract.Policy.Hash, "getMaxBlockSystemFee", out StackItem result, null, null, false)) return;

            Console.WriteLine("Max block fee: " + result.GetInteger());
        }

        /// <summary>
        /// Process "get max block size"
        /// </summary>
        [ConsoleCommand("get max block", Category = "Policy Commands")]
        private void OnGetMaxBlockSizeCommand()
        {
            if (!OnInvokeWithResult(NativeContract.Policy.Hash, "getMaxBlockSize", out StackItem result, null, null, false)) return;

            Console.WriteLine("Max block size: " + result.GetInteger());
        }

        /// <summary>
        /// Process "get max transactions per block"
        /// </summary>
        [ConsoleCommand("get max transactions", Category = "Policy Commands")]
        private void OnGetMaxTransactionsPerBlockCommand()
        {
            if (!OnInvokeWithResult(NativeContract.Policy.Hash, "getMaxTransactionsPerBlock", out StackItem result, null, null, false)) return;

            Console.WriteLine("Max block transactions: " + result.GetInteger());
        }

        /// <summary>
        /// 
        /// </summary>
        [ConsoleCommand("get designated", Category = "Other Commands")]
        private void OnGetDesignatedByRoleCommand(uint role)
        {
            var arg = new JObject();
            arg["type"] = "Integer";
            arg["value"] = role;
            if (!OnInvokeWithResult(NativeContract.Designate.Hash, "getDesignatedByRole", out StackItem result, null, new JArray(arg), false)) return;

            var resJArray = (VM.Types.Array)result;

            if (resJArray.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Designated:");

                foreach (var item in resJArray)
                {
                    Console.WriteLine(((ByteString)item)?.GetSpan().ToHexString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConsoleCommand("get designated by index", Category = "Other Commands")]
        private void OnGetDesignatedByIndexCommand(uint role, uint index)
        {
            var arg1 = new JObject();
            arg1["type"] = "Integer";
            arg1["value"] = role;
            var arg2 = new JObject();
            arg2["type"] = "Integer";
            arg2["value"] = index;

            if (!OnInvokeWithResult(NativeContract.Designate.Hash, "getDesignatedByRoleAndIndex", out StackItem result, null, new JArray(arg1, arg2), false)) return;

            var resJArray = (VM.Types.Array)result;

            if (resJArray.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Designated:");

                foreach (var item in resJArray)
                {
                    Console.WriteLine(((ByteString)item)?.GetSpan().ToHexString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ConsoleCommand("designate", Category = "Other Commands")]
        private void OnDesignateAsRoleCommand(JArray contractParameters = null, UInt160 sender = null, UInt160[] signerAccounts = null)
        {
            //[{"type":"Integer","value":4},{"type":"Array","value":[{"type":"PublicKey","value":"02a9ea6842cc0cb3b0f2317b07c850de3d1e2b243a98ed2d56a3ff4ca66aaf330b"},{"type":"PublicKey","value":"02a9ea6842cc0cb3b0f2317b07c850de3d1e2b243a98ed2d56a3ff4ca66aaf330b"}]}]

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
                signers = signerAccounts.Select(p => new Signer() { Account = p, Scopes = WitnessScope.CalledByEntry }).ToArray();
            }

            if (NoWallet())
            {
                Console.WriteLine("Need open wallet!");
                return;
            }

            Transaction tx = new Transaction
            {
                Signers = signers,
                Attributes = System.Array.Empty<TransactionAttribute>(),
                Witnesses = System.Array.Empty<Witness>(),
            };

            if (!OnInvokeWithResult(NativeContract.Designate.Hash, "designateAsRole", out _, tx, contractParameters)) return;

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

            //List<ContractParameter> parameters = new List<ContractParameter>();
            //parameters.Add(new ContractParameter() { Type = ContractParameterType.Integer, Value = role });
            //parameters.Add(new ContractParameter() { Type = ContractParameterType.Array, Value = publicKeys.Select(p=> new ContractParameter() { Type = ContractParameterType.PublicKey, Value = p }).ToArray() });

            //byte[] script;
            //using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            //{
            //    scriptBuilder.EmitAppCall(NativeContract.Designate.Hash, "designateAsRole", parameters.ToArray());
            //    script = scriptBuilder.ToArray();
            //}

            //SendTransaction(script, senderAccount);
        }
    }
}
