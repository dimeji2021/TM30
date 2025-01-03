﻿namespace Payment.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IBankAccountRepository BankAccounts { get; }
        IBankRepository Banks { get; }
        ITransactionRepository Transactions { get; }
        IVirtualAccountRepository VirtualAccounts { get; }
        IWalletRepository Wallets { get; }
        IApplicationUserRepository Users { get; }
        Task Save();
        Task Rollback();
    }
}
