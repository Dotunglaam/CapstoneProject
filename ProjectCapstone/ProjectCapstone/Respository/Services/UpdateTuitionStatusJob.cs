using Quartz;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class UpdateTuitionStatusJob: IJob
    {
        private readonly ITuitionService _tuitionService;

        public UpdateTuitionStatusJob(ITuitionService tuitionService)
        {
            _tuitionService = tuitionService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _tuitionService.UpdateTuitionStatusAndNotifyAsync();
        }
    }
}
