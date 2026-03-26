using System;
using System.Collections.Generic;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
    public class SubjectTests
    {
        [Fact]
        public void OnNext__NotifiesSubscriber__When__ValueEmitted()
        {
            var subject = new Subject<int>();
            var received = -1;
            subject.Subscribe(v => received = v);
            subject.OnNext(42);
            received.Should().Be(42);
        }

        [Fact]
        public void OnNext__NotifiesAllSubscribers__When__MultipleSubscribed()
        {
            var subject = new Subject<int>();
            var values = new List<int>();
            subject.Subscribe(v => values.Add(v));
            subject.Subscribe(v => values.Add(v * 10));
            subject.OnNext(5);
            values.Should().BeEquivalentTo(new[] { 5, 50 });
        }

        [Fact]
        public void Subscribe__ThrowsArgumentNull__When__ObserverIsNull()
        {
            var subject = new Subject<int>();
            Action act = () => subject.Subscribe((IObserver<int>)null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Dispose__StopsNotifications__When__Unsubscribed()
        {
            var subject = new Subject<int>();
            var received = 0;
            var sub = subject.Subscribe(v => received = v);
            subject.OnNext(1);
            received.Should().Be(1);

            sub.Dispose();
            subject.OnNext(2);
            received.Should().Be(1); // unchanged
        }

        [Fact]
        public void OnNext__DoesNothing__When__NoSubscribers()
        {
            var subject = new Subject<string>();
            Action act = () => subject.OnNext("hello");
            act.Should().NotThrow();
        }

        [Fact]
        public void Subscribe__WorksWithActionExtension__When__Called()
        {
            IObservable<int> observable = new Subject<int>();
            var received = -1;
            observable.Subscribe(v => received = v);
            ((Subject<int>)observable).OnNext(7);
            received.Should().Be(7);
        }
    }
}
