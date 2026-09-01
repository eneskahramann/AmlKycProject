using AmlKycProject.Api.Entities;

namespace AmlKycProject.Api.Services;

public interface ITransferService
{
    Task<(bool IsSuccess, string Message, Transfer? TransferRecord)> ExecuteTransferAsync(int senderAccountId, int receiverAccountId, decimal amount);
}