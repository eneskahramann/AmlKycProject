namespace AmlKycProject.Api.Entities;

public class Customer{
    public int Id {get;set;}
    public string FirstName {get;set;}
    public string LastName {get;set;}
    public string IdentityNumber{get;set;} // TC İdentity Number
    public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

    public ICollection<Account> Accounts {get;set;}
}