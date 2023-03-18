using Microsoft.Extensions.Options;
using Tmf.Saarthi.Core.Enums;
using Tmf.Saarthi.Core.Options;
using Tmf.Saarthi.Core.RequestModels.Document;
using Tmf.Saarthi.Core.ResponseModels.Fleet;
using Tmf.Saarthi.Core.ResponseModels.SanctionLetter;
using Tmf.Saarthi.Infrastructure.Interfaces;
using Tmf.Saarthi.Infrastructure.Models.Request.SanctionLetter;
using Tmf.Saarthi.Infrastructure.Models.Response.DocumentTypeMstr;
using Tmf.Saarthi.Infrastructure.Models.Response.SanctionLetter;
using Tmf.Saarthi.Infrastructure.Models.Response.StageMaster;
using Tmf.Saarthi.Manager.Interfaces;

namespace Tmf.Saarthi.Manager.Services;

public class SantionLetterManager : ISanctionLetterManager
{
    private readonly ISantionLetterRepository _santionLetterRepository;
    private readonly IFleetManager _fleetManager;
    private readonly IUploadManager _uploadManager;
    private readonly LetterOptions _letterOptions;
    private readonly IStageMasterManager _stageMasterManager;
    private readonly IDocumentTypeMstrManager _documentTypeMstrManager;

    public SantionLetterManager(ISantionLetterRepository santionLetterRepository,
                                IFleetManager fleetManager,
                                IUploadManager uploadManager,
                                IOptions<LetterOptions> letterOptions,
                                IStageMasterManager stageMasterManager,
                                IDocumentTypeMstrManager documentTypeMstrManager)
    {
        _santionLetterRepository = santionLetterRepository;
        _fleetManager = fleetManager;
        _uploadManager = uploadManager;
        _letterOptions = letterOptions.Value;
        _stageMasterManager = stageMasterManager;
        _documentTypeMstrManager = documentTypeMstrManager;
    }

    public async Task<SanctionLetterResponse> GenerateSanctionLetter(long FleetId, long CreatedBy)
    {
        LetterMasterDataResponse letterMasterDataResponse = await _fleetManager.LetterMasterData(FleetId);

        SanctionLetterResponse sanctionLetterResponse = new SanctionLetterResponse();

        if (letterMasterDataResponse != null && !string.IsNullOrEmpty(letterMasterDataResponse.BorrowerName))
        {
            SanctionLetterRequestModel sanctionLetterRequestModel = new SanctionLetterRequestModel
            {
                FanNo = letterMasterDataResponse.FanNo,
                BorrowerName = letterMasterDataResponse.BorrowerName,
                BorrowerAddressLine1 = letterMasterDataResponse.BorrowerAddressLine1,
                BorrowerAddressLine2 = letterMasterDataResponse.BorrowerAddressLine2,
                BorrowerAddressLine3 = letterMasterDataResponse.BorrowerAddressLine3,
                CoBorrowerName = letterMasterDataResponse.CoBorrowerName,
                CoBorrowerAddressLine1 = letterMasterDataResponse.CoBorrowerAddressLine1,
                CoBorrowerAddressLine2 = letterMasterDataResponse.CoBorrowerAddressLine2,
                CoBorrowerAddressLine3 = letterMasterDataResponse.CoBorrowerAddressLine3,
                SanctionLimit = letterMasterDataResponse.Limit,
                CutOffLimit = letterMasterDataResponse.CutOffLimit,
                ProcessingFee = letterMasterDataResponse.ProcessingFees,
                StampDuty = letterMasterDataResponse.StampDuty,
                CLI = letterMasterDataResponse.Cli,
                Aetna = letterMasterDataResponse.Aetna,
                LegalExpenses = letterMasterDataResponse.LegalExpenses,
                ChequeBouncingCharges = letterMasterDataResponse.ChequeBouncingCharges,
                RetainerCharges = letterMasterDataResponse.RetainerCharges,
                InterestRate = letterMasterDataResponse.InterestRate,
                AcceleratedInterestrate = letterMasterDataResponse.AcceleratedInterest,
                BorrowerAuthorisedPersonName = letterMasterDataResponse.BorrowerAuthorisedPersonName,
                CoBorrowerAuthorisedPersonName = letterMasterDataResponse.CoBorrowerAuthorisedPersonName
            };

            SanctionLetterResponseModel sanctionLetterResponseModel = await _santionLetterRepository.GenerateSanctionLetter(sanctionLetterRequestModel);
            sanctionLetterResponse.Letter = sanctionLetterResponseModel.Letter;

            if (!string.IsNullOrWhiteSpace(sanctionLetterResponse.Letter))
            {
                StageMasterResponseModel stageMasterResponseModel = await _stageMasterManager.GetStageMasterByStageCode(StageCodeFlag.SANLGE.ToString());
                DocumentTypeMstrResponseModel documentTypeMstrResponseModel = await _documentTypeMstrManager.GetDocumentTypeMstrByDocumentCode(DocumentCodeFlag.SANL.ToString());

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
                    DocumentName = "SanctionLetter"
                };

                await _uploadManager.AddDocument(documentRequest);

                if (!string.IsNullOrWhiteSpace(_letterOptions.DocumentFolderPath))
                {
                    string sharedFolderPath = Path.Combine(_letterOptions.DocumentFolderPath, FleetId.ToString());
                    if (!Directory.Exists(sharedFolderPath))
                    {
                        Directory.CreateDirectory(sharedFolderPath);
                    }

                    string pdfPath = Path.Combine(sharedFolderPath, "SanctionLetter.pdf");

                    byte[] bytes = Convert.FromBase64String(sanctionLetterResponse.Letter);

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
                throw new ArgumentNullException("Sanction letter is not generated.");
            }
        }

        return sanctionLetterResponse;
    }
}
