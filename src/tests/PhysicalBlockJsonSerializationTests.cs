using core.Ledger;
using core.Security;
using Newtonsoft.Json;
using Node.Ledger;

namespace tests;

public class PhysicalBlockJsonSerializationTests
{
    [Fact]
    public void PhysicalBlockSerializesCorrectlyTest()
    {
        string parentHash = HashUtility.ComputeHash("parent");
        var signBlockTypeFactory = new JsonSignBlockTypeFactory();
        
        PhysicalBlock block = new PhysicalBlock()
        {
            Id = 1,
            ParentHash = parentHash,
            ParentId = 1,
            ReferenceId = 0,
            ReferenceHash = parentHash,
            LedgerId = 1,
            TransactionData = System.Text.Encoding.ASCII.GetBytes(parentHash),
            SignBlock = signBlockTypeFactory.Create()
        };
        string json = block.ToJson();
        
        var settings = new JsonSerializerSettings
        {
            Converters = { new SignBlockConverter(signBlockTypeFactory) },
            Formatting = Formatting.Indented
        };

        PhysicalBlock? deserialized = PhysicalBlock.FromJson(json, settings);
        Assert.NotNull(deserialized);
        Assert.Equal(block.Id, deserialized.Id);
        Assert.Equal(block.ParentId, deserialized.ParentId);
        Assert.Equal(block.ReferenceId, deserialized.ReferenceId);
        Assert.Equal(block.ParentHash, deserialized.ParentHash);
        Assert.Equal(block.ReferenceHash, deserialized.ReferenceHash);
        Assert.Equal(block.LedgerId, deserialized.LedgerId);
        Assert.Equal(block.TimeStamp, deserialized.TimeStamp);
        Assert.Equal(block.Status, deserialized.Status);
        Assert.Equal(block.Nonce.Value, deserialized.Nonce.Value);
        Assert.Equal(block.Hash, deserialized.Hash);
    }
}
