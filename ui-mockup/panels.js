// Fram3d — Panel System
// Layered on top of global-timeline.js

// ── Panel state ──
const panelVisible = {
  elements: false,
  assets: false,
  timeline: true,
  overview: true,
};

// ── Link data (6.3.3) ──
const LINK_DATA = {
  'Evidence Folder': { parent: 'Detective', slot: 'lap' },
};

// ── Sample data ──
const ELEMENTS = [
  { name: 'Camera', icon: '◇', tag: 'cam', selected: true },
  { name: 'Detective', icon: '○', tag: 'character', children: [
    { name: 'Body', icon: '│', tag: 'mesh' },
    { name: 'Face_Rig', icon: '│', tag: 'rig' },
  ]},
  { name: 'Witness', icon: '○', tag: 'character' },
  { name: 'Evidence Folder', icon: '◇', tag: 'prop' },
  { name: 'Table', icon: '□', tag: 'prop' },
  { name: 'Room_Set', icon: '■', tag: 'environment' },
  { name: 'Key Light', icon: '☆', tag: 'light' },
  { name: 'Fill Light', icon: '☆', tag: 'light' },
];

const ASSETS = [
  { name: 'Detective', icon: '🕵️', type: 'character' },
  { name: 'Witness', icon: '🧍', type: 'character' },
  { name: 'Officer', icon: '👮', type: 'character' },
  { name: 'Revolver', icon: '🔫', type: 'object' },
  { name: 'Notebook', icon: '📓', type: 'object' },
  { name: 'Flashlight', icon: '🔦', type: 'object' },
  { name: 'Radio', icon: '📻', type: 'object' },
  { name: 'Badge', icon: '🪙', type: 'object' },
  { name: 'Coffee Cup', icon: '☕', type: 'object' },
  { name: 'File Folder', icon: '📁', type: 'object' },
  { name: 'Office Set', icon: '🏢', type: 'environment' },
  { name: 'Alley Set', icon: '🏙️', type: 'environment' },
];

// ── Populate panels ──

function populateElements() {
  const list = document.getElementById('elements-list');
  list.innerHTML = '';
  const nonLights = ELEMENTS.filter(o => o.tag !== 'light');
  const lights = ELEMENTS.filter(o => o.tag === 'light');

  nonLights.forEach(item => {
    list.appendChild(makeElementItem(item, false));
    if (item.children) {
      item.children.forEach(child => list.appendChild(makeElementItem(child, true)));
    }
  });

  if (feat('lights-section') && lights.length > 0) {
    const header = document.createElement('div');
    header.className = 'elements-section-header';
    header.textContent = 'Lights';
    list.appendChild(header);
  }
  lights.forEach(item => list.appendChild(makeElementItem(item, false)));
}

function makeElementItem(item, isChild) {
  const el = document.createElement('div');
  el.className = 'element-item' + (item.selected ? ' selected' : '') + (isChild ? ' child' : '');
  let html = `<span class="el-icon">${item.icon}</span><span class="el-name">${item.name}</span>`;
  if (feat('link-indicators') && LINK_DATA[item.name]) {
    const link = LINK_DATA[item.name];
    html += `<span class="el-link" title="Linked to ${link.parent} (${link.slot})">→ ${link.parent}</span>`;
  }
  html += `<span class="el-tag">${item.tag}</span>`;
  el.innerHTML = html;
  el.addEventListener('click', () => {
    document.querySelectorAll('.element-item.selected').forEach(s => s.classList.remove('selected'));
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
  if (panelId === 'elements' && !panelVisible.elements) panelVisible.assets = false;
  else if (panelId === 'assets' && !panelVisible.assets) panelVisible.elements = false;
  panelVisible[panelId] = !panelVisible[panelId];
  applyPanelState();
}

function applyPanelState() {
  const mapping = {
    elements: document.getElementById('elements-panel'),
    assets: document.getElementById('assets-panel'),
    timeline: document.getElementById('timeline-section'),
    overview: document.getElementById('overview-container'),
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
    case 'O': togglePanel('elements'); break;
    case 'A':
      if (!feat('full-shortcuts')) togglePanel('assets');
      break;
    case 'T': togglePanel('timeline'); break;
    case 'TAB':
      e.preventDefault();
      const allHidden = !panelVisible.elements && !panelVisible.assets && !panelVisible.timeline && !panelVisible.overview;
      if (allHidden) {
        panelVisible.timeline = true; panelVisible.overview = true;
      } else {
        panelVisible.elements = false; panelVisible.assets = false;
        panelVisible.timeline = false; panelVisible.overview = false;
      }
      applyPanelState();
      break;
  }
});

// ── Initialize ──
populateElements();
populateAssets();
applyPanelState();
