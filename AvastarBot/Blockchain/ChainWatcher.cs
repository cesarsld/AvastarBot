using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts;
using System.Numerics;
using Discord;
namespace AvastarBot.Blockchain
{
    public class ChainWatcher
    {
        public static string web3Url = "https://mainnet.infura.io/v3/b4e2781f02a94a5a96dcf8ce8cab9449";
        public static string avastarContractAddress = "0xF3E778F839934fC819cFA1040AabaCeCBA01e049";

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

            BlockParameter lastBlock = await GetLastBlockCheckpoint(web3);
            BlockParameter firstBlock = new BlockParameter(new HexBigInteger(lastBlock.BlockNumber.Value - 4));
            while (isOn)
            {
                if (first)
                {
                    firstBlock = new BlockParameter(new HexBigInteger(lastBlock.BlockNumber.Value - 400));
                    first = false;
                }
                else
                    firstBlock = new BlockParameter(new HexBigInteger(lastBlock.BlockNumber.Value));
                lastBlock = await GetLastBlockCheckpoint(web3);
                try
                {
                    // event filters
                    var newPrimeFilter = newPrimeEvent.CreateFilterInput(firstBlock, lastBlock);

                    // event logs from block range
                    var newPrimeLogs = await newPrimeEvent.GetAllChanges(newPrimeFilter);

                    foreach (var prime in newPrimeLogs)
                    {
                        await PostToBirthChannel((int)prime.Event.id);
                    }
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
            var embed = await AvastarCommands.GenerateAvastarEmbed(id, 0, "", "", " just got minted!");
            var channel = Bot.GetChannelContext(716716146514198571) as IMessageChannel;
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
}
