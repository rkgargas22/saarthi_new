using Tmf.Saarthi.Infrastructure.Models.Response.StageMaster;

namespace Tmf.Saarthi.Manager.Interfaces
{
    public interface IStageMasterManager
    {
        Task<StageMasterResponseModel> GetStageMasterByStageCode(string stageCode);
    }
}
