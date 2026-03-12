// Fram3d — Feature Toggle System
// Each feature can be toggled on/off to show UI growth over milestones.

const FEATURES = {
  // 1.1 Virtual Camera
  'camera-info':        { name: 'Camera Info',                   milestone: '1.1', section: '1.1.3', enabled: true },

  // 1.2 Camera Overlays
  'aspect-ratio':       { name: 'Aspect Ratio Masks',            milestone: '1.2', section: '1.2.1', enabled: true },
  'composition-guides': { name: 'Composition Guides',            milestone: '1.2', section: '1.2.2', enabled: true },

  // 2.1 Scene Management
  'active-tool':        { name: 'Active Tool Indicator (QWER)',  milestone: '2.1', section: '2.1.2', enabled: true },
  'director-view':      { name: 'Director View Toggle',          milestone: '2.1', section: '2.1.5', enabled: true },

  // 3.1 Shot Track
  'shot-management':    { name: 'Shot Add / Delete',             milestone: '3.1', section: '3.1.2', enabled: true },
  'shot-reorder':       { name: 'Shot Drag Reorder',             milestone: '3.1', section: '3.1.2', enabled: true },
  'duration-edit':      { name: 'Duration Editing',              milestone: '3.1', section: '3.1.2', enabled: true },
  'aggregate-duration': { name: 'Aggregate Duration',            milestone: '3.1', section: '3.1.2', enabled: true },
  'hover-thumbnails':   { name: 'Shot Hover Thumbnails',         milestone: '3.1', section: '3.1.2', enabled: true },
  'scenes':             { name: 'Scenes Switcher',               milestone: '3.1', section: '3.1.4', enabled: true },

  // 3.2 Keyframe Animation
  'transport-bar':      { name: 'Transport Bar',                 milestone: '3.2', section: '3.2.1', enabled: true },
  'status-bar':         { name: 'Status Bar (Keyboard Hints)',   milestone: '3.2', section: '3.2.1', enabled: true },
  'track-expand':       { name: 'Track Collapse / Expand',       milestone: '3.2', section: '3.2.2', enabled: true },
  'keyframe-drag':      { name: 'Keyframe Drag',                 milestone: '3.2', section: '3.2.3', enabled: true },
  'camera-path':        { name: 'Camera Path Toggle',            milestone: '3.2', section: '3.2.6', enabled: true },

  // 2.2 View Layout System
  'full-shortcuts':     { name: 'Panel Layout Switcher',         milestone: '2.2', section: '2.2.1', enabled: true },

  // 6.3 Element Linking & Grouping
  'link-indicators':    { name: 'Link Indicators in Panel',      milestone: '6.3', section: '6.3.3', enabled: true },
  'lights-section':     { name: 'Separate Lights Section',       milestone: '6.3', section: '6.3.3', enabled: true },

  // 8.1 Selection Refinements
  'interp-curves':      { name: 'Interpolation Curve Presets',   milestone: '8.1', section: '8.1.3', enabled: true },

  // 9.1 Multi-camera
  'active-cam-keys':    { name: 'Active Camera Shortcuts',       milestone: '9.1', section: '9.1.3', enabled: true },
  'active-angle-track': { name: 'Active Angle Track',            milestone: '9.1', section: '9.1.4', enabled: true },
  'multi-split':        { name: 'Multi-split',                   milestone: '9.1', section: '9.1.5', enabled: true },
};

function feat(id) {
  return FEATURES[id] && FEATURES[id].enabled;
}

function setFeature(id, enabled) {
  if (FEATURES[id]) {
    FEATURES[id].enabled = enabled;
    if (typeof applyFeatureVisibility === 'function') applyFeatureVisibility();
    if (typeof render === 'function') render();
  }
}

function setMilestone(milestone, enabled) {
  Object.values(FEATURES).forEach(f => {
    if (f.milestone === milestone) f.enabled = enabled;
  });
  if (typeof applyFeatureVisibility === 'function') applyFeatureVisibility();
  if (typeof render === 'function') render();
}

function getMilestones() {
  const milestones = {};
  Object.entries(FEATURES).forEach(([id, f]) => {
    if (!milestones[f.milestone]) milestones[f.milestone] = [];
    milestones[f.milestone].push({ id, ...f });
  });
  return milestones;
}
