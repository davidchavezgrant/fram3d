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

        public void OnNext(T value)
        {
            foreach (var observer in this._observers)
            {
                observer.OnNext(value);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            this._observers.Add(observer);
            return new Unsubscriber(this._observers, observer);
        }


        private sealed class Unsubscriber: IDisposable
        {
            private readonly IObserver<T>       _observer;
            private readonly List<IObserver<T>> _observers;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this._observers = observers;
                this._observer  = observer;
            }

            public void Dispose() => this._observers.Remove(this._observer);
        }
    }
}