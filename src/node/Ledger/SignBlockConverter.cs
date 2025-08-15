using core.Ledger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Node.Ledger;

public class SignBlockConverter : JsonConverter<ILedgerSignBlock>
{
    private readonly ILedgerSignBlockFactory factory;

    public SignBlockConverter(ILedgerSignBlockFactory factory)
    {
        this.factory = factory;
    }

    public override ILedgerSignBlock ReadJson(JsonReader reader, Type objectType, ILedgerSignBlock existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        ILedgerSignBlock instance = factory.Create();
        serializer.Populate(obj.CreateReader(), instance);
        return instance;
    }

    public override void WriteJson(JsonWriter writer, ILedgerSignBlock value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override bool CanWrite => true;
}