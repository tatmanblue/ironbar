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
            _logger.LogInformation($"LedgerManager is started, using '{System.IO.Path.GetFullPath(_options.DataPath)}' for data path");

            // 1 - inform bootnode, if this instance is not the bootnode,
            // that this node is starting up the ledger logic

            // 2 - open the master ledger secrets file and initalize the master ledger
            // TODO: get path from cmd + config and post fix master path
            // TODO: make master path name something else
            Ledger masterLedger = new Ledger(MASTER_LEDGER_ID, "master");
            _ledger.Add(masterLedger);

            try
            {
                // 3 - validate it
                // 4 - add new checkpoint record
                // 5 - save it
                // 6 - set status to OK
                masterLedger.Validate();
                _isOperational = true;
                // 7 - inform bootnode, if appropriate, that this nodes master ledger is functional


                // TODO: if bootnode, inform any other nodes that came online while
                // initializing that bootnode is good to go
                _logger.LogInformation($"Ledger {masterLedger.Id} is up and running");
            }
            catch(LedgerException lex)
            {
                _logger.LogError(lex, string.Empty, null);

                // TODO:  this node isn't good, no matter what.  in the case
                // when this is the bootnode...guess theres more to do figuring
                // that out
            }
        }

        public void Stop()
        {
            _logger.LogInformation("LedgerManager is stopped");
            // inform bootnode if not bootnode
            // if bootnode, gotta let the others know
            
        }
    }
}
