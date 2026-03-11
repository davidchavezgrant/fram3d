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

// ── Sample data ──
// Link data for objects (3.3.3)
const LINK_DATA = {
  'Sword': { parent: 'Character_A', slot: 'hand' },
};

const OBJECTS = [
  { name: 'Camera', icon: '\u25C7', tag: 'cam', selected: true },
  { name: 'Character_A', icon: '\u25CB', tag: 'actor', children: [
    { name: 'Body', icon: '\u2502', tag: 'mesh' },
    { name: 'Face_Rig', icon: '\u2502', tag: 'rig' },
  ]},
  { name: 'Character_B', icon: '\u25CB', tag: 'actor' },
  { name: 'Sword', icon: '\u25C7', tag: 'prop' },
  { name: 'Table', icon: '\u25A1', tag: 'prop' },
  { name: 'Chair_1', icon: '\u25A1', tag: 'prop' },
  { name: 'Room_Set', icon: '\u25A0', tag: 'set' },
  { name: 'Key_Light', icon: '\u2606', tag: 'light' },
  { name: 'Fill_Light', icon: '\u2606', tag: 'light' },
];

const ASSETS = [
  { name: 'Detective', icon: '\u{1F575}\uFE0F', type: 'character' },
  { name: 'Witness', icon: '\u{1F9CD}', type: 'character' },
  { name: 'Officer', icon: '\u{1F46E}', type: 'character' },
  { name: 'Revolver', icon: '\u{1F52B}', type: 'prop' },
  { name: 'Notebook', icon: '\u{1F4D3}', type: 'prop' },
  { name: 'Flashlight', icon: '\u{1F526}', type: 'prop' },
  { name: 'Radio', icon: '\u{1F4FB}', type: 'prop' },
  { name: 'Badge', icon: '\u{1FA99}', type: 'prop' },
  { name: 'Coffee Cup', icon: '\u2615', type: 'prop' },
  { name: 'File Folder', icon: '\u{1F4C1}', type: 'prop' },
  { name: 'Office Set', icon: '\u{1F3E2}', type: 'set' },
  { name: 'Alley Set', icon: '\u{1F3D9}\uFE0F', type: 'set' },
];

// ── Populate panels ──

function populateObjects() {
  const list = document.getElementById('objects-list');
  list.innerHTML = '';

  // Split objects into non-lights and lights
  const nonLights = OBJECTS.filter(o => o.tag !== 'light');
  const lights = OBJECTS.filter(o => o.tag === 'light');

  // Render non-light objects
  nonLights.forEach(obj => {
    list.appendChild(makeObjectItem(obj, false));
    if (obj.children) {
      obj.children.forEach(child => {
        list.appendChild(makeObjectItem(child, true));
      });
    }
  });

  // Lights section header (3.3.3)
  if (feat('lights-section') && lights.length > 0) {
    const header = document.createElement('div');
    header.className = 'objects-section-header';
    header.textContent = 'Lights';
    list.appendChild(header);
  }

  // Render lights
  lights.forEach(obj => {
    list.appendChild(makeObjectItem(obj, false));
  });
}

function makeObjectItem(obj, isChild) {
  const el = document.createElement('div');
  el.className = 'object-item' + (obj.selected ? ' selected' : '') + (isChild ? ' child' : '');

  let html = `<span class="obj-icon">${obj.icon}</span><span class="obj-name">${obj.name}</span>`;

  // Link indicator (3.3.3)
  if (feat('link-indicators') && LINK_DATA[obj.name]) {
    const link = LINK_DATA[obj.name];
    html += `<span class="obj-link" title="Linked to ${link.parent} (${link.slot})">\u2192 ${link.parent}</span>`;
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

function updateInspector(objectName) {
  const content = document.getElementById('inspector-content');
  if (!objectName) {
    content.innerHTML = '<div class="inspector-empty">Select an object to inspect</div>';
    return;
  }

  const track = SCENE.tracks.find(t => t.name === objectName);
  const obj = OBJECTS.find(o => o.name === objectName);

  if (objectName === 'Camera') {
    let html = `
      <div class="inspector-section">
        <div class="inspector-section-title">Camera</div>
        <div class="inspector-row"><span class="inspector-label">Lens</span><span class="inspector-value highlight">50mm</span></div>
        <div class="inspector-row"><span class="inspector-label">Aperture</span><span class="inspector-value">f/2.8</span></div>
        <div class="inspector-row"><span class="inspector-label">Focus</span><span class="inspector-value">Character_A</span></div>
        <div class="inspector-row"><span class="inspector-label">DOF</span><span class="inspector-value">On</span></div>
      </div>
      <div class="inspector-section">
        <div class="inspector-section-title">Transform</div>
        <div class="inspector-row"><span class="inspector-label">Position</span><div class="inspector-value-group"><span class="inspector-value">2.4</span><span class="inspector-value">1.6</span><span class="inspector-value">-3.2</span></div></div>
        <div class="inspector-row"><span class="inspector-label">Rotation</span><div class="inspector-value-group"><span class="inspector-value">12\u00B0</span><span class="inspector-value">-45\u00B0</span><span class="inspector-value">0\u00B0</span></div></div>
      </div>
      <div class="inspector-section">
        <div class="inspector-section-title">Current Shot</div>
        <div class="inspector-row"><span class="inspector-label">Name</span><span class="inspector-value">Wide Establishing</span></div>
        <div class="inspector-row"><span class="inspector-label">Duration</span><span class="inspector-value">6.0s (144f)</span></div>
        <div class="inspector-row"><span class="inspector-label">Keyframes</span><span class="inspector-value">3</span></div>
      </div>
    `;

    // Interpolation section (3.5.3)
    if (feat('interp-curves')) {
      html += `
        <div class="inspector-section">
          <div class="inspector-section-title">Interpolation</div>
          <div class="inspector-row"><span class="inspector-label">Position</span><span class="inspector-value">Ease In-Out</span></div>
          <div class="inspector-row"><span class="inspector-label">Rotation</span><span class="inspector-value">Linear</span></div>
          <div class="inspector-row"><span class="inspector-label">Focal</span><span class="inspector-value">Ease Out</span></div>
        </div>
      `;
    }

    content.innerHTML = html;
  } else {
    const kfCount = track ? track.keyframes.length : 0;
    const tag = obj ? obj.tag : '';
    let html = `
      <div class="inspector-section">
        <div class="inspector-section-title">${objectName}</div>
        <div class="inspector-row"><span class="inspector-label">Type</span><span class="inspector-value">${tag}</span></div>
        <div class="inspector-row"><span class="inspector-label">Keyframes</span><span class="inspector-value">${kfCount}</span></div>
        <div class="inspector-row"><span class="inspector-label">Visible</span><span class="inspector-value">Yes</span></div>
      </div>
      <div class="inspector-section">
        <div class="inspector-section-title">Transform</div>
        <div class="inspector-row"><span class="inspector-label">Position</span><div class="inspector-value-group"><span class="inspector-value">0.0</span><span class="inspector-value">0.0</span><span class="inspector-value">0.0</span></div></div>
        <div class="inspector-row"><span class="inspector-label">Rotation</span><div class="inspector-value-group"><span class="inspector-value">0\u00B0</span><span class="inspector-value">0\u00B0</span><span class="inspector-value">0\u00B0</span></div></div>
        <div class="inspector-row"><span class="inspector-label">Scale</span><div class="inspector-value-group"><span class="inspector-value">1.0</span><span class="inspector-value">1.0</span><span class="inspector-value">1.0</span></div></div>
      </div>
    `;

    // Link info in inspector (3.3.3)
    if (feat('link-indicators') && LINK_DATA[objectName]) {
      const link = LINK_DATA[objectName];
      html += `
        <div class="inspector-section">
          <div class="inspector-section-title">Link</div>
          <div class="inspector-row"><span class="inspector-label">Parent</span><span class="inspector-value">${link.parent}</span></div>
          <div class="inspector-row"><span class="inspector-label">Slot</span><span class="inspector-value">${link.slot}</span></div>
          <div class="inspector-row"><span class="inspector-label">Mode</span><span class="inspector-value">Temporal</span></div>
        </div>
      `;
    }

    // Interpolation section (3.5.3)
    if (feat('interp-curves') && kfCount > 1) {
      html += `
        <div class="inspector-section">
          <div class="inspector-section-title">Interpolation</div>
          <div class="inspector-row"><span class="inspector-label">Position</span><span class="inspector-value">Linear</span></div>
          <div class="inspector-row"><span class="inspector-label">Rotation</span><span class="inspector-value">Ease In-Out</span></div>
        </div>
      `;
    }

    content.innerHTML = html;
  }
}

// ── Panel toggle logic ──

function togglePanel(panelId) {
  if (panelId === 'objects' && !panelVisible.objects) {
    panelVisible.assets = false;
  } else if (panelId === 'assets' && !panelVisible.assets) {
    panelVisible.objects = false;
  }

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

  if (typeof render === 'function') {
    requestAnimationFrame(render);
  }
}

// ── Wire up gutter tabs ──
document.querySelectorAll('.gutter-tab, .gutter-tab-h').forEach(btn => {
  btn.addEventListener('click', () => togglePanel(btn.dataset.panel));
});

// ── Keyboard shortcuts for panels ──
document.addEventListener('keydown', (e) => {
  if (e.target.tagName === 'INPUT') return;

  // When full-shortcuts is enabled, 'A' is reserved for aspect ratio cycling
  const upper = e.key.toUpperCase();

  switch (upper) {
    case 'O': togglePanel('objects'); break;
    case 'I': togglePanel('inspector'); break;
    case 'A':
      // Only toggle assets panel if full-shortcuts is not active
      if (!feat('full-shortcuts')) togglePanel('assets');
      break;
    case 'T': togglePanel('timeline'); break;
    case 'TAB':
      e.preventDefault();
      const allHidden = !panelVisible.objects && !panelVisible.inspector && !panelVisible.assets && !panelVisible.timeline && !panelVisible.overview;
      if (allHidden) {
        panelVisible.inspector = true;
        panelVisible.timeline = true;
        panelVisible.overview = true;
      } else {
        panelVisible.objects = false;
        panelVisible.inspector = false;
        panelVisible.assets = false;
        panelVisible.timeline = false;
        panelVisible.overview = false;
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
