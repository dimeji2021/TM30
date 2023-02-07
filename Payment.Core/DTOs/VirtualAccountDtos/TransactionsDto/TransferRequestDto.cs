using Payment.Core.DTOs.BankDtos;
using System.ComponentModel.DataAnnotations;

namespace Payment.Core.DTOs.TransactionsDto
{
    public class TransferRequestDto
    {
       
        public string RecipientType { get; set; }

        [Required]
        public decimal Amount { get; set; } 

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public string AccountNumber { get; set; } = null!;

        [Required]
        public string AccountName { get; set; } = null!;

        [Required]
        public string TransferPin { get; set; } = null!;

        [Required]
        public BeneficiaryBankDto BeneficiaryBank { get; set; } = null!;

        public bool Check { get; set; }
    }
}
