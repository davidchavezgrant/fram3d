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

// ── State ──
let playhead = 0;
let viewStart = 0;
let viewEnd = SCENE.totalDuration;
let playing = false;
let selectedKeyframe = null;
let animFrame = null;
let shotBarLastClick = { index: -1, time: 0 };

// Shot boundary drag
let boundaryDragging = false;
let boundaryIndex = -1;

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

// ── Derived track rows ──
// Camera rows change based on current shot; object rows are fixed.

function getVisibleRows() {
  const si = getCurrentShot();
  const shot = SCENE.shots[si];
  const rows = [];

  shot.cameras.forEach((cam, ci) => {
    rows.push({ type: 'camera', name: cam.name, shotIndex: si, cameraIndex: ci });
  });

  SCENE.tracks.forEach((track, ti) => {
    rows.push({ type: 'object', name: track.name, trackIndex: ti });
  });

  return rows;
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
  // Clear dynamic children
  shotBar.querySelectorAll('.shot-rect, .shot-boundary-handle').forEach(el => el.remove());
  const currentShot = getCurrentShot();

  SCENE.shots.forEach((shot, i) => {
    const el = document.createElement('div');
    el.className = 'shot-rect' + (i === currentShot ? ' active' : '');
    const left = timeToX(shot.start);
    const right = timeToX(shot.end);
    el.style.left = left + 'px';
    el.style.width = Math.max(0, right - left - 2) + 'px';
    el.style.background = shot.color;
    el.textContent = shot.name;

    el.addEventListener('mousedown', (e) => {
      e.stopPropagation();
      e.preventDefault();
      const now = Date.now();
      if (shotBarLastClick.index === i && now - shotBarLastClick.time < 350) {
        // Double-click: zoom to shot
        shotBarLastClick = { index: -1, time: 0 };
        viewStart = shot.start;
        viewEnd = shot.end;
        playhead = shot.start;
      } else {
        shotBarLastClick = { index: i, time: now };
        playhead = shot.start;
      }
      render();
    });
    shotBar.appendChild(el);
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
    });
    shotBar.appendChild(handle);
  }

  // Shot bar view range (hidden for now)
  document.getElementById('shot-bar-view-range').style.display = 'none';

  // Playhead on shot bar
  shotBar.appendChild(playheadShotbar);
  playheadShotbar.style.left = timeToX(playhead) + 'px';
}

function renderRuler() {
  ruler.querySelectorAll('.ruler-tick, .ruler-label').forEach(el => el.remove());

  const duration = viewEnd - viewStart;
  let interval;
  if (duration <= 2) interval = 1 / SCENE.fps; // per-frame
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
  const rows = getVisibleRows();
  const rowHeight = 28;

  rows.forEach((rowData, i) => {
    // Label
    const label = document.createElement('div');
    if (rowData.type === 'camera') {
      label.className = 'track-label camera-label';
      const cam = currentShot.cameras[rowData.cameraIndex];
      label.innerHTML = `<div class="dot" style="background:${currentShot.color}"></div><div class="name">${cam.name}</div>`;
    } else {
      label.className = 'track-label';
      const track = SCENE.tracks[rowData.trackIndex];
      label.innerHTML = `<div class="dot" style="background:${track.color}"></div><div class="name">${track.name}</div>`;
    }
    trackLabels.appendChild(label);

    // Track row
    const row = document.createElement('div');
    row.className = 'track-row';
    row.style.top = (i * rowHeight) + 'px';
    trackArea.appendChild(row);

    if (rowData.type === 'camera') {
      renderCameraRow(row, rowData, currentShotIndex);
    } else {
      renderObjectRow(row, rowData, currentShotIndex);
    }
  });

  const totalHeight = rows.length * rowHeight;
  trackArea.style.height = totalHeight + 'px';
  trackLabels.style.height = totalHeight + 'px';
}

function renderCameraRow(row, rowData, currentShotIndex) {
  const currentShot = SCENE.shots[currentShotIndex];
  const cam = currentShot.cameras[rowData.cameraIndex];

  // Shot background tinting
  SCENE.shots.forEach((shot, si) => {
    const bg = document.createElement('div');
    bg.className = 'shot-bg' + (si === currentShotIndex ? ' active-shot' : '');
    const left = timeToX(shot.start);
    const width = timeToX(shot.end) - left;
    bg.style.left = left + 'px';
    bg.style.width = width + 'px';
    bg.style.background = shot.color;
    row.appendChild(bg);
  });

  // Collapsed blocks for non-current shots
  SCENE.shots.forEach((shot, si) => {
    if (si === currentShotIndex) return;
    const block = document.createElement('div');
    block.className = 'camera-block';
    const left = timeToX(shot.start);
    const width = timeToX(shot.end) - left;
    block.style.left = left + 'px';
    block.style.width = width + 'px';
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

  // Camera keyframes — shot colored
  cam.keyframes.forEach((kf, ki) => {
    if (kf.time < currentShot.start || kf.time > currentShot.end) return;
    const el = document.createElement('div');
    el.className = 'keyframe';
    el.style.left = timeToX(kf.time) + 'px';
    el.style.background = currentShot.color;

    const key = `cam-${currentShotIndex}-${rowData.cameraIndex}-${ki}`;
    if (selectedKeyframe === key) el.classList.add('selected');
    el.title = `${cam.name} @ ${formatTimecode(kf.time)}`;
    el.addEventListener('click', (e) => {
      e.stopPropagation();
      selectedKeyframe = (selectedKeyframe === key) ? null : key;
      playhead = kf.time;
      render();
    });
    row.appendChild(el);
  });
}

function renderObjectRow(row, rowData, currentShotIndex) {
  const track = SCENE.tracks[rowData.trackIndex];

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

  // Object keyframes — track colored (greens)
  track.keyframes.forEach((kf, ki) => {
    const el = document.createElement('div');
    el.className = 'keyframe';
    el.style.left = timeToX(kf.time) + 'px';
    el.style.background = track.color;

    const key = `obj-${rowData.trackIndex}-${ki}`;
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

  // Tracks
  const tracksContainer = document.getElementById('minimap-tracks');
  tracksContainer.innerHTML = '';
  const rows = getVisibleRows();
  const trackH = Math.max(4, (48 - 17) / rows.length);

  rows.forEach((rowData, i) => {
    const row = document.createElement('div');
    row.className = 'minimap-track-row';
    row.style.top = (i * trackH) + 'px';
    row.style.height = trackH + 'px';

    if (rowData.type === 'camera') {
      const shot = SCENE.shots[rowData.shotIndex];
      const cam = shot.cameras[rowData.cameraIndex];
      cam.keyframes.forEach(kf => {
        const el = document.createElement('div');
        el.className = 'minimap-kf';
        el.style.left = tToX(kf.time) + 'px';
        el.style.background = shot.color;
        row.appendChild(el);
      });
    } else {
      const track = SCENE.tracks[rowData.trackIndex];
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
    }
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
  const camCount = shot.cameras.length;
  viewportShotLabel.textContent = `Shot ${si + 1}: ${shot.name}  (${camCount} cam${camCount > 1 ? 's' : ''})`;
  viewportTimecode.textContent = formatTimecode(playhead);
}

// ── Interactions ──

// Track area click/drag to move playhead
let trackDragging = false;

trackArea.addEventListener('mousedown', (e) => {
  if (e.button === 0) {
    trackDragging = true;
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
  // Shot boundary drag
  if (boundaryDragging) {
    const rect = shotBar.getBoundingClientRect();
    const x = e.clientX - rect.left;
    let newTime = xToTime(x);

    const leftShot = SCENE.shots[boundaryIndex];
    const rightShot = SCENE.shots[boundaryIndex + 1];
    const minDuration = 1;
    newTime = Math.max(leftShot.start + minDuration, Math.min(rightShot.end - minDuration, newTime));

    // Snap to frame boundaries
    newTime = Math.round(newTime * SCENE.fps) / SCENE.fps;

    leftShot.end = newTime;
    rightShot.start = newTime;

    // Show floating label
    const leftDur = leftShot.end - leftShot.start;
    const rightDur = rightShot.end - rightShot.start;
    const leftFrames = Math.round(leftDur * SCENE.fps);
    const rightFrames = Math.round(rightDur * SCENE.fps);
    resizeLabel.style.display = 'block';
    resizeLabel.style.left = (e.clientX + 14) + 'px';
    resizeLabel.style.top = (e.clientY - 24) + 'px';
    resizeLabel.textContent = `\u2190 ${leftDur.toFixed(1)}s (${leftFrames}f)  |  ${rightDur.toFixed(1)}s (${rightFrames}f) \u2192`;

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

document.getElementById('btn-zoom-fit').addEventListener('click', () => {
  viewStart = 0;
  viewEnd = SCENE.totalDuration;
  render();
});

// ── Minimap interaction ──

minimapEl.addEventListener('mousedown', (e) => {
  if (e.button !== 0) return;
  minimapDragging = true;
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
    document.getElementById('btn-zoom-fit').click();
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
