using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Tmf.Saarthi.Core.Options;
using Tmf.Saarthi.Core.RequestModels.Document;
using Tmf.Saarthi.Core.ResponseModels.Document;
using Tmf.Saarthi.Infrastructure.Interfaces;
using Tmf.Saarthi.Infrastructure.Models.Request.Document;
using Tmf.Saarthi.Infrastructure.Models.Response.Customer;
using Tmf.Saarthi.Infrastructure.Models.Response.Document;
using Tmf.Saarthi.Manager.Interfaces;

namespace Tmf.Saarthi.Manager.Services;

public class UploadManager : IUploadManager
{
    private readonly IUploadDocumentRepository _uploadDocumentRepository;
    private readonly LetterOptions _letterOptions;
    public UploadManager(IUploadDocumentRepository uploadDocumentRepository,IOptions<LetterOptions> letterOptions)
    {
        _uploadDocumentRepository = uploadDocumentRepository;
        _letterOptions = letterOptions.Value;
    }

    public async Task<DocumentResponse> AddDocument(DocumentRequest documentRequest)
    {
        DocumentRequestModel documentRequestModel = new DocumentRequestModel
        {
            FleetId = documentRequest.FleetId,
            DocTypeId = documentRequest.DocTypeId,
            StageId = documentRequest.StageId,
            Extension = documentRequest.Extension,
            IsActive = documentRequest.IsActive,
            CreatedBy = documentRequest.CreatedBy,
            DocumentName = documentRequest.DocumentName
        };

        DocumentResponseModel addFleetVehicleResponseModel = await _uploadDocumentRepository.AddDocument(documentRequestModel);

        DocumentResponse documentResponse = new DocumentResponse
        {
            FleetId = addFleetVehicleResponseModel.FleetID
        };

        return documentResponse;
    }


    public async Task<UploadDocumentsResponse> UploadDocuments(UploadDocumentsRequest uploadDocumentsRequest, long createdBy)
    {       

        UploadDocumentsRequestModel uploadDocumentsRequestModel = new UploadDocumentsRequestModel();
        uploadDocumentsRequestModel.FleetId = uploadDocumentsRequest.FleetId;
        uploadDocumentsRequestModel.DocTypeId = uploadDocumentsRequest.DocTypeId;
        uploadDocumentsRequestModel.StageId = uploadDocumentsRequest.StageId;
        uploadDocumentsRequestModel.DocumentName = uploadDocumentsRequest.DocumentName;        
        uploadDocumentsRequestModel.Extension = uploadDocumentsRequest.Extension;
        uploadDocumentsRequestModel.CreatedBy = createdBy;

        UploadDocumentsResponseModel uploadDocumentsResponseModel = await _uploadDocumentRepository.UploadDocuments(uploadDocumentsRequestModel);

        UploadDocumentsResponse uploadDocumentsResponse = new UploadDocumentsResponse();
        if (uploadDocumentsResponseModel.fleetId == 0)
        {
            uploadDocumentsResponse.Message = "Document already exists with the same name";
        }
        else
        {
            // upload document
            string path = Path.Combine(_letterOptions.DocumentFolderPath, uploadDocumentsRequest.FleetId.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, uploadDocumentsRequest.DocumentName + uploadDocumentsRequest.Extension);
            await File.WriteAllBytesAsync(path, uploadDocumentsRequest.DocumentData);
            uploadDocumentsResponse.Message = "Updated Successfully";
        }
        return uploadDocumentsResponse;
    }


    public async Task<List<DownloadDocumentResponse>> DownloadDocument(long fleetId, int stageId, int DocTypeId)
    {
        List<DownloadDocumentResponseModel> downloadDocumentResponseModelList = await _uploadDocumentRepository.DownloadDocument(fleetId, stageId, DocTypeId);
        List<DownloadDocumentResponse> downloadDocumentResponses = new List<DownloadDocumentResponse>();

        foreach (DownloadDocumentResponseModel model in downloadDocumentResponseModelList)
        {
            DownloadDocumentResponse downloadDocumentResponse = new DownloadDocumentResponse();
            downloadDocumentResponse.DocumentID = model.DocumentID;
            downloadDocumentResponse.FleetId = model.FleetId;
            downloadDocumentResponse.DocumentUrl = model.DocumentUrl;
            downloadDocumentResponse.Documenttype = model.Documenttype;
            downloadDocumentResponse.StageId = model.StageId;
            downloadDocumentResponses.Add(downloadDocumentResponse);
        }

        return downloadDocumentResponses;
    }
}
