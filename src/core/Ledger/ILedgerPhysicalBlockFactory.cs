using System;

namespace core.Ledger;

/// <summary>
/// To abstract the creation of PhysicalBlocks instances with idea that different implementations can be provided.
/// </summary>
public interface ILedgerPhysicalBlockFactory
{
    /// <summary>
    /// Creates a block that has no reference to another block (other than its parent)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="parentHash"></param>
    /// <param name="parentId"></param>
    /// <param name="ledgerId"></param>
    /// <param name="transactionData"></param>
    /// <param name="signBlock"></param>
    /// <returns></returns>
    ILedgerPhysicalBlock Create(int id, BlockStatus status, string parentHash, int parentId, int ledgerId, 
        byte[] transactionData, ILedgerSignBlock signBlock);

    /// <summary>
    /// Creates a block reference to another block (other than its parent) earlier in the chain.
    /// When a reference it exists, it implies the block is advancing the data in the referenced block 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="parentHash"></param>
    /// <param name="parentId"></param>
    /// <param name="referenceId"></param>
    /// <param name="referenceHash"></param>
    /// <param name="ledgerId"></param>
    /// <param name="transactionData"></param>
    /// <param name="signBlock"></param>
    /// <returns></returns>
    ILedgerPhysicalBlock Create(int id, BlockStatus status, string parentHash, int parentId, int referenceId, 
        string referenceHash, int ledgerId, byte[] transactionData, ILedgerSignBlock signBlock);
    
    /// <summary>
    /// Primarily for creating new blocks via "serialized" data and isolating the implementation details
    /// </summary>
    /// <param name="block"></param>
    /// <returns>ILedgerPhysicalBlock</returns>
    ILedgerPhysicalBlock Create(string block, ILedgerSignBlockFactory signBlockFactory);

}