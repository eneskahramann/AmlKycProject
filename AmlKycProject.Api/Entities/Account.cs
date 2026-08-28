namespace AmlKycProject.Api.Entities;

public class Account
{
    public int Id {get;set;}
    public int CustomerId{get;set;}
    public Customer Customer {get;set;}
    public decimal Balance {get;set;}
    public string Currency {get;set;}
    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
}
