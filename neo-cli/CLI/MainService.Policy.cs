using Neo.ConsoleService;
using Neo.SmartContract.Native;
using Neo.VM;
using System;
using Neo.Cryptography.ECC;
using Neo.VM.Types;

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
        private void OnSetFeePerBytePerBlockCommand(long value, UInt160 senderAccount)
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
            var result = OnInvokeWithResult(NativeContract.Policy.Hash, "getBlockedAccounts", null, null, false);

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
            var result = OnInvokeWithResult(NativeContract.Policy.Hash, "getFeePerByte", null, null, false);

            Console.WriteLine("Fee per byte: " + result.GetBigInteger());
        }

        /// <summary>
        /// Process "get max block size"
        /// </summary>
        [ConsoleCommand("get max block", Category = "Policy Commands")]
        private void OnGetMaxBlockSizeCommand()
        {
            var result = OnInvokeWithResult(NativeContract.Policy.Hash, "getMaxBlockSize", null, null, false);

            Console.WriteLine("Max block size: " + result.GetBigInteger());
        }

        /// <summary>
        /// Process "get max transactions per block"
        /// </summary>
        [ConsoleCommand("get max transactions", Category = "Policy Commands")]
        private void OnGetMaxTransactionsPerBlockCommand()
        {
            var result = OnInvokeWithResult(NativeContract.Policy.Hash, "getMaxTransactionsPerBlock", null, null, false);

            Console.WriteLine("Max block transactions: " + result.GetBigInteger());
        }
    }
}
