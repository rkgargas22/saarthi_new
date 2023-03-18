﻿using Tmf.Saarthi.Infrastructure.Models.Response.DocumentTypeMstr;

namespace Tmf.Saarthi.Infrastructure.Interfaces
{
    public interface IDocumentTypeMstrRepository
    {
        Task<DocumentTypeMstrResponseModel> GetDocumentTypeMstrByDocumentCode(string documentCode);
    }
}
