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
        Task<T> WaitAndRetry<T>(Func<Task<T>> func, int secondsWait);
        Task<T> WaitAndRetry<T>(Func<Task<T>> func, int secondsWait, int retryCount);
        Task<T> WaitAndRetry<T>(Func<Task<T>> func, int secondsWait, int retryCount, Func<Exception, TimeSpan, Task> onRetryAsync);
    }

    public class PollyService : IPollyService
    {
        public async Task<T> Retry<T>(Func<Task<T>> func) =>
            await RetryInner(func);

        public async Task<T> Retry<T>(Func<Task<T>> func, int retryCount) =>
            await RetryInner(func, retryCount);

        public async Task<T> Retry<T>(Func<Task<T>> func, int retryCount, Func<Exception, int, Task> onRetry) =>
            await RetryInner(func, retryCount, onRetry);

        public async Task<T> WaitAndRetry<T>(Func<Task<T>> func, int secondsWait) =>
            await WaitAndRetryInner<T>(func, secondsWait);

        public async Task<T> WaitAndRetry<T>(Func<Task<T>> func, int secondsWait, int retryCount) =>
            await WaitAndRetryInner<T>(func, secondsWait, retryCount);

        public async Task<T> WaitAndRetry<T>(Func<Task<T>> func, int secondsWait, int retryCount, Func<Exception, TimeSpan, Task> onRetryAsync) =>
            await WaitAndRetryInner<T>(func, secondsWait, retryCount, onRetryAsync);

        #region Inner Methods

        private async Task<T> RetryInner<T>(Func<Task<T>> func, int retryCount = 1, Func<Exception, int, Task> onRetry = null)
        {
            var onRetryInner = new Func<Exception, int, Task>((e, i) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    Console.WriteLine(
                        $"Retry #{i} in {DateTimeOffset.Now} due to exception '{(e.InnerException ?? e).Message}'"); //TODO Futura integracao com o AppCenter
                });
            });

            return await Policy.Handle<Exception>()
                .RetryAsync(retryCount, onRetry ?? onRetryInner)
                .ExecuteAsync<T>(func);
        }

        private async Task<T> WaitAndRetryInner<T>(Func<Task<T>> func, int secondsWait, int retryCount = 1, Func<Exception, TimeSpan, Task> onRetryAsync = null)
        {
            var onRetryInner = new Func<Exception, TimeSpan, Task>((e, t) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    Console.WriteLine(
                        $"Retrying in {t.ToString("g")} due to exception '{(e.InnerException ?? e).Message}' in {DateTimeOffset.Now} "); //TODO Futura integracao com o appCenter
                });
            }); ;
                
            var sleepDurationProvider = new Func<int, TimeSpan>((i => TimeSpan.FromSeconds(secondsWait)));

            return await Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, sleepDurationProvider, onRetryAsync ?? onRetryInner)
                .ExecuteAsync<T>(func);
        }

        #endregion
    }
}