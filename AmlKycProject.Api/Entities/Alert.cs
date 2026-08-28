public class Alert 
{
    public int Id {get;set;}
    public int TransferId{get;set;}
    public Transfer Transfer {get;set;}
    public int RiskLogId {get;set;}
    public RiskLog RiskLog {get;set;}
    public AnalystDecision Decision {get;set;} = AnalystDecision.Pending;
    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
    public DateTime? ResolvedAt{get;set;}
}