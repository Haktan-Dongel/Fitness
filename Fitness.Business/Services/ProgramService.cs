using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Business.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fitness.Business.Services
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepository _repository;
        private readonly ILogger<ProgramService> _logger;

        public ProgramService(IProgramRepository repository, ILogger<ProgramService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> HasAvailableSpaceAsync(string programCode)
        {
            var currentCount = await _repository.GetCurrentMemberCountAsync(programCode);
            var program = await _repository.GetByProgramCodeAsync(programCode);
            
            if (program == null)
                throw new NotFoundException($"Program {programCode} not found");
                
            return currentCount < program.MaxMembers;
        }

        public async Task AddMemberToProgramAsync(string programCode, int memberId)
        {
            if (string.IsNullOrEmpty(programCode))
                throw new ValidationException("Program code is required");

            if (memberId <= 0)
                throw new ValidationException("Invalid member ID");

            if (!await HasAvailableSpaceAsync(programCode))
            {
                _logger.LogWarning("Attempted to add member {MemberId} to full program {ProgramCode}", memberId, programCode);
                throw new ValidationException("Program is full");
            }

            if (await _repository.IsMemberEnrolledAsync(programCode, memberId))
            {
                _logger.LogWarning("Member {MemberId} is already enrolled in program {ProgramCode}", memberId, programCode);
                throw new ValidationException("Member is already enrolled");
            }

            var programMember = new ProgramMembers
            {
                ProgramCode = programCode,
                MemberId = memberId
            };

            await _repository.AddMemberToProgramAsync(programCode, memberId);
            _logger.LogInformation("Successfully added member {MemberId} to program {ProgramCode}", memberId, programCode);
        }

    }
}
