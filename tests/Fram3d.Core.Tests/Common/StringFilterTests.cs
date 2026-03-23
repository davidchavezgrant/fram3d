using System.Collections.Generic;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;
namespace Fram3d.Core.Tests.Common
{
    public sealed class StringFilterTests
    {
        private static readonly List<string> ITEMS = new()
        {
            "ARRI ALEXA Mini",
            "ARRI ALEXA 35",
            "RED V-RAPTOR",
            "Sony VENICE 2",
            "Canon C500 Mark II"
        };

        [Fact]
        public void Match__ReturnsAll__When__QueryIsEmpty()
        {
            StringFilter.Match(ITEMS, "").Should().HaveCount(5);
        }

        [Fact]
        public void Match__ReturnsAll__When__QueryIsNull()
        {
            StringFilter.Match(ITEMS, null).Should().HaveCount(5);
        }

        [Fact]
        public void Match__ReturnsAll__When__QueryIsWhitespace()
        {
            StringFilter.Match(ITEMS, "   ").Should().HaveCount(5);
        }

        [Fact]
        public void Match__FiltersBySubstring__When__SingleWord()
        {
            var results = StringFilter.Match(ITEMS, "ARRI");

            results.Should().HaveCount(2);
            results.Should().Contain("ARRI ALEXA Mini");
            results.Should().Contain("ARRI ALEXA 35");
        }

        [Fact]
        public void Match__RequiresAllWords__When__MultipleWords()
        {
            var results = StringFilter.Match(ITEMS, "ARRI Mini");

            results.Should().HaveCount(1);
            results[0].Should().Be("ARRI ALEXA Mini");
        }

        [Fact]
        public void Match__IsCaseInsensitive__When__MixedCase()
        {
            var results = StringFilter.Match(ITEMS, "arri alexa");

            results.Should().HaveCount(2);
        }

        [Fact]
        public void Match__ReturnsEmpty__When__NoMatches()
        {
            StringFilter.Match(ITEMS, "Panavision").Should().BeEmpty();
        }

        [Fact]
        public void Match__ReturnsEmpty__When__NullItems()
        {
            StringFilter.Match(null, "test").Should().BeEmpty();
        }

        [Fact]
        public void Match__HandlesPartialWords__When__SubstringMatch()
        {
            var results = StringFilter.Match(ITEMS, "VEN");

            results.Should().HaveCount(1);
            results[0].Should().Be("Sony VENICE 2");
        }
    }
}
