using core.Ledger;

namespace Node.Ledger;

public class TextFileSignBlockTypeFactory : ILedgerSignBlockFactory
{
    public ILedgerSignBlock Create()
    {
        return new SignBlock();
    }
}