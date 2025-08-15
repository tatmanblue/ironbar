using core.Ledger;

namespace Node.Ledger;

public class JsonSignBlockTypeFactory : ILedgerSignBlockFactory
{
    public ILedgerSignBlock Create()
    {
        return new SignBlock();
    }
}