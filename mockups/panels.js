// Fram3d — Panel System
// Layered on top of global-timeline.js

// ── Panel state ──
const panelVisible = {
  objects: false,
  inspector: true,
  assets: false,
  timeline: true,
  overview: true,
};

// ── Link data (3.3.3) ──
const LINK_DATA = {
  'Evidence Folder': { parent: 'Detective', slot: 'lap' },
};

// ── Sample data ──
const OBJECTS = [
  { name: 'Camera', icon: '◇', tag: 'cam', selected: true },
  { name: 'Detective', icon: '○', tag: 'actor', children: [
    { name: 'Body', icon: '│', tag: 'mesh' },
    { name: 'Face_Rig', icon: '│', tag: 'rig' },
  ]},
  { name: 'Witness', icon: '○', tag: 'actor' },
  { name: 'Evidence Folder', icon: '◇', tag: 'prop' },
  { name: 'Table', icon: '□', tag: 'prop' },
  { name: 'Room_Set', icon: '■', tag: 'set' },
  { name: 'Key Light', icon: '☆', tag: 'light' },
  { name: 'Fill Light', icon: '☆', tag: 'light' },
];

const ASSETS = [
  { name: 'Detective', icon: '🕵️', type: 'character' },
  { name: 'Witness', icon: '🧍', type: 'character' },
  { name: 'Officer', icon: '👮', type: 'character' },
  { name: 'Revolver', icon: '🔫', type: 'prop' },
  { name: 'Notebook', icon: '📓', type: 'prop' },
  { name: 'Flashlight', icon: '🔦', type: 'prop' },
  { name: 'Radio', icon: '📻', type: 'prop' },
  { name: 'Badge', icon: '🪙', type: 'prop' },
  { name: 'Coffee Cup', icon: '☕', type: 'prop' },
  { name: 'File Folder', icon: '📁', type: 'prop' },
  { name: 'Office Set', icon: '🏢', type: 'set' },
  { name: 'Alley Set', icon: '🏙️', type: 'set' },
];

// ── Keyframe state for stopwatch UI (mock) ──
const keyframedProps = {
  'Camera': { pos: true, rot: true, focal: true },
  'Detective': { pos: true, rot: true, scale: false },
  'Witness': { pos: true, rot: true, scale: false },
  'Evidence Folder': { pos: true, rot: false, scale: false },
  'Table': { pos: false, rot: false, scale: false },
  'Key Light': { pos: true, rot: true, scale: true },
};

// ── Populate panels ──

function populateObjects() {
  const list = document.getElementById('objects-list');
  list.innerHTML = '';
  const nonLights = OBJECTS.filter(o => o.tag !== 'light');
  const lights = OBJECTS.filter(o => o.tag === 'light');

  nonLights.forEach(obj => {
    list.appendChild(makeObjectItem(obj, false));
    if (obj.children) {
      obj.children.forEach(child => list.appendChild(makeObjectItem(child, true)));
    }
  });

  if (feat('lights-section') && lights.length > 0) {
    const header = document.createElement('div');
    header.className = 'objects-section-header';
    header.textContent = 'Lights';
    list.appendChild(header);
  }
  lights.forEach(obj => list.appendChild(makeObjectItem(obj, false)));
}

function makeObjectItem(obj, isChild) {
  const el = document.createElement('div');
  el.className = 'object-item' + (obj.selected ? ' selected' : '') + (isChild ? ' child' : '');
  let html = `<span class="obj-icon">${obj.icon}</span><span class="obj-name">${obj.name}</span>`;
  if (feat('link-indicators') && LINK_DATA[obj.name]) {
    const link = LINK_DATA[obj.name];
    html += `<span class="obj-link" title="Linked to ${link.parent} (${link.slot})">→ ${link.parent}</span>`;
  }
  html += `<span class="obj-tag">${obj.tag}</span>`;
  el.innerHTML = html;
  el.addEventListener('click', () => {
    document.querySelectorAll('.object-item.selected').forEach(s => s.classList.remove('selected'));
    el.classList.add('selected');
    updateInspector(obj.name);
  });
  return el;
}

function populateAssets() {
  const grid = document.getElementById('assets-grid');
  grid.innerHTML = '';
  const container = document.createElement('div');
  container.className = 'asset-grid';
  ASSETS.forEach(asset => {
    const el = document.createElement('div');
    el.className = 'asset-item';
    el.innerHTML = `<span class="asset-icon">${asset.icon}</span><span class="asset-name">${asset.name}</span><span class="asset-type">${asset.type}</span>`;
    container.appendChild(el);
  });
  grid.appendChild(container);
}

// ── Stopwatch icon helper ──
function swIcon(objName, prop) {
  const kf = keyframedProps[objName];
  const active = kf && kf[prop];
  const cls = active ? 'kf-stopwatch active' : 'kf-stopwatch';
  return `<span class="${cls}" data-obj="${objName}" data-prop="${prop}" title="${active?'Keyframed':'Not keyframed'}">⏱</span>`;
}

// ── Inspector with live values ──
function updateInspector(objectName) {
  const content = document.getElementById('inspector-content');
  if (!objectName) {
    content.innerHTML = '<div class="inspector-empty">Select an object to inspect</div>';
    return;
  }

  const track = SCENE.tracks.find(t => t.name === objectName);
  const obj = OBJECTS.find(o => o.name === objectName);
  const t = typeof playhead !== 'undefined' ? playhead : 0;

  if (objectName === 'Camera') {
    const si = typeof getCurrentShot === 'function' ? getCurrentShot() : 0;
    const display = typeof getDisplayCamera === 'function' ? getDisplayCamera(si) : null;
    const cam = display ? display.cam : null;
    const kfs = cam ? cam.keyframes : [];

    const pos = interpolateTrack(kfs, t, 'pos') || [0,0,0];
    const rot = interpolateTrack(kfs, t, 'rot') || [0,0,0];
    const focal = interpolateTrack(kfs, t, 'focal') || 50;
    const shot = SCENE.shots[si];

    let html = `
      <div class="inspector-section">
        <div class="inspector-section-title">Camera</div>
        <div class="inspector-row">${swIcon('Camera','focal')}<span class="inspector-label">Lens</span><span class="inspector-value highlight">${focal.toFixed(0)}mm</span></div>
        <div class="inspector-row">${swIcon('Camera','focal')}<span class="inspector-label">Aperture</span><span class="inspector-value">f/2.8</span></div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Focus</span><span class="inspector-value">Detective</span></div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">DOF</span><span class="inspector-value">On</span></div>
      </div>
      <div class="inspector-section">
        <div class="inspector-section-title">Transform</div>
        <div class="inspector-row">${swIcon('Camera','pos')}<span class="inspector-label">Position</span><span class="inspector-value tuple">${fmtTuple(pos)}</span></div>
        <div class="inspector-row">${swIcon('Camera','rot')}<span class="inspector-label">Rotation</span><span class="inspector-value tuple">${rot.map(v=>v.toFixed(1)+'°').join(', ')}</span></div>
      </div>
      <div class="inspector-section">
        <div class="inspector-section-title">Current Shot</div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Name</span><span class="inspector-value">${shot.name}</span></div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Duration</span><span class="inspector-value">${(shot.end-shot.start).toFixed(1)}s (${Math.round((shot.end-shot.start)*SCENE.fps)}f)</span></div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Keyframes</span><span class="inspector-value">${kfs.length}</span></div>
      </div>
    `;

    if (feat('interp-curves')) {
      html += `
        <div class="inspector-section">
          <div class="inspector-section-title">Interpolation</div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Position</span><span class="inspector-value">Ease In-Out</span></div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Rotation</span><span class="inspector-value">Linear</span></div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Focal</span><span class="inspector-value">Ease Out</span></div>
        </div>
      `;
    }
    content.innerHTML = html;
  } else {
    const kfs = track ? track.keyframes : [];
    const kfCount = kfs.length;
    const tag = obj ? obj.tag : '';
    const pos = interpolateTrack(kfs, t, 'pos') || [0,0,0];
    const rot = interpolateTrack(kfs, t, 'rot') || [0,0,0];
    const scale = interpolateTrack(kfs, t, 'scale') || [1,1,1];

    let html = `
      <div class="inspector-section">
        <div class="inspector-section-title">${objectName}</div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Type</span><span class="inspector-value">${tag}</span></div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Keyframes</span><span class="inspector-value">${kfCount}</span></div>
        <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Visible</span><span class="inspector-value">Yes</span></div>
      </div>
      <div class="inspector-section">
        <div class="inspector-section-title">Transform</div>
        <div class="inspector-row">${swIcon(objectName,'pos')}<span class="inspector-label">Position</span><span class="inspector-value tuple">${fmtTuple(pos)}</span></div>
        <div class="inspector-row">${swIcon(objectName,'rot')}<span class="inspector-label">Rotation</span><span class="inspector-value tuple">${rot.map(v=>v.toFixed(1)+'°').join(', ')}</span></div>
        <div class="inspector-row">${swIcon(objectName,'scale')}<span class="inspector-label">Scale</span><span class="inspector-value tuple">${fmtTuple(scale)}</span></div>
      </div>
    `;

    if (feat('link-indicators') && LINK_DATA[objectName]) {
      const link = LINK_DATA[objectName];
      html += `
        <div class="inspector-section">
          <div class="inspector-section-title">Link</div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Parent</span><span class="inspector-value">${link.parent}</span></div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Slot</span><span class="inspector-value">${link.slot}</span></div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Mode</span><span class="inspector-value">Temporal</span></div>
        </div>
      `;
    }

    if (feat('interp-curves') && kfCount > 1) {
      html += `
        <div class="inspector-section">
          <div class="inspector-section-title">Interpolation</div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Position</span><span class="inspector-value">Linear</span></div>
          <div class="inspector-row"><span class="kf-stopwatch">&nbsp;</span><span class="inspector-label">Rotation</span><span class="inspector-value">Ease In-Out</span></div>
        </div>
      `;
    }
    content.innerHTML = html;
  }

  // Wire up stopwatch clicks
  content.querySelectorAll('.kf-stopwatch[data-obj]').forEach(sw => {
    sw.addEventListener('click', () => {
      const obj = sw.dataset.obj;
      const prop = sw.dataset.prop;
      if (!keyframedProps[obj]) keyframedProps[obj] = {};
      keyframedProps[obj][prop] = !keyframedProps[obj][prop];
      sw.classList.toggle('active');
    });
  });
}

// ── Update inspector on playhead move ──
let lastInspectorUpdate = 0;
const origRender = typeof render === 'function' ? render : null;
// Periodically refresh inspector values
setInterval(() => {
  if (typeof playhead !== 'undefined' && document.getElementById('inspector-content')) {
    const selected = document.querySelector('.object-item.selected .obj-name');
    if (selected && Date.now() - lastInspectorUpdate > 100) {
      lastInspectorUpdate = Date.now();
      updateInspector(selected.textContent);
    }
  }
}, 150);

// ── Panel toggle logic ──

function togglePanel(panelId) {
  if (panelId === 'objects' && !panelVisible.objects) panelVisible.assets = false;
  else if (panelId === 'assets' && !panelVisible.assets) panelVisible.objects = false;
  panelVisible[panelId] = !panelVisible[panelId];
  applyPanelState();
}

function applyPanelState() {
  const mapping = {
    objects: document.getElementById('objects-panel'),
    inspector: document.getElementById('inspector-panel'),
    assets: document.getElementById('assets-panel'),
    timeline: document.getElementById('timeline-section'),
    overview: document.getElementById('minimap-container'),
  };
  for (const [id, el] of Object.entries(mapping)) {
    if (!el) continue;
    el.classList.toggle('hidden', !panelVisible[id]);
  }
  document.querySelectorAll('.gutter-tab, .gutter-tab-h').forEach(btn => {
    const panel = btn.dataset.panel;
    btn.classList.toggle('active', panelVisible[panel]);
  });
  if (typeof render === 'function') requestAnimationFrame(render);
  if (typeof updateViewportFrame === 'function') requestAnimationFrame(updateViewportFrame);
}

// ── Wire up gutter tabs ──
document.querySelectorAll('.gutter-tab, .gutter-tab-h').forEach(btn => {
  btn.addEventListener('click', () => togglePanel(btn.dataset.panel));
});

// ── Keyboard shortcuts for panels ──
document.addEventListener('keydown', (e) => {
  if (e.target.tagName === 'INPUT') return;
  const upper = e.key.toUpperCase();
  switch (upper) {
    case 'O': togglePanel('objects'); break;
    case 'I': togglePanel('inspector'); break;
    case 'A':
      if (!feat('full-shortcuts')) togglePanel('assets');
      break;
    case 'T': togglePanel('timeline'); break;
    case 'TAB':
      e.preventDefault();
      const allHidden = !panelVisible.objects && !panelVisible.inspector && !panelVisible.assets && !panelVisible.timeline && !panelVisible.overview;
      if (allHidden) {
        panelVisible.inspector = true; panelVisible.timeline = true; panelVisible.overview = true;
      } else {
        panelVisible.objects = false; panelVisible.inspector = false; panelVisible.assets = false;
        panelVisible.timeline = false; panelVisible.overview = false;
      }
      applyPanelState();
      break;
  }
});

// ── Initialize ──
populateObjects();
populateAssets();
updateInspector('Camera');
applyPanelState();
