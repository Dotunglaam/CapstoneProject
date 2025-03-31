using Quartz;
using Respository.Interfaces;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class GenerateTuitionJob : IJob
    {
        private readonly ITuitionService _tuitionService;

        // Constructor to inject services
        public GenerateTuitionJob(ITuitionService tuitionService)
        {
            _tuitionService = tuitionService; 
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _tuitionService.GenerateMonthlyTuitionRecords();  
        }

    }
}
