using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts;
using System.Numerics;
using Discord;
using AvastarBot.Mongo;
using MongoDB.Driver;
namespace AvastarBot.Blockchain
{
    public class ChainWatcher
    {
        public static string web3Url = "https://mainnet.infura.io/v3/b4e2781f02a94a5a96dcf8ce8cab9449";
        public static string avastarContractAddress = "0xF3E778F839934fC819cFA1040AabaCeCBA01e049";
        private static int lastMinted = 0;

        public static async Task<BigInteger> GetAvastarCount()
        {
            var web3 = new Web3(web3Url);

            var handler = web3.Eth.GetContractQueryHandler<totalSupplyFunction>();
            var number = await handler.QueryAsync<BigInteger>(avastarContractAddress);
            return number;
        }

        public static async Task WatchChainForEvents()
        {
            // initiate web, contract and events
            bool first = false;
            Web3 web3 = new Web3(web3Url);
            var newPrimeEvent = web3.Eth.GetEvent<NewPrimeEvent>(avastarContractAddress);
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
                    if (lastBlock.BlockNumber.Value - firstBlock.BlockNumber.Value > 20000) {
                        lastBlock = new BlockParameter(new HexBigInteger(firstBlock.BlockNumber.Value + 20000));
                    }
                    // event filters
                    var newPrimeFilter = newPrimeEvent.CreateFilterInput(firstBlock, lastBlock);

                    // event logs from block range
                    var newPrimeLogs = await newPrimeEvent.GetAllChanges(newPrimeFilter);

                    foreach (var prime in newPrimeLogs)
                    {
                        if ((int)prime.Event.id > lastMinted)
                        {
                            if (await AvastarObject.GetAvaCount() == (int)prime.Event.id)
                            {
                                await AvastarObject.CreateAva((int)prime.Event.id);
                                lastMinted = (int)prime.Event.id;
                                await PostToBirthChannel((int)prime.Event.id);
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

        private static async Task PostToBirthChannel(int id)
        {
            var embed = await AvastarCommands.GenerateAvastarEmbed(id, 0, "", "", " teleported!");
            var channel = Bot.GetChannelContext(725041690133397555) as IMessageChannel;
            await channel.SendMessageAsync(embed: embed.Build());
        }

        private static async Task<BlockParameter> GetLastBlockCheckpoint(Web3 web3)
        {
            var lastBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumber = lastBlock.Value - 4;
            return new BlockParameter(new HexBigInteger(blockNumber));
        }
    }

    [FunctionOutput]
    public class AvastarCount : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public BigInteger Balance { get; set; }
    }

    [Function("totalSupply", "uint256")]
    public class totalSupplyFunction : FunctionMessage
    {
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
}
