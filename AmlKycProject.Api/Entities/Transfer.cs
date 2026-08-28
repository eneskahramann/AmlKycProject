namespace AmlKycProject.Api.Entities;

public class Transfer 
{
    public int Id{get;set;}
    public int SenderAccountId{get;set;}
    public int ReceiverAccountId {get;set;}
    public decimal Amount {get;set;}
    public DateTime TransferDate {get;set;}= DateTime.UtcNow;
    public bool IsSuccessful {get;set;}
}



