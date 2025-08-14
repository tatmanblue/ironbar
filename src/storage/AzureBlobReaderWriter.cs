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
public class AzureBlobReaderWriter : ILedgerReader, ILedgerWriter
{
    private readonly ILogger<AzureBlobReaderWriter> logger;
    private readonly BlobServiceClient blobServiceClient;
    private readonly string containerName;
    
    private const string INDEX_NAME = "index.json";
    private const string BLOB_PREFIX_NAME = "block-";
    
    // Cache for ledger index list
    private List<ILedgerIndex> cachedLedgerIndexList;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="nodeName">Since each node stores its own indexes and blocks, nodeName services as index into the object storage</param>
    /// <param name="accountName"></param>
    /// <param name="accountKey"></param>
    public AzureBlobReaderWriter(ILogger<AzureBlobReaderWriter> logger, string nodeName, string accountName, string accountKey)
    {
        this.logger = logger;
        containerName = nodeName;
        blobServiceClient = GetBlobServiceClient(accountName, accountKey);
    }
    
    public int CountBlocks()
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        int count = 0;
        foreach (var _ in containerClient.GetBlobs(prefix: BLOB_PREFIX_NAME))
        {
            count++;
        }
        return count;
    }

    public ILedgerPhysicalBlock GetLedgerPhysicalBlock(int id, Func<string, ILedgerPhysicalBlock> blockAllocator)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobName = $"{BLOB_PREFIX_NAME}{id}.json";
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.Exists())
            throw new LedgerNotValidException($"Block {id} not found in container {containerName}");

        var downloadInfo = blobClient.DownloadContent();
        var json = downloadInfo.Value.Content.ToString();

        return blockAllocator(json);
    }

    public ILedgerIndex GetLedgerIndex(int index, Func<string, ILedgerIndex> indexAllocator)
    {
        // Load cache if not loaded
        if (cachedLedgerIndexList == null)
        {
            cachedLedgerIndexList = GetLedgerIndex(indexAllocator);
        }

        if (index < 0 || index >= cachedLedgerIndexList.Count)
            throw new LedgerNotValidException($"Ledger index {index} is out of range.");

        return cachedLedgerIndexList[index];
    }

    public List<ILedgerIndex> GetLedgerIndex(Func<string, ILedgerIndex> indexAllocator)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(INDEX_NAME);

        if (!blobClient.Exists())
            throw new LedgerNotFoundException($"{containerName} ledger index not found");

        var downloadInfo = blobClient.DownloadContent();
        var json = downloadInfo.Value.Content.ToString();
        
        var indexList = new List<ILedgerIndex>();
        var jsonArray = System.Text.Json.JsonDocument.Parse(json).RootElement;

        foreach (var element in jsonArray.EnumerateArray())
        {
            var elementJson = element.GetRawText();
            var index = indexAllocator(elementJson);
            indexList.Add(index);
        }
        
        cachedLedgerIndexList = indexList;

        return indexList;
    }

    public void SaveBlock(ILedgerPhysicalBlock block)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (!containerClient.Exists())
            throw new LedgerNotFoundException($"{containerName} ledger storage not found");

        // Blob name for the block
        var blobName = $"{BLOB_PREFIX_NAME}{block.Id}.json";
        var blobClient = containerClient.GetBlobClient(blobName);

        // Serialize block to JSON
        var json = System.Text.Json.JsonSerializer.Serialize(block);

        // Upload block JSON to blob
        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        blobClient.Upload(stream, overwrite: true);
    }

    public void SaveLedgerIndex(List<ILedgerIndex> index)
    {
        var jsonList = new List<string>();
        foreach (var idx in index)
        {
            // Serialize each index object
            var json = System.Text.Json.JsonSerializer.Serialize(idx);
            jsonList.Add(json);
        }
        var jsonArray = "[" + string.Join(",", jsonList) + "]";

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(INDEX_NAME);
        
        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonArray));
        blobClient.Upload(stream, overwrite: true);
        
        cachedLedgerIndexList = new List<ILedgerIndex>(index);
    }

    public void InitializeStorage()
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (!containerClient.Exists())
        {
            containerClient.Create();
            logger.LogInformation($"Created Azure Blob container: {containerName}");
        }
    }

    public void CheckStorage()
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (!containerClient.Exists())
            throw new LedgerNotFoundException($"{containerName} ledger storage not found");
        
        logger.LogInformation($"Azure Blob container '{containerName}' already exists.");
    }

    private BlobServiceClient GetBlobServiceClient(string accountName, string accountKey)
    {
        Azure.Storage.StorageSharedKeyCredential sharedKeyCredential =
            new StorageSharedKeyCredential(accountName, accountKey);

        string blobUri = "https://" + accountName + ".blob.core.windows.net";

        return new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);
    }
}