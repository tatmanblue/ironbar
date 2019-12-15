using System;
using System.Collections.Generic;
using core.Ledger;
using Microsoft.Extensions.Logging;
using node.General;

namespace node.Ledger
{
    /// <summary>
    /// So that we can use dotnet DI
    /// </summary>
    public interface ILedgerManager
    {
        void Start();
        void Stop();
    }

    /// <summary>
    /// Controller/orchestrator for interacting with ledgers
    /// </summary>
    public class LedgerManager : ILedgerManager
    {
        private const int MASTER_LEDGER_ID = 1;

        // Dependency Injected
        private readonly ILogger<LedgerManager> _logger;
        private IOptions _options;

        // Instance allocated
        private List<ILedger> _ledger = new List<ILedger>();
        private bool _isOperational = false;


        public LedgerManager(ILogger<LedgerManager> logger, IOptions options)
        {
            _logger = logger;
            _logger.LogInformation("LedgerManager is allocated");

            _options = options;
        }

        public void Start()
        {
            _logger.LogInformation($"LedgerManager is started, using '{_options.DataPath}' for data path");

            // inform bootnode, if not bootnode, this node is starting up the ledger logic
            // open the master ledger secrets file and initalize the master ledger
            // TODO: get path from config and post fix master path
            // TODO: make master path name something else
            Ledger masterLedger = new Ledger(MASTER_LEDGER_ID, "master");
            // validate it
            // add new checkpoint record
            // save it
            // set status to OK
            _isOperational = true;
            // inform bootnode if not bootnode


            // TODO: if bootnode, inform any other nodes that came online while
            // initializing that bootnode is good to go
        }

        public void Stop()
        {
            _logger.LogInformation("LedgerManager is stopped");
            // inform bootnode if not bootnode
            // if bootnode, gotta let the others know
            
        }
    }
}
