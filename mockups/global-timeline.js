// Fram3d — Global Timeline Visualization Mockup

// ── Data model ──
const SCENE = {
  totalDuration: 18, // seconds
  fps: 24,

  shots: [
    { name: 'Wide Establishing',  start: 0,  end: 6,  color: '#3a5a7a' },
    { name: 'Over the Shoulder',  start: 6,  end: 12, color: '#5a4a6a' },
    { name: 'Close-up Reaction',  start: 12, end: 18, color: '#4a6a5a' },
  ],

  tracks: [
    {
      name: 'Camera',
      type: 'camera',
      color: '#e8a030',
      keyframes: [
        { time: 0,    shot: 0 },
        { time: 2.5,  shot: 0 },
        { time: 5.5,  shot: 0 },
        { time: 6,    shot: 1 },
        { time: 8,    shot: 1 },
        { time: 10,   shot: 1 },
        { time: 11.5, shot: 1 },
        { time: 12,   shot: 2 },
        { time: 14,   shot: 2 },
        { time: 17,   shot: 2 },
      ]
    },
    {
      name: 'Character_A',
      type: 'object',
      color: '#5588dd',
      keyframes: [
        { time: 0 },
        { time: 3 },
        { time: 5 },
        { time: 7.5 },
        { time: 10 },
        { time: 13 },
        { time: 16 },
      ],
      linkedPeriods: []
    },
    {
      name: 'Sword',
      type: 'object-alt',
      color: '#7755cc',
      keyframes: [
        { time: 0 },
        { time: 2 },
        { time: 13 },
        { time: 15.5 },
        { time: 17 },
      ],
      linkedPeriods: [
        { start: 4, end: 11, parent: 'Character_A hand' }
      ]
    },
    {
      name: 'Table',
      type: 'object',
      color: '#5588dd',
      keyframes: [
        { time: 0 },
      ],
      linkedPeriods: []
    },
  ]
};

// ── State ──
let playhead = 0;
let viewStart = 0;
let viewEnd = SCENE.totalDuration;
let playing = false;
let selectedKeyframe = null;
let animFrame = null;
let cameraMode = 'clips';
let shotBarLastClick = { index: -1, time: 0 };

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
const timeDisplay = document.getElementById('time-display');
const totalDurationEl = document.getElementById('total-duration');
const viewportShotLabel = document.getElementById('viewport-shot-label');
const viewportTime = document.getElementById('viewport-time');
const minimapEl = document.getElementById('minimap');

// ── Helpers ──

function timeToX(t) {
  const w = trackArea.clientWidth;
  return ((t - viewStart) / (viewEnd - viewStart)) * w;
}

function xToTime(x) {
  const w = trackArea.clientWidth;
  return viewStart + (x / w) * (viewEnd - viewStart);
}

function formatTime(t) {
  const min = Math.floor(t / 60);
  const sec = t % 60;
  return `${min}:${sec.toFixed(1).padStart(4, '0')}`;
}

function getCurrentShot() {
  for (let i = 0; i < SCENE.shots.length; i++) {
    if (playhead >= SCENE.shots[i].start && playhead < SCENE.shots[i].end) return i;
  }
  return SCENE.shots.length - 1;
}

function adjustAlpha(hex, factor) {
  const r = parseInt(hex.slice(1,3), 16);
  const g = parseInt(hex.slice(3,5), 16);
  const b = parseInt(hex.slice(5,7), 16);
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
  shotBar.querySelectorAll('.shot-rect').forEach(el => el.remove());
  const currentShot = getCurrentShot();

  SCENE.shots.forEach((shot, i) => {
    const el = document.createElement('div');
    el.className = 'shot-rect' + (i === currentShot ? ' active' : '');
    const left = timeToX(shot.start);
    const right = timeToX(shot.end);
    el.style.left = left + 'px';
    el.style.width = (right - left - 2) + 'px';
    el.style.background = shot.color;
    el.textContent = shot.name;
    el.addEventListener('mousedown', (e) => {
      e.stopPropagation();
      e.preventDefault();
      const now = Date.now();
      if (shotBarLastClick.index === i && now - shotBarLastClick.time < 350) {
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

  document.getElementById('shot-bar-view-range').style.display = 'none';
  shotBar.appendChild(playheadShotbar);
  playheadShotbar.style.left = timeToX(playhead) + 'px';
}

function renderRuler() {
  ruler.querySelectorAll('.ruler-tick, .ruler-label').forEach(el => el.remove());

  const duration = viewEnd - viewStart;
  let interval;
  if (duration <= 2) interval = 0.25;
  else if (duration <= 5) interval = 0.5;
  else if (duration <= 12) interval = 1;
  else if (duration <= 30) interval = 2;
  else interval = 5;

  const startTick = Math.ceil(viewStart / interval) * interval;
  for (let t = startTick; t <= viewEnd; t += interval) {
    const x = timeToX(t);
    if (x < -10 || x > ruler.clientWidth + 10) continue;

    const isMajor = Math.abs(t - Math.round(t)) < 0.01;
    const tick = document.createElement('div');
    tick.className = 'ruler-tick';
    tick.style.left = x + 'px';
    tick.style.height = isMajor ? '22px' : '10px';
    tick.style.bottom = '0';
    ruler.appendChild(tick);

    if (isMajor || interval < 1) {
      const label = document.createElement('div');
      label.className = 'ruler-label';
      label.style.left = x + 'px';
      label.textContent = formatTime(t);
      ruler.appendChild(label);
    }
  }

  playheadRuler.style.left = timeToX(playhead) + 'px';
}

function renderTracks() {
  trackLabels.innerHTML = '';
  trackArea.querySelectorAll(':not(#playhead)').forEach(el => el.remove());

  const currentShot = getCurrentShot();

  SCENE.tracks.forEach((track, i) => {
    // Label
    const label = document.createElement('div');
    label.className = 'track-label';
    label.innerHTML = `<div class="dot" style="background:${track.color}"></div><div class="name">${track.name}</div>`;
    trackLabels.appendChild(label);

    // Track row
    const row = document.createElement('div');
    row.className = 'track-row';
    row.style.top = (i * 36) + 'px';
    trackArea.appendChild(row);

    if (track.type === 'camera') {
      renderCameraTrack(row, track, i, currentShot);
    } else {
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
          const region = document.createElement('div');
          region.className = 'linked-region';
          const x1 = timeToX(lp.start);
          const x2 = timeToX(lp.end);
          region.style.left = x1 + 'px';
          region.style.width = (x2 - x1) + 'px';
          region.title = `Linked to ${lp.parent} — unlink to keyframe independently`;

          const linkLabel = document.createElement('div');
          linkLabel.className = 'link-label';
          if (x2 - x1 > 80) {
            linkLabel.textContent = `→ ${lp.parent}`;
          }
          region.appendChild(linkLabel);
          row.appendChild(region);

          [lp.start, lp.end].forEach(t => {
            const kf = document.createElement('div');
            kf.className = 'keyframe link-boundary';
            kf.style.left = timeToX(t) + 'px';
            kf.title = `Link boundary at ${formatTime(t)}`;
            row.appendChild(kf);
          });
        });
      }

      // Object keyframes
      track.keyframes.forEach((kf, ki) => {
        const el = document.createElement('div');
        el.className = 'keyframe ' + track.type;
        el.style.left = timeToX(kf.time) + 'px';

        const key = `${i}-${ki}`;
        if (selectedKeyframe === key) el.classList.add('selected');

        el.title = `${track.name} keyframe at ${formatTime(kf.time)}`;
        el.addEventListener('click', (e) => {
          e.stopPropagation();
          selectedKeyframe = (selectedKeyframe === key) ? null : key;
          playhead = kf.time;
          render();
        });
        row.appendChild(el);
      });
    }
  });

  const totalHeight = SCENE.tracks.length * 36;
  trackArea.style.height = totalHeight + 'px';
  trackLabels.style.height = totalHeight + 'px';
}

function renderCameraTrack(row, track, trackIndex, currentShot) {
  // Shot background tinting (all modes)
  SCENE.shots.forEach((shot, si) => {
    const bg = document.createElement('div');
    bg.className = 'shot-bg' + (si === currentShot ? ' active-shot' : '');
    bg.style.left = timeToX(shot.start) + 'px';
    bg.style.width = (timeToX(shot.end) - timeToX(shot.start)) + 'px';
    bg.style.background = shot.color;
    row.appendChild(bg);
  });

  if (cameraMode === 'clips') {
    // Each shot's camera is a rounded clip container
    SCENE.shots.forEach((shot, si) => {
      const clip = document.createElement('div');
      clip.className = 'camera-clip' + (si === currentShot ? ' active-clip' : '');
      clip.style.left = timeToX(shot.start) + 'px';
      clip.style.width = (timeToX(shot.end) - timeToX(shot.start)) + 'px';

      const label = document.createElement('div');
      label.className = 'clip-label';
      label.textContent = 'CAM';
      clip.appendChild(label);
      row.appendChild(clip);
    });

    track.keyframes.forEach((kf, ki) => {
      const el = document.createElement('div');
      el.className = 'keyframe camera';
      el.style.left = timeToX(kf.time) + 'px';
      if (kf.shot !== currentShot) el.style.opacity = '0.35';

      const key = `${trackIndex}-${ki}`;
      if (selectedKeyframe === key) el.classList.add('selected');
      el.title = `Camera keyframe at ${formatTime(kf.time)}`;
      el.addEventListener('click', (e) => {
        e.stopPropagation();
        selectedKeyframe = (selectedKeyframe === key) ? null : key;
        playhead = kf.time;
        render();
      });
      row.appendChild(el);
    });

  } else if (cameraMode === 'splice') {
    // Splice marks at shot boundaries
    SCENE.shots.forEach(shot => {
      if (shot.start > 0) {
        const splice = document.createElement('div');
        splice.className = 'splice-mark';
        splice.style.left = timeToX(shot.start) + 'px';
        const line = document.createElement('div');
        line.className = 'splice-mark-line';
        splice.appendChild(line);
        row.appendChild(splice);
      }
    });

    track.keyframes.forEach((kf, ki) => {
      const el = document.createElement('div');
      el.className = 'keyframe camera';
      el.style.left = timeToX(kf.time) + 'px';
      if (kf.shot !== currentShot) el.style.opacity = '0.35';

      const key = `${trackIndex}-${ki}`;
      if (selectedKeyframe === key) el.classList.add('selected');
      el.title = `Camera keyframe at ${formatTime(kf.time)}`;
      el.addEventListener('click', (e) => {
        e.stopPropagation();
        selectedKeyframe = (selectedKeyframe === key) ? null : key;
        playhead = kf.time;
        render();
      });
      row.appendChild(el);
    });

  } else if (cameraMode === 'shapes') {
    // Different shape for shot-start keyframes
    SCENE.shots.forEach(shot => {
      if (shot.start > 0) {
        const line = document.createElement('div');
        line.className = 'shot-boundary';
        line.style.left = timeToX(shot.start) + 'px';
        row.appendChild(line);
      }
    });

    const shotStartTimes = new Set(SCENE.shots.map(s => s.start));

    track.keyframes.forEach((kf, ki) => {
      const isStart = shotStartTimes.has(kf.time);
      const el = document.createElement('div');

      if (isStart) {
        el.className = 'keyframe camera-start';
      } else {
        el.className = 'keyframe camera';
      }
      el.style.left = timeToX(kf.time) + 'px';
      if (kf.shot !== currentShot) el.style.opacity = '0.35';

      const key = `${trackIndex}-${ki}`;
      if (selectedKeyframe === key) el.classList.add('selected');
      el.title = `Camera ${isStart ? 'start' : 'keyframe'} at ${formatTime(kf.time)}`;
      el.addEventListener('click', (e) => {
        e.stopPropagation();
        selectedKeyframe = (selectedKeyframe === key) ? null : key;
        playhead = kf.time;
        render();
      });
      row.appendChild(el);
    });

  } else if (cameraMode === 'hide') {
    // Only show current shot's keyframes
    SCENE.shots.forEach(shot => {
      if (shot.start > 0) {
        const line = document.createElement('div');
        line.className = 'shot-boundary';
        line.style.left = timeToX(shot.start) + 'px';
        row.appendChild(line);
      }
    });

    SCENE.shots.forEach((shot, si) => {
      if (si !== currentShot) {
        const block = document.createElement('div');
        block.style.position = 'absolute';
        block.style.top = '6px';
        block.style.bottom = '6px';
        block.style.left = timeToX(shot.start) + 'px';
        block.style.width = (timeToX(shot.end) - timeToX(shot.start)) + 'px';
        block.style.background = 'rgba(232,160,48,0.12)';
        block.style.borderRadius = '3px';
        block.style.border = '1px solid rgba(232,160,48,0.15)';
        row.appendChild(block);
      }
    });

    track.keyframes.forEach((kf, ki) => {
      if (kf.shot !== currentShot) return;
      const el = document.createElement('div');
      el.className = 'keyframe camera';
      el.style.left = timeToX(kf.time) + 'px';

      const key = `${trackIndex}-${ki}`;
      if (selectedKeyframe === key) el.classList.add('selected');
      el.title = `Camera keyframe at ${formatTime(kf.time)}`;
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

function renderPlayhead() {
  playheadEl.style.left = timeToX(playhead) + 'px';
  timeDisplay.textContent = formatTime(playhead);
  totalDurationEl.textContent = formatTime(SCENE.totalDuration);
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
  const w = minimapEl.clientWidth;
  const tToX = (t) => (t / SCENE.totalDuration) * w;

  // Shots
  const shotsContainer = document.getElementById('minimap-shots');
  shotsContainer.innerHTML = '';
  const currentShot = getCurrentShot();

  SCENE.shots.forEach((shot, i) => {
    const el = document.createElement('div');
    el.className = 'minimap-shot';
    const left = tToX(shot.start);
    const right = tToX(shot.end);
    el.style.left = left + 'px';
    el.style.width = (right - left - 1) + 'px';
    el.style.background = i === currentShot ? shot.color : adjustAlpha(shot.color, 0.6);
    if (right - left > 40) el.textContent = shot.name;
    shotsContainer.appendChild(el);
  });

  // Tracks with keyframes
  const tracksContainer = document.getElementById('minimap-tracks');
  tracksContainer.innerHTML = '';
  const trackCount = SCENE.tracks.length;
  const trackH = (48 - 17) / trackCount;

  SCENE.tracks.forEach((track, i) => {
    const row = document.createElement('div');
    row.className = 'minimap-track-row';
    row.style.top = (i * trackH) + 'px';
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
  viewportShotLabel.textContent = `Shot ${si + 1}: ${shot.name}`;
  viewportTime.textContent = formatTime(playhead);
}

// ── Interactions ──

// Click track area to move playhead
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

// Middle-drag on shot bar to pan
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

// Scroll to zoom (track area, ruler, shot bar)
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

// Global mousemove / mouseup for all drag operations
let zoomDragging = false;
let zoomDragStartX = 0;
let zoomDragStartViewStart = 0;
let minimapDragging = false;

document.addEventListener('mousemove', (e) => {
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

// Drag zoom thumb
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
  document.getElementById('btn-play').textContent = playing ? '\u23F8 Pause' : '\u25B6 Play';
  if (playing) animLoop();
});

document.getElementById('btn-prev-shot').addEventListener('click', () => {
  const si = getCurrentShot();
  const target = Math.max(0, si - 1);
  const shot = SCENE.shots[target];
  playhead = shot.start;
  const duration = viewEnd - viewStart;
  if (duration <= shot.end - shot.start) {
    viewStart = shot.start;
    viewEnd = shot.end;
  } else {
    const mid = (shot.start + shot.end) / 2;
    viewStart = mid - duration / 2;
    viewEnd = mid + duration / 2;
  }
  clampView();
  render();
});

document.getElementById('btn-next-shot').addEventListener('click', () => {
  const si = getCurrentShot();
  const target = Math.min(SCENE.shots.length - 1, si + 1);
  const shot = SCENE.shots[target];
  playhead = shot.start;
  const duration = viewEnd - viewStart;
  if (duration <= shot.end - shot.start) {
    viewStart = shot.start;
    viewEnd = shot.end;
  } else {
    const mid = (shot.start + shot.end) / 2;
    viewStart = mid - duration / 2;
    viewEnd = mid + duration / 2;
  }
  clampView();
  viewEnd = SCENE.shots[target].end;
  render();
});

document.getElementById('btn-zoom-fit').addEventListener('click', () => {
  viewStart = 0;
  viewEnd = SCENE.totalDuration;
  render();
});

// ── Mode switcher ──

document.querySelectorAll('.mode-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    cameraMode = btn.dataset.mode;
    document.querySelectorAll('.mode-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    render();
  });
});

document.addEventListener('keydown', (e) => {
  if (e.key >= '1' && e.key <= '4') {
    const modes = ['clips', 'splice', 'shapes', 'hide'];
    const idx = parseInt(e.key) - 1;
    cameraMode = modes[idx];
    document.querySelectorAll('.mode-btn').forEach(b => b.classList.remove('active'));
    document.querySelector(`[data-mode="${modes[idx]}"]`).classList.add('active');
    render();
  }
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

// Prevent middle-click scroll/paste on timeline elements
trackArea.addEventListener('auxclick', (e) => e.preventDefault());
ruler.addEventListener('auxclick', (e) => e.preventDefault());
shotBar.addEventListener('auxclick', (e) => e.preventDefault());
minimapEl.addEventListener('auxclick', (e) => e.preventDefault());

// ── Keyboard ──

document.addEventListener('keydown', (e) => {
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
