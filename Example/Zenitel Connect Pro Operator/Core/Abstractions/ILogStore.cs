using ConnectPro.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZenitelConnectProOperator.Core.Abstractions;

public interface ILogStore
{
    void Initialize();
    void LogException(string sender, Exception ex);
    Task<IReadOnlyList<CallLog>> GetLatestCallLogsAsync(int take);
    Task<bool> ExistsSimilarAsync(CallLog entry, DateTime windowStartUtcOrLocal);
    Task AddAsync(CallLog entry);
}
