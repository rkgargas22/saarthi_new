﻿using Tmf.Saarthi.Infrastructure.Models.Request.Fleet;
using Tmf.Saarthi.Infrastructure.Models.Response.Fleet;

namespace Tmf.Saarthi.Infrastructure.Interfaces;

public interface IFleetRepository
{
    Task<FleetResponseModel> GetFleet(GetFleetRequestModel getFleetRequest);

    Task<FleetResponseModel> AddFleet(AddFleetRequestModel addFleetRequest);

    Task<VerifyFleetResponseModel> GetFleetDetailByFleetId(long fleetId);

    Task<ProvisionApprovalResponseModel> ProvisionApproval(ProvisionApprovalRequestModel provisionApprovalRequest);

    Task<SanctionApprovalResponseModel> SanctionApproval(SanctionApprovalRequestModel sanctionApprovalRequestModel);

    Task<EAgreementApprovalResponseModel> EAgreementApproval(EAgreementApprovalRequestModel egreementApprovalRequest);

    Task<UpdateFleetAmountResponseModel> UpdateFleetAmount(UpdateFleetAmountRequestModel updateFleetAmountRequest);

    Task<UpdateFleetFanNoResponseModel> UpdateFleetFanNo(UpdateFleetFanNoRequestModel updateFleetFanNoRequestModel);

    Task<LetterMasterDataResponseModel> LetterMasterData(long FleetID);

    Task<CommentResponseModel> UpdateComment(CommentRequestModel commentRequestModel);

    Task<AdditionalInformationResponseModel> UpdateAdditionalInformation(AdditionalInformationRequestModel additionalInformationRequestModel);

    Task<AddressChangeResponseModel> AddressChange(AddressChangeRequestModel addressChangeRequest);

    Task<string> GetVehicleType(string VehicleModel);
}
