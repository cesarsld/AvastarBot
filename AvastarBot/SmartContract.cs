using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace AvastarBot
{
    public class SmartContract
    {
        public static async Task<float> GetGas(string method)
        {
            string data = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    data = await wc.DownloadStringTaskAsync("https://ethgasstation.info/api/ethgasAPI.json");
                }
                catch (Exception e)
                {

                }
            }
            var json = JObject.Parse(data);
            return ((float)json[method]) / 10;
        }

        /*

        public static async Task WatchChainForEvents()
        {
            // initiate web, contract and events
            Web3 web3;
            if (Program.IsRelease)
                web3 = new Web3("https://mainnet.infura.io/v3/b4e2781f02a94a5a96dcf8ce8cab9449");
            else
                web3 = new Web3("https://rinkeby.infura.io/v3/b4e2781f02a94a5a96dcf8ce8cab9449");
            var tipContractAddress = DiscordKeyGetter.GetFileData($"data/TipContract/address");
            var discordDepositEvent = web3.Eth.GetEvent<DiscordDepositEvent>(tipContractAddress);
            bool isOn = true;

            // get last block param from db
            var checkCollec = DatabaseConnection.GetDb().GetCollection<Checkpoint>("Checkpoints");
            var checkpoint = (await checkCollec.FindAsync(c => c.id == 1)).FirstOrDefault();

            BlockParameter lastBlock = await GetLastBlockCheckpoint(web3);
            BlockParameter firstBlock = new BlockParameter(new HexBigInteger(new BigInteger(checkpoint.lastBlockChecked)));
            while (isOn)
            {
                checkpoint = (await checkCollec.FindAsync(c => c.id == 1)).FirstOrDefault();
                firstBlock = new BlockParameter(new HexBigInteger(new BigInteger(checkpoint.lastBlockChecked)));
                lastBlock = await GetLastBlockCheckpoint(web3);
                try
                {
                    // event filters
                    var discordDepositFilter = discordDepositEvent.CreateFilterInput(firstBlock, lastBlock);

                    // event logs from block range
                    var depositLogs = await discordDepositEvent.GetAllChanges(discordDepositFilter);

                    foreach (var deposit in depositLogs)
                    {
                        if (!(await TransactionLog.CheckifEventIsLogged(deposit.Log.TransactionHash)))
                        {
                            var token = await ServiceData.GetToken(deposit.Event.tokenContract);
                            if (token != null)
                            {
                                await TransferFunctions.DepositTokens(
                                    deposit.Event.discordId,
                                    deposit.Event.tokenContract,
                                    deposit.Event.amount,
                                    deposit.Log.TransactionHash);
                                await Bot.GetUser(deposit.Event.discordId).SendMessageAsync($"Your deposit of {TransferFunctions.ConvertUintToReadable(deposit.Event.amount.ToString(), token.Decimal)} {token.Symbol} has been added to your account!");
                            }
                        }
                    }
                    checkpoint.lastBlockChecked = Convert.ToInt32(lastBlock.BlockNumber.Value.ToString());
                    await checkCollec.FindOneAndReplaceAsync(c => c.id == 1, checkpoint);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                await Task.Delay(60000);
            }
        }

        */
        private static async Task<BlockParameter> GetLastBlockCheckpoint(Web3 web3)
        {
            var lastBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumber = lastBlock.Value - 6;
            return new BlockParameter(new HexBigInteger(blockNumber));
        }

        private static async Task PostToTestChannel(string msg)
        {
            var channel = Bot.GetChannelContext(582891241906241546) as IMessageChannel;
            await channel.SendMessageAsync(msg);
        }

        private static async Task PostToTestChannel(Embed msg)
        {
            var channel = Bot.GetChannelContext(582891241906241546) as IMessageChannel;
            await channel.SendMessageAsync(embed: msg);
        }
    }
}

public class Checkpoint
{
    public int id;
    public int lastBlockChecked;
    public Checkpoint(int _id, int _lastBlockChecked)
    {
        id = _id;
        lastBlockChecked = _lastBlockChecked;
    }
}
