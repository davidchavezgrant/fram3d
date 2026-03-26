using System;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Convenience: subscribes to an IObservable with just an Action callback.
    /// </summary>
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(new ActionObserver<T>(onNext));
        }


        private sealed class ActionObserver<T>: IObserver<T>
        {
            private readonly Action<T> _onNext;

            public ActionObserver(Action<T> onNext)
            {
                this._onNext = onNext;
            }

            public void OnCompleted()            {}
            public void OnError(Exception error) {}
            public void OnNext(T          value) => this._onNext(value);
        }
    }
}
