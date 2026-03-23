using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Xunit;
namespace Fram3d.Core.Tests.Scene
{
    public sealed class GizmoStateTests
    {
        private static ElementId NewId() => new(Guid.NewGuid());

        [Fact]
        public void ActiveTool__DefaultsToTranslate__When__Constructed()
        {
            var state = new GizmoState();

            state.ActiveTool.Should().BeSameAs(ActiveTool.TRANSLATE);
        }

        [Fact]
        public void SetActiveTool__ChangesTool__When__Called()
        {
            var state = new GizmoState();

            state.SetActiveTool(ActiveTool.ROTATE);

            state.ActiveTool.Should().BeSameAs(ActiveTool.ROTATE);
        }

        [Fact]
        public void OnSelectionChanged__ResetsToTranslate__When__NewSelection()
        {
            var state = new GizmoState();
            state.SetActiveTool(ActiveTool.SCALE);

            var changed = state.OnSelectionChanged(NewId());

            changed.Should().BeTrue();
            state.ActiveTool.Should().BeSameAs(ActiveTool.TRANSLATE);
        }

        [Fact]
        public void OnSelectionChanged__ReturnsFalse__When__SameSelection()
        {
            var state = new GizmoState();
            var id    = NewId();
            state.OnSelectionChanged(id);
            state.SetActiveTool(ActiveTool.ROTATE);

            var changed = state.OnSelectionChanged(id);

            changed.Should().BeFalse();
            state.ActiveTool.Should().BeSameAs(ActiveTool.ROTATE);
        }

        [Fact]
        public void TryResetActiveTool__ResetsPosition__When__TranslateActive()
        {
            var state   = new GizmoState();
            var element = new Element(NewId(), "Test");
            element.Position = new Vector3(5, 5, 5);

            var result = state.TryResetActiveTool(element);

            result.Should().BeTrue();
            element.Position.Should().Be(Vector3.Zero);
        }

        [Fact]
        public void TryResetActiveTool__ResetsRotation__When__RotateActive()
        {
            var state   = new GizmoState();
            var element = new Element(NewId(), "Test");
            element.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1f);
            state.SetActiveTool(ActiveTool.ROTATE);

            state.TryResetActiveTool(element);

            element.Rotation.Should().Be(Quaternion.Identity);
        }

        [Fact]
        public void TryResetActiveTool__ResetsScale__When__ScaleActive()
        {
            var state   = new GizmoState();
            var element = new Element(NewId(), "Test");
            element.Scale = 2.5f;
            state.SetActiveTool(ActiveTool.SCALE);

            state.TryResetActiveTool(element);

            element.Scale.Should().Be(1f);
        }

        [Fact]
        public void TryResetActiveTool__ReturnsFalse__When__NullElement()
        {
            var state = new GizmoState();

            state.TryResetActiveTool(null).Should().BeFalse();
        }

        [Fact]
        public void TryResetActiveTool__ReturnsFalse__When__SelectToolActive()
        {
            var state   = new GizmoState();
            var element = new Element(NewId(), "Test");
            state.SetActiveTool(ActiveTool.SELECT);

            state.TryResetActiveTool(element).Should().BeFalse();
        }
    }
}
