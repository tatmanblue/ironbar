﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using core;
using core.Ledger;
using core.Security;
using Node.Interfaces;
using IConfiguration = core.IConfiguration;

namespace Node.Ledger;

/// <summary>
/// Controller/orchestrator for interacting with ledgers
/// TODO could this be a IHostedService
/// </summary>
public class LedgerManager : ILedgerManager
{
    protected const int MASTER_LEDGER_ID = 1;

    // Dependency Injected
    private readonly ILogger<LedgerManager> logger;
    protected readonly ILedgerIndexFactory ledgerIndexFactory;

    // Instance allocated
    protected List<Ledger> ledgers = new List<Ledger>();
    protected bool isOperational = false;
    protected IServicesEventPub eventPub;

    protected LedgerManager(IServiceProvider serviceProvider)
    {
        this.logger = serviceProvider.GetRequiredService<ILogger<LedgerManager>>();
        this.ledgerIndexFactory = serviceProvider.GetRequiredService<ILedgerIndexFactory>();
        this.eventPub = serviceProvider.GetRequiredService<IServicesEventPub>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="eventPub"></param>
    public LedgerManager(ILogger<LedgerManager> logger, ILedgerIndexFactory ledgerIndexFactory, IServicesEventPub eventPub)
    {
        this.logger = logger;
        this.eventPub = eventPub;
        this.ledgerIndexFactory = ledgerIndexFactory;
        logger.LogInformation("Ledger Manager is now available");
    }

    public virtual void Start(IServiceProvider serviceProvider)
    {
        IConfiguration options = serviceProvider.GetRequiredService<IConfiguration>();
        
        logger.LogInformation($"LedgerManager is started, using '{System.IO.Path.GetFullPath(options.DataPath)}' for data path");

        ILogger<Ledger> ledgerLogger = serviceProvider.GetRequiredService<ILogger<Ledger>>();

        // 2 - open the master ledger index file and initialize the master ledger
        Ledger masterLedger = new Ledger(ledgerLogger, MASTER_LEDGER_ID, options.FriendlyName, options.DataPath);
        ledgers.Add(masterLedger);

        try
        {
            try
            {
                masterLedger.Validate();
            }
            catch (LedgerNotFoundException)
            {
                if (true == options.IsBootNode)
                {
                    // TODO: what would happen if the ledger missing is an error
                    // instead of new case of the ledger running?
                    logger.LogCritical($"ledger {masterLedger.Name} not found....initializing");
                    masterLedger.Initialize();
                    masterLedger.Validate();
                }
                else
                {
                    logger.LogCritical($"ledger {masterLedger.Name} not found....syncing with boot node");
                    masterLedger.InitializeStorage();
                }
            }

            isOperational = true;
            // 7 - inform bootnode, if appropriate, that this nodes master ledger is functional


            // TODO: if bootnode, inform any other nodes that came online while
            // initializing that bootnode is good to go
            logger.LogInformation($"Ledger {masterLedger.Name} is up and running");

            // 8 -- initialize ledger plugins
        }
        catch(LedgerException lex)
        {
            logger.LogError(lex, string.Empty, null);

            // TODO:  this node isn't good, no matter what.  in the case
            //        when this is the bootnode...guess theres more to do 
        }
    }

    public virtual void Stop()
    {
        isOperational = false;
        logger.LogInformation("LedgerManager is stopped");
    }

    public ILedgerPhysicalBlock Create(string blockData)
    {
        if (false == isOperational)
            throw new LedgerException("LedgerManager", $"LedgerManager is not operational");
        
        // couple of problems here:
        // 1 we started designing a system to have several ledgers 
        // 2 did not complete building out the interfaces for it
        
        ILedgerPhysicalBlock pb = ledgers[0].AddBlock(System.Text.Encoding.ASCII.GetBytes(blockData));
        
        eventPub?.FireBlockCreated(pb);

        return pb;
    }

    public ILedgerPhysicalBlock GetBlock(int id)
    {
        // couple of problems here:
        // 1 we started designing a system to have several ledgers 
        // 2 did not complete building out the interfaces for it
        return ledgers[0].ReadBlock(id);
    }

    public List<ILedgerIndex> ListAllBlocks()
    {
        // couple of problems here:
        // 1 we started designing a system to have several ledgers 
        // 2 did not complete building out the interfaces for it

        List<ILedgerIndex> indexes = ledgers[0].Indexes.ListAllIndexes();
        
        return indexes;
    }

    /// <summary>
    /// this is a child node only function
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="verification"></param>
    /// <exception cref="BlockChainException"></exception>
    public void SyncIndex(IList<string> rows, string verification)
    {
        string runningProof = string.Empty;
        int count = 1;
        List<ILedgerIndex> indexes = new List<ILedgerIndex>();
        foreach (string idx in rows)
        {
            logger.LogInformation($"{count}: {idx}");
            ILedgerIndex ledgerIndex = ledgerIndexFactory.CreateLedgerIndex(idx);
            indexes.Add(ledgerIndex);
            runningProof = HashUtility.ComputeHash($"{runningProof}{ledgerIndex.Hash}");
            
            count++;
        }

        logger.LogDebug($"Proof hash: {runningProof}");
        if (0 != runningProof.CompareTo(verification))
            throw new BlockChainException("Index sync failed");

        ledgers[0].Indexes.InitializeFromSync(indexes);
        isOperational = true;
        
        eventPub.FireIndexInitialized();
    }

    /// <summary>
    /// This is a child node only function
    /// </summary>
    /// <param name="block"></param>
    /// <param name="verification"></param>
    public void SyncBlock(string block, string verification)
    {
        ILedgerPhysicalBlock pb = PhysicalBlock.FromString(block);
        if (pb.Hash != verification)
            throw new BlockChainException($"Hash mismatch {pb.Id}");
        
        ledgers[0].SyncBlock(pb);
        logger.LogInformation($"Block {pb.Id} received");
    }
}
