using System;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<LedgerManager> _logger;

        public LedgerManager(ILogger<LedgerManager> logger)
        {
            _logger = logger;
            _logger.LogInformation("LedgerManager is allocated");
        }

        public void Start()
        {
            _logger.LogInformation("LedgerManager is started");
        }

        public void Stop()
        {
            _logger.LogInformation("LedgerManager is stopped");
        }
    }
}
