using core.Ledger;
using IConfiguration = core.IConfiguration;
namespace Node.Ledger;

/// <summary>
/// 
/// </summary>
public class ChildLedgerManager : LedgerManager
{
    private readonly ILogger<ChildLedgerManager> logger;
    
    public ChildLedgerManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.logger = serviceProvider.GetRequiredService<ILogger<ChildLedgerManager>>();
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
        ILogger<Ledger> ledgerLogger = serviceProvider.GetRequiredService<ILogger<Ledger>>();
        IPhysicalBlockValidator blockValidator = serviceProvider.GetRequiredService<IPhysicalBlockValidator>();
        
        logger.LogInformation($"LedgerManager is started, using '{System.IO.Path.GetFullPath(options.DataPath)}' for data path");

        
        Ledger masterLedger = new Ledger(ledgerLogger, blockValidator, MASTER_LEDGER_ID, options.FriendlyName, options.DataPath);
        ledgers.Add(masterLedger);
        masterLedger.InitializeStorage();
    }
}