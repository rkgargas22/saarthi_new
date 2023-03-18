using Tmf.Saarthi.Infrastructure.Interfaces;
using Tmf.Saarthi.Infrastructure.Models.Response.StageMaster;
using Tmf.Saarthi.Manager.Interfaces;

namespace Tmf.Saarthi.Manager.Services
{
    public class StageMasterManager : IStageMasterManager
    {
        private readonly IStageMasterRepository _stageMasterRepository;

        public StageMasterManager(IStageMasterRepository stageMasterRepository)
        {
            _stageMasterRepository = stageMasterRepository;
        }

        public async Task<StageMasterResponseModel> GetStageMasterByStageCode(string stageCode)
        {
            return await _stageMasterRepository.GetStageMasterByStageCode(stageCode);
        }
    }
}
