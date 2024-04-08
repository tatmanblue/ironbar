using Node.Ledger;

namespace Node.Interfaces;

public interface ILedgerIndexManager
{
    LedgerIndex GetIndex(int id);
    List<LedgerIndex> ListAllIndexes();

}