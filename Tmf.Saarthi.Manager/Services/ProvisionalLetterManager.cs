using Microsoft.Extensions.Options;
using Tmf.Saarthi.Core.Enums;
using Tmf.Saarthi.Core.Options;
using Tmf.Saarthi.Core.RequestModels.Document;
using Tmf.Saarthi.Core.RequestModels.Letter;
using Tmf.Saarthi.Core.ResponseModels.Fleet;
using Tmf.Saarthi.Core.ResponseModels.ProvisionalLette;
using Tmf.Saarthi.Infrastructure.Interfaces;
using Tmf.Saarthi.Infrastructure.Models.Request.ProvisionalLetter;
using Tmf.Saarthi.Infrastructure.Models.Response.DocumentTypeMstr;
using Tmf.Saarthi.Infrastructure.Models.Response.ProvisionalLetter;
using Tmf.Saarthi.Infrastructure.Models.Response.StageMaster;
using Tmf.Saarthi.Manager.Interfaces;

namespace Tmf.Saarthi.Manager.Services;

public class ProvisionalLetterManager : IProvisionalLetterManager
{
    private readonly IProvisionalLetterRepository _provisionalLetterRepository;
    private readonly IFleetManager _fleetManager;
    private readonly IUploadManager _uploadManager;
    private readonly LetterOptions _letterOptions;
    private readonly IStageMasterManager _stageMasterManager;
    private readonly IDocumentTypeMstrManager _documentTypeMstrManager;

    public ProvisionalLetterManager(IProvisionalLetterRepository provisionalLetterRepository,
                                    IFleetManager fleetManager,
                                    IUploadManager uploadManager,
                                    IOptions<LetterOptions> letterOptions,
                                    IStageMasterManager stageMasterManager,
                                    IDocumentTypeMstrManager documentTypeMstrManager)
    {
        _provisionalLetterRepository = provisionalLetterRepository;
        _fleetManager = fleetManager;
        _uploadManager = uploadManager;
        _letterOptions = letterOptions.Value;
        _stageMasterManager = stageMasterManager;
        _documentTypeMstrManager = documentTypeMstrManager;
    }
    public async Task<ProvisionalLetterResponse> GenerateProvisionalLetter(long FleetId, long CreatedBy)
    {
        LetterMasterDataResponse letterMasterDataResponse = await _fleetManager.LetterMasterData(FleetId);

        ProvisionalLetterResponse provisionalLetterResponse = new ProvisionalLetterResponse();

        if (letterMasterDataResponse != null && !string.IsNullOrEmpty(letterMasterDataResponse.BorrowerName))
        {
            ProvisionalLetterRequestModel provisionalLetterRequestModel = new ProvisionalLetterRequestModel
            {
                Name = letterMasterDataResponse.BorrowerName ?? "",
                ProcessingFee = letterMasterDataResponse.ProcessingFees ?? 0,
                ApplicationNumber = FleetId,
                LoanAmount = letterMasterDataResponse.TotalAmountofLoan ?? 0,
                LoanTenure = 12,
                RateOfInterest = letterMasterDataResponse.InterestRate ?? 0
            };

            ProvisionalLetteResponseModel provisionalLetteResponseModel = await _provisionalLetterRepository.GenerateprovisionalLetter(provisionalLetterRequestModel);

            provisionalLetterResponse.Letter = provisionalLetteResponseModel.Letter;

            if (!string.IsNullOrWhiteSpace(provisionalLetterResponse.Letter))
            {
                StageMasterResponseModel stageMasterResponseModel = await _stageMasterManager.GetStageMasterByStageCode(StageCodeFlag.PROLGE.ToString());
                DocumentTypeMstrResponseModel documentTypeMstrResponseModel = await _documentTypeMstrManager.GetDocumentTypeMstrByDocumentCode(DocumentCodeFlag.PROL.ToString());

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
                    DocumentName = "ProvisionalLetter"
                };

                await _uploadManager.AddDocument(documentRequest);

                if (!string.IsNullOrWhiteSpace(_letterOptions.DocumentFolderPath))
                {
                    string sharedFolderPath = Path.Combine(_letterOptions.DocumentFolderPath, FleetId.ToString());
                    if (!Directory.Exists(sharedFolderPath))
                    {
                        Directory.CreateDirectory(sharedFolderPath);
                    }

                    string pdfPath = Path.Combine(sharedFolderPath, "ProvisionalLetter.pdf");

                    byte[] bytes = Convert.FromBase64String(provisionalLetterResponse.Letter);

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
                throw new ArgumentNullException("Provisional letter is not generated.");
            }
        }
        return provisionalLetterResponse;
    }

    public async Task<DisagreeProvisionalResponse> DisagreeProvisionalLetter(DisagreeProvisionalResquest disagreeProvisionalResquest)
    {
        DisagreeProvisionalResquestModel disagreeProvisionalResquestModel = new DisagreeProvisionalResquestModel();
        disagreeProvisionalResquestModel.FleetId = disagreeProvisionalResquest.FleetId;

        DisagreeProvisionalResponseModel disagreeProvisionalResponseModel = await _provisionalLetterRepository.DisagreeProvisionalLetter(disagreeProvisionalResquestModel);

        DisagreeProvisionalResponse disagreeProvisionalResponse = new DisagreeProvisionalResponse();
        if (disagreeProvisionalResponseModel.FleetId == 0)
        {
            disagreeProvisionalResponse.message = "Update Failed";
        }
        else
        {
            disagreeProvisionalResponse.message = "Updated Successfully";
        }
        return disagreeProvisionalResponse;
    }
}
