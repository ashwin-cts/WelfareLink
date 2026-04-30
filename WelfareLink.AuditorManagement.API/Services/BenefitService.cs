using WelfareLink.AuditorManagement.API.Exceptions;
using WelfareLink.AuditorManagement.API.Interfaces;
using WelfareLink.AuditorManagement.API.Models;

namespace WelfareLink.AuditorManagement.API.Services
{
    public class BenefitService : IBenefitService
    {
        private readonly IBenefitRepository _benefitRepository;
        private readonly IDisbursementRepository _disbursementRepository;
        private readonly IWelfareApplicationRepository _applicationRepository;
        private readonly IEligibilityCheckRepository _eligibilityCheckRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _validBenefitTypes = { "Cash", "Food", "Medical", "Education", "Housing" };
        private readonly string[] _validStatuses = { "Allocation Pending", "Allocated", "Partially Disbursed", "Fully Disbursed", "Failed" };

        public BenefitService(IBenefitRepository benefitRepository, IDisbursementRepository disbursementRepository, IWelfareApplicationRepository applicationRepository, IEligibilityCheckRepository eligibilityCheckRepository, IResourceRepository resourceRepository, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
        {
            _benefitRepository = benefitRepository;
            _disbursementRepository = disbursementRepository;
            _applicationRepository = applicationRepository;
            _eligibilityCheckRepository = eligibilityCheckRepository;
            _resourceRepository = resourceRepository;
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCurrentUserId()
        {
            return _httpContextAccessor?.HttpContext?.Session.GetInt32("UserId");
        }

        public async Task<IEnumerable<Benefit>> GetAllBenefitsAsync()
        {
            return await _benefitRepository.GetAllAsync();
        }

        public async Task<Benefit?> GetBenefitByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException("Benefit ID must be greater than zero.");
            }
            return await _benefitRepository.GetByIdAsync(id);
        }

        public async Task<Benefit> CreateBenefitAsync(Benefit benefit, int officerId = 0)
        {
            ValidateBenefit(benefit);

            // Check if the application has a rejected eligibility check
            await ValidateEligibilityCheckAsync(benefit.ApplicationID);

            // Validate benefit amount against programme budget and remaining resources
            await ValidateBudgetAndResourcesAsync(benefit);

            var createdBenefit = await _benefitRepository.AddAsync(benefit);

            var userId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "Create",
                entityType: "Benefit",
                entityId: createdBenefit.BenefitID,
                description: $"Created benefit '{createdBenefit.Type}' - Amount: \u20b9{createdBenefit.Amount:N2}",
                newValue: createdBenefit.Status,
                status: "Success"
            );

            // When benefit is created with "Allocated" status, auto-create a disbursement
            if (createdBenefit.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
            {
                var application = await _applicationRepository.GetByIdAsync(createdBenefit.ApplicationID);
                var disbursement = new Disbursement
                {
                    BenefitID = createdBenefit.BenefitID,
                    CitizenID = application?.CitizenID ?? 0,
                    OfficerID = officerId,
                    Amount = createdBenefit.Amount,
                    Date = DateTime.Now,
                    Status = "Disbursement Pending"
                };
                await _disbursementRepository.AddAsync(disbursement);
            }

            return createdBenefit;
        }

        public async Task<Benefit> UpdateBenefitAsync(Benefit benefit, int officerId = 0)
        {
            if (benefit.BenefitID <= 0)
            {
                throw new BadRequestException("Benefit ID must be greater than zero.");
            }

            var existingBenefit = await _benefitRepository.GetByIdAsync(benefit.BenefitID);
            if (existingBenefit == null)
            {
                throw new BusinessValidationException($"Benefit with ID {benefit.BenefitID} does not exist.");
            }

            ValidateBenefit(benefit);

            // Validate benefit amount against programme budget and remaining resources
            await ValidateBudgetAndResourcesAsync(benefit);

            var updatedBenefit = await _benefitRepository.UpdateAsync(benefit);

            var userId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "Update",
                entityType: "Benefit",
                entityId: benefit.BenefitID,
                description: $"Updated benefit status",
                oldValue: existingBenefit.Status,
                newValue: benefit.Status,
                status: "Success"
            );

            // When status transitions to "Allocated", auto-create a disbursement with "Disbursement Pending"
            if (!existingBenefit.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase) &&
                benefit.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
            {
                var application = await _applicationRepository.GetByIdAsync(updatedBenefit.ApplicationID);
                var disbursement = new Disbursement
                {
                    BenefitID = updatedBenefit.BenefitID,
                    CitizenID = application?.CitizenID ?? 0,
                    OfficerID = officerId,
                    Amount = updatedBenefit.Amount,
                    Date = DateTime.Now,
                    Status = "Disbursement Pending"
                };
                await _disbursementRepository.AddAsync(disbursement);
            }

            return updatedBenefit;
        }

        public async Task<bool> DeleteBenefitAsync(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException("Benefit ID must be greater than zero.");
            }

            if (!await _benefitRepository.ExistsAsync(id))
            {
                throw new BusinessValidationException($"Benefit with ID {id} does not exist.");
            }

            // Block deletion if any disbursement for this benefit has been completed
            var disbursements = await _disbursementRepository.GetByBenefitIdAsync(id);
            if (disbursements.Any(d => d.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
            {
                throw new BusinessValidationException("Cannot delete a benefit that has completed disbursements.");
            }

            // Capture application ID before deleting so we can reset its status
            var benefit = await _benefitRepository.GetByIdAsync(id);
            var applicationId = benefit?.ApplicationID ?? 0;

            var result = await _benefitRepository.DeleteAsync(id);

            // Reset the application back to Pending so it can be re-allocated
            if (result && applicationId > 0)
            {
                await _applicationRepository.UpdateStatusAsync(applicationId, "Pending");
            }

            return result;
        }

        public async Task<bool> BenefitExistsAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }
            return await _benefitRepository.ExistsAsync(id);
        }

        public async Task<Benefit?> CreateBenefitForApprovedApplicationAsync(int applicationId)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                return null;
            }

            // Avoid creating a duplicate while a benefit is still awaiting allocation
            var existing = await _benefitRepository.GetByApplicationIdAsync(applicationId);
            if (existing.Any(b => b.Status.Equals("Allocation Pending", StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            var benefit = new Benefit
            {
                ApplicationID = applicationId,
                Type = "Cash",
                Amount = 0,
                Date = DateTime.Now,
                Status = "Allocation Pending"
            };

            return await _benefitRepository.AddAsync(benefit);
        }

        #region Private Validation Methods

        private void ValidateBenefit(Benefit benefit)
        {
            if (benefit == null)
            {
                throw new BadRequestException("Benefit cannot be null.");
            }

            ValidateApplicationId(benefit.ApplicationID);
            ValidateBenefitType(benefit.Type);
            ValidateAmount(benefit.Amount);
            ValidateDate(benefit.Date);
            ValidateStatus(benefit.Status);
        }

        private void ValidateApplicationId(int applicationId)
        {
            if (applicationId <= 0)
            {
                throw new BadRequestException("Application ID must be greater than zero.");
            }
        }

        private void ValidateBenefitType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new BadRequestException("Benefit type is required.");
            }

            if (!_validBenefitTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException($"Invalid benefit type. Valid types are: {string.Join(", ", _validBenefitTypes)}");
            }
        }

        private void ValidateAmount(double amount)
        {
            if (amount <= 0)
            {
                throw new BadRequestException("Amount must be greater than zero.");
            }

            if (amount > 10000000) // 1 crore max limit
            {
                throw new BadRequestException("Amount exceeds maximum allowed limit.");
            }
        }

        private void ValidateDate(DateTime date)
        {
            if (date == default)
            {
                throw new BadRequestException("Date is required.");
            }

            if (date > DateTime.Now.AddDays(1))
            {
                throw new BadRequestException("Date cannot be in the future.");
            }

            if (date < DateTime.Now.AddYears(-10))
            {
                throw new BadRequestException("Date cannot be older than 10 years.");
            }
        }

        private void ValidateStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new BadRequestException("Status is required.");
            }

            if (!_validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException($"Invalid status. Valid statuses are: {string.Join(", ", _validStatuses)}");
            }
        }

        private async Task ValidateBudgetAndResourcesAsync(Benefit benefit)
        {
            // Only enforce when the benefit is being allocated (has a meaningful amount)
            if (!benefit.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
                return;

            var application = await _applicationRepository.GetByIdAsync(benefit.ApplicationID);
            if (application?.Program == null) return;

            var program = application.Program;
            var programId = application.ProgramID;

            // 1. Benefit amount must not exceed the programme's total budget
            if ((decimal)benefit.Amount > program.Budget)
            {
                throw new BusinessValidationException(
                    $"Budget Exceeded: Benefit amount \u20B9{benefit.Amount:N2} exceeds the programme budget of \u20B9{(double)program.Budget:N2}. " +
                    $"Programme '{program.Title}' has a total budget of \u20B9{(double)program.Budget:N2}. " +
                    $"Please reduce the benefit amount or contact the Programme Manager to increase the budget.");
            }

            // 2. Benefit amount must not exceed the remaining resource allocation for the programme
            var resources = await _resourceRepository.GetResourcesByProgramIdAsync(programId);
            var totalResourceAllocation = (double)resources.Sum(r => r.Quantity);

            if (totalResourceAllocation == 0) return; // No resource constraint defined

            // Sum benefit amounts already allocated to this programme (excluding the current benefit when updating)
            var allBenefits = await _benefitRepository.GetAllAsync();
            var alreadyAllocated = allBenefits
                .Where(b => b.WelfareApplication?.ProgramID == programId
                            && !b.Status.Equals("Allocation Pending", StringComparison.OrdinalIgnoreCase)
                            && !b.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase)
                            && b.BenefitID != benefit.BenefitID)
                .Sum(b => b.Amount);

            var remainingResources = totalResourceAllocation - alreadyAllocated;

            if (benefit.Amount > remainingResources)
            {
                if (remainingResources <= 0)
                {
                    throw new BusinessValidationException(
                        $"Resource Exhausted: Programme '{program.Title}' has no remaining resource allocation. " +
                        $"Total resources: \u20B9{totalResourceAllocation:N2}, Already allocated to benefits: \u20B9{alreadyAllocated:N2}. " +
                        $"Please contact the Programme Manager to increase resource allocation.");
                }
                else
                {
                    throw new BusinessValidationException(
                        $"Resource Insufficient: Benefit amount \u20B9{benefit.Amount:N2} exceeds available resources \u20B9{remainingResources:N2}. " +
                        $"Programme '{program.Title}' \u2014 Total resources: \u20B9{totalResourceAllocation:N2}, " +
                        $"Already allocated: \u20B9{alreadyAllocated:N2}, Remaining: \u20B9{remainingResources:N2}. " +
                        $"Please reduce the benefit amount or contact the Programme Manager.");
                }
            }
            //Check if the Max Benefit exceeds.
            if ((decimal)benefit.Amount > program.MaxBenefitPerCitizen)
            {
                throw new BusinessValidationException(
                    $"Budget Exceeded: Benefit amount \u20B9{benefit.Amount:N2} exceeds the Max budget per citizen of \u20B9{(double)program.MaxBenefitPerCitizen:N2}. " +
                    $"Programme '{program.Title}' has a Max budget per citizen of \u20B9{(double)program.MaxBenefitPerCitizen:N2}. " +
                    $"Please reduce the benefit amount or contact the Programme Manager");
            }
        }

        private async Task ValidateEligibilityCheckAsync(int applicationId)
        {
            // Get the latest eligibility check for this application
            var latestCheck = await _eligibilityCheckRepository.GetLatestCheckForApplicationAsync(applicationId);

            if (latestCheck != null)
            {
                // Check if the result is "Rejected" or result code indicates rejection
                if (latestCheck.Result.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ||
                    latestCheck.ResultCode.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ||
                    latestCheck.ResultCode.Equals("REJECTED", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessValidationException($"Cannot create benefit for application #{applicationId}. The application has been rejected in the eligibility check.");
                }
            }
        }

        #endregion
    }
}