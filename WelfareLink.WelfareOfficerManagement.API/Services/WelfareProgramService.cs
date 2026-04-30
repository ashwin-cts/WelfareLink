using WelfareLink.WelfareOfficerManagement.API.Exceptions;
using WelfareLink.WelfareOfficerManagement.API.Interfaces;
using WelfareLink.WelfareOfficerManagement.API.Models;

namespace WelfareLink.WelfareOfficerManagement.API.Services;

public class WelfareProgramService : IWelfareProgramService
{
    private readonly IWelfareProgramRepository _programRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WelfareProgramService(IWelfareProgramRepository programRepository, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
    {
        _programRepository = programRepository;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    private int? GetCurrentUserId()
    {
        return _httpContextAccessor?.HttpContext?.Session.GetInt32("UserId");
    }

    public async Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync()
    {
        var programs = (await _programRepository.GetAllProgramsAsync()).ToList();
        var expiredIds = programs
            .Where(p => p.Status == "Active" && p.EndDate.Date < DateTime.Today)
            .Select(p => p.ProgramID)
            .ToList();
        foreach (var id in expiredIds)
            await _programRepository.UpdateStatusAsync(id, "Expired");
        if (expiredIds.Count > 0)
            programs = (await _programRepository.GetAllProgramsAsync()).ToList();
        return programs;
    }

    public async Task<WelfareProgram> GetProgramByIdAsync(int id)
    {
        var program = await _programRepository.GetProgramByIdAsync(id);
        if (program != null && program.Status == "Active" && program.EndDate.Date < DateTime.Today)
        {
            await _programRepository.UpdateStatusAsync(id, "Expired");
            program = await _programRepository.GetProgramByIdAsync(id);
        }
        return program;
    }

    public async Task AddProgramAsync(WelfareProgram program)
    {
        ValidateProgramDates(program, isNewProgram: true);
        ValidateProgramBudget(program);
        await ValidateDuplicateTitle(program.Title, program.ProgramID);

        program.Status = "Active";

        await _programRepository.AddProgramAsync(program);

        var userId = GetCurrentUserId();
        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "Create",
            entityType: "Program",
            entityId: program.ProgramID,
            description: $"Created program '{program.Title}' with budget ₹{program.Budget:N2}",
            status: "Success"
        );
    }

    public async Task UpdateProgramAsync(WelfareProgram program)
    {
        ValidateProgramDates(program, isNewProgram: false);
        ValidateProgramBudget(program);
        await ValidateDuplicateTitle(program.Title, program.ProgramID);

        await _programRepository.UpdateProgramAsync(program);

        var userId = GetCurrentUserId();
        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "Update",
            entityType: "Program",
            entityId: program.ProgramID,
            description: $"Updated program '{program.Title}'",
            status: "Success"
        );
    }

    public async Task SuspendProgramAsync(int id)
    {
        var program = await _programRepository.GetProgramByIdAsync(id);
        if (program == null)
            throw new NotFoundException($"Programme #{id} not found.");
        if (program.Status == "Expired")
            throw new BusinessValidationException("Cannot suspend an already expired programme.");
        if (program.Status == "Suspended")
            throw new BusinessValidationException("Programme is already suspended.");
        await _programRepository.UpdateStatusAsync(id, "Suspended");

        var userId = GetCurrentUserId();
        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "Update",
            entityType: "Program",
            entityId: id,
            description: $"Suspended program '{program.Title}'",
            status: "Success"
        );
    }

    public async Task DeleteProgramAsync(int id)
    {
        var program = await _programRepository.GetProgramByIdAsync(id);
        if (program == null)
        {
            throw new NotFoundException($"Program with ID {id} not found.");
        }

        if (program.Status == "Active")
        {
            throw new BusinessValidationException("Cannot delete an active program. Please suspend or complete it first.");
        }

        await _programRepository.DeleteProgramAsync(id);
    }

    private void ValidateProgramDates(WelfareProgram program, bool isNewProgram)
    {
        if (program.EndDate <= program.StartDate)
        {
            throw new BadRequestException("Programme end date must be after the start date.");
        }

        // Only check if start date is in the past for NEW programmes
        // Allow updates to existing programmes even if start date is in the past
        if (isNewProgram && program.StartDate < DateTime.Today)
        {
            throw new BadRequestException("Programme start date cannot be in the past.");
        }
    }

    private void ValidateProgramBudget(WelfareProgram program)
    {
        if (program.Budget <= 0)
        {
            throw new BadRequestException("Programme budget must be greater than zero.");
        }
    }

    private async Task ValidateDuplicateTitle(string title, int programId)
    {
        var existingPrograms = await _programRepository.GetAllProgramsAsync();
        var duplicate = existingPrograms.FirstOrDefault(p =>
            p.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
            p.ProgramID != programId);

        if (duplicate != null)
        {
            throw new BusinessValidationException($"A programme with the title '{title}' already exists.");
        }
    }
}