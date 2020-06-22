using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using core;
using core.Ledger;

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
        private IConfiguration _options;

        // Instance allocated
        private List<ILedger> _ledger = new List<ILedger>();
        private bool _isOperational = false;


        public LedgerManager(ILogger<LedgerManager> logger, IConfiguration options)
        {
            _logger = logger;
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
            Ledger masterLedger = new Ledger(MASTER_LEDGER_ID, "master", _options.DataPath);
            _ledger.Add(masterLedger);

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
                    _logger.LogInformation($"ledger {masterLedger.Name} not found....initializing");
                    masterLedger.Initialize();
                    masterLedger.Validate();
                }

                _isOperational = true;
                // 7 - inform bootnode, if appropriate, that this nodes master ledger is functional


                // TODO: if bootnode, inform any other nodes that came online while
                // initializing that bootnode is good to go
                _logger.LogInformation($"Ledger {masterLedger.Name} is up and running");

                // 8 -- initialize ledger plugins
            }
            catch(LedgerException lex)
            {
                _logger.LogError(lex, string.Empty, null);

                // TODO:  this node isn't good, no matter what.  in the case
                // when this is the bootnode...guess theres more to do 
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
