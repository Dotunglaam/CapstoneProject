using Quartz;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class SendTuitionReminderJob : IJob
    {
        private readonly ITuitionService _tuitionService;

        public SendTuitionReminderJob(ITuitionService tuitionService)
        {
            _tuitionService = tuitionService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _tuitionService.SendTuitionReminderAsync();
        }
    }

}
