using core.Ledger;

namespace Node.Interfaces;

/// <summary>
/// At some point, there might be several ledger types and each
/// manager is responsible for one type.
/// </summary>
public interface ILedgerManager
{
    void Start(IServiceProvider serviceProvider);
    void Stop();

    ILedgerPhysicalBlock Create(string blockData);
    ILedgerPhysicalBlock GetBlock(int id);
    List<ILedgerIndex> ListAllBlocks();

    /// <summary>
    /// used by client nodes to get in sync with bootnoode
    /// </summary>
    /// <param name="rows"></param>
    void SyncIndex(IList<string> rows, string verification);
}