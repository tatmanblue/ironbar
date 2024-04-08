using core.Ledger;

namespace Node.Interfaces;

/// <summary>
/// At some point, there might be several ledger types and each
/// manager is responsible for one type.
/// </summary>
public interface ILedgerManager
{
    void Start();
    void Stop();

    void Create(string blockData);
    ILedgerPhysicalBlock GetBlock(int id);
    List<ILedgerIndex> ListAllBlocks();
}