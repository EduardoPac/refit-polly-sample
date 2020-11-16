using System;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

namespace PollyRefitSample.Services
{
    public interface IPollyService
    {
        Task<T> Retry<T>(Func<Task<T>> func, int retryCount, Func<Exception, int, Task> onRetry = null);

        Task<T> WaitAndRetry<T>(Func<Task<T>> func, int millisecondsWait, int retryCount = 1,
            Func<Exception, TimeSpan, Task> onRetry = null);

        Task<T> CircuitBreaker<T>(Func<Task<T>> func, int numberValidRetry, int secondsWait = 1, Action<Exception, TimeSpan, Context> onBreak = null, Action onHalfOpen = null, Action<Context> onReset = null);
    }

    public class PollyService : IPollyService
    {
        public async Task<T> Retry<T>(Func<Task<T>> func, int retryCount, Func<Exception, int, Task> onRetry) =>
            await RetryInner(func, retryCount, onRetry);

        public async Task<T> WaitAndRetry<T>(Func<Task<T>> func, int secondsWait, int retryCount,
            Func<Exception, TimeSpan, Task> onRetry) =>
            await WaitAndRetryInner<T>(func, secondsWait, retryCount, onRetry);

        public async Task<T> CircuitBreaker<T>(Func<Task<T>> func, int numberValidRetry, int secondsWait, Action<Exception, TimeSpan, Context> onBreak = null, Action onHalfOpen = null, Action<Context> onReset = null) =>
            await CircuitBreakerInner(func, numberValidRetry, secondsWait, onBreak, onHalfOpen, onReset);

        #region Inner Methods

        private async Task<T> RetryInner<T>(Func<Task<T>> func, int retryCount = 1,
            Func<Exception, int, Task> onRetry = null)
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

        private async Task<T> WaitAndRetryInner<T>(Func<Task<T>> func, int millisecondsWait, int retryCount = 1,
            Func<Exception, TimeSpan, Task> onRetryAsync = null)
        {
            var onRetryInner = new Func<Exception, TimeSpan, Task>((e, t) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    Console.WriteLine(
                        $"Retrying in {t.ToString("g")} due to exception '{(e.InnerException ?? e).Message}' in {DateTimeOffset.Now} "); //TODO Futura integracao com o appCenter
                });
            });
            ;

            var sleepDurationProvider = new Func<int, TimeSpan>((i => TimeSpan.FromMilliseconds(millisecondsWait)));

            return await Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, sleepDurationProvider, onRetryAsync ?? onRetryInner)
                .ExecuteAsync<T>(func);
        }


        private async Task<T> CircuitBreakerInner<T>(Func<Task<T>> func, int numberValidRetry, int secondsWait, Action<Exception, TimeSpan, Context> onBreak = null, Action onHalfOpen = null, Action<Context> onReset = null)
        {
            var sleepDurationProvider = new Func<int, TimeSpan>((i => TimeSpan.FromSeconds(secondsWait)));

            var retry = Policy.Handle<Exception>()
                .WaitAndRetryAsync(numberValidRetry + 1, sleepDurationProvider);

            var onBreakInner = new Action<Exception, TimeSpan, Context>((ex, t, c) => 
                Console.WriteLine($"Circuit went into a fault state with the exception ({(ex.InnerException ?? ex).Message}) retrying in {t:g})"));
            
            var onResetInner = new Action<Context>(context => Console.WriteLine($"Circuit left the fault state"));

            var onHalfOpenInner = new Action(() => Console.WriteLine("Half Open"));

            var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreakerAsync(numberValidRetry,
                TimeSpan.FromSeconds(secondsWait),
                onBreak ?? onBreakInner, 
                onReset ?? onResetInner,
                onHalfOpen ?? onHalfOpenInner);

            return await retry.WrapAsync(circuitBreakerPolicy).ExecuteAsync<T>(func);
        }

        #endregion
    }
}