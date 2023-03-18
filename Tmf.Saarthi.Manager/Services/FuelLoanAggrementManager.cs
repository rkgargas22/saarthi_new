using Microsoft.Extensions.Options;
using Tmf.Saarthi.Core.Enums;
using Tmf.Saarthi.Core.Options;
using Tmf.Saarthi.Core.RequestModels.Document;
using Tmf.Saarthi.Core.ResponseModels.Fleet;
using Tmf.Saarthi.Core.ResponseModels.FuelLoanAgreement;
using Tmf.Saarthi.Core.ResponseModels.SanctionLetter;
using Tmf.Saarthi.Infrastructure.Interfaces;
using Tmf.Saarthi.Infrastructure.Models.Request.FuelLoanAggrement;
using Tmf.Saarthi.Infrastructure.Models.Response.DocumentTypeMstr;
using Tmf.Saarthi.Infrastructure.Models.Response.FuelLoanAggrement;
using Tmf.Saarthi.Infrastructure.Models.Response.StageMaster;
using Tmf.Saarthi.Manager.Interfaces;

namespace Tmf.Saarthi.Manager.Services;

public class FuelLoanAggrementManager : IFuelLoanAggrementManager
{
    private readonly IFuelLoanAggrementRepository _fuelLoanAggrementRepository;
    private readonly IFleetManager _fleetManager;
    private readonly IUploadManager _uploadManager;
    private readonly LetterOptions _letterOptions;
    private readonly IStageMasterManager _stageMasterManager;
    private readonly IDocumentTypeMstrManager _documentTypeMstrManager;

    public FuelLoanAggrementManager(IFuelLoanAggrementRepository fuelLoanAggrementRepository,
                                    IFleetManager fleetManager,
                                    IUploadManager uploadManager,
                                    IOptions<LetterOptions> letterOptions,
                                    IStageMasterManager stageMasterManager,
                                    IDocumentTypeMstrManager documentTypeMstrManager)
    {
        _fuelLoanAggrementRepository = fuelLoanAggrementRepository;
        _fleetManager = fleetManager;
        _uploadManager = uploadManager;
        _letterOptions = letterOptions.Value;
        _stageMasterManager = stageMasterManager;
        _documentTypeMstrManager = documentTypeMstrManager;
    }
    public async Task<FuelLoanAgreementResponse> GenerateFuelLoanAgreement(long FleetId, long CreatedBy)
    {
        LetterMasterDataResponse letterMasterDataResponse = await _fleetManager.LetterMasterData(FleetId);

        FuelLoanAgreementResponse fuelLoanAgreementResponse = new FuelLoanAgreementResponse();

        if (letterMasterDataResponse != null && !string.IsNullOrEmpty(letterMasterDataResponse.BorrowerName))
        {
            FuelLoanAggrementRequestModel fuelLoanAggrementRequestModel = new FuelLoanAggrementRequestModel
            {
                BorrowerAuthorisedPersonName = letterMasterDataResponse.BorrowerAuthorisedPersonName,
                CoBorrowerAuthorisedPersonName = letterMasterDataResponse.CoBorrowerAuthorisedPersonName,
                AgreementDate = letterMasterDataResponse.AgreementDate,
                AgreementPlace = letterMasterDataResponse.AgreementPlace,
                FileAccountNumber = letterMasterDataResponse.FileAccountNumber,
                OfficeOrbranchAddress = letterMasterDataResponse.OfficeOrbranchAddress,
                BorrowerName = letterMasterDataResponse.BorrowerName,
                BorrowerConstitution = letterMasterDataResponse.BorrowerConstitution,
                BorrowerAddress = string.Concat(letterMasterDataResponse.BorrowerAddressLine1, " ", letterMasterDataResponse.BorrowerAddressLine2 + " " + letterMasterDataResponse.BorrowerAddressLine3),
                BorrowerMobileNumber = letterMasterDataResponse.BorrowerMobileNumber,
                BorrowerEmailID = letterMasterDataResponse.BorrowerEmailID,
                CoBorrowerName = letterMasterDataResponse.CoBorrowerName,
                CoBorrowerConstitution = letterMasterDataResponse.CoBorrowerConstitution,
                CoBorrowerAddress = string.Concat(letterMasterDataResponse.CoBorrowerAddressLine1 + " " + letterMasterDataResponse.CoBorrowerAddressLine2 + " " + letterMasterDataResponse.CoBorrowerAddressLine3),
                CoBorrowerMobileNumber = letterMasterDataResponse.CoBorrowerMobileNumber,
                CoBorrowerEmailID = letterMasterDataResponse.CoBorrowerEmailID,
                TotalAmountofLoan = letterMasterDataResponse.TotalAmountofLoan,
                Limit = letterMasterDataResponse.Limit,
                CutOffLimit = letterMasterDataResponse.CutOffLimit,
                InterestRate = letterMasterDataResponse.InterestRate,
                TypeofInterest = letterMasterDataResponse.TypeofInterest,
                AcceleratedInterest = letterMasterDataResponse.AcceleratedInterest,
                PurposeoftheLoan = letterMasterDataResponse.PurposeoftheLoan,
                AvailabilityPeriod = letterMasterDataResponse.AvailabilityPeriod,
                OilCompanyName = letterMasterDataResponse.OilCompanyName,
                FuelProgrammeName = letterMasterDataResponse.FuelProgrammeName,
                OilCompanyDesignatedAccount = letterMasterDataResponse.OilCompanyDesignatedAccount,
                LegalExpenses = letterMasterDataResponse.LegalExpenses,
                ServiceCharges = letterMasterDataResponse.ServiceCharges,
                ProcessingFees = letterMasterDataResponse.ProcessingFees,
                StampDuty = letterMasterDataResponse.StampDuty,
                CLI = letterMasterDataResponse.Cli,
                AETNA = letterMasterDataResponse.Aetna,
                OtherCharges = letterMasterDataResponse.OtherCharges
            };

            FuelLoanAggrementResponseModel fuelLoanAggrementResponseModel = await _fuelLoanAggrementRepository.GenerateFuelLoanAgreement(fuelLoanAggrementRequestModel);

            fuelLoanAgreementResponse.Letter = fuelLoanAggrementResponseModel.Letter;

            if (!string.IsNullOrWhiteSpace(fuelLoanAgreementResponse.Letter))
            {
                StageMasterResponseModel stageMasterResponseModel = await _stageMasterManager.GetStageMasterByStageCode(StageCodeFlag.AGLGEN.ToString());
                DocumentTypeMstrResponseModel documentTypeMstrResponseModel = await _documentTypeMstrManager.GetDocumentTypeMstrByDocumentCode(DocumentCodeFlag.AGRL.ToString());

                int StageId = stageMasterResponseModel.StageId;
                int DocTypeId = documentTypeMstrResponseModel.DocTypeId;

                DocumentRequest documentRequest = new DocumentRequest
                {
                    FleetId = FleetId,
                    DocTypeId = DocTypeId,
                    StageId = StageId,
                    Extension = ".pdf",
                    IsActive = true,
                    CreatedBy = CreatedBy,
                    DocumentName = "FuelLoanAgreement"
                };

                await _uploadManager.AddDocument(documentRequest);

                if (!string.IsNullOrWhiteSpace(_letterOptions.DocumentFolderPath))
                {
                    string sharedFolderPath = Path.Combine(_letterOptions.DocumentFolderPath, FleetId.ToString());
                    if (!Directory.Exists(sharedFolderPath))
                    {
                        Directory.CreateDirectory(sharedFolderPath);
                    }

                    string pdfPath = Path.Combine(sharedFolderPath, "FuelLoanAgreementLetter.pdf");

                    byte[] bytes = Convert.FromBase64String(fuelLoanAgreementResponse.Letter);

                    using FileStream stream = new FileStream(pdfPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    using BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(bytes, 0, bytes.Length);
                    writer.Close();

                }
                else
                {
                    throw new ArgumentNullException("Document path is not configured");
                }
            }
            else
            {
                throw new ArgumentNullException("FuelLoanAgreement letter is not generated.");
            }
        }

        return fuelLoanAgreementResponse;
    }
}
