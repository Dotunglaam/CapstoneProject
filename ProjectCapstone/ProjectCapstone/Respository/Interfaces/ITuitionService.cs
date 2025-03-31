using BusinessObject.DTOS;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using static Respository.Services.TuitionService;

namespace Respository.Interfaces
{
    public interface ITuitionService
    {
        public Task GenerateMonthlyTuitionRecords();
        Task<List<object>> GetTuitionRecordsByParentIdAsync(int parentId); // New method for fetching tuition records
        Task<List<TuitionRecord>> GenerateMonthlyTuitionRecordsClick(bool overrideCheck = false);
        Task UpdateTuitionStatusAndNotifyAsync();
        Task<List<TuitionDTO>> GetAllTuitionsAsync();
        Task ApproveAndSendEmailsForAllTuitions();
        Task<Result> SendTuitionReminderAsync(bool isManualTrigger = false);
        Task<int> DeleteUnpaidTuitionsAsync();
    }
}
