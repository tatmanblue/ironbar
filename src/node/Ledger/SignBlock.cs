using System;
using core.Ledger;
using core.Utility;
using Newtonsoft.Json;

namespace Node.Ledger;

public class SignBlock : ILedgerSignBlock
{
    protected internal SignBlock()
    {
    }

    public Nonce Nonce { get; private set; } = Nonce.New();

    public DateTime DateStamp { get; private set; } = DateTime.Now.ToUniversalTime();
    
    public static SignBlock FromJson(string json)
    {
        return JsonConvert.DeserializeObject<SignBlock>(json) ?? 
               throw new LedgerBlockException("Failed to deserialize SignBlock from JSON");
    }
}
