using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public sealed class KeyframeRecorderTests
    {
        private static Shot MakeShot()
        {
            var shot = new Shot(new ShotId(Guid.NewGuid()), "Test");
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Vector3.Zero));
            shot.CameraRotationKeyframes.Add(
                new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Quaternion.Identity));
            return shot;
        }

        private static ElementTrack MakeTrack() =>
            new(new ElementId(Guid.NewGuid()));

        [Fact]
        public void RecordCamera__CreatesPositionKeyframe__When__StopwatchOnAndPositionChanged()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current  = new CameraSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = Quaternion.Identity };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            // Shot starts with 1 position keyframe at t=0, so now should have 2
            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraPositionKeyframes.Keyframes[1].Value.Should().Be(new Vector3(1f, 0f, 0f));
            shot.CameraPositionKeyframes.Keyframes[1].Time.Should().Be(time);
        }

        [Fact]
        public void RecordCamera__CreatesRotationKeyframe__When__StopwatchOnAndRotationChanged()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var rotated  = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current  = new CameraSnapshot { Position = Vector3.Zero, Rotation = rotated };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            // Shot starts with 1 rotation keyframe at t=0, so now should have 2
            shot.CameraRotationKeyframes.Count.Should().Be(2);
            shot.CameraRotationKeyframes.Keyframes[1].Value.Should().Be(rotated);
        }

        [Fact]
        public void RecordCamera__DoesNotRecord__When__StopwatchOff()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            // all off by default
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current  = new CameraSnapshot { Position = new Vector3(5f, 5f, 5f), Rotation = Quaternion.Identity };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            // Should still have only the initial mandatory keyframes
            shot.CameraPositionKeyframes.Count.Should().Be(1);
            shot.CameraRotationKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void RecordCamera__DoesNotRecord__When__ChangeBelowThreshold()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time = new TimePosition(1.0);
            // Position change below threshold (0.001)
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current  = new CameraSnapshot { Position = new Vector3(0.0005f, 0f, 0f), Rotation = Quaternion.Identity };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraPositionKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void RecordCamera__UpdatesExisting__When__WithinMergeWindow()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);

            // First record at t=1.0
            var time1    = new TimePosition(1.0);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current1 = new CameraSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = Quaternion.Identity };
            KeyframeRecorder.RecordCamera(shot, stopwatch, time1, current1, previous);

            // Second record within merge window (0.1s) of first
            var time2    = new TimePosition(1.05);
            var current2 = new CameraSnapshot { Position = new Vector3(2f, 0f, 0f), Rotation = Quaternion.Identity };
            KeyframeRecorder.RecordCamera(shot, stopwatch, time2, current2, previous);

            // Should merge — still 2 keyframes (initial + 1), not 3
            shot.CameraPositionKeyframes.Count.Should().Be(2);
            // The merged keyframe should have the updated value
            shot.CameraPositionKeyframes.Keyframes[1].Value.Should().Be(new Vector3(2f, 0f, 0f));
        }

        [Fact]
        public void RecordCamera__CreatesNew__When__OutsideMergeWindow()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);

            // First record at t=1.0
            var time1    = new TimePosition(1.0);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current1 = new CameraSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = Quaternion.Identity };
            KeyframeRecorder.RecordCamera(shot, stopwatch, time1, current1, previous);

            // Second record outside merge window (>0.1s away)
            var time2    = new TimePosition(2.0);
            var current2 = new CameraSnapshot { Position = new Vector3(2f, 0f, 0f), Rotation = Quaternion.Identity };
            KeyframeRecorder.RecordCamera(shot, stopwatch, time2, current2, previous);

            // Should have 3 keyframes: initial + 2 new
            shot.CameraPositionKeyframes.Count.Should().Be(3);
        }

        [Fact]
        public void RecordCamera__RecordsOnlyRotation__When__OnlyRotationChanged()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var rotated  = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current  = new CameraSnapshot { Position = Vector3.Zero, Rotation = rotated };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            // Position unchanged — should still be just the initial keyframe
            shot.CameraPositionKeyframes.Count.Should().Be(1);
            // Rotation changed — should have 2
            shot.CameraRotationKeyframes.Count.Should().Be(2);
            // Focal, focus, aperture unchanged — all 0
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
            shot.CameraApertureKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void RecordCamera__RecordsBothPositionAndFocal__When__DollyZoomChanges()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot
            {
                Position    = Vector3.Zero,
                Rotation    = Quaternion.Identity,
                FocalLength = 50f
            };
            var current = new CameraSnapshot
            {
                Position    = new Vector3(0f, 0f, 1f),
                Rotation    = Quaternion.Identity,
                FocalLength = 35f
            };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraFocalLengthKeyframes.Count.Should().Be(1);
            shot.CameraFocalLengthKeyframes.Keyframes[0].Value.Should().Be(35f);
        }

        [Fact]
        public void RecordCamera__SkipsProperty__When__PropertyStopwatchOff()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            // Only enable position, leave rotation off
            stopwatch.Set(CameraProperty.POSITION.Index, true);
            var time     = new TimePosition(1.0);
            var rotated  = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current  = new CameraSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = rotated };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            // Position should be recorded
            shot.CameraPositionKeyframes.Count.Should().Be(2);
            // Rotation stopwatch is off — should not record
            shot.CameraRotationKeyframes.Count.Should().Be(1);
        }

        // --- RecordElement tests ---

        [Fact]
        public void RecordElement__CreatesPositionKeyframe__When__PositionChanged()
        {
            var track     = MakeTrack();
            var stopwatch = new StopwatchState(ElementProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1f };
            var current  = new ElementSnapshot { Position = new Vector3(2f, 0f, 0f), Rotation = Quaternion.Identity, Scale = 1f };

            KeyframeRecorder.RecordElement(track, stopwatch, time, current, previous);

            track.PositionKeyframes.Count.Should().Be(1);
            track.PositionKeyframes.Keyframes[0].Value.Should().Be(new Vector3(2f, 0f, 0f));
        }

        [Fact]
        public void RecordElement__DoesNotRecord__When__StopwatchOff()
        {
            var track     = MakeTrack();
            var stopwatch = new StopwatchState(ElementProperty.COUNT);
            // all off by default
            var time     = new TimePosition(1.0);
            var previous = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1f };
            var current  = new ElementSnapshot { Position = new Vector3(5f, 5f, 5f), Rotation = Quaternion.Identity, Scale = 2f };

            KeyframeRecorder.RecordElement(track, stopwatch, time, current, previous);

            track.PositionKeyframes.Count.Should().Be(0);
            track.RotationKeyframes.Count.Should().Be(0);
            track.ScaleKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void RecordElement__UpdatesExisting__When__WithinMergeWindow()
        {
            var track     = MakeTrack();
            var stopwatch = new StopwatchState(ElementProperty.COUNT);
            stopwatch.SetAll(true);

            var time1    = new TimePosition(1.0);
            var previous = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1f };
            var current1 = new ElementSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = Quaternion.Identity, Scale = 1f };
            KeyframeRecorder.RecordElement(track, stopwatch, time1, current1, previous);

            var time2    = new TimePosition(1.05);
            var current2 = new ElementSnapshot { Position = new Vector3(3f, 0f, 0f), Rotation = Quaternion.Identity, Scale = 1f };
            KeyframeRecorder.RecordElement(track, stopwatch, time2, current2, previous);

            // Should merge — still 1 keyframe, not 2
            track.PositionKeyframes.Count.Should().Be(1);
            track.PositionKeyframes.Keyframes[0].Value.Should().Be(new Vector3(3f, 0f, 0f));
        }

        // --- ForceRecord tests ---

        [Fact]
        public void ForceRecordCamera__RecordsAll__When__Called()
        {
            var shot = MakeShot();
            var time = new TimePosition(1.0);
            var snap = new CameraSnapshot
            {
                Position     = new Vector3(1f, 2f, 3f),
                Rotation     = Quaternion.Identity,
                FocalLength  = 50f,
                FocusDistance = 5f,
                Aperture     = 2.8f
            };

            KeyframeRecorder.ForceRecordCamera(shot, time, snap);

            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraRotationKeyframes.Count.Should().Be(2);
            shot.CameraFocalLengthKeyframes.Count.Should().Be(1);
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(1);
            shot.CameraApertureKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void ForceRecordElement__RecordsAll__When__Called()
        {
            var track = MakeTrack();
            var time  = new TimePosition(1.0);
            var snap  = new ElementSnapshot
            {
                Position = new Vector3(1f, 2f, 3f),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f),
                Scale    = 2f
            };

            KeyframeRecorder.ForceRecordElement(track, time, snap);

            track.PositionKeyframes.Count.Should().Be(1);
            track.RotationKeyframes.Count.Should().Be(1);
            track.ScaleKeyframes.Count.Should().Be(1);
        }
        // --- Auto-record on enable (ForceRecord at t=0) ---

        [Fact]
        public void ForceRecordCamera__MergesWithInitial__When__AtTimeZero()
        {
            var shot = MakeShot();
            var snap = new CameraSnapshot
            {
                Position     = new Vector3(5f, 3f, -2f),
                Rotation     = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.3f),
                FocalLength  = 35f,
                FocusDistance = 2f,
                Aperture     = 4f
            };

            KeyframeRecorder.ForceRecordCamera(shot, TimePosition.ZERO, snap);

            // Should merge with the mandatory initial keyframes, not create duplicates
            shot.CameraPositionKeyframes.Count.Should().Be(1);
            shot.CameraPositionKeyframes.Keyframes[0].Value.Should().Be(new Vector3(5f, 3f, -2f));
            shot.CameraRotationKeyframes.Count.Should().Be(1);
            shot.CameraFocalLengthKeyframes.Count.Should().Be(1);
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(1);
            shot.CameraApertureKeyframes.Count.Should().Be(1);
        }

        // --- Camera gizmo drag (only position/rotation change) ---

        [Fact]
        public void RecordCamera__RecordsOnlyPosition__When__GizmoDragMovesPosition()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(2.0);
            var previous = new CameraSnapshot
            {
                Position     = Vector3.Zero,
                Rotation     = Quaternion.Identity,
                FocalLength  = 50f,
                FocusDistance = 10f,
                Aperture     = 5.6f
            };
            var current = new CameraSnapshot
            {
                Position     = new Vector3(3f, 1f, -2f),
                Rotation     = Quaternion.Identity,
                FocalLength  = 50f,
                FocusDistance = 10f,
                Aperture     = 5.6f
            };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraRotationKeyframes.Count.Should().Be(1); // unchanged
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
            shot.CameraApertureKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void RecordCamera__RecordsPositionAndRotation__When__GizmoDragMovesBoth()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(2.0);
            var rotated  = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f);
            var previous = new CameraSnapshot
            {
                Position     = Vector3.Zero,
                Rotation     = Quaternion.Identity,
                FocalLength  = 50f,
                FocusDistance = 10f,
                Aperture     = 5.6f
            };
            var current = new CameraSnapshot
            {
                Position     = new Vector3(3f, 1f, -2f),
                Rotation     = rotated,
                FocalLength  = 50f,
                FocusDistance = 10f,
                Aperture     = 5.6f
            };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraRotationKeyframes.Count.Should().Be(2);
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
        }

        // --- ClearAllCameraKeyframes preserves stopwatch ---

        [Fact]
        public void ClearAllCameraKeyframes__PreservesStopwatch__When__Called()
        {
            var shot = MakeShot();
            shot.CameraStopwatch.SetAll(true);

            shot.ClearAllCameraKeyframes();

            shot.CameraPositionKeyframes.Count.Should().Be(0);
            shot.CameraRotationKeyframes.Count.Should().Be(0);
            shot.CameraStopwatch.AllRecording.Should().BeTrue();
        }

        // --- Focus distance and aperture recording ---

        [Fact]
        public void RecordCamera__RecordsFocusDistance__When__Changed()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot { FocusDistance = 5f };
            var current  = new CameraSnapshot { FocusDistance = 12f };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraFocusDistanceKeyframes.Count.Should().Be(1);
            shot.CameraFocusDistanceKeyframes.Keyframes[0].Value.Should().Be(12f);
        }

        [Fact]
        public void RecordCamera__RecordsAperture__When__Changed()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot { Aperture = 2.8f };
            var current  = new CameraSnapshot { Aperture = 5.6f };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraApertureKeyframes.Count.Should().Be(1);
            shot.CameraApertureKeyframes.Keyframes[0].Value.Should().Be(5.6f);
        }

        // --- Element scale recording ---

        [Fact]
        public void RecordElement__RecordsScale__When__ScaleChanged()
        {
            var track     = MakeTrack();
            var stopwatch = new StopwatchState(ElementProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1f };
            var current  = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 2.5f };

            KeyframeRecorder.RecordElement(track, stopwatch, time, current, previous);

            track.ScaleKeyframes.Count.Should().Be(1);
            track.ScaleKeyframes.Keyframes[0].Value.Should().Be(2.5f);
            track.PositionKeyframes.Count.Should().Be(0); // unchanged
            track.RotationKeyframes.Count.Should().Be(0); // unchanged
        }

        [Fact]
        public void RecordElement__DoesNotRecordScale__When__ChangeBelowThreshold()
        {
            var track     = MakeTrack();
            var stopwatch = new StopwatchState(ElementProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1f };
            var current  = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1.0005f };

            KeyframeRecorder.RecordElement(track, stopwatch, time, current, previous);

            track.ScaleKeyframes.Count.Should().Be(0);
        }

        // --- Below-threshold rejection for each property type ---

        [Fact]
        public void RecordCamera__DoesNotRecordRotation__When__RotationBelowThreshold()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time = new TimePosition(1.0);
            // Rotation change below threshold (0.01 degrees ~ 0.000175 radians)
            var tinyRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.005f * MathF.PI / 180f);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current  = new CameraSnapshot { Position = Vector3.Zero, Rotation = tinyRotation };

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraRotationKeyframes.Count.Should().Be(1); // only the initial
        }

        [Fact]
        public void RecordCamera__DoesNotRecordFocalLength__When__ChangeBelowThreshold()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot { FocalLength = 50f };
            var current  = new CameraSnapshot { FocalLength = 50.005f }; // below 0.01

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void RecordCamera__DoesNotRecordFocusDistance__When__ChangeBelowThreshold()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot { FocusDistance = 5f };
            var current  = new CameraSnapshot { FocusDistance = 5.0005f }; // below 0.001

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void RecordCamera__DoesNotRecordAperture__When__ChangeBelowThreshold()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time     = new TimePosition(1.0);
            var previous = new CameraSnapshot { Aperture = 2.8f };
            var current  = new CameraSnapshot { Aperture = 2.805f }; // below threshold

            KeyframeRecorder.RecordCamera(shot, stopwatch, time, current, previous);

            shot.CameraApertureKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void RecordElement__DoesNotRecordRotation__When__RotationBelowThreshold()
        {
            var track     = MakeTrack();
            var stopwatch = new StopwatchState(ElementProperty.COUNT);
            stopwatch.SetAll(true);
            var time = new TimePosition(1.0);
            var tinyRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.005f * MathF.PI / 180f);
            var previous = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1f };
            var current  = new ElementSnapshot { Position = Vector3.Zero, Rotation = tinyRotation, Scale = 1f };

            KeyframeRecorder.RecordElement(track, stopwatch, time, current, previous);

            track.RotationKeyframes.Count.Should().Be(0);
        }

        // --- Merge window boundary ---

        [Fact]
        public void RecordCamera__CreatesNew__When__ExactlyAtMergeWindowBoundary()
        {
            var shot      = MakeShot();
            var stopwatch = new StopwatchState(CameraProperty.COUNT);
            stopwatch.SetAll(true);
            var time1    = new TimePosition(1.0);
            var previous = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var current1 = new CameraSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = Quaternion.Identity };
            KeyframeRecorder.RecordCamera(shot, stopwatch, time1, current1, previous);

            // Exactly 0.1s away — should create new, not merge
            var time2    = new TimePosition(1.1);
            var current2 = new CameraSnapshot { Position = new Vector3(3f, 0f, 0f), Rotation = Quaternion.Identity };
            KeyframeRecorder.RecordCamera(shot, stopwatch, time2, current2, previous);

            // Initial + 2 new = 3
            shot.CameraPositionKeyframes.Count.Should().Be(3);
        }
    }
}
