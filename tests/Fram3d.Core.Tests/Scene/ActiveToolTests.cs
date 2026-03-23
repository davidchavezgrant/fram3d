using FluentAssertions;
using Fram3d.Core.Scene;
using Xunit;
namespace Fram3d.Core.Tests.Scene
{
    public sealed class ActiveToolTests
    {
        [Fact]
        public void Rotate__HasCorrectProperties__When__Accessed()
        {
            ActiveTool.ROTATE.Name.Should().Be("Rotate");
            ActiveTool.ROTATE.Shortcut.Should().Be('E');
        }

        [Fact]
        public void Scale__HasCorrectProperties__When__Accessed()
        {
            ActiveTool.SCALE.Name.Should().Be("Scale");
            ActiveTool.SCALE.Shortcut.Should().Be('R');
        }

        [Fact]
        public void Select__HasCorrectProperties__When__Accessed()
        {
            ActiveTool.SELECT.Name.Should().Be("Select");
            ActiveTool.SELECT.Shortcut.Should().Be('Q');
        }

        [Fact]
        public void ToString__ReturnsName__When__Called()
        {
            ActiveTool.TRANSLATE.ToString().Should().Be("Translate");
        }

        [Fact]
        public void Translate__HasCorrectProperties__When__Accessed()
        {
            ActiveTool.TRANSLATE.Name.Should().Be("Translate");
            ActiveTool.TRANSLATE.Shortcut.Should().Be('W');
        }

        [Fact]
        public void AllInstances__AreDistinct__When__Compared()
        {
            var tools = new[] { ActiveTool.SELECT, ActiveTool.TRANSLATE, ActiveTool.ROTATE, ActiveTool.SCALE };

            for (var i = 0; i < tools.Length; i++)
            {
                for (var j = i + 1; j < tools.Length; j++)
                {
                    tools[i].Should().NotBeSameAs(tools[j]);
                }
            }
        }
    }
}
