using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using core;
using core.Ledger;
using Node.Interfaces;
using IConfiguration = core.IConfiguration;

namespace Node.Ledger;

/// <summary>
/// Controller/orchestrator for interacting with ledgers
/// </summary>
public class LedgerManager : ILedgerManager
{
    private const int MASTER_LEDGER_ID = 1;

    // Dependency Injected
    private readonly ILogger<LedgerManager> logger;
    private readonly ILogger<Ledger> ledgerLogger;
    private IConfiguration options;

    // Instance allocated
    private List<Ledger> ledgers = new List<Ledger>();
    private bool _isOperational = false;


    public LedgerManager(ILogger<LedgerManager> logger, ILogger<Ledger> ledgerLogger, IConfiguration options)
    {
        this.logger = logger;
        this.ledgerLogger = ledgerLogger;
        this.options = options;
    }

    public void Start()
    {
        logger.LogInformation($"LedgerManager is started, using '{System.IO.Path.GetFullPath(options.DataPath)}' for data path");

        // 1 - inform bootnode, if this instance is not the bootnode,
        // that this node is starting up the ledger logic

        // 2 - open the master ledger secrets file and initalize the master ledger
        // TODO: make 'master' path name something else
        Ledger masterLedger = new Ledger(ledgerLogger, MASTER_LEDGER_ID, "master", options.DataPath);
        ledgers.Add(masterLedger);

        try
        {
            try
            {
                // 3 - validate it
                // 4 - add new checkpoint record
                // 5 - save it
                // 6 - set status to OK
                masterLedger.Validate();
            }
            catch (LedgerNotFoundException)
            {
                // TODO: what would happen if the ledger missing is an error
                // instead of new case of the ledger running?
                logger.LogCritical($"ledger {masterLedger.Name} not found....initializing");
                masterLedger.Initialize();
                masterLedger.Validate();
            }

            _isOperational = true;
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

    public void Stop()
    {
        logger.LogInformation("LedgerManager is stopped");
    }

    public void Create(string blockData)
    {
        throw new NotImplementedException();
    }

    public ILedgerPhysicalBlock GetBlock(int id)
    {
        throw new NotImplementedException();
    }

    public List<ILedgerIndex> ListAllBlocks()
    {
        // couple of problems here:
        // 1 we started designing a system to have several ledgers 
        // 2 did not complete building out the interfaces for it

        List<LedgerIndex> indexes = ledgers[0].Indexes.ListAllIndexes();
        
        return indexes.Cast<ILedgerIndex>().ToList();
    }
}
