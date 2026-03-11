// Fram3d — Panel System
// Layered on top of global-timeline.js

// ── Panel state ──
const panelVisible = {
  objects: false,
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
  if (e.target.tagName === 'INPUT' || e.target.tagName === 'SELECT') return;
  const upper = e.key.toUpperCase();
  switch (upper) {
    case 'O': togglePanel('objects'); break;
    case 'A':
      if (!feat('full-shortcuts')) togglePanel('assets');
      break;
    case 'T': togglePanel('timeline'); break;
    case 'TAB':
      e.preventDefault();
      const allHidden = !panelVisible.objects && !panelVisible.assets && !panelVisible.timeline && !panelVisible.overview;
      if (allHidden) {
        panelVisible.timeline = true; panelVisible.overview = true;
      } else {
        panelVisible.objects = false; panelVisible.assets = false;
        panelVisible.timeline = false; panelVisible.overview = false;
      }
      applyPanelState();
      break;
  }
});

// ── Initialize ──
populateObjects();
populateAssets();
applyPanelState();
