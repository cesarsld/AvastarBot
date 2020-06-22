using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;
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
