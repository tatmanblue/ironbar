using System;
using System.Security.Cryptography;
using System.Text;
using core.Ledger;
using core.Security;
using core.Utility;

namespace Node.Ledger;

public class PhysicalBlock : ILedgerPhysicalBlock
{
    #region ILedgerPhysicalBlock properties
    public int Id { get; internal set; } = -1;

    public int ParentId { get; internal set; } = -1;
    public int ReferenceId { get; internal set; } = 0;

    public string ParentHash { get; internal set; } = "0";
    public string ReferenceHash { get; internal set; } = HashUtility.ComputeHash(Nonce.New().ToString());

    public int LedgerId { get; internal set; }

    public DateTime TimeStamp { get; private set; } = DateTime.Now.ToUniversalTime();

    public Nonce Nonce { get; private set; } = Nonce.New();

    public string Hash
    {
        get { return ComputeHash(); }
    }

    public byte[] TransactionData { get; internal set; }

    public BlockStatus Status { get; private set; }

    public ILedgerSignBlock SignBlock { get; internal set; }
    #endregion
    
    protected internal PhysicalBlock(BlockStatus status)
    {
        Status = status;
    }
    
    private string ComputeHash()
    {
        return HashUtility.ComputeHash(HashString());
    }

    /// <summary>
    /// a string representation of the object sans a hash
    /// </summary>
    /// <returns></returns>
    private string HashString()
    {
        // TODO: what about the sign block and Status
        string transactionDataString = Encoding.UTF8.GetString(TransactionData, 0, TransactionData.Length);
        string objectString = $"{ParentId}:{Id}:{ParentHash}:{ReferenceId}:{ReferenceHash}:{LedgerId}:{TimeStamp.ToFileDateTime()}:{Nonce}:{transactionDataString}";

        return objectString;
    }

    public override string ToString()
    {
        /*
          Order of elements in the string are as follows
          0 - ParentId
          1 - BlockId
          2 - ParentHash
          3 - ReferenceId
          4 - ReferenceHash
          5 - LedgerId
          6 - Time Stamp
          7 - Nonce
          8 - status
          9 - actual data
         10 - hash
        */
        string transactionDataString = Encoding.UTF8.GetString(TransactionData, 0, TransactionData.Length); 
        
        // TODO: what about the sign block
        // DO NOT use  Verbatim String Literals (@) to make this easier to read is it affects actual output
        string objectString = $@"{ParentId}:{Id}:{ParentHash}:{ReferenceId}:{ReferenceHash}:{LedgerId}:{TimeStamp.ToFileDateTime()}:{Nonce}:{Status}:{transactionDataString}:{Hash}";

        return objectString;
    }

    public static PhysicalBlock FromString(string data)
    {
        /*
          Order of elements in the string are as follows
          0 - ParentId
          1 - BlockId
          2 - ParentHash
          3 - ReferenceId
          4 - ReferenceHash
          5 - LedgerId
          6 - Time Stamp
          7 - Nonce
          8 - status
          9 - actual data
         10 - hash
          
        */
        
        
        string[] elements = data.Split(":");
        DateTime timeStamp;
        if (false == DateTime.TryParseExact(elements[6], Constants.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out timeStamp))
            throw new LedgerBlockException($"Failed to get time stamp: {elements[6]}");

        BlockStatus status = Enum.Parse<BlockStatus>(elements[8]);

        PhysicalBlock block = new PhysicalBlock(status)
        {
            ParentId = Convert.ToInt32(elements[0]),
            Id = Convert.ToInt32(elements[1]),
            ParentHash = elements[2],
            ReferenceId = Convert.ToInt32(elements[3]),
            ReferenceHash = elements[4],
            LedgerId = Convert.ToInt32(elements[5]),
            TimeStamp = timeStamp,
            Nonce = Nonce.FromString(elements[7]),
            TransactionData = Encoding.UTF8.GetBytes(elements[9])
        };

        // compare hash from string with computedHash. they should match
        if (elements[10] != block.Hash)
            throw new LedgerBlockException($"block {block.Id} PhysicalBlock failed to deserialize");

        return block;
    }
    
    public static PhysicalBlock FromJson(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<PhysicalBlock>(json);
    }
}
