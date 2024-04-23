using System;
using System.Security.Cryptography;
using System.Text;
using core.Ledger;
using core.Security;
using core.Utility;

namespace Node.Ledger;

public class PhysicalBlock : ILedgerPhysicalBlock
{
    public int Id { get; internal set; } = -1;

    public int ParentId { get; internal set; } = -1;
    public int ReferenceId { get; internal set; } = 0;

    public string ParentHash { get; internal set; } = "0";

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

    protected internal PhysicalBlock(BlockStatus status)
    {
        Status = status;
    }
    
    public string ComputeHash()
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
        string objectString = $"{ParentId}:{Id}:{ParentHash}:{ReferenceId}:{LedgerId}:{TimeStamp}:{Nonce}:{transactionDataString}";

        return objectString;
    }

    public override string ToString()
    {
        string transactionDataString = Encoding.UTF8.GetString(TransactionData, 0, TransactionData.Length); 
        // TODO: what about the sign block
        string objectString = $"{ParentId}:{Id}:{ParentHash}:{ReferenceId}:{LedgerId}:{TimeStamp.ToFileDateTime()}:{Nonce}:{Status}:{transactionDataString}:{Hash}";

        return objectString;
    }

    public static PhysicalBlock FromString(string data)
    {
        string[] elements = data.Split(":");
        DateTime timeStamp;
        if (false == DateTime.TryParseExact(elements[5], Constants.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out timeStamp))
            throw new LedgerNotValidException($"{elements[5]}");

        BlockStatus status = Enum.Parse<BlockStatus>(elements[7]);

        PhysicalBlock block = new PhysicalBlock(status)
        {
            ParentId = Convert.ToInt32(elements[0]),
            Id = Convert.ToInt32(elements[1]),
            ParentHash = elements[2],
            ReferenceId = Convert.ToInt32(elements[3]),
            LedgerId = Convert.ToInt32(elements[4]),
            TimeStamp = timeStamp,
            Nonce = Nonce.FromString(elements[6]),
            TransactionData = Encoding.UTF8.GetBytes(elements[8])
        };

        // compare hash from string with computedHash. they should match
        if (elements[9] != block.Hash)
            throw new LedgerNotValidException($"block {block.Id}");

        return block;
    }
}
