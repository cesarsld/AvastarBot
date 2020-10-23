using System;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace AvastarBot.Blockchain
{
    // NewPrime (uint256 id, uint256 serial, uint8 generation, uint8 series, uint8 gender, uint256 traits)
    [Event("NewPrime")]
    public class NewPrimeEvent : IEventDTO
    {
        [Parameter("uint256", "id", 1)]
        public BigInteger id { get; set; }

        [Parameter("uint256", "serial", 2)]
        public BigInteger serial { get; set; }

        [Parameter("uint8", "generation", 3)]
        public int generation { get; set; }

        [Parameter("uint8", "series", 4)]
        public int series { get; set; }

        [Parameter("uint8", "gender", 5)]
        public int gender { get; set; }

        [Parameter("uint256", "traits", 6)]
        public BigInteger traits { get; set; }
    }

    [Event("Transfer")]
    public class Transfer : IEventDTO {
        [Parameter("address", "from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "tokenId", 3, true)]
        public BigInteger TokenId { get; set; }
    }
}
