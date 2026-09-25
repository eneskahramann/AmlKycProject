namespace AmlKycProject.Api.DTOs;

public class CreateCustomerRequestDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdentityNumber { get; set; }
    public decimal InitialBalance { get; set; }
}