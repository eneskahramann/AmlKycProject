using System.ComponentModel.DataAnnotations.Schema;
namespace AmlKycProject.Api.Entities;

public class RiskLog 
{
    public int Id {get;set;}
    public int TransferId{get;set;}
    public Transfer Transfer {get;set;}
    public int RiskScore {get;set;}
    public RiskLevel RiskLevel{get;set;}

    [Column(TypeName = "jsonb")]
    public string TriggeredRules { get; set; }

    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
}