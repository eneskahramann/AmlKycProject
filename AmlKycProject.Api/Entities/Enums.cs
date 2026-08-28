namespace AmlKycProject.Api.Entities;

public enum RiskLevel
{
    Low = 1,      // between 0-39 
    Medium = 2,   // between 40-69 
    High = 3      // between 70-100  
}

public enum AnalystDecision
{
    Pending = 0,
    Approved = 1,   //normal event
    Suspicious = 2  //suspicious event
}