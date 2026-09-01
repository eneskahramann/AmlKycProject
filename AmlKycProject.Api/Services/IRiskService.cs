using AmlKycProject.Api.Entities;

namespace AmlKycProject.Api.Services;

public interface IRiskService
{
    // Transfer nesnesini alıp risk analizi yapacak metodumuz
    Task EvaluateTransferRiskAsync(Transfer transfer);
}