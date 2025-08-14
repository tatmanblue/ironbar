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
        ILedgerReader reader = serviceProvider.GetRequiredService<ILedgerReader>();
        ILedgerWriter writer = serviceProvider.GetRequiredService<ILedgerWriter>();
        ILedgerIndexFactory indexFactory = serviceProvider.GetRequiredService<ILedgerIndexFactory>();
        
        logger.LogInformation($"ChildLedgerManager is started");

        
        Ledger masterLedger = new Ledger(ledgerLogger, blockValidator, reader,writer, indexFactory,
            MASTER_LEDGER_ID, options.FriendlyName);
        ledgers.Add(masterLedger);
        writer.InitializeStorage();
    }
}