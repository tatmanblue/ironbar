using System.Runtime;
using core.Ledger;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace storage;

/// <summary>
/// For now, combines both reader and writer for Azure Blob storage
/// Will use AccountKey authentication for simplicity
/// for reference:  https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-dotnet-get-started?tabs=azure-ad
/// </summary>
public class AzureBlobReaderWriter : ILedgerReader, ILedgerWriter
{
    private readonly ILogger<AzureBlobReaderWriter> logger;
    private readonly ILedgerIndexFactory indexFactory;
    private readonly ILedgerPhysicalBlockFactory blockFactory;
    private readonly BlobServiceClient blobServiceClient;
    private readonly string containerName;
    
    private const string INDEX_NAME = "index.json";
    private const string BLOB_PREFIX_NAME = "block-";
    
    // For performance (and potentially costs reasons) cache for ledger index list
    private List<ILedgerIndex> cachedLedgerIndexList = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="nodeName">Since each node stores its own indexes and blocks, nodeName services as index into the object storage</param>
    /// <param name="accountName"></param>
    /// <param name="accountKey"></param>
    public AzureBlobReaderWriter(ILogger<AzureBlobReaderWriter> logger, ILedgerIndexFactory indexFactory, 
        ILedgerPhysicalBlockFactory blockFactory, string nodeName, string accountName, string accountKey)
    {
        this.logger = logger;
        this.indexFactory = indexFactory;
        this.blockFactory = blockFactory;
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
        
        logger.LogDebug($"block index data has been retrieved | {json} |");

        var indexList = indexFactory.CreateList(json);
        
        cachedLedgerIndexList = indexList;

        return indexList;
    }

    public void SaveBlock(ILedgerPhysicalBlock block)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (!containerClient.Exists())
            throw new LedgerNotFoundException($"{containerName} ledger storage not found");
        
        var blobName = $"{BLOB_PREFIX_NAME}{block.Id}.json";
        var blobClient = containerClient.GetBlobClient(blobName);
        
        // TODO: would we want to use TypeFactory to handle different formats?
        // Upload block JSON to blob
        var json = block.ToJson();
        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        blobClient.Upload(stream, overwrite: true);
    }

    public void SaveLedgerIndex(List<ILedgerIndex> index)
    {
        var jsonArray = JsonConvert.SerializeObject(index);

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