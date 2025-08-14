using core.Ledger;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;

namespace storage;

/// <summary>
/// For now, combines both reader and writer for Azure Blob storage
/// Will use AccountKey authentication for simplicity
/// for reference:  https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-dotnet-get-started?tabs=azure-ad
/// </summary>
public class AzureBlogReaderWriter : ILedgerReader, ILedgerWriter
{
    private readonly ILogger<AzureBlogReaderWriter> logger;
    private readonly BlobServiceClient blobServiceClient;
    private readonly string containerName;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="nodeName">Since each node stores its own indexes and blocks, nodeName services as index into the object storage</param>
    /// <param name="accountName"></param>
    /// <param name="accountKey"></param>
    public AzureBlogReaderWriter(ILogger<AzureBlogReaderWriter> logger, string nodeName, string accountName, string accountKey)
    {
        this.logger = logger;
        containerName = nodeName;
        blobServiceClient = GetBlobServiceClient(accountName, accountKey);
    }
    
    public int CountBlocks()
    {
        throw new NotImplementedException();
    }

    public ILedgerPhysicalBlock GetLedgerPhysicalBlock(int id, Func<string, ILedgerPhysicalBlock> blockAllocator)
    {
        throw new NotImplementedException();
    }

    public ILedgerIndex GetLedgerIndex(int index, Func<string, ILedgerIndex> indexAllocator)
    {
        throw new NotImplementedException();
    }

    public List<ILedgerIndex> GetLedgerIndex(Func<string, ILedgerIndex> indexAllocator)
    {
        // Assume container name is "ledger" and blob name is "ledger-index.json"
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient("index.json");

        if (!blobClient.Exists())
            throw new LedgerNotFoundException($"{containerName} ledger index not found");

        var downloadInfo = blobClient.DownloadContent();
        var json = downloadInfo.Value.Content.ToString();

        // Assume the blob contains a JSON array of index objects
        var indexList = new List<ILedgerIndex>();
        var jsonArray = System.Text.Json.JsonDocument.Parse(json).RootElement;

        foreach (var element in jsonArray.EnumerateArray())
        {
            var elementJson = element.GetRawText();
            var index = indexAllocator(elementJson);
            indexList.Add(index);
        }

        return indexList;
    }

    public void SaveBlock(ILedgerPhysicalBlock block)
    {
        throw new NotImplementedException();
    }

    public void SaveLedgerIndex(List<ILedgerIndex> index)
    {
        // Serialize each ILedgerIndex to JSON using System.Text.Json
        var jsonList = new List<string>();
        foreach (var idx in index)
        {
            // Serialize each index object
            var json = System.Text.Json.JsonSerializer.Serialize(idx);
            jsonList.Add(json);
        }
        // Combine into a JSON array
        var jsonArray = "[" + string.Join(",", jsonList) + "]";

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient("index.json");

        // Upload the JSON array to the blob
        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonArray));
        blobClient.Upload(stream, overwrite: true);
    }
    
    private BlobServiceClient GetBlobServiceClient(string accountName, string accountKey)
    {
        Azure.Storage.StorageSharedKeyCredential sharedKeyCredential =
            new StorageSharedKeyCredential(accountName, accountKey);

        string blobUri = "https://" + accountName + ".blob.core.windows.net";

        return new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);
    }
}