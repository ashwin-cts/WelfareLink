using WelfareLink.Interfaces;
using WelfareLink.Models;


namespace WelfareLink.Services
{
    public class BenefitService : IBenefitService
    {
        private readonly IBenefitRepository _benefitRepository;
        private readonly IDisbursementRepository _disbursementRepository;
        private readonly string[] _validBenefitTypes = { "Cash", "Food", "Medical", "Education", "Housing" };
        private readonly string[] _validStatuses = { "Allocated", "Partially Disbursed", "Fully Disbursed" };

        public BenefitService(IBenefitRepository benefitRepository, IDisbursementRepository disbursementRepository)
        {
            _benefitRepository = benefitRepository;
            _disbursementRepository = disbursementRepository;
        }

        public async Task<IEnumerable<Benefit>> GetAllBenefitsAsync()
        {
            return await _benefitRepository.GetAllAsync();
        }

        public async Task<Benefit?> GetBenefitByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Benefit ID must be greater than zero.", nameof(id));
            }
            return await _benefitRepository.GetByIdAsync(id);
        }

        public async Task<Benefit> CreateBenefitAsync(Benefit benefit)
        {
            ValidateBenefit(benefit);

            // Create the benefit first
            var createdBenefit = await _benefitRepository.AddAsync(benefit);

            // Auto-create a disbursement with "Pending" status for this benefit with full amount
            var disbursement = new Disbursement
            {
                BenefitID = createdBenefit.BenefitID,
                CitizenID = 0, // To be assigned by officer later
                OfficerID = 0, // To be assigned by officer later
                Amount = createdBenefit.Amount, // Full benefit amount pending disbursement
                Date = DateTime.Now,
                Status = "Pending"
            };
            await _disbursementRepository.AddAsync(disbursement);

            return createdBenefit;
        }

        public async Task<Benefit> UpdateBenefitAsync(Benefit benefit)
        {
            if (benefit.BenefitID <= 0)
            {
                throw new ArgumentException("Benefit ID must be greater than zero.", nameof(benefit.BenefitID));
            }

            if (!await _benefitRepository.ExistsAsync(benefit.BenefitID))
            {
                throw new InvalidOperationException($"Benefit with ID {benefit.BenefitID} does not exist.");
            }

            ValidateBenefit(benefit);
            return await _benefitRepository.UpdateAsync(benefit);
        }

        public async Task<bool> DeleteBenefitAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Benefit ID must be greater than zero.", nameof(id));
            }

            if (!await _benefitRepository.ExistsAsync(id))
            {
                throw new InvalidOperationException($"Benefit with ID {id} does not exist.");
            }

            return await _benefitRepository.DeleteAsync(id);
        }

        public async Task<bool> BenefitExistsAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }
            return await _benefitRepository.ExistsAsync(id);
        }

        #region Private Validation Methods

        private void ValidateBenefit(Benefit benefit)
        {
            if (benefit == null)
            {
                throw new ArgumentNullException(nameof(benefit), "Benefit cannot be null.");
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
                throw new ArgumentException("Application ID must be greater than zero.", nameof(applicationId));
            }
        }

        private void ValidateBenefitType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Benefit type is required.", nameof(type));
            }

            if (!_validBenefitTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid benefit type. Valid types are: {string.Join(", ", _validBenefitTypes)}", nameof(type));
            }
        }

        private void ValidateAmount(double amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            }

            if (amount > 10000000) // 1 crore max limit
            {
                throw new ArgumentException("Amount exceeds maximum allowed limit.", nameof(amount));
            }
        }

        private void ValidateDate(DateTime date)
        {
            if (date == default)
            {
                throw new ArgumentException("Date is required.", nameof(date));
            }

            if (date > DateTime.Now.AddDays(1))
            {
                throw new ArgumentException("Date cannot be in the future.", nameof(date));
            }

            if (date < DateTime.Now.AddYears(-10))
            {
                throw new ArgumentException("Date cannot be older than 10 years.", nameof(date));
            }
        }

        private void ValidateStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Status is required.", nameof(status));
            }

            if (!_validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", _validStatuses)}", nameof(status));
            }
        }

        #endregion
    }
}
