using System;
using System.Text.Json;
using core.Ledger;
using core.Utility;
using Newtonsoft.Json;

namespace Node.Ledger;

/// <summary>
/// The original design intended to protect the data structure by the interface and
/// private setters for data.  However, this complicates deserialization with JSON.NET and other serializers.
/// Thus, this is now a class with public getters and setters.  We can revisit
/// the design later if needed.
/// </summary>
[Serializable]
public class LedgerIndex : ILedgerIndex
{
    /// <summary>
    /// Ledger.Id
    /// </summary>
    public int BlockId { get; set; }
    public string Hash { get; set; }
    public DateTime Created { get; set; }
    
    public BlockStatus Status { get; set; }

    public override string ToString()
    {
        return $"{BlockId}:{Status}:{Hash}:{Created.ToFileDateTime()}";
    }

    /// <summary>
    /// used in conjunction with JSON deserialization for blob storage and/or where storage type is not file system based
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    /// <exception cref="LedgerBlockException"></exception>
    public static ILedgerIndex FromJson(string json)
    {
        return JsonConvert.DeserializeObject<LedgerIndex>(json) ?? throw new LedgerBlockException("Failed to deserialize LedgerIndex from JSON");
    }
    
    /// <summary>
    /// Primarily for creating new blocks via "serialized" data and isolating the implementation details
    /// when using files as the storage system:
    /// IRONBAR_STORAGE_TYPE=files, StorageType.FileSystem
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="LedgerNotValidException"></exception>
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
}
