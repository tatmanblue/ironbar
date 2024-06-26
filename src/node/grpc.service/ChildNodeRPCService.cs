﻿using core.Ledger;
using core.Security;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Node.General;
using Node.Interfaces;
using Node.Ledger; // Ensure you have the correct namespace here

namespace Node.grpc.service;

/// <summary>
/// ChildNodeRPCService is the counter part to BootNodeRPCService and listens for
/// messages from the boot node.  This type only runs on child nodes
/// </summary>
public class ChildNodeRPCService : NodeToNodeConnection.NodeToNodeConnectionBase
{
    private readonly ILogger<BootNodeRPCService> logger;
    private ILedgerIndexFactory ledgerIndexFactory;
    private ILedgerManager ledgerManager;
    private readonly IServicesEventPub eventPub;
    
    public ChildNodeRPCService(ILogger<BootNodeRPCService> logger, ILedgerManager ledgerManager, ILedgerIndexFactory ledgerIndexFactory, IServicesEventPub eventPub)
    {
        this.logger = logger;
        this.eventPub = eventPub;
        this.ledgerIndexFactory = ledgerIndexFactory;
        this.ledgerManager = ledgerManager;
        logger.LogInformation($"ChildNodeRPCService initialized.");
    }

    public override Task<Empty> SyncIndex(IndexRequest request, ServerCallContext context)
    {
        try 
        {
            logger.LogInformation($"Received index from boot node.  Verification hash: {request.Verification}");
            ledgerManager.SyncIndex(request.Indexes, request.Verification);
            return Task.FromResult(new Empty());
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            throw new BlockChainException($"unable to process Index");
        }
    }

    public override Task<BlockCreatedReply> BlockCreated(BlockCreatedRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogTrace($"Blocked received {request.Block}");
            ILedgerPhysicalBlock pb = ledgerManager.SyncBlock(request.Block, request.Verification);

            BlockCreatedReply reply = new BlockCreatedReply()
            {
                Block = request.Block,
                Verification = request.Verification,
                Result = BlockStatus.Approved.ToString()
            };

            logger.LogDebug($"Blocked accepted {request.Block}");
            return Task.FromResult(reply);
        }
        catch (LedgerBlockException lbe)
        {
            logger.LogError($"LedgerBlockException: {lbe.Message}");
            BlockCreatedReply reply = new BlockCreatedReply()
            {
                Block = request.Block,
                Verification = request.Verification,
                Result = BlockStatus.Rejected.ToString()
            };

            return Task.FromResult(reply);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            throw new BlockChainException($"unable to process block");
        }
    }
}