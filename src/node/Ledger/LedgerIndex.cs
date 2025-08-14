using System;
using System.Text.Json;
using core.Ledger;
using core.Utility;

namespace Node.Ledger;

[Serializable]
public class LedgerIndex : ILedgerIndex
{
    /// <summary>
    /// Ledger.Id
    /// </summary>
    public int BlockId { get; internal set; }
    public string Hash { get; internal set; }
    public DateTime Created { get; internal set; }
    
    public BlockStatus Status { get; internal set; }

    public override string ToString()
    {
        return $"{BlockId}:{Status}:{Hash}:{Created.ToFileDateTime()}";
    }

    public static LedgerIndex FromString(string data)
    {
        const string SEPARATOR = ":";
        const int BLOCK_ID = 0;
        const int STATUS = 1;
        const int HASH = 2;
        const int DATE = 3;
        
        string[] elements = data.Split(SEPARATOR);
        LedgerIndex index = new LedgerIndex();
        index.BlockId = Convert.ToInt32(elements[BLOCK_ID]);
        index.Status = Enum.Parse<BlockStatus>(elements[STATUS]);
        index.Hash = elements[HASH];
        DateTime expectedDate;
        if (false == DateTime.TryParseExact(elements[DATE], Constants.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out expectedDate))
            throw new LedgerNotValidException($"index is not valid");

        index.Created = expectedDate;
        return index;
    }

    public static ILedgerIndex FromJson(string json)
    {
        return JsonSerializer.Deserialize<LedgerIndex>(json);
    }
}
