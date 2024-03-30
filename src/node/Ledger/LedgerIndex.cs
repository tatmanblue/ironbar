using System;
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

    public override string ToString()
    {
        return $"{BlockId}:{Hash}:{Created.ToFileDateTime()}";
    }

    public static LedgerIndex FromString(string data)
    {
        string[] elements = data.Split(":");
        LedgerIndex index = new LedgerIndex();
        index.BlockId = Convert.ToInt32(elements[0]);
        index.Hash = elements[1];
        DateTime expectedDate;
        if (false == DateTime.TryParseExact(elements[2], Constants.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out expectedDate))
            throw new LedgerNotValidException($"index is not valid");

        index.Created = expectedDate;
        return index;
    }
}
