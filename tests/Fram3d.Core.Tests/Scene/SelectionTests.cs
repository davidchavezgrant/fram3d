using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Scenes;
using Xunit;
namespace Fram3d.Core.Tests.Scene
{
    public sealed class SelectionTests
    {
        private static ElementId NewId() => new(Guid.NewGuid());

        [Fact]
        public void ClearHover__RemovesHover__When__ElementIsHovered()
        {
            var selection = new Selection();
            var id        = NewId();
            selection.Hover(id);

            selection.ClearHover();

            selection.HoveredId.Should().BeNull();
        }

        [Fact]
        public void Deselect__ClearsSelection__When__ElementIsSelected()
        {
            var selection = new Selection();
            var id        = NewId();
            selection.Select(id);

            selection.Deselect();

            selection.SelectedId.Should().BeNull();
        }

        [Fact]
        public void Hover__ClearsHover__When__IdIsNull()
        {
            var selection = new Selection();
            selection.Hover(NewId());

            selection.Hover(null);

            selection.HoveredId.Should().BeNull();
        }

        [Fact]
        public void Hover__DoesNotHover__When__ElementIsSelected()
        {
            var selection = new Selection();
            var id        = NewId();
            selection.Select(id);

            selection.Hover(id);

            selection.HoveredId.Should().BeNull();
        }

        [Fact]
        public void Hover__SetsHoveredId__When__ElementIsNotSelected()
        {
            var selection = new Selection();
            var id        = NewId();

            selection.Hover(id);

            selection.HoveredId.Should().Be(id);
        }

        [Fact]
        public void Hover__SetsNewHover__When__DifferentElementIsSelected()
        {
            var selection = new Selection();
            var selected  = NewId();
            var hovered   = NewId();
            selection.Select(selected);

            selection.Hover(hovered);

            selection.HoveredId.Should().Be(hovered);
        }

        [Fact]
        public void Select__ClearsHover__When__HoveredElementIsSelected()
        {
            var selection = new Selection();
            var id        = NewId();
            selection.Hover(id);

            selection.Select(id);

            selection.HoveredId.Should().BeNull();
        }

        [Fact]
        public void Select__DeselectsPrevious__When__NewElementSelected()
        {
            var selection = new Selection();
            var first     = NewId();
            var second    = NewId();
            selection.Select(first);

            selection.Select(second);

            selection.SelectedId.Should().Be(second);
        }

        [Fact]
        public void Select__SetsSelectedId__When__Called()
        {
            var selection = new Selection();
            var id        = NewId();

            selection.Select(id);

            selection.SelectedId.Should().Be(id);
        }

        [Fact]
        public void Select__ClearsSelectedId__When__CalledWithNull()
        {
            var selection = new Selection();
            selection.Select(NewId());

            selection.Select(null);

            selection.SelectedId.Should().BeNull();
        }

        [Fact]
        public void Select__PreservesHover__When__CalledWithNull()
        {
            var selection = new Selection();
            var hovered   = NewId();
            selection.Select(NewId());
            selection.Hover(hovered);

            selection.Select(null);

            selection.HoveredId.Should().Be(hovered);
        }

        [Fact]
        public void Select__IsIdempotent__When__SameElementSelectedTwice()
        {
            var selection = new Selection();
            var id        = NewId();
            selection.Select(id);

            selection.Select(id);

            selection.SelectedId.Should().Be(id);
        }

        [Fact]
        public void Hover__ResumesAfterDeselect__When__PreviouslySupressed()
        {
            var selection = new Selection();
            var id        = NewId();
            selection.Select(id);
            selection.Hover(id); // suppressed

            selection.Deselect();
            selection.Hover(id); // should now work

            selection.HoveredId.Should().Be(id);
        }

        [Fact]
        public void Deselect__DoesNotClearHover__When__DifferentElementHovered()
        {
            var selection = new Selection();
            var selected  = NewId();
            var hovered   = NewId();
            selection.Select(selected);
            selection.Hover(hovered);

            selection.Deselect();

            selection.HoveredId.Should().Be(hovered);
        }

        [Fact]
        public void Selection__StartsEmpty__When__Created()
        {
            var selection = new Selection();

            selection.SelectedId.Should().BeNull();
            selection.HoveredId.Should().BeNull();
        }
    }
}
