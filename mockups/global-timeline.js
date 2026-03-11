// Fram3d — Global Timeline Visualization Mockup

// ── Data model ──
// Each shot owns its cameras. Object tracks span the entire timeline.
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
      ]
    },
    {
      name: 'Over Shoulder', start: 6, end: 10, color: '#5577aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 6 }, { time: 8 }, { time: 9.5 }] },
        { name: 'Cam B', keyframes: [{ time: 7 }, { time: 9 }] },
      ]
    },
    {
      name: 'Close-up Reaction', start: 10, end: 18, color: '#aa8844',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 10 }, { time: 13 }, { time: 17 }] },
      ]
    },
    {
      name: 'Tracking', start: 18, end: 23, color: '#8855aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 18 }, { time: 20 }, { time: 22.5 }] },
        { name: 'Cam B', keyframes: [{ time: 19 }, { time: 21 }] },
        { name: 'Cam C', keyframes: [{ time: 18.5 }, { time: 22 }] },
      ]
    },
    {
      name: 'Two-Shot', start: 23, end: 30, color: '#aa5577',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 23 }, { time: 26 }, { time: 29 }] },
        { name: 'Cam B', keyframes: [{ time: 24.5 }, { time: 28 }] },
      ]
    },
    {
      name: 'Insert Detail', start: 30, end: 33, color: '#557799',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 30 }, { time: 32 }] },
      ]
    },
    {
      name: 'Dolly In', start: 33, end: 39, color: '#aa7744',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 33 }, { time: 35 }, { time: 38 }] },
        { name: 'Cam B', keyframes: [{ time: 34 }, { time: 37 }] },
        { name: 'Cam C', keyframes: [{ time: 33.5 }, { time: 36 }, { time: 38.5 }] },
        { name: 'Cam D', keyframes: [{ time: 34.5 }, { time: 37.5 }] },
      ]
    },
    {
      name: 'Reverse Angle', start: 39, end: 43, color: '#7755aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 39 }, { time: 41 }, { time: 42.5 }] },
        { name: 'Cam B', keyframes: [{ time: 40 }, { time: 42 }] },
      ]
    },
    {
      name: 'Steadicam Walk', start: 43, end: 53, color: '#996655',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 43 }, { time: 46 }, { time: 50 }, { time: 52 }] },
      ]
    },
    {
      name: 'High Angle', start: 53, end: 58, color: '#5588aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 53 }, { time: 55 }, { time: 57 }] },
        { name: 'Cam B', keyframes: [{ time: 54 }, { time: 56.5 }] },
        { name: 'Cam C', keyframes: [{ time: 53.5 }, { time: 57.5 }] },
      ]
    },
    {
      name: 'POV', start: 58, end: 65, color: '#aa6655',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 58 }, { time: 61 }, { time: 64 }] },
        { name: 'Cam B', keyframes: [{ time: 59.5 }, { time: 63 }] },
      ]
    },
    {
      name: 'Whip Pan', start: 65, end: 68, color: '#6677aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 65 }, { time: 67 }] },
      ]
    },
    {
      name: 'Push In', start: 68, end: 76, color: '#aa5555',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 68 }, { time: 71 }, { time: 75 }] },
        { name: 'Cam B', keyframes: [{ time: 69.5 }, { time: 73 }] },
      ]
    },
    {
      name: 'Master Wide', start: 76, end: 82, color: '#558899',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 76 }, { time: 78 }, { time: 81 }] },
        { name: 'Cam B', keyframes: [{ time: 77 }, { time: 80 }] },
        { name: 'Cam C', keyframes: [{ time: 76.5 }, { time: 79 }, { time: 81.5 }] },
      ]
    },
    {
      name: 'Final Close-up', start: 82, end: 90, color: '#8866aa',
      cameras: [
        { name: 'Cam A', keyframes: [{ time: 82 }, { time: 85 }, { time: 89 }] },
      ]
    },
  ],

  // Object tracks — green shades, span full timeline
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

// ── State ──
let playhead = 0;
let viewStart = 0;
let viewEnd = SCENE.totalDuration;
let playing = false;
let selectedKeyframe = null;
let animFrame = null;

// Per-shot active camera (index). Default: 0 (Cam A)
const activeCameraPerShot = {};
SCENE.shots.forEach((_, i) => { activeCameraPerShot[i] = 0; });

// Single-click preview: shows keyframes without making camera active
let previewCamera = null; // { shotIndex, cameraIndex } or null

// Shot bar double-click detection
let shotBarLastClick = { shotIndex: -1, camIndex: -1, time: 0 };

// Shot boundary drag (ripple)
let boundaryDragging = false;
let boundaryIndex = -1;
let boundarySnapshot = null;

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

// ── Init shot bar height ──
document.getElementById('shot-bar-container').style.height = (MAX_CAMERAS * CAM_ROW_HEIGHT + 2) + 'px';

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

// Which camera to display in the track area for a given shot
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
}

function renderShotBar() {
  shotBar.querySelectorAll('.shot-cam, .shot-boundary-handle').forEach(el => el.remove());
  const currentShotIndex = getCurrentShot();

  // Auto-clear preview if we've moved to a different shot
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

      el.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        e.preventDefault();

        const now = Date.now();
        const isDouble = shotBarLastClick.shotIndex === si &&
          shotBarLastClick.camIndex === ci &&
          now - shotBarLastClick.time < 350;

        if (isDouble) {
          // Double-click: activate this camera (no zoom change)
          activeCameraPerShot[si] = ci;
          previewCamera = null;
          shotBarLastClick = { shotIndex: -1, camIndex: -1, time: 0 };
        } else {
          shotBarLastClick = { shotIndex: si, camIndex: ci, time: now };

          // Jump playhead if clicking a different shot
          if (si !== currentShotIndex) playhead = shot.start;

          // Preview non-active camera; clear preview if clicking active
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

  // Boundary drag handles (span full shot bar height)
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
      // Snapshot state for ripple calculation
      boundarySnapshot = {
        shots: SCENE.shots.map(s => ({ start: s.start, end: s.end })),
        cameraTimes: SCENE.shots.map(s => s.cameras.map(c => c.keyframes.map(kf => kf.time))),
        objectTimes: SCENE.tracks.map(t => t.keyframes.map(kf => kf.time)),
        linkedPeriods: SCENE.tracks.map(t => (t.linkedPeriods || []).map(lp => ({ start: lp.start, end: lp.end }))),
        origBoundary: SCENE.shots[idx].end,
      };
    });
    shotBar.appendChild(handle);
  }

  // View range indicator (hidden)
  document.getElementById('shot-bar-view-range').style.display = 'none';

  // Playhead on shot bar
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
  const rowHeight = 28;

  // Camera track (single row)
  const camLabel = document.createElement('div');
  camLabel.className = 'track-label camera-label';
  camLabel.innerHTML = `<div class="dot" style="background:${currentShot.color}"></div><div class="name">Cam ${displayLetter}</div>`;
  trackLabels.appendChild(camLabel);

  const camRow = document.createElement('div');
  camRow.className = 'track-row';
  camRow.style.top = '0px';
  trackArea.appendChild(camRow);
  renderCameraRow(camRow, currentShotIndex);

  // Object tracks
  SCENE.tracks.forEach((track, ti) => {
    const label = document.createElement('div');
    label.className = 'track-label';
    label.innerHTML = `<div class="dot" style="background:${track.color}"></div><div class="name">${track.name}</div>`;
    trackLabels.appendChild(label);

    const row = document.createElement('div');
    row.className = 'track-row';
    row.style.top = ((ti + 1) * rowHeight) + 'px';
    trackArea.appendChild(row);
    renderObjectRow(row, ti, currentShotIndex);
  });

  const totalHeight = (1 + SCENE.tracks.length) * rowHeight;
  trackArea.style.height = totalHeight + 'px';
  trackLabels.style.height = totalHeight + 'px';
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
      el.addEventListener('click', (e) => {
        e.stopPropagation();
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
    el.addEventListener('click', (e) => {
      e.stopPropagation();
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

  // Shots
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

  // Tracks: 1 camera row + N object rows
  const tracksContainer = document.getElementById('minimap-tracks');
  tracksContainer.innerHTML = '';
  const rowCount = 1 + SCENE.tracks.length;
  const trackH = Math.max(4, (48 - 17) / rowCount);

  // Camera minimap row — show active camera keyframes for each shot
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

  // Object minimap rows
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

  // View window
  const viewWindow = document.getElementById('minimap-view-window');
  const vwLeft = tToX(viewStart);
  const vwRight = tToX(viewEnd);
  viewWindow.style.left = vwLeft + 'px';
  viewWindow.style.width = (vwRight - vwLeft) + 'px';
  viewWindow.style.display = (viewEnd - viewStart >= SCENE.totalDuration - 0.01) ? 'none' : 'block';

  // Playhead
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

// Click/drag ruler to scrub
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
  // Shot boundary drag (ripple)
  if (boundaryDragging && boundarySnapshot) {
    const rect = shotBar.getBoundingClientRect();
    const x = e.clientX - rect.left;
    let newTime = xToTime(x);

    // Clamp: dragged shot can't be shorter than 1 second
    const snap = boundarySnapshot;
    const minDuration = 1;
    newTime = Math.max(snap.shots[boundaryIndex].start + minDuration, newTime);

    // Snap to frame boundary
    newTime = Math.round(newTime * SCENE.fps) / SCENE.fps;

    const delta = newTime - snap.origBoundary;

    // Update dragged shot's end
    SCENE.shots[boundaryIndex].end = newTime;

    // Ripple: shift all subsequent shots and their camera keyframes
    for (let j = boundaryIndex + 1; j < SCENE.shots.length; j++) {
      SCENE.shots[j].start = snap.shots[j].start + delta;
      SCENE.shots[j].end = snap.shots[j].end + delta;
      SCENE.shots[j].cameras.forEach((cam, ci) => {
        cam.keyframes.forEach((kf, ki) => {
          kf.time = snap.cameraTimes[j][ci][ki] + delta;
        });
      });
    }

    // Shift object keyframes at or past the original boundary
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

    // Update total duration
    SCENE.totalDuration = SCENE.shots[SCENE.shots.length - 1].end;

    // Clamp view if total duration shrank
    if (viewEnd > SCENE.totalDuration) {
      const dur = viewEnd - viewStart;
      viewEnd = SCENE.totalDuration;
      viewStart = Math.max(0, viewEnd - dur);
    }

    // Show floating label with shot name + new duration
    const leftShot = SCENE.shots[boundaryIndex];
    const leftDur = leftShot.end - leftShot.start;
    const leftFrames = Math.round(leftDur * SCENE.fps);
    resizeLabel.style.display = 'block';
    resizeLabel.style.left = (e.clientX + 14) + 'px';
    resizeLabel.style.top = (e.clientY - 24) + 'px';
    resizeLabel.textContent = `${leftShot.name}: ${leftDur.toFixed(1)}s (${leftFrames}f)`;

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

// ── Buttons ──

document.getElementById('btn-play').addEventListener('click', () => {
  playing = !playing;
  document.getElementById('btn-play').innerHTML = playing ? '&#10074;&#10074;' : '&#9654;';
  if (playing) animLoop();
});

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

  if (e.key === ' ') {
    e.preventDefault();
    document.getElementById('btn-play').click();
  } else if (e.key === 'ArrowLeft') {
    playhead = Math.max(0, playhead - 1 / SCENE.fps);
    render();
  } else if (e.key === 'ArrowRight') {
    playhead = Math.min(SCENE.totalDuration, playhead + 1 / SCENE.fps);
    render();
  } else if (e.key === '\\') {
    viewStart = 0;
    viewEnd = SCENE.totalDuration;
    render();
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

window.addEventListener('resize', render);
render();
