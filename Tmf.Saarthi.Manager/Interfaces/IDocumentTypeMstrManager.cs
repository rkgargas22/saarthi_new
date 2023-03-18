﻿using Tmf.Saarthi.Infrastructure.Models.Response.DocumentTypeMstr;

namespace Tmf.Saarthi.Manager.Interfaces
{
    public interface IDocumentTypeMstrManager
    {
        Task<DocumentTypeMstrResponseModel> GetDocumentTypeMstrByDocumentCode(string documentCode);
    }
}
