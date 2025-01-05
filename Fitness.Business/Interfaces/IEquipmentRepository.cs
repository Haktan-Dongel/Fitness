using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface IEquipmentRepository : IGenericRepository<Equipment>
    {
        Task<bool> IsAvailableForTimeSlotAsync(int equipmentId, int timeSlotId, DateTime date);
        Task<IEnumerable<Equipment>> GetAvailableEquipmentForTimeSlotAsync(int timeSlotId, DateTime date);
        Task<IEnumerable<Equipment>> GetByTypeAsync(string deviceType);
    }
}
