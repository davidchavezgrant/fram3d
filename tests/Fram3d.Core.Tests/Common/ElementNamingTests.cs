using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;
namespace Fram3d.Core.Tests.Common
{
    public sealed class ElementNamingTests
    {
        [Fact]
        public void GenerateDuplicateName__ReturnsSuffix1__When__NoExistingDuplicates()
        {
            var result = ElementNaming.GenerateDuplicateName("Chair", new[] { "Chair" });
            result.Should().Be("Chair_1");
        }

        [Fact]
        public void GenerateDuplicateName__ReturnsNextSuffix__When__DuplicatesExist()
        {
            var result = ElementNaming.GenerateDuplicateName("Chair_1", new[] { "Chair", "Chair_1" });
            result.Should().Be("Chair_2");
        }

        [Fact]
        public void GenerateDuplicateName__SkipsGaps__When__SuffixesAreNonContiguous()
        {
            var result = ElementNaming.GenerateDuplicateName("Chair", new[] { "Chair", "Chair_3" });
            result.Should().Be("Chair_4");
        }

        [Fact]
        public void GenerateDuplicateName__ReturnsSuffix1__When__NoExistingNamesMatch()
        {
            var result = ElementNaming.GenerateDuplicateName("Chair", new[] { "Table", "Lamp" });
            result.Should().Be("Chair_1");
        }

        [Fact]
        public void GenerateDuplicateName__ReturnsSuffix1__When__ExistingNamesEmpty()
        {
            var result = ElementNaming.GenerateDuplicateName("Chair", new string[0]);
            result.Should().Be("Chair_1");
        }

        [Fact]
        public void GenerateDuplicateName__IncrementsFromSource__When__SourceHasSuffix()
        {
            var result = ElementNaming.GenerateDuplicateName("Chair_2", new[] { "Chair", "Chair_1", "Chair_2" });
            result.Should().Be("Chair_3");
        }

        [Fact]
        public void GenerateDuplicateName__PreservesUnderscoredBaseName__When__SuffixIsNotNumeric()
        {
            var result = ElementNaming.GenerateDuplicateName("Table_Top", new[] { "Table_Top" });
            result.Should().Be("Table_Top_1");
        }

        [Fact]
        public void GenerateDuplicateName__HandlesMultipleUnderscores__When__BaseNameHasUnderscores()
        {
            var result = ElementNaming.GenerateDuplicateName("Big_Red_Chair_3",
                                                              new[] { "Big_Red_Chair", "Big_Red_Chair_3" });
            result.Should().Be("Big_Red_Chair_4");
        }

        [Fact]
        public void GenerateDuplicateName__IgnoresDifferentBase__When__OtherFamilyHasHigherSuffix()
        {
            var result = ElementNaming.GenerateDuplicateName("Chair", new[] { "Chair", "Table_5" });
            result.Should().Be("Chair_1");
        }

        [Fact]
        public void GenerateDuplicateName__HandlesSequentialDuplication__When__CalledRepeatedly()
        {
            var existing = new[] { "Chair", "Chair_1", "Chair_2" };
            var result   = ElementNaming.GenerateDuplicateName("Chair_2", existing);
            result.Should().Be("Chair_3");
        }

        [Fact]
        public void ParseBaseName__ReturnsOriginal__When__NoSuffix()
        {
            ElementNaming.ParseBaseName("Chair").Should().Be("Chair");
        }

        [Fact]
        public void ParseBaseName__StripsNumericSuffix__When__SuffixIsNumber()
        {
            ElementNaming.ParseBaseName("Chair_2").Should().Be("Chair");
        }

        [Fact]
        public void ParseBaseName__PreservesName__When__SuffixIsNotNumeric()
        {
            ElementNaming.ParseBaseName("Table_Top").Should().Be("Table_Top");
        }

        [Fact]
        public void ParseBaseName__StripsOnlyLastSuffix__When__MultipleUnderscores()
        {
            ElementNaming.ParseBaseName("Big_Red_Chair_5").Should().Be("Big_Red_Chair");
        }
    }
}
