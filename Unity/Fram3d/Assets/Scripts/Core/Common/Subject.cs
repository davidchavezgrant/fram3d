using System;
using System.Collections.Generic;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Minimal IObservable implementation for synchronous event delivery.
    /// No central event bus — each source owns its own subjects.
    /// </summary>
    public sealed class Subject<T>: IObservable<T>
    {
        private readonly List<IObserver<T>> _observers = new();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            this._observers.Add(observer);
            return new Unsubscriber(this._observers, observer);
        }

        public void OnNext(T value)
        {
            foreach (var observer in this._observers)
            {
                observer.OnNext(value);
            }
        }

        private sealed class Unsubscriber: IDisposable
        {
            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T>       _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this._observers = observers;
                this._observer  = observer;
            }

            public void Dispose() => this._observers.Remove(this._observer);
        }
    }

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

            public void OnCompleted()             { }
            public void OnError(Exception error)   { }
            public void OnNext(T           value) => this._onNext(value);
        }
    }
}
