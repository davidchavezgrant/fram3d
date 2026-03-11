// Fram3d — Global Timeline Visualization Mockup
// All features gated by feat() from features.js

// ── Data model ──
const SCENE = {
  fps: 24,

  shots: [
    {
      name: 'Wide Establishing', start: 0, end: 6, color: '#9a5555',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 0 }, { time: 2.5 }, { time: 5.5 }] },
        { name: 'Cam B', keyframes: [{ time: 1 }, { time: 4 }] },
        { name: 'Cam C', keyframes: [{ time: 0.5 }, { time: 3 }, { time: 5 }] },
        { name: 'Cam D', keyframes: [{ time: 2 }, { time: 4.5 }] },
      ],
      coverage: [
        { camera: 0, start: 0, end: 2 },
        { camera: 1, start: 2, end: 4 },
        { camera: 0, start: 4, end: 6 },
      ]
    },
    {
      name: 'Over Shoulder', start: 6, end: 10, color: '#5577aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 6 }, { time: 8 }, { time: 9.5 }] },
        { name: 'Cam B', keyframes: [{ time: 7 }, { time: 9 }] },
      ],
      coverage: [
        { camera: 0, start: 6, end: 8 },
        { camera: 1, start: 8, end: 10 },
      ]
    },
    {
      name: 'Close-up Reaction', start: 10, end: 18, color: '#aa8844',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 10 }, { time: 13 }, { time: 17 }] },
      ],
      coverage: [{ camera: 0, start: 10, end: 18 }]
    },
    {
      name: 'Tracking', start: 18, end: 23, color: '#8855aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 18 }, { time: 20 }, { time: 22.5 }] },
        { name: 'Cam B', keyframes: [{ time: 19 }, { time: 21 }] },
        { name: 'Cam C', keyframes: [{ time: 18.5 }, { time: 22 }] },
      ],
      coverage: [
        { camera: 0, start: 18, end: 20 },
        { camera: 2, start: 20, end: 21.5 },
        { camera: 1, start: 21.5, end: 23 },
      ]
    },
    {
      name: 'Two-Shot', start: 23, end: 30, color: '#aa5577',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 23 }, { time: 26 }, { time: 29 }] },
        { name: 'Cam B', keyframes: [{ time: 24.5 }, { time: 28 }] },
      ],
      coverage: [
        { camera: 0, start: 23, end: 26 },
        { camera: 1, start: 26, end: 30 },
      ]
    },
    {
      name: 'Insert Detail', start: 30, end: 33, color: '#557799',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 30 }, { time: 32 }] },
      ],
      coverage: [{ camera: 0, start: 30, end: 33 }]
    },
    {
      name: 'Dolly In', start: 33, end: 39, color: '#aa7744',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 33 }, { time: 35 }, { time: 38 }] },
        { name: 'Cam B', keyframes: [{ time: 34 }, { time: 37 }] },
        { name: 'Cam C', keyframes: [{ time: 33.5 }, { time: 36 }, { time: 38.5 }] },
        { name: 'Cam D', keyframes: [{ time: 34.5 }, { time: 37.5 }] },
      ],
      coverage: [
        { camera: 0, start: 33, end: 35 },
        { camera: 2, start: 35, end: 37 },
        { camera: 1, start: 37, end: 39 },
      ]
    },
    {
      name: 'Reverse Angle', start: 39, end: 43, color: '#7755aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 39 }, { time: 41 }, { time: 42.5 }] },
        { name: 'Cam B', keyframes: [{ time: 40 }, { time: 42 }] },
      ],
      coverage: [
        { camera: 0, start: 39, end: 41 },
        { camera: 1, start: 41, end: 43 },
      ]
    },
    {
      name: 'Steadicam Walk', start: 43, end: 53, color: '#996655',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 43 }, { time: 46 }, { time: 50 }, { time: 52 }] },
      ],
      coverage: [{ camera: 0, start: 43, end: 53 }]
    },
    {
      name: 'High Angle', start: 53, end: 58, color: '#5588aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 53 }, { time: 55 }, { time: 57 }] },
        { name: 'Cam B', keyframes: [{ time: 54 }, { time: 56.5 }] },
        { name: 'Cam C', keyframes: [{ time: 53.5 }, { time: 57.5 }] },
      ],
      coverage: [
        { camera: 0, start: 53, end: 55 },
        { camera: 1, start: 55, end: 56.5 },
        { camera: 2, start: 56.5, end: 58 },
      ]
    },
    {
      name: 'POV', start: 58, end: 65, color: '#aa6655',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 58 }, { time: 61 }, { time: 64 }] },
        { name: 'Cam B', keyframes: [{ time: 59.5 }, { time: 63 }] },
      ],
      coverage: [
        { camera: 0, start: 58, end: 61 },
        { camera: 1, start: 61, end: 65 },
      ]
    },
    {
      name: 'Whip Pan', start: 65, end: 68, color: '#6677aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 65 }, { time: 67 }] },
      ],
      coverage: [{ camera: 0, start: 65, end: 68 }]
    },
    {
      name: 'Push In', start: 68, end: 76, color: '#aa5555',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 68 }, { time: 71 }, { time: 75 }] },
        { name: 'Cam B', keyframes: [{ time: 69.5 }, { time: 73 }] },
      ],
      coverage: [
        { camera: 0, start: 68, end: 72 },
        { camera: 1, start: 72, end: 76 },
      ]
    },
    {
      name: 'Master Wide', start: 76, end: 82, color: '#558899',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 76 }, { time: 78 }, { time: 81 }] },
        { name: 'Cam B', keyframes: [{ time: 77 }, { time: 80 }] },
        { name: 'Cam C', keyframes: [{ time: 76.5 }, { time: 79 }, { time: 81.5 }] },
      ],
      coverage: [
        { camera: 0, start: 76, end: 78 },
        { camera: 2, start: 78, end: 80 },
        { camera: 1, start: 80, end: 82 },
      ]
    },
    {
      name: 'Final Close-up', start: 82, end: 90, color: '#8866aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 82 }, { time: 85 }, { time: 89 }] },
      ],
      coverage: [{ camera: 0, start: 82, end: 90 }]
    },
  ],

  tracks: [
    {
      name: 'Character_A', color: '#4a9a4a',
      keyframes: [
        { time: 0 }, { time: 3 }, { time: 7 }, { time: 12 }, { time: 17 },
        { time: 22 }, { time: 27 }, { time: 35 }, { time: 42 }, { time: 48 },
        { time: 55 }, { time: 62 }, { time: 70 }, { time: 78 }, { time: 86 },
      ],
      linkedPeriods: []
    },
    {
      name: 'Character_B', color: '#3a8a5a',
      keyframes: [
        { time: 0 }, { time: 5 }, { time: 10 }, { time: 18 }, { time: 25 },
        { time: 33 }, { time: 45 }, { time: 58 }, { time: 68 }, { time: 80 },
      ],
      linkedPeriods: []
    },
    {
      name: 'Sword', color: '#5a9a3a',
      keyframes: [
        { time: 0 }, { time: 2 }, { time: 13 }, { time: 15.5 }, { time: 17 },
        { time: 40 }, { time: 55 }, { time: 70 }, { time: 85 },
      ],
      linkedPeriods: [
        { start: 4, end: 11, parent: 'Character_A hand' }
      ]
    },
    {
      name: 'Table', color: '#4a7a6a',
      keyframes: [{ time: 0 }],
      linkedPeriods: []
    },
    {
      name: 'Key_Light', color: '#6a8a4a',
      keyframes: [
        { time: 0 }, { time: 18 }, { time: 43 }, { time: 65 }, { time: 82 },
      ],
      linkedPeriods: []
    },
  ]
};

SCENE.totalDuration = SCENE.shots[SCENE.shots.length - 1].end;

// ── Constants ──
const MAX_CAMERAS = Math.max(...SCENE.shots.map(s => s.cameras.length));
const CAM_ROW_HEIGHT = 20;
const TRACK_ROW_HEIGHT = 28;
const SUB_TRACK_HEIGHT = 22;

// Camera property sub-tracks
const CAMERA_PROPS = ['Position X', 'Position Y', 'Position Z', 'Pan', 'Tilt', 'Roll', 'Focal Length'];
// Object property sub-tracks
const OBJECT_PROPS = ['Position X', 'Position Y', 'Position Z', 'Scale', 'Rotation X', 'Rotation Y', 'Rotation Z'];

// Interpolation curve names
const INTERP_CURVES = ['linear', 'ease-in', 'ease-out', 'ease-in-out', 'bezier'];
const INTERP_SYMBOLS = { 'linear': '─', 'ease-in': '⌒', 'ease-out': '⌓', 'ease-in-out': '~', 'bezier': '∿' };

// Tool modes (1.3.2)
const TOOL_MODES = [
  { key: 'Q', label: 'SELECT', icon: '◇' },
  { key: 'W', label: 'MOVE', icon: '✥' },
  { key: 'E', label: 'ROTATE', icon: '↻' },
  { key: 'R', label: 'SCALE', icon: '⬡' },
];

// Aspect ratios (1.2.1)
const ASPECT_RATIOS = [
  { name: '16:9', ratio: 16 / 9 },
  { name: '2.39:1', ratio: 2.39 },
  { name: '2.35:1', ratio: 2.35 },
  { name: '1.85:1', ratio: 1.85 },
  { name: '4:3', ratio: 4 / 3 },
  { name: '1:1', ratio: 1 },
  { name: '9:16', ratio: 9 / 16 },
  { name: 'None', ratio: null },
];

// ── State ──
let playhead = 0;
let viewStart = 0;
let viewEnd = SCENE.totalDuration;
let playing = false;
let selectedKeyframe = null;
let animFrame = null;

// Per-shot active camera
const activeCameraPerShot = {};
SCENE.shots.forEach((_, i) => { activeCameraPerShot[i] = 0; });

// Preview camera
let previewCamera = null;

// Shot bar double-click detection
let shotBarLastClick = { shotIndex: -1, camIndex: -1, time: 0 };

// Shot boundary drag (ripple)
let boundaryDragging = false;
let boundaryIndex = -1;
let boundarySnapshot = null;

// Track expand/collapse state (1.5.2)
const trackExpanded = {}; // key: 'cam' or 'obj-N' → boolean

// Tool mode state (1.3.2)
let currentToolMode = 0; // index into TOOL_MODES

// Director view state (1.3.5)
let directorView = false;

// Camera path state (1.5.6)
let cameraPathVisible = false;

// Aspect ratio state (1.2.1)
let currentAspectIndex = 0; // 16:9 default

// Frame guides state (1.2.2)
let guidesVisible = false;

// Keyframe drag state (1.5.3)
let kfDragging = false;
let kfDragInfo = null; // { type, trackIndex, kfIndex, startX, origTime }

// Scene state (1.4.4)
let currentScene = 0;
const SCENES = ['Scene 1', 'Scene 2', 'Scene 3'];

// ── DOM refs ──
const shotBar = document.getElementById('shot-bar');
const ruler = document.getElementById('ruler');
const trackLabels = document.getElementById('track-labels');
const trackArea = document.getElementById('track-area');
const zoomBar = document.getElementById('zoom-bar');
const zoomThumb = document.getElementById('zoom-thumb');
const zoomPlayheadIndicator = document.getElementById('zoom-playhead-indicator');
const playheadEl = document.getElementById('playhead');
const playheadRuler = document.getElementById('playhead-ruler');
const playheadShotbar = document.getElementById('playhead-shotbar');
const viewportShotLabel = document.getElementById('viewport-shot-label');
const viewportTimecode = document.getElementById('viewport-timecode');
const minimapEl = document.getElementById('minimap');
const resizeLabel = document.getElementById('resize-label');
const shotTooltip = document.getElementById('shot-tooltip');

// ── Init shot bar height ──
document.getElementById('shot-bar-container').style.height = (MAX_CAMERAS * CAM_ROW_HEIGHT + 2) + 'px';

// ── Feature visibility ──
function applyFeatureVisibility() {
  // Map feature IDs to CSS milestone classes
  const milestoneMap = {
    '1.1': 'feature-1-1',
    '1.2': 'feature-1-2',
    '1.3': 'feature-1-3',
    '1.4': 'feature-1-4',
    '1.5': 'feature-1-5',
    '1.6': 'feature-1-6',
    '3.3': 'feature-3-3',
    '3.5': 'feature-3-5',
    '3.6': 'feature-3-6',
  };

  // Check each milestone — show its elements only if ANY feature in that milestone is enabled
  for (const [milestone, cls] of Object.entries(milestoneMap)) {
    const anyEnabled = Object.values(FEATURES).some(f => f.milestone === milestone && f.enabled);
    document.querySelectorAll('.' + cls).forEach(el => {
      el.style.display = anyEnabled ? '' : 'none';
    });
  }

  // Fine-grained per-feature visibility for specific elements
  const featureElements = {
    'hud': ['viewport-hud'],
    'aspect-ratio': ['aspect-mask-top', 'aspect-mask-bottom', 'aspect-mask-left', 'aspect-mask-right'],
    'frame-guides': ['viewport-guides'],
    'tool-mode': ['tool-mode-badge'],
    'director-view': ['director-badge'],
    'shot-management': ['shot-mgmt-buttons'],
    'aggregate-duration': ['aggregate-duration'],
    'scenes': ['scenes-switcher'],
    'transport-bar': ['transport-bar'],
    'status-bar': ['status-bar'],
    'camera-path': ['camera-path-badge'],
    'coverage-track': ['coverage-container'],
  };

  for (const [featId, elIds] of Object.entries(featureElements)) {
    const show = feat(featId);
    elIds.forEach(id => {
      const el = document.getElementById(id);
      if (el) el.style.display = show ? '' : 'none';
    });
  }

  // Director badge only shows if feature enabled AND director view is active
  const dirBadge = document.getElementById('director-badge');
  if (dirBadge) dirBadge.style.display = (feat('director-view') && directorView) ? '' : 'none';

  // Camera path badge only shows if feature enabled AND path is visible
  const pathBadge = document.getElementById('camera-path-badge');
  if (pathBadge) pathBadge.style.display = (feat('camera-path') && cameraPathVisible) ? '' : 'none';

  // Frame guides only if visible
  const guidesEl = document.getElementById('viewport-guides');
  if (guidesEl) guidesEl.style.display = (feat('frame-guides') && guidesVisible) ? '' : 'none';

  // Aspect masks only if not "None"
  if (feat('aspect-ratio') && ASPECT_RATIOS[currentAspectIndex].ratio !== null) {
    updateAspectMasks();
  } else {
    ['aspect-mask-top', 'aspect-mask-bottom', 'aspect-mask-left', 'aspect-mask-right'].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.style.display = 'none';
    });
  }
}

function updateAspectMasks() {
  const viewport = document.getElementById('viewport');
  if (!viewport) return;
  const ar = ASPECT_RATIOS[currentAspectIndex];
  if (!ar.ratio) return;

  const vw = viewport.clientWidth;
  const vh = viewport.clientHeight;
  const viewportAR = vw / vh;

  const top = document.getElementById('aspect-mask-top');
  const bottom = document.getElementById('aspect-mask-bottom');
  const left = document.getElementById('aspect-mask-left');
  const right = document.getElementById('aspect-mask-right');

  if (ar.ratio < viewportAR) {
    // Pillarbox (vertical bars on sides)
    const contentWidth = vh * ar.ratio;
    const barWidth = (vw - contentWidth) / 2;
    top.style.display = 'none';
    bottom.style.display = 'none';
    left.style.display = '';
    right.style.display = '';
    left.style.width = barWidth + 'px';
    right.style.width = barWidth + 'px';
  } else {
    // Letterbox (horizontal bars top/bottom)
    const contentHeight = vw / ar.ratio;
    const barHeight = (vh - contentHeight) / 2;
    left.style.display = 'none';
    right.style.display = 'none';
    top.style.display = '';
    bottom.style.display = '';
    top.style.height = barHeight + 'px';
    bottom.style.height = barHeight + 'px';
  }

  // Update HUD ratio display
  const hudRatio = document.getElementById('hud-ratio');
  if (hudRatio) hudRatio.textContent = ar.name;
}

// ── Helpers ──

function timeToX(t) {
  const w = trackArea.clientWidth;
  return ((t - viewStart) / (viewEnd - viewStart)) * w;
}

function xToTime(x) {
  const w = trackArea.clientWidth;
  return viewStart + (x / w) * (viewEnd - viewStart);
}

function formatTimecode(t) {
  const totalFrames = Math.max(0, Math.round(t * SCENE.fps));
  const ff = totalFrames % 24;
  const totalSeconds = Math.floor(totalFrames / 24);
  const ss = totalSeconds % 60;
  const mm = Math.floor(totalSeconds / 60) % 60;
  const hh = Math.floor(totalSeconds / 3600);
  return `${String(hh).padStart(2, '0')};${String(mm).padStart(2, '0')};${String(ss).padStart(2, '0')};${String(ff).padStart(2, '0')}`;
}

function formatRulerTime(t) {
  const totalFrames = Math.max(0, Math.round(t * SCENE.fps));
  const ff = totalFrames % 24;
  const totalSeconds = Math.floor(totalFrames / 24);
  const ss = totalSeconds % 60;
  const mm = Math.floor(totalSeconds / 60);
  const duration = viewEnd - viewStart;
  if (duration < 3) {
    return `${mm};${String(ss).padStart(2, '0')};${String(ff).padStart(2, '0')}`;
  }
  return `${mm};${String(ss).padStart(2, '0')}`;
}

function getCurrentShot() {
  for (let i = 0; i < SCENE.shots.length; i++) {
    if (playhead >= SCENE.shots[i].start && playhead < SCENE.shots[i].end) return i;
  }
  return SCENE.shots.length - 1;
}

function adjustAlpha(hex, factor) {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return `rgba(${r},${g},${b},${factor})`;
}

function clampView() {
  const duration = viewEnd - viewStart;
  const clampedDuration = Math.min(SCENE.totalDuration, Math.max(0.5, duration));
  viewStart = Math.max(0, Math.min(SCENE.totalDuration - clampedDuration, viewStart));
  viewEnd = viewStart + clampedDuration;
}

function zoomAtPosition(mouseTimeFraction, deltaY) {
  const duration = viewEnd - viewStart;
  const factor = deltaY > 0 ? 1.15 : 0.87;
  const newDuration = Math.min(SCENE.totalDuration, Math.max(0.5, duration * factor));
  const mouseTime = viewStart + mouseTimeFraction * duration;
  viewStart = mouseTime - mouseTimeFraction * newDuration;
  viewEnd = viewStart + newDuration;
  clampView();
  render();
}

function getDisplayCamera(shotIndex) {
  const shot = SCENE.shots[shotIndex];
  if (previewCamera && previewCamera.shotIndex === shotIndex) {
    return { cam: shot.cameras[previewCamera.cameraIndex], index: previewCamera.cameraIndex, isPreviewed: true };
  }
  const idx = activeCameraPerShot[shotIndex] || 0;
  return { cam: shot.cameras[idx], index: idx, isPreviewed: false };
}

// ── Render ──

function render() {
  renderShotBar();
  renderRuler();
  renderTracks();
  renderPlayhead();
  renderZoomBar();
  renderMinimap();
  renderViewportInfo();
  if (feat('transport-bar')) renderTransportBar();
  if (feat('status-bar')) renderStatusBar();
  if (feat('coverage-track')) renderCoverageTrack();
  if (feat('aggregate-duration')) renderAggregateDuration();
  if (feat('tool-mode')) renderToolMode();
}

function renderShotBar() {
  shotBar.querySelectorAll('.shot-cam, .shot-boundary-handle').forEach(el => el.remove());
  const currentShotIndex = getCurrentShot();

  if (previewCamera && previewCamera.shotIndex !== currentShotIndex) {
    previewCamera = null;
  }

  SCENE.shots.forEach((shot, si) => {
    const left = timeToX(shot.start);
    const width = timeToX(shot.end) - left;
    const isCurrentShot = si === currentShotIndex;
    const activeCamIdx = activeCameraPerShot[si] || 0;

    shot.cameras.forEach((cam, ci) => {
      const el = document.createElement('div');
      el.className = 'shot-cam';

      const isActive = isCurrentShot && ci === activeCamIdx;
      const isPreviewed = previewCamera && previewCamera.shotIndex === si && previewCamera.cameraIndex === ci;

      if (isActive) el.classList.add('active-cam');
      else if (isPreviewed) el.classList.add('previewed');
      else if (isCurrentShot) el.classList.add('dimmed');
      else el.classList.add('inactive');

      el.style.left = (left + 1) + 'px';
      el.style.width = Math.max(0, width - 3) + 'px';
      el.style.top = (ci * CAM_ROW_HEIGHT + 1) + 'px';
      el.style.height = (CAM_ROW_HEIGHT - 2) + 'px';
      el.style.background = shot.color;

      const letter = String.fromCharCode(65 + ci);
      el.textContent = `${letter}: ${shot.name}`;

      // Hover tooltip (1.4.2)
      if (feat('hover-thumbnails')) {
        el.addEventListener('mouseenter', (e) => {
          const dur = shot.end - shot.start;
          const frames = Math.round(dur * SCENE.fps);
          shotTooltip.innerHTML = `<div class="tooltip-name">${shot.name}</div><div class="tooltip-detail">${cam.name} · ${dur.toFixed(1)}s (${frames}f) · ${cam.keyframes.length} kf</div>`;
          shotTooltip.style.display = 'block';
          shotTooltip.style.left = (e.clientX + 12) + 'px';
          shotTooltip.style.top = (e.clientY - 30) + 'px';
        });
        el.addEventListener('mousemove', (e) => {
          shotTooltip.style.left = (e.clientX + 12) + 'px';
          shotTooltip.style.top = (e.clientY - 30) + 'px';
        });
        el.addEventListener('mouseleave', () => {
          shotTooltip.style.display = 'none';
        });
      }

      el.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        e.preventDefault();

        const now = Date.now();
        const isDouble = shotBarLastClick.shotIndex === si &&
          shotBarLastClick.camIndex === ci &&
          now - shotBarLastClick.time < 350;

        if (isDouble) {
          activeCameraPerShot[si] = ci;
          previewCamera = null;
          shotBarLastClick = { shotIndex: -1, camIndex: -1, time: 0 };
        } else {
          shotBarLastClick = { shotIndex: si, camIndex: ci, time: now };
          if (si !== currentShotIndex) playhead = shot.start;
          if (ci !== activeCamIdx || si !== currentShotIndex) {
            previewCamera = { shotIndex: si, cameraIndex: ci };
          } else {
            previewCamera = null;
          }
        }
        render();
      });

      shotBar.appendChild(el);
    });
  });

  // Boundary drag handles
  for (let i = 0; i < SCENE.shots.length - 1; i++) {
    const boundaryTime = SCENE.shots[i].end;
    const handle = document.createElement('div');
    handle.className = 'shot-boundary-handle';
    handle.style.left = timeToX(boundaryTime) + 'px';

    const idx = i;
    handle.addEventListener('mousedown', (e) => {
      e.stopPropagation();
      e.preventDefault();
      boundaryDragging = true;
      boundaryIndex = idx;
      boundarySnapshot = {
        shots: SCENE.shots.map(s => ({ start: s.start, end: s.end })),
        cameraTimes: SCENE.shots.map(s => s.cameras.map(c => c.keyframes.map(kf => kf.time))),
        objectTimes: SCENE.tracks.map(t => t.keyframes.map(kf => kf.time)),
        linkedPeriods: SCENE.tracks.map(t => (t.linkedPeriods || []).map(lp => ({ start: lp.start, end: lp.end }))),
        coverageTimes: SCENE.shots.map(s => (s.coverage || []).map(seg => ({ start: seg.start, end: seg.end }))),
        origBoundary: SCENE.shots[idx].end,
      };
    });
    shotBar.appendChild(handle);
  }

  document.getElementById('shot-bar-view-range').style.display = 'none';
  shotBar.appendChild(playheadShotbar);
  playheadShotbar.style.left = timeToX(playhead) + 'px';
}

function renderRuler() {
  ruler.querySelectorAll('.ruler-tick, .ruler-label').forEach(el => el.remove());

  const duration = viewEnd - viewStart;
  let interval;
  if (duration <= 2) interval = 1 / SCENE.fps;
  else if (duration <= 5) interval = 0.5;
  else if (duration <= 15) interval = 1;
  else if (duration <= 40) interval = 2;
  else if (duration <= 60) interval = 5;
  else interval = 10;

  const startTick = Math.ceil(viewStart / interval) * interval;
  for (let t = startTick; t <= viewEnd; t += interval) {
    const x = timeToX(t);
    if (x < -10 || x > ruler.clientWidth + 10) continue;

    const isMajor = interval >= 1
      ? Math.abs(t - Math.round(t / (interval >= 5 ? 10 : 5)) * (interval >= 5 ? 10 : 5)) < 0.01
      : Math.abs(t - Math.round(t)) < 0.01;

    const tick = document.createElement('div');
    tick.className = 'ruler-tick';
    tick.style.left = x + 'px';
    tick.style.height = isMajor ? '22px' : '10px';
    tick.style.bottom = '0';
    ruler.appendChild(tick);

    if (isMajor || interval >= 1) {
      const label = document.createElement('div');
      label.className = 'ruler-label';
      label.style.left = x + 'px';
      label.textContent = formatRulerTime(t);
      ruler.appendChild(label);
    }
  }

  playheadRuler.style.left = timeToX(playhead) + 'px';
}

function renderTracks() {
  trackLabels.innerHTML = '';
  trackArea.querySelectorAll(':not(#playhead)').forEach(el => el.remove());

  const currentShotIndex = getCurrentShot();
  const currentShot = SCENE.shots[currentShotIndex];
  const display = getDisplayCamera(currentShotIndex);
  const displayLetter = String.fromCharCode(65 + display.index);
  let yOffset = 0;

  // Camera track label
  const camLabel = document.createElement('div');
  camLabel.className = 'track-label camera-label';
  let camLabelHTML = '';
  if (feat('track-expand')) {
    const expanded = trackExpanded['cam'] || false;
    camLabelHTML += `<span class="expand-arrow ${expanded ? 'expanded' : ''}" data-track="cam">▶</span>`;
  }
  camLabelHTML += `<div class="dot" style="background:${currentShot.color}"></div><div class="name">Cam ${displayLetter}</div>`;
  camLabel.innerHTML = camLabelHTML;
  trackLabels.appendChild(camLabel);

  // Camera track row
  const camRow = document.createElement('div');
  camRow.className = 'track-row';
  camRow.style.top = yOffset + 'px';
  trackArea.appendChild(camRow);
  renderCameraRow(camRow, currentShotIndex);
  yOffset += TRACK_ROW_HEIGHT;

  // Camera sub-tracks (1.5.2)
  if (feat('track-expand') && trackExpanded['cam']) {
    CAMERA_PROPS.forEach((prop, pi) => {
      const subLabel = document.createElement('div');
      subLabel.className = 'track-label sub-track-label';
      subLabel.innerHTML = `<div class="name">${prop}</div>`;
      trackLabels.appendChild(subLabel);

      const subRow = document.createElement('div');
      subRow.className = 'track-row sub-track-row';
      subRow.style.top = yOffset + 'px';
      subRow.style.height = SUB_TRACK_HEIGHT + 'px';
      trackArea.appendChild(subRow);

      // Show a subset of the camera's keyframes for this property
      if (display.cam) {
        const shotKfs = display.cam.keyframes.filter(kf =>
          kf.time >= currentShot.start && kf.time <= currentShot.end
        );
        shotKfs.forEach((kf, ki) => {
          // Distribute keyframes across sub-tracks for visual variety
          if (ki % CAMERA_PROPS.length === pi || pi < 3) {
            const el = document.createElement('div');
            el.className = 'keyframe';
            el.style.left = timeToX(kf.time) + 'px';
            el.style.background = currentShot.color;
            el.style.width = '7px';
            el.style.height = '7px';
            el.title = `${prop} @ ${formatTimecode(kf.time)}`;
            subRow.appendChild(el);
          }
        });

        // Interpolation curve indicators (3.5.3)
        if (feat('interp-curves') && shotKfs.length > 1) {
          for (let k = 0; k < shotKfs.length - 1; k++) {
            if (k % CAMERA_PROPS.length === pi || pi < 3) {
              const x1 = timeToX(shotKfs[k].time);
              const x2 = timeToX(shotKfs[k + 1] ? shotKfs[k + 1].time : shotKfs[k].time);
              const midX = (x1 + x2) / 2;
              if (x2 - x1 > 30) {
                const curveType = INTERP_CURVES[(k + pi) % INTERP_CURVES.length];
                const ind = document.createElement('div');
                ind.className = 'interp-indicator';
                ind.style.left = midX + 'px';
                ind.textContent = INTERP_SYMBOLS[curveType];
                ind.title = curveType;
                subRow.appendChild(ind);
              }
            }
          }
        }
      }

      // Shot boundaries
      SCENE.shots.forEach(shot => {
        if (shot.start > 0) {
          const line = document.createElement('div');
          line.className = 'shot-boundary';
          line.style.left = timeToX(shot.start) + 'px';
          subRow.appendChild(line);
        }
      });

      yOffset += SUB_TRACK_HEIGHT;
    });
  }

  // Object tracks
  SCENE.tracks.forEach((track, ti) => {
    const label = document.createElement('div');
    label.className = 'track-label';
    let labelHTML = '';
    if (feat('track-expand')) {
      const expanded = trackExpanded['obj-' + ti] || false;
      labelHTML += `<span class="expand-arrow ${expanded ? 'expanded' : ''}" data-track="obj-${ti}">▶</span>`;
    }
    labelHTML += `<div class="dot" style="background:${track.color}"></div><div class="name">${track.name}</div>`;
    label.innerHTML = labelHTML;
    trackLabels.appendChild(label);

    const row = document.createElement('div');
    row.className = 'track-row';
    row.style.top = yOffset + 'px';
    trackArea.appendChild(row);
    renderObjectRow(row, ti, currentShotIndex);
    yOffset += TRACK_ROW_HEIGHT;

    // Object sub-tracks (1.5.2)
    if (feat('track-expand') && trackExpanded['obj-' + ti]) {
      OBJECT_PROPS.forEach((prop, pi) => {
        const subLabel = document.createElement('div');
        subLabel.className = 'track-label sub-track-label';
        subLabel.innerHTML = `<div class="name">${prop}</div>`;
        trackLabels.appendChild(subLabel);

        const subRow = document.createElement('div');
        subRow.className = 'track-row sub-track-row';
        subRow.style.top = yOffset + 'px';
        subRow.style.height = SUB_TRACK_HEIGHT + 'px';
        trackArea.appendChild(subRow);

        // Show some keyframes on sub-tracks
        track.keyframes.forEach((kf, ki) => {
          if (ki % OBJECT_PROPS.length === pi || pi < 3) {
            const el = document.createElement('div');
            el.className = 'keyframe';
            el.style.left = timeToX(kf.time) + 'px';
            el.style.background = track.color;
            el.style.width = '7px';
            el.style.height = '7px';
            el.title = `${prop} @ ${formatTimecode(kf.time)}`;
            subRow.appendChild(el);
          }
        });

        // Interpolation indicators (3.5.3)
        if (feat('interp-curves') && track.keyframes.length > 1) {
          for (let k = 0; k < track.keyframes.length - 1; k++) {
            if (k % OBJECT_PROPS.length === pi || pi < 3) {
              const x1 = timeToX(track.keyframes[k].time);
              const x2 = timeToX(track.keyframes[k + 1].time);
              const midX = (x1 + x2) / 2;
              if (x2 - x1 > 30) {
                const curveType = INTERP_CURVES[(k + pi) % INTERP_CURVES.length];
                const ind = document.createElement('div');
                ind.className = 'interp-indicator';
                ind.style.left = midX + 'px';
                ind.textContent = INTERP_SYMBOLS[curveType];
                ind.title = curveType;
                subRow.appendChild(ind);
              }
            }
          }
        }

        SCENE.shots.forEach(shot => {
          if (shot.start > 0) {
            const line = document.createElement('div');
            line.className = 'shot-boundary';
            line.style.left = timeToX(shot.start) + 'px';
            subRow.appendChild(line);
          }
        });

        yOffset += SUB_TRACK_HEIGHT;
      });
    }
  });

  trackArea.style.height = yOffset + 'px';
  trackLabels.style.height = yOffset + 'px';
}

function renderCameraRow(row, currentShotIndex) {
  const currentShot = SCENE.shots[currentShotIndex];
  const display = getDisplayCamera(currentShotIndex);

  // Shot background tinting
  SCENE.shots.forEach((shot, si) => {
    const bg = document.createElement('div');
    bg.className = 'shot-bg' + (si === currentShotIndex ? ' active-shot' : '');
    const left = timeToX(shot.start);
    bg.style.left = left + 'px';
    bg.style.width = (timeToX(shot.end) - left) + 'px';
    bg.style.background = shot.color;
    row.appendChild(bg);
  });

  // Collapsed blocks for non-current shots
  SCENE.shots.forEach((shot, si) => {
    if (si === currentShotIndex) return;
    const block = document.createElement('div');
    block.className = 'camera-block';
    const left = timeToX(shot.start);
    block.style.left = left + 'px';
    block.style.width = (timeToX(shot.end) - left) + 'px';
    block.style.background = shot.color;
    row.appendChild(block);
  });

  // Shot boundary lines
  SCENE.shots.forEach(shot => {
    if (shot.start > 0) {
      const line = document.createElement('div');
      line.className = 'shot-boundary';
      line.style.left = timeToX(shot.start) + 'px';
      row.appendChild(line);
    }
  });

  // Camera keyframes
  if (display.cam) {
    const kfOpacity = display.isPreviewed ? 0.55 : 1;
    display.cam.keyframes.forEach((kf, ki) => {
      if (kf.time < currentShot.start || kf.time > currentShot.end) return;
      const el = document.createElement('div');
      el.className = 'keyframe';
      el.style.left = timeToX(kf.time) + 'px';
      el.style.background = currentShot.color;
      el.style.opacity = kfOpacity;

      const key = `cam-${currentShotIndex}-${display.index}-${ki}`;
      if (selectedKeyframe === key) el.classList.add('selected');
      el.title = `${display.cam.name} @ ${formatTimecode(kf.time)}`;

      // Keyframe drag (1.5.3)
      el.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        if (feat('keyframe-drag')) {
          kfDragging = true;
          kfDragInfo = {
            type: 'cam', shotIndex: currentShotIndex, camIndex: display.index,
            kfIndex: ki, startX: e.clientX, origTime: kf.time
          };
          el.classList.add('dragging');
        }
        selectedKeyframe = (selectedKeyframe === key) ? null : key;
        playhead = kf.time;
        render();
      });
      row.appendChild(el);
    });
  }
}

function renderObjectRow(row, trackIndex, currentShotIndex) {
  const track = SCENE.tracks[trackIndex];

  // Shot boundary lines
  SCENE.shots.forEach(shot => {
    if (shot.start > 0) {
      const line = document.createElement('div');
      line.className = 'shot-boundary';
      line.style.left = timeToX(shot.start) + 'px';
      row.appendChild(line);
    }
  });

  // Linked periods
  if (track.linkedPeriods) {
    track.linkedPeriods.forEach(lp => {
      const x1 = timeToX(lp.start);
      const x2 = timeToX(lp.end);

      const region = document.createElement('div');
      region.className = 'linked-region';
      region.style.left = x1 + 'px';
      region.style.width = (x2 - x1) + 'px';
      region.title = `Linked to ${lp.parent}`;

      const linkLabel = document.createElement('div');
      linkLabel.className = 'link-label';
      if (x2 - x1 > 80) linkLabel.textContent = `\u2192 ${lp.parent}`;
      region.appendChild(linkLabel);
      row.appendChild(region);

      [lp.start, lp.end].forEach(t => {
        const kf = document.createElement('div');
        kf.className = 'keyframe link-boundary';
        kf.style.left = timeToX(t) + 'px';
        kf.title = `Link boundary @ ${formatTimecode(t)}`;
        row.appendChild(kf);
      });
    });
  }

  // Object keyframes
  track.keyframes.forEach((kf, ki) => {
    const el = document.createElement('div');
    el.className = 'keyframe';
    el.style.left = timeToX(kf.time) + 'px';
    el.style.background = track.color;

    const key = `obj-${trackIndex}-${ki}`;
    if (selectedKeyframe === key) el.classList.add('selected');
    el.title = `${track.name} @ ${formatTimecode(kf.time)}`;

    el.addEventListener('mousedown', (e) => {
      e.stopPropagation();
      if (feat('keyframe-drag')) {
        kfDragging = true;
        kfDragInfo = {
          type: 'obj', trackIndex, kfIndex: ki,
          startX: e.clientX, origTime: kf.time
        };
        el.classList.add('dragging');
      }
      selectedKeyframe = (selectedKeyframe === key) ? null : key;
      playhead = kf.time;
      render();
    });
    row.appendChild(el);
  });
}

function renderPlayhead() {
  playheadEl.style.left = timeToX(playhead) + 'px';
}

function renderZoomBar() {
  const w = zoomBar.clientWidth;
  const thumbLeft = (viewStart / SCENE.totalDuration) * w;
  const thumbRight = (viewEnd / SCENE.totalDuration) * w;
  zoomThumb.style.left = thumbLeft + 'px';
  zoomThumb.style.width = Math.max(30, thumbRight - thumbLeft) + 'px';

  const px = (playhead / SCENE.totalDuration) * w;
  zoomPlayheadIndicator.style.left = (px - 2) + 'px';
}

function renderMinimap() {
  if (!minimapEl) return;
  const w = minimapEl.clientWidth;
  const tToX = (t) => (t / SCENE.totalDuration) * w;
  const currentShotIndex = getCurrentShot();

  const shotsContainer = document.getElementById('minimap-shots');
  shotsContainer.innerHTML = '';
  SCENE.shots.forEach((shot, i) => {
    const el = document.createElement('div');
    el.className = 'minimap-shot';
    const left = tToX(shot.start);
    const right = tToX(shot.end);
    el.style.left = left + 'px';
    el.style.width = (right - left - 1) + 'px';
    el.style.background = i === currentShotIndex ? shot.color : adjustAlpha(shot.color, 0.6);
    if (right - left > 30) el.textContent = shot.name;
    shotsContainer.appendChild(el);
  });

  const tracksContainer = document.getElementById('minimap-tracks');
  tracksContainer.innerHTML = '';
  const rowCount = 1 + SCENE.tracks.length;
  const trackH = Math.max(4, (48 - 17) / rowCount);

  const camRow = document.createElement('div');
  camRow.className = 'minimap-track-row';
  camRow.style.top = '0px';
  camRow.style.height = trackH + 'px';
  SCENE.shots.forEach((shot, si) => {
    const camIdx = activeCameraPerShot[si] || 0;
    const cam = shot.cameras[camIdx];
    cam.keyframes.forEach(kf => {
      const el = document.createElement('div');
      el.className = 'minimap-kf';
      el.style.left = tToX(kf.time) + 'px';
      el.style.background = shot.color;
      camRow.appendChild(el);
    });
  });
  tracksContainer.appendChild(camRow);

  SCENE.tracks.forEach((track, ti) => {
    const row = document.createElement('div');
    row.className = 'minimap-track-row';
    row.style.top = ((ti + 1) * trackH) + 'px';
    row.style.height = trackH + 'px';

    if (track.linkedPeriods) {
      track.linkedPeriods.forEach(lp => {
        const region = document.createElement('div');
        region.className = 'minimap-link-region';
        region.style.left = tToX(lp.start) + 'px';
        region.style.width = (tToX(lp.end) - tToX(lp.start)) + 'px';
        row.appendChild(region);
      });
    }

    track.keyframes.forEach(kf => {
      const el = document.createElement('div');
      el.className = 'minimap-kf';
      el.style.left = tToX(kf.time) + 'px';
      el.style.background = track.color;
      row.appendChild(el);
    });

    tracksContainer.appendChild(row);
  });

  const viewWindow = document.getElementById('minimap-view-window');
  const vwLeft = tToX(viewStart);
  const vwRight = tToX(viewEnd);
  viewWindow.style.left = vwLeft + 'px';
  viewWindow.style.width = (vwRight - vwLeft) + 'px';
  viewWindow.style.display = (viewEnd - viewStart >= SCENE.totalDuration - 0.01) ? 'none' : 'block';

  document.getElementById('minimap-playhead').style.left = tToX(playhead) + 'px';
}

function renderViewportInfo() {
  const si = getCurrentShot();
  const shot = SCENE.shots[si];
  const display = getDisplayCamera(si);
  const letter = String.fromCharCode(65 + display.index);
  viewportShotLabel.textContent = `Shot ${si + 1}${letter}: ${shot.name}`;
  viewportTimecode.textContent = formatTimecode(playhead);
}

// ── Feature-specific render functions ──

function renderTransportBar() {
  const el = document.getElementById('transport-bar');
  if (!el) return;
  const si = getCurrentShot();
  const shot = SCENE.shots[si];
  document.getElementById('transport-time').textContent = playhead.toFixed(1);
  document.getElementById('transport-duration').textContent = SCENE.totalDuration.toFixed(1);
  document.getElementById('transport-shot').textContent = shot.name;
  const playBtn = document.getElementById('transport-play');
  playBtn.textContent = playing ? '⏸' : '▶';
  playBtn.classList.toggle('playing', playing);
}

function renderStatusBar() {
  const hints = document.getElementById('status-hints');
  if (!hints) return;
  let parts = [
    '<kbd>Space</kbd> Play/Pause',
    '<kbd>←</kbd><kbd>→</kbd> Frame step',
  ];
  if (feat('full-shortcuts')) {
    parts.push(
      '<kbd>Home</kbd> Start',
      '<kbd>End</kbd> End',
      '<kbd>G</kbd> Guides',
      '<kbd>H</kbd> HUD',
    );
  }
  if (feat('active-cam-keys')) {
    parts.push('<kbd>Shift+1-4</kbd> Camera');
  }
  if (feat('tool-mode')) {
    parts.push('<kbd>QWER</kbd> Tools');
  }
  hints.innerHTML = parts.join(' &nbsp;│&nbsp; ');
}

function renderCoverageTrack() {
  const track = document.getElementById('coverage-track');
  if (!track) return;
  track.querySelectorAll('.coverage-segment, .coverage-divider').forEach(el => el.remove());

  const si = getCurrentShot();
  const shot = SCENE.shots[si];
  const coverage = shot.coverage || [];

  coverage.forEach((seg, i) => {
    const cam = shot.cameras[seg.camera];
    if (!cam) return;
    const left = timeToX(seg.start);
    const width = timeToX(seg.end) - left;

    const el = document.createElement('div');
    el.className = 'coverage-segment';
    el.style.left = left + 'px';
    el.style.width = width + 'px';
    el.style.background = shot.color;
    el.style.opacity = seg.camera === (activeCameraPerShot[si] || 0) ? '1' : '0.5';
    const letter = String.fromCharCode(65 + seg.camera);
    if (width > 20) el.textContent = letter;
    el.title = `${cam.name}: ${(seg.end - seg.start).toFixed(1)}s`;

    // Right-click to split (3.6.5)
    if (feat('multi-split')) {
      el.addEventListener('contextmenu', (e) => {
        e.preventDefault();
        const rect = track.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const splitTime = xToTime(x);
        if (splitTime > seg.start + 0.2 && splitTime < seg.end - 0.2) {
          const snapTime = Math.round(splitTime * SCENE.fps) / SCENE.fps;
          const nextCam = (seg.camera + 1) % shot.cameras.length;
          coverage.splice(i + 1, 0, { camera: nextCam, start: snapTime, end: seg.end });
          seg.end = snapTime;
          render();
        }
      });
    }

    track.appendChild(el);

    // Dividers between segments
    if (i > 0) {
      const divider = document.createElement('div');
      divider.className = 'coverage-divider';
      divider.style.left = left + 'px';
      track.appendChild(divider);
    }
  });

  // Coverage playhead
  const phCov = document.getElementById('playhead-coverage');
  if (phCov) phCov.style.left = timeToX(playhead) + 'px';
}

function renderAggregateDuration() {
  const el = document.getElementById('aggregate-duration');
  if (el) el.textContent = `Total: ${SCENE.totalDuration.toFixed(1)}s`;
}

function renderToolMode() {
  const mode = TOOL_MODES[currentToolMode];
  const icon = document.getElementById('tool-mode-icon');
  const label = document.getElementById('tool-mode-label');
  const key = document.getElementById('tool-mode-key');
  if (icon) icon.textContent = mode.icon;
  if (label) label.textContent = mode.label;
  if (key) key.textContent = mode.key;
}

// ── Interactions ──

// Track area click/drag to move playhead
let trackDragging = false;

trackArea.addEventListener('mousedown', (e) => {
  if (e.button === 0) {
    trackDragging = true;
    previewCamera = null;
    const rect = trackArea.getBoundingClientRect();
    const x = e.clientX - rect.left;
    playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(x)));
    selectedKeyframe = null;
    render();
  }
  if (e.button === 1) {
    e.preventDefault();
    panDragging = true;
    panStartX = e.clientX;
    panStartViewStart = viewStart;
    trackArea.style.cursor = 'grabbing';
  }
});

// Ruler scrub
let rulerDragging = false;

ruler.addEventListener('mousedown', (e) => {
  if (e.button !== 0) return;
  rulerDragging = true;
  previewCamera = null;
  const rect = ruler.getBoundingClientRect();
  const x = e.clientX - rect.left;
  playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(x)));
  render();
});

// Middle-drag to pan
let panDragging = false;
let panStartX = 0;
let panStartViewStart = 0;

shotBar.addEventListener('mousedown', (e) => {
  if (e.button === 1) {
    e.preventDefault();
    panDragging = true;
    panStartX = e.clientX;
    panStartViewStart = viewStart;
    shotBar.style.cursor = 'grabbing';
  }
});

// Scroll to zoom
function handleZoomWheel(e) {
  e.preventDefault();
  const rect = e.currentTarget.getBoundingClientRect();
  const mouseX = e.clientX - rect.left;
  const fraction = mouseX / rect.width;

  if (e.shiftKey) {
    const duration = viewEnd - viewStart;
    const panAmount = (e.deltaY > 0 ? 0.1 : -0.1) * duration;
    viewStart += panAmount;
    viewEnd += panAmount;
    clampView();
    render();
    return;
  }
  zoomAtPosition(fraction, e.deltaY);
}

trackArea.addEventListener('wheel', handleZoomWheel, { passive: false });
ruler.addEventListener('wheel', handleZoomWheel, { passive: false });
shotBar.addEventListener('wheel', handleZoomWheel, { passive: false });

// Global mousemove / mouseup
let zoomDragging = false;
let zoomDragStartX = 0;
let zoomDragStartViewStart = 0;
let minimapDragging = false;

document.addEventListener('mousemove', (e) => {
  // Keyframe drag (1.5.3)
  if (kfDragging && kfDragInfo) {
    const rect = trackArea.getBoundingClientRect();
    const x = e.clientX - rect.left;
    let newTime = xToTime(x);
    // Snap to 0.1s grid
    newTime = Math.round(newTime * 10) / 10;
    newTime = Math.max(0, Math.min(SCENE.totalDuration, newTime));

    if (kfDragInfo.type === 'cam') {
      const shot = SCENE.shots[kfDragInfo.shotIndex];
      newTime = Math.max(shot.start, Math.min(shot.end, newTime));
      shot.cameras[kfDragInfo.camIndex].keyframes[kfDragInfo.kfIndex].time = newTime;
    } else {
      SCENE.tracks[kfDragInfo.trackIndex].keyframes[kfDragInfo.kfIndex].time = newTime;
    }
    playhead = newTime;
    render();
    return;
  }

  // Shot boundary drag (ripple)
  if (boundaryDragging && boundarySnapshot) {
    const rect = shotBar.getBoundingClientRect();
    const x = e.clientX - rect.left;
    let newTime = xToTime(x);

    const snap = boundarySnapshot;
    const minDuration = 1;
    newTime = Math.max(snap.shots[boundaryIndex].start + minDuration, newTime);
    newTime = Math.round(newTime * SCENE.fps) / SCENE.fps;

    const delta = newTime - snap.origBoundary;

    SCENE.shots[boundaryIndex].end = newTime;

    for (let j = boundaryIndex + 1; j < SCENE.shots.length; j++) {
      SCENE.shots[j].start = snap.shots[j].start + delta;
      SCENE.shots[j].end = snap.shots[j].end + delta;
      SCENE.shots[j].cameras.forEach((cam, ci) => {
        cam.keyframes.forEach((kf, ki) => {
          kf.time = snap.cameraTimes[j][ci][ki] + delta;
        });
      });
      // Ripple coverage segments too
      if (SCENE.shots[j].coverage) {
        SCENE.shots[j].coverage.forEach((seg, si) => {
          const orig = snap.coverageTimes[j][si];
          if (orig) {
            seg.start = orig.start + delta;
            seg.end = orig.end + delta;
          }
        });
      }
    }

    if (!e.shiftKey) {
      SCENE.tracks.forEach((track, ti) => {
        track.keyframes.forEach((kf, ki) => {
          const origTime = snap.objectTimes[ti][ki];
          kf.time = origTime >= snap.origBoundary ? origTime + delta : origTime;
        });
        (track.linkedPeriods || []).forEach((lp, li) => {
          const origLp = snap.linkedPeriods[ti][li];
          lp.start = origLp.start >= snap.origBoundary ? origLp.start + delta : origLp.start;
          lp.end = origLp.end >= snap.origBoundary ? origLp.end + delta : origLp.end;
        });
      });
    } else {
      SCENE.tracks.forEach((track, ti) => {
        track.keyframes.forEach((kf, ki) => {
          kf.time = snap.objectTimes[ti][ki];
        });
        (track.linkedPeriods || []).forEach((lp, li) => {
          const origLp = snap.linkedPeriods[ti][li];
          lp.start = origLp.start;
          lp.end = origLp.end;
        });
      });
    }

    SCENE.totalDuration = SCENE.shots[SCENE.shots.length - 1].end;

    if (viewEnd > SCENE.totalDuration) {
      const dur = viewEnd - viewStart;
      viewEnd = SCENE.totalDuration;
      viewStart = Math.max(0, viewEnd - dur);
    }

    const leftShot = SCENE.shots[boundaryIndex];
    const leftDur = leftShot.end - leftShot.start;
    const leftFrames = Math.round(leftDur * SCENE.fps);
    resizeLabel.style.display = 'block';
    resizeLabel.style.left = (e.clientX + 14) + 'px';
    resizeLabel.style.top = (e.clientY - 24) + 'px';
    const mode = e.shiftKey ? 'shots only' : 'ripple';
    resizeLabel.textContent = `${leftShot.name}: ${leftDur.toFixed(1)}s (${leftFrames}f)  [${mode}]`;

    render();
    return;
  }

  if (trackDragging) {
    const rect = trackArea.getBoundingClientRect();
    const x = e.clientX - rect.left;
    playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(x)));
    render();
    return;
  }

  if (rulerDragging) {
    const rect = ruler.getBoundingClientRect();
    const x = e.clientX - rect.left;
    playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(x)));
    render();
    return;
  }

  if (panDragging) {
    const dx = e.clientX - panStartX;
    const w = trackArea.clientWidth;
    const duration = viewEnd - viewStart;
    const dt = -(dx / w) * duration;
    viewStart = panStartViewStart + dt;
    viewEnd = viewStart + duration;
    clampView();
    render();
    return;
  }

  if (zoomDragging) {
    const dx = e.clientX - zoomDragStartX;
    const w = zoomBar.clientWidth;
    const dt = (dx / w) * SCENE.totalDuration;
    const duration = viewEnd - viewStart;
    viewStart = zoomDragStartViewStart + dt;
    viewEnd = viewStart + duration;
    clampView();
    render();
    return;
  }

  if (minimapDragging) {
    handleMinimapClick(e);
    return;
  }
});

document.addEventListener('mouseup', () => {
  if (kfDragging) {
    kfDragging = false;
    kfDragInfo = null;
  }
  if (boundaryDragging) {
    boundaryDragging = false;
    boundaryIndex = -1;
    boundarySnapshot = null;
    resizeLabel.style.display = 'none';
  }
  trackDragging = false;
  rulerDragging = false;
  minimapDragging = false;
  if (panDragging) {
    panDragging = false;
    trackArea.style.cursor = 'crosshair';
    shotBar.style.cursor = 'pointer';
  }
  zoomDragging = false;
});

// Zoom thumb drag
zoomThumb.addEventListener('mousedown', (e) => {
  zoomDragging = true;
  zoomDragStartX = e.clientX;
  zoomDragStartViewStart = viewStart;
  e.preventDefault();
});

// Click zoom bar to jump
zoomBar.addEventListener('click', (e) => {
  if (e.target === zoomThumb) return;
  const rect = zoomBar.getBoundingClientRect();
  const x = e.clientX - rect.left;
  const t = (x / rect.width) * SCENE.totalDuration;
  const duration = viewEnd - viewStart;
  viewStart = t - duration / 2;
  viewEnd = viewStart + duration;
  clampView();
  playhead = Math.max(0, Math.min(SCENE.totalDuration, t));
  render();
});

// ── Track expand/collapse click delegation ──
trackLabels.addEventListener('click', (e) => {
  const arrow = e.target.closest('.expand-arrow');
  if (!arrow || !feat('track-expand')) return;
  const trackKey = arrow.dataset.track;
  trackExpanded[trackKey] = !trackExpanded[trackKey];
  render();
});

// ── Buttons ──

document.getElementById('btn-play').addEventListener('click', () => {
  playing = !playing;
  document.getElementById('btn-play').innerHTML = playing ? '&#10074;&#10074;' : '&#9654;';
  if (playing) animLoop();
});

// Transport bar play button (1.5.1)
const transportPlay = document.getElementById('transport-play');
if (transportPlay) {
  transportPlay.addEventListener('click', () => {
    document.getElementById('btn-play').click();
  });
}

// Add shot button (1.4.2)
const addShotBtn = document.getElementById('btn-add-shot');
if (addShotBtn) {
  addShotBtn.addEventListener('click', () => {
    if (!feat('shot-management')) return;
    const lastShot = SCENE.shots[SCENE.shots.length - 1];
    const newStart = lastShot.end;
    const newEnd = newStart + 4;
    const shotNum = SCENE.shots.length + 1;
    const hue = (shotNum * 47) % 360;
    SCENE.shots.push({
      name: `Shot ${shotNum}`,
      start: newStart,
      end: newEnd,
      color: `hsl(${hue}, 40%, 45%)`,
      cameras: [{ name: 'Cam A', keyframes: [{ time: newStart }, { time: newEnd - 0.5 }] }],
      coverage: [{ camera: 0, start: newStart, end: newEnd }],
    });
    SCENE.totalDuration = newEnd;
    activeCameraPerShot[SCENE.shots.length - 1] = 0;
    if (viewEnd < SCENE.totalDuration) viewEnd = SCENE.totalDuration;
    render();
  });
}

// Scenes switcher (1.4.4)
const scenesSwitcher = document.getElementById('scenes-switcher');
if (scenesSwitcher) {
  scenesSwitcher.addEventListener('click', () => {
    if (!feat('scenes')) return;
    currentScene = (currentScene + 1) % SCENES.length;
    document.getElementById('scenes-label').textContent = SCENES[currentScene];
  });
}

// ── Minimap interaction ──

minimapEl.addEventListener('mousedown', (e) => {
  if (e.button !== 0) return;
  minimapDragging = true;
  previewCamera = null;
  handleMinimapClick(e);
});

function handleMinimapClick(e) {
  const rect = minimapEl.getBoundingClientRect();
  const x = e.clientX - rect.left;
  const t = (x / rect.width) * SCENE.totalDuration;
  const duration = viewEnd - viewStart;
  viewStart = t - duration / 2;
  viewEnd = viewStart + duration;
  clampView();
  playhead = Math.max(0, Math.min(SCENE.totalDuration, t));
  render();
}

// Prevent middle-click scroll/paste
trackArea.addEventListener('auxclick', (e) => e.preventDefault());
ruler.addEventListener('auxclick', (e) => e.preventDefault());
shotBar.addEventListener('auxclick', (e) => e.preventDefault());
minimapEl.addEventListener('auxclick', (e) => e.preventDefault());

// ── Keyboard ──

document.addEventListener('keydown', (e) => {
  if (e.target.tagName === 'INPUT') return;

  // Basic controls (always active)
  if (e.key === ' ') {
    e.preventDefault();
    document.getElementById('btn-play').click();
    return;
  }
  if (e.key === 'ArrowLeft') {
    playhead = Math.max(0, playhead - 1 / SCENE.fps);
    render();
    return;
  }
  if (e.key === 'ArrowRight') {
    playhead = Math.min(SCENE.totalDuration, playhead + 1 / SCENE.fps);
    render();
    return;
  }
  if (e.key === '\\') {
    viewStart = 0;
    viewEnd = SCENE.totalDuration;
    render();
    return;
  }

  // Tool mode shortcuts (1.3.2)
  if (feat('tool-mode')) {
    const upper = e.key.toUpperCase();
    const modeIdx = TOOL_MODES.findIndex(m => m.key === upper);
    if (modeIdx !== -1) {
      currentToolMode = modeIdx;
      render();
      return;
    }
  }

  // Full keyboard shortcuts (1.6.2)
  if (feat('full-shortcuts')) {
    switch (e.key) {
      case 'Home':
        e.preventDefault();
        playhead = 0;
        render();
        return;
      case 'End':
        e.preventDefault();
        playhead = SCENE.totalDuration;
        render();
        return;
    }

    const upper = e.key.toUpperCase();
    switch (upper) {
      case 'G':
        guidesVisible = !guidesVisible;
        applyFeatureVisibility();
        return;
      case 'H':
        // Toggle HUD visibility
        const hud = document.getElementById('viewport-hud');
        if (hud) hud.style.display = hud.style.display === 'none' ? '' : 'none';
        return;
      case 'P':
        if (feat('camera-path')) {
          cameraPathVisible = !cameraPathVisible;
          applyFeatureVisibility();
        }
        return;
      case 'D':
        if (feat('director-view')) {
          directorView = !directorView;
          applyFeatureVisibility();
        }
        return;
    }

    // Aspect ratio cycling (A key, but only when full-shortcuts active)
    if (upper === 'A' && feat('aspect-ratio')) {
      currentAspectIndex = (currentAspectIndex + 1) % ASPECT_RATIOS.length;
      applyFeatureVisibility();
      return;
    }
  }

  // Active camera shortcuts Shift+1-4 (3.6.3)
  if (feat('active-cam-keys') && e.shiftKey) {
    const num = parseInt(e.key);
    if (num >= 1 && num <= 4) {
      const si = getCurrentShot();
      const shot = SCENE.shots[si];
      if (num - 1 < shot.cameras.length) {
        activeCameraPerShot[si] = num - 1;
        previewCamera = null;
        render();
      }
      return;
    }
  }
});

// ── Animation loop ──

let lastTime = null;
function animLoop() {
  if (!playing) { lastTime = null; return; }
  animFrame = requestAnimationFrame((ts) => {
    if (lastTime !== null) {
      const dt = (ts - lastTime) / 1000;
      playhead += dt;
      if (playhead >= SCENE.totalDuration) {
        playhead = 0;
      }
      if (playhead < viewStart || playhead > viewEnd) {
        const duration = viewEnd - viewStart;
        viewStart = playhead;
        viewEnd = viewStart + duration;
        if (viewEnd > SCENE.totalDuration) {
          viewEnd = SCENE.totalDuration;
          viewStart = viewEnd - duration;
        }
      }
      render();
    }
    lastTime = ts;
    animLoop();
  });
}

// ── Resize & initial render ──

window.addEventListener('resize', () => {
  render();
  if (feat('aspect-ratio')) updateAspectMasks();
});

applyFeatureVisibility();
render();
