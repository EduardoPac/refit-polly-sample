using System;
using System.Threading.Tasks;
using Polly;

namespace PollyRefitSample.Services
{
    public interface IPollyService
    {
        Task<T> Retry<T>(Func<Task<T>> func);
        Task<T> Retry<T>(Func<Task<T>> func, int retryCount);
        Task<T> Retry<T>(Func<Task<T>> func, int retryCount, Func<Exception, int, Task> onRetry);
        Task<T> WaitAndRetry<T>(Func<Task<T>> func, Func<int, TimeSpan> sleepDurationProvider);
        Task<T> WaitAndRetry<T>(Func<Task<T>> func, Func<int, TimeSpan> sleepDurationProvider, int retryCount);
        Task<T> WaitAndRetry<T>(Func<Task<T>> func, Func<int, TimeSpan> sleepDurationProvider, int retryCount, Func<Exception, TimeSpan, Task> onRetryAsync);
    }

    public class PollyService : IPollyService
    {
        public async Task<T> Retry<T>(Func<Task<T>> func) => 
            await RetryInner(func);

        public async Task<T> Retry<T>(Func<Task<T>> func, int retryCount) => 
            await RetryInner(func, retryCount);

        public async Task<T> Retry<T>(Func<Task<T>> func, int retryCount, Func<Exception, int, Task> onRetry) => 
            await RetryInner(func, retryCount, onRetry);

        public async Task<T> WaitAndRetry<T>(Func<Task<T>> func, Func<int, TimeSpan> sleepDurationProvider) => 
            await WaitAndRetryInner<T>(func, sleepDurationProvider);

        public async Task<T> WaitAndRetry<T>(Func<Task<T>> func, Func<int, TimeSpan> sleepDurationProvider, int retryCount) =>
            await WaitAndRetryInner<T>(func, sleepDurationProvider, retryCount);

        public async Task<T> WaitAndRetry<T>(Func<Task<T>> func, Func<int, TimeSpan> sleepDurationProvider, int retryCount, Func<Exception, TimeSpan, Task> onRetryAsync) =>
            await WaitAndRetryInner<T>(func, sleepDurationProvider, retryCount, onRetryAsync);

        #region Inner Methods

        private async Task<T> RetryInner<T>(Func<Task<T>> func, int retryCount = 1, Func<Exception, int, Task> onRetry = null)
        {
            var onRetryInner = new Func<Exception, int, Task>((e, i) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Retry #{i} due to exception '{(e.InnerException ?? e).Message}'"); //TODO Futura integracao com o AppCenter
                });
            });

            return await Policy.Handle<Exception>()
                .RetryAsync(retryCount, onRetry ?? onRetryInner)
                .ExecuteAsync<T>(func);
        }

        private async Task<T> WaitAndRetryInner<T>(Func<Task<T>> func, Func<int, TimeSpan> sleepDurationProvider, int retryCount = 1, Func<Exception, TimeSpan, Task> onRetryAsync = null)
        {
            var onRetryInner = new Func<Exception, TimeSpan, Task>((e, t) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Retrying in {t.ToString("g")} due to exception '{(e.InnerException ?? e).Message}'"); //TODO Futura integracao com o appCenter
                });
            });

            return await Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, sleepDurationProvider, onRetryAsync ?? onRetryInner)
                .ExecuteAsync<T>(func);
        }

        #endregion
    }
}