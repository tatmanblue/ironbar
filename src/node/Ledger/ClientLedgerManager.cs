using IConfiguration = core.IConfiguration;
namespace Node.Ledger;

/// <summary>
/// 
/// </summary>
public class ClientLedgerManager : LedgerManager
{
    private readonly ILogger<ClientLedgerManager> logger;
    
    public ClientLedgerManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.logger = serviceProvider.GetRequiredService<ILogger<ClientLedgerManager>>();
        logger.LogInformation("Client Ledger Manager is now available");
    }

    /// <summary>
    /// Start is different for Client node ledgers
    ///
    /// TODO there is a better way to implement these differences than inheritance
    /// </summary>
    /// <param name="serviceProvider"></param>
    public override void Start(IServiceProvider serviceProvider)
    {
        IConfiguration options = serviceProvider.GetRequiredService<IConfiguration>();
        
        logger.LogInformation($"LedgerManager is started, using '{System.IO.Path.GetFullPath(options.DataPath)}' for data path");

        ILogger<Ledger> ledgerLogger = serviceProvider.GetRequiredService<ILogger<Ledger>>();
        
        Ledger masterLedger = new Ledger(ledgerLogger, MASTER_LEDGER_ID, options.FriendlyName, options.DataPath);
        ledgers.Add(masterLedger);
        masterLedger.InitializeStorage();
    }
}