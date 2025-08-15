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
        logger.LogInformation("Client Ledger Manager is now created");
    }

    /// <summary>
    /// Start is different for Client node ledgers
    ///
    /// TODO there is a better way to implement these differences than inheritance
    /// </summary>
    /// <param name="serviceProvider"></param>
    public override void Start(IServiceProvider serviceProvider)
    {
        ILedgerWriter writer = serviceProvider.GetRequiredService<ILedgerWriter>();
        
        logger.LogInformation($"ChildLedgerManager is started");
        CreateMasterLedger(serviceProvider);
        writer.InitializeStorage();
    }
}