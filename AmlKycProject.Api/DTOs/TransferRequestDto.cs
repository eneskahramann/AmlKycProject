namespace AmlKycProject.Api.DTOs;

public class TransferRequestDto
{
    public int SenderAccountId { get; set; }
    public int ReceiverAccountId { get; set; }
    public decimal Amount { get; set; }
}

