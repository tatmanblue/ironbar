using core.Ledger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Node.Ledger;

/// <summary>
/// Because the LedgerIndexManager(s) work on the interaction of concrete types we
/// need a custom converter to handle the interface during JSON (de)serialization
/// </summary>
public class LedgerIndexConverter : JsonConverter<ILedgerIndex>
{
    private readonly ILedgerIndexFactory factory;

    public LedgerIndexConverter(ILedgerIndexFactory factory)
    {
        this.factory = factory;
    }

    public override ILedgerIndex ReadJson(JsonReader reader, Type objectType, ILedgerIndex existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        ILedgerIndex instance = factory.Create();
        serializer.Populate(obj.CreateReader(), instance);
        return instance;
    }

    public override void WriteJson(JsonWriter writer, ILedgerIndex value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override bool CanWrite => true;
}