// Fram3d — Global Timeline Visualization Mockup
// All features gated by feat() from features.js

// ── Scene data (3 scenes with realistic shot/camera data) ──

const ALL_SCENES = [
  // ─── Scene 1: The Interrogation ───
  {
    name: 'The Interrogation',
    location: 'INT. INTERROGATION ROOM — NIGHT',
    shots: [
      { name: 'WIDE ESTABLISHING', start: 0, end: 3, color: '#9a5555',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 0, pos: [3.5, 2.2, -3.0], rot: [15, -35, 0], focal: 24 },
            { time: 2.8, pos: [3.2, 2.0, -2.8], rot: [12, -32, 0], focal: 24 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 0.5, pos: [-1.8, 1.6, -2.5], rot: [5, 60, 0], focal: 35 },
            { time: 2.5, pos: [-1.2, 1.5, -1.8], rot: [3, 55, 0], focal: 35 },
          ]},
        ],
        coverage: [{ camera: 0, start: 0, end: 3 }],
      },
      { name: 'MED DET ENTERS', start: 3, end: 7, color: '#5577aa',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 3, pos: [-1.0, 1.5, -1.0], rot: [0, 80, 0], focal: 50 },
            { time: 5, pos: [-0.5, 1.4, -0.5], rot: [0, 60, 0], focal: 50 },
            { time: 6.5, pos: [-0.3, 1.4, -0.8], rot: [-2, 50, 0], focal: 50 },
          ]},
        ],
        coverage: [{ camera: 0, start: 3, end: 7 }],
      },
      { name: 'OTS DET→WIT', start: 7, end: 10.5, color: '#aa8844',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 7, pos: [-0.3, 1.5, -0.8], rot: [5, 15, 0], focal: 65 },
            { time: 10, pos: [-0.2, 1.5, -0.7], rot: [3, 12, 0], focal: 65 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 7.5, pos: [0.3, 1.5, 0.8], rot: [5, -165, 0], focal: 65 },
            { time: 10, pos: [0.2, 1.5, 0.7], rot: [3, -162, 0], focal: 65 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 7, end: 8.8 },
          { camera: 1, start: 8.8, end: 10.5 },
        ],
      },
      { name: 'CU WITNESS REACT', start: 10.5, end: 13, color: '#8855aa',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 10.5, pos: [0.0, 1.3, 0.5], rot: [0, -180, 0], focal: 85 },
            { time: 12.8, pos: [0.0, 1.3, 0.4], rot: [-1, -180, 0], focal: 85 },
          ]},
        ],
        coverage: [{ camera: 0, start: 10.5, end: 13 }],
      },
      { name: 'TWO-SHOT TABLE', start: 13, end: 18, color: '#aa5577',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 13, pos: [2.0, 1.4, 0.0], rot: [5, -90, 0], focal: 35 },
            { time: 15, pos: [1.8, 1.3, 0.0], rot: [3, -90, 0], focal: 35 },
            { time: 17.5, pos: [1.6, 1.3, 0.0], rot: [2, -88, 0], focal: 40 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 13.5, pos: [0.5, 2.2, 0.0], rot: [40, -90, 0], focal: 24 },
            { time: 17, pos: [0.5, 2.0, 0.0], rot: [35, -88, 0], focal: 24 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 13, end: 15.5 },
          { camera: 1, start: 15.5, end: 18 },
        ],
      },
      { name: 'INSERT EVIDENCE', start: 18, end: 19.5, color: '#557799',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 18, pos: [0.0, 2.0, 0.0], rot: [88, 0, 0], focal: 50 },
            { time: 19.3, pos: [0.1, 1.8, 0.1], rot: [85, 5, 0], focal: 50 },
          ]},
        ],
        coverage: [{ camera: 0, start: 18, end: 19.5 }],
      },
      { name: 'CU DET WATCHES', start: 19.5, end: 22.5, color: '#aa7744',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 19.5, pos: [0.0, 1.3, -0.5], rot: [0, 0, 0], focal: 85 },
            { time: 22, pos: [0.0, 1.3, -0.4], rot: [-2, 0, 0], focal: 85 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 20, pos: [-1.0, 1.4, -0.3], rot: [0, 30, 0], focal: 65 },
            { time: 22, pos: [-0.8, 1.4, -0.3], rot: [0, 28, 0], focal: 65 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 19.5, end: 21 },
          { camera: 1, start: 21, end: 22.5 },
        ],
      },
      { name: 'OTS WIT→DET', start: 22.5, end: 25, color: '#7755aa',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 22.5, pos: [0.3, 1.5, 0.8], rot: [5, -165, 0], focal: 50 },
            { time: 24.5, pos: [0.2, 1.5, 0.7], rot: [3, -160, 0], focal: 50 },
          ]},
        ],
        coverage: [{ camera: 0, start: 22.5, end: 25 }],
      },
      { name: 'MED WIT EXPLAINS', start: 25, end: 29, color: '#996655',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 25, pos: [-1.0, 1.4, 0.8], rot: [0, -120, 0], focal: 50 },
            { time: 27, pos: [-0.8, 1.4, 0.6], rot: [0, -115, 0], focal: 50 },
            { time: 28.5, pos: [-0.6, 1.3, 0.5], rot: [-1, -112, 0], focal: 55 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 25.5, pos: [0.8, 1.3, 0.3], rot: [0, -150, 0], focal: 65 },
            { time: 28, pos: [0.6, 1.3, 0.3], rot: [0, -148, 0], focal: 65 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 25, end: 27 },
          { camera: 1, start: 27, end: 29 },
        ],
      },
      { name: 'INSERT HANDS', start: 29, end: 31, color: '#5588aa',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 29, pos: [0.3, 1.0, 0.5], rot: [30, -150, 0], focal: 100 },
            { time: 30.8, pos: [0.2, 1.0, 0.4], rot: [28, -148, 0], focal: 100 },
          ]},
        ],
        coverage: [{ camera: 0, start: 29, end: 31 }],
      },
      { name: 'WIDE TENSION', start: 31, end: 34, color: '#aa6655',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 31, pos: [3.0, 2.0, 0.0], rot: [10, -90, 0], focal: 35 },
            { time: 33.5, pos: [2.8, 1.9, 0.0], rot: [8, -90, 0], focal: 35 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 31.5, pos: [2.0, 1.8, -2.0], rot: [12, -50, 8], focal: 28 },
            { time: 33.5, pos: [1.8, 1.7, -1.8], rot: [10, -48, 6], focal: 28 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 31, end: 32.5 },
          { camera: 1, start: 32.5, end: 34 },
        ],
      },
      { name: 'PUSH IN DET', start: 34, end: 38, color: '#6677aa',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 34, pos: [1.5, 1.3, -1.5], rot: [0, -30, 0], focal: 50 },
            { time: 36, pos: [1.0, 1.3, -1.0], rot: [0, -20, 0], focal: 65 },
            { time: 37.8, pos: [0.5, 1.3, -0.6], rot: [-2, -10, 0], focal: 85 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 34.5, pos: [-0.8, 1.4, -0.8], rot: [0, 45, 0], focal: 50 },
            { time: 37.5, pos: [-0.6, 1.4, -0.6], rot: [0, 40, 0], focal: 50 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 34, end: 37 },
          { camera: 1, start: 37, end: 38 },
        ],
      },
      { name: 'ECU WIT EYES', start: 38, end: 40, color: '#aa5555',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 38, pos: [0.0, 1.3, 0.3], rot: [0, -180, 0], focal: 135 },
            { time: 39.8, pos: [0.0, 1.3, 0.25], rot: [0, -180, 0], focal: 135 },
          ]},
        ],
        coverage: [{ camera: 0, start: 38, end: 40 }],
      },
      { name: 'MED DET LEANS BACK', start: 40, end: 43, color: '#558899',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 40, pos: [-1.0, 1.4, -0.5], rot: [0, 30, 0], focal: 50 },
            { time: 42.5, pos: [-1.2, 1.5, -0.8], rot: [2, 35, 0], focal: 50 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 40.5, pos: [2.0, 1.3, -1.5], rot: [0, -50, 0], focal: 65 },
            { time: 42.5, pos: [2.2, 1.4, -1.8], rot: [2, -55, 0], focal: 65 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 40, end: 41.5 },
          { camera: 1, start: 41.5, end: 43 },
        ],
      },
      { name: 'WIDE DENOUEMENT', start: 43, end: 48, color: '#8866aa',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 43, pos: [2.5, 2.5, -2.5], rot: [20, -40, 0], focal: 24 },
            { time: 47.5, pos: [2.5, 2.5, -2.5], rot: [18, -38, 0], focal: 24 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 43.5, pos: [0.0, 2.5, -3.0], rot: [25, 0, 0], focal: 24 },
            { time: 47, pos: [0.0, 3.5, -3.0], rot: [30, 0, 0], focal: 24 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 43, end: 45.5 },
          { camera: 1, start: 45.5, end: 48 },
        ],
      },
    ],
    tracks: [
      { name: 'Detective', color: '#4a9a4a', keyframes: [
        { time: 0, pos: [-2.0, 0.0, -2.5], rot: [0, 90, 0], scale: [1,1,1] },
        { time: 3, pos: [-1.0, 0.0, -1.5], rot: [0, 45, 0], scale: [1,1,1] },
        { time: 5, pos: [0.0, 0.9, -1.2], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 13, pos: [0.0, 0.9, -1.2], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 15, pos: [0.1, 0.8, -1.0], rot: [-5, 0, 0], scale: [1,1,1] },
        { time: 18, pos: [0.0, 0.9, -1.2], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 34, pos: [0.0, 0.9, -1.2], rot: [-5, 0, 0], scale: [1,1,1] },
        { time: 38, pos: [0.1, 0.8, -1.0], rot: [-10, 0, 0], scale: [1,1,1] },
        { time: 40, pos: [0.0, 1.0, -1.4], rot: [5, 0, 0], scale: [1,1,1] },
        { time: 48, pos: [0.0, 0.9, -1.2], rot: [0, 0, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Witness', color: '#3a8a5a', keyframes: [
        { time: 0, pos: [0.0, 0.9, 1.2], rot: [0, 180, 0], scale: [1,1,1] },
        { time: 7, pos: [0.0, 0.9, 1.2], rot: [5, 175, 0], scale: [1,1,1] },
        { time: 10.5, pos: [0.0, 0.9, 1.2], rot: [-3, 180, 0], scale: [1,1,1] },
        { time: 18, pos: [0.0, 0.9, 1.2], rot: [0, 180, 0], scale: [1,1,1] },
        { time: 25, pos: [0.1, 0.8, 1.1], rot: [-5, 178, 0], scale: [1,1,1] },
        { time: 29, pos: [0.0, 0.9, 1.2], rot: [0, 180, 0], scale: [1,1,1] },
        { time: 33, pos: [0.0, 0.9, 1.2], rot: [-2, 182, 0], scale: [1,1,1] },
        { time: 38, pos: [0.0, 0.8, 1.1], rot: [-8, 180, 0], scale: [1,1,1] },
        { time: 43, pos: [0.0, 0.9, 1.2], rot: [0, 180, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Evidence Folder', color: '#7a6a4a', keyframes: [
        { time: 0, pos: [0.0, 0.76, -0.8], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 13, pos: [0.0, 0.76, -0.8], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 15, pos: [0.0, 0.76, -0.2], rot: [0, 10, 0], scale: [1,1,1] },
        { time: 18, pos: [0.0, 0.76, 0.0], rot: [0, 5, 0], scale: [1,1,1] },
      ], linkedPeriods: [
        { start: 0, end: 13, parent: 'Detective lap' },
      ] },
      { name: 'Table', color: '#4a6a8a', keyframes: [
        { time: 0, pos: [0.0, 0.0, 0.0], rot: [0, 0, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Key Light', color: '#8a7a4a', keyframes: [
        { time: 0, pos: [0.5, 3.0, -0.5], rot: [-60, 20, 0], scale: [1,1,1] },
        { time: 13, pos: [0.5, 3.0, -0.5], rot: [-60, 20, 0], scale: [1,1,1] },
        { time: 31, pos: [0.3, 3.0, -0.3], rot: [-65, 15, 0], scale: [0.8,0.8,0.8] },
        { time: 38, pos: [0.5, 3.0, -0.5], rot: [-55, 25, 0], scale: [1.1,1.1,1.1] },
        { time: 48, pos: [0.5, 3.0, -0.5], rot: [-60, 20, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
    ],
  },

  // ─── Scene 2: The Alley ───
  {
    name: 'The Alley',
    location: 'EXT. BACK ALLEY — NIGHT',
    shots: [
      { name: 'WIDE ALLEY APPROACH', start: 0, end: 5, color: '#4a6688',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 0, pos: [0, 1.6, -15], rot: [2, 0, 0], focal: 85 },
            { time: 4.5, pos: [0, 1.6, -15], rot: [0, 0, 0], focal: 85 },
          ]},
        ],
        coverage: [{ camera: 0, start: 0, end: 5 }],
      },
      { name: 'MED DET AT TAPE', start: 5, end: 9, color: '#886644',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 5, pos: [2, 1.4, -4], rot: [5, -30, 0], focal: 50 },
            { time: 7, pos: [1.5, 1.2, -3.5], rot: [10, -25, 0], focal: 50 },
            { time: 8.5, pos: [1.2, 1.0, -3], rot: [15, -20, 0], focal: 50 },
          ]},
        ],
        coverage: [{ camera: 0, start: 5, end: 9 }],
      },
      { name: 'POV DET SCANS', start: 9, end: 13, color: '#668844',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 9, pos: [0.5, 1.6, -3], rot: [30, -10, 2], focal: 35 },
            { time: 11, pos: [0.5, 1.5, -2.5], rot: [45, 20, -3], focal: 35 },
            { time: 12.5, pos: [0.5, 1.3, -2], rot: [50, 5, 1], focal: 35 },
          ]},
        ],
        coverage: [{ camera: 0, start: 9, end: 13 }],
      },
      { name: 'CU EVIDENCE GROUND', start: 13, end: 15.5, color: '#884466',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 13, pos: [0.2, 0.3, -1.5], rot: [70, 0, 0], focal: 100 },
            { time: 15, pos: [0.2, 0.25, -1.4], rot: [72, 2, 0], focal: 100 },
          ]},
        ],
        coverage: [{ camera: 0, start: 13, end: 15.5 }],
      },
      { name: 'MED DET KNEELS', start: 15.5, end: 20, color: '#446688',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 15.5, pos: [1.5, 1.4, -2], rot: [10, -20, 0], focal: 50 },
            { time: 17, pos: [1.2, 1.0, -1.8], rot: [20, -15, 0], focal: 50 },
            { time: 19.5, pos: [1.0, 0.8, -1.5], rot: [25, -10, 0], focal: 65 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 16, pos: [-0.5, 0.6, -1.5], rot: [15, 45, 0], focal: 35 },
            { time: 19, pos: [-0.3, 0.5, -1.3], rot: [20, 40, 0], focal: 35 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 15.5, end: 18 },
          { camera: 1, start: 18, end: 20 },
        ],
      },
      { name: 'OTS DET→UNIFORM', start: 20, end: 24, color: '#664488',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 20, pos: [-0.5, 1.5, -2.5], rot: [3, 20, 0], focal: 50 },
            { time: 23.5, pos: [-0.3, 1.5, -2.3], rot: [2, 18, 0], focal: 50 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 20.5, pos: [1, 1.5, -1], rot: [3, -160, 0], focal: 50 },
            { time: 23, pos: [0.8, 1.5, -1], rot: [2, -158, 0], focal: 50 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 20, end: 22 },
          { camera: 1, start: 22, end: 24 },
        ],
      },
      { name: 'CU DET REALIZES', start: 24, end: 27, color: '#886644',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 24, pos: [0.2, 1.3, -2.2], rot: [0, 5, 0], focal: 85 },
            { time: 26.5, pos: [0.15, 1.3, -2.1], rot: [-2, 3, 0], focal: 85 },
          ]},
        ],
        coverage: [{ camera: 0, start: 24, end: 27 }],
      },
      { name: 'WIDE DET EXITS', start: 27, end: 32, color: '#448866',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 27, pos: [3, 2, -5], rot: [10, -30, 0], focal: 28 },
            { time: 31.5, pos: [3, 2, -5], rot: [8, -35, 0], focal: 28 },
          ]},
        ],
        coverage: [{ camera: 0, start: 27, end: 32 }],
      },
      { name: 'WIDE ALLEY EMPTY', start: 32, end: 36, color: '#446666',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 32, pos: [0, 3, -8], rot: [15, 0, 0], focal: 24 },
            { time: 35.5, pos: [0, 3.5, -8], rot: [18, 0, 0], focal: 24 },
          ]},
        ],
        coverage: [{ camera: 0, start: 32, end: 36 }],
      },
    ],
    tracks: [
      { name: 'Detective', color: '#4a9a4a', keyframes: [
        { time: 0, pos: [0, 0, -12], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 5, pos: [0, 0, -4], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 7, pos: [0.2, 0, -3.5], rot: [-15, 0, 0], scale: [1,1,1] },
        { time: 15.5, pos: [0.3, 0, -2], rot: [-30, 10, 0], scale: [1,1,1] },
        { time: 17, pos: [0.3, -0.5, -1.5], rot: [-40, 5, 0], scale: [1,1,1] },
        { time: 24, pos: [0.3, 0, -2], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 27, pos: [0.3, 0, -2], rot: [0, 180, 0], scale: [1,1,1] },
        { time: 32, pos: [0, 0, -12], rot: [0, 180, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Officer', color: '#3a8a5a', keyframes: [
        { time: 20, pos: [0.5, 0, -1], rot: [0, -160, 0], scale: [1,1,1] },
        { time: 24, pos: [0.5, 0, -1], rot: [0, -160, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Evidence Marker', color: '#7a6a4a', keyframes: [
        { time: 0, pos: [0.2, 0, -1.5], rot: [0, 0, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Crime Tape', color: '#8a4a4a', keyframes: [
        { time: 0, pos: [0, 1, -4], rot: [0, 0, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Streetlight', color: '#8a7a4a', keyframes: [
        { time: 0, pos: [-2, 4, -3], rot: [0, 0, 0], scale: [1,1,1] },
        { time: 31, pos: [-2, 4, -3], rot: [0, 0, 0], scale: [0.9,0.9,0.9] },
        { time: 36, pos: [-2, 4, -3], rot: [0, 0, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
    ],
  },

  // ─── Scene 3: The Office ───
  {
    name: 'The Office',
    location: 'INT. DETECTIVE\'S OFFICE — DAY',
    shots: [
      { name: 'WIDE OFFICE ENTER', start: 0, end: 4, color: '#7a8855',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 0, pos: [3, 2, -2], rot: [12, -45, 0], focal: 28 },
            { time: 3.5, pos: [2.5, 1.8, -1.8], rot: [8, -40, 0], focal: 28 },
          ]},
        ],
        coverage: [{ camera: 0, start: 0, end: 4 }],
      },
      { name: 'MED AT DESK', start: 4, end: 9, color: '#557788',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 4, pos: [1.5, 1.4, 0], rot: [5, -90, 0], focal: 50 },
            { time: 6, pos: [1.2, 1.3, 0], rot: [3, -85, 0], focal: 50 },
            { time: 8.5, pos: [1.0, 1.3, 0], rot: [2, -80, 0], focal: 55 },
          ]},
        ],
        coverage: [{ camera: 0, start: 4, end: 9 }],
      },
      { name: 'CU EVIDENCE BOARD', start: 9, end: 13, color: '#885566',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 9, pos: [-2, 1.6, 1], rot: [0, 90, 0], focal: 35 },
            { time: 11, pos: [-2, 1.6, 0], rot: [0, 90, 0], focal: 35 },
            { time: 12.5, pos: [-2, 1.4, -0.5], rot: [-5, 90, 0], focal: 40 },
          ]},
        ],
        coverage: [{ camera: 0, start: 9, end: 13 }],
      },
      { name: 'MED PHONE CALL', start: 13, end: 18, color: '#667755',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 13, pos: [1, 1.4, -0.5], rot: [5, -70, 0], focal: 50 },
            { time: 15, pos: [0.8, 1.4, -0.3], rot: [3, -65, 0], focal: 50 },
            { time: 17.5, pos: [0.8, 1.3, -0.3], rot: [0, -60, 0], focal: 55 },
          ]},
          { name: 'Cam B', keyframes: [
            { time: 13.5, pos: [-0.5, 1.3, -0.5], rot: [0, 30, 0], focal: 65 },
            { time: 17, pos: [-0.3, 1.3, -0.3], rot: [0, 25, 0], focal: 65 },
          ]},
        ],
        coverage: [
          { camera: 0, start: 13, end: 15.5 },
          { camera: 1, start: 15.5, end: 18 },
        ],
      },
      { name: 'OTS BOARD CONNECT', start: 18, end: 23, color: '#775588',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 18, pos: [0.5, 1.5, 0.5], rot: [5, 100, 0], focal: 35 },
            { time: 20, pos: [0.3, 1.5, 0.3], rot: [3, 95, 0], focal: 35 },
            { time: 22.5, pos: [0.2, 1.4, 0.2], rot: [0, 90, 0], focal: 40 },
          ]},
        ],
        coverage: [{ camera: 0, start: 18, end: 23 }],
      },
      { name: 'CU DET EUREKA', start: 23, end: 26, color: '#887744',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 23, pos: [0.5, 1.3, -0.2], rot: [0, -60, 0], focal: 85 },
            { time: 25.5, pos: [0.4, 1.3, -0.1], rot: [-2, -55, 0], focal: 85 },
          ]},
        ],
        coverage: [{ camera: 0, start: 23, end: 26 }],
      },
      { name: 'WIDE EXITS OFFICE', start: 26, end: 32, color: '#558866',
        cameras: [
          { name: 'Cam A', keyframes: [
            { time: 26, pos: [3, 2, -2], rot: [10, -45, 0], focal: 28 },
            { time: 29, pos: [2.5, 1.8, -1.5], rot: [5, -50, 0], focal: 28 },
            { time: 31.5, pos: [3, 2, -3], rot: [12, -30, 0], focal: 24 },
          ]},
        ],
        coverage: [{ camera: 0, start: 26, end: 32 }],
      },
    ],
    tracks: [
      { name: 'Detective', color: '#4a9a4a', keyframes: [
        { time: 0, pos: [2, 0, -2], rot: [0, -90, 0], scale: [1,1,1] },
        { time: 4, pos: [0, 0.9, 0], rot: [0, -90, 0], scale: [1,1,1] },
        { time: 9, pos: [0, 0.9, 0], rot: [0, 90, 0], scale: [1,1,1] },
        { time: 13, pos: [0, 0.9, 0], rot: [0, -45, 0], scale: [1,1,1] },
        { time: 18, pos: [0, 0.9, 0], rot: [0, 90, 0], scale: [1,1,1] },
        { time: 23, pos: [0, 0.9, 0], rot: [0, -60, 0], scale: [1,1,1] },
        { time: 26, pos: [0, 0, 0], rot: [0, -90, 0], scale: [1,1,1] },
        { time: 32, pos: [2.5, 0, -2.5], rot: [0, -135, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Evidence Board', color: '#7a6a4a', keyframes: [
        { time: 0, pos: [-2.5, 1.5, 0], rot: [0, 90, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Phone', color: '#6a5a8a', keyframes: [
        { time: 0, pos: [0.3, 0.76, 0.2], rot: [0, -30, 0], scale: [1,1,1] },
        { time: 13, pos: [0.3, 0.76, 0.2], rot: [0, -30, 0], scale: [1,1,1] },
        { time: 14, pos: [0.1, 1.2, -0.1], rot: [-20, -30, 0], scale: [1,1,1] },
        { time: 17, pos: [0.3, 0.76, 0.2], rot: [0, -30, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Desk', color: '#4a6a8a', keyframes: [
        { time: 0, pos: [0, 0, 0], rot: [0, 0, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
      { name: 'Desk Lamp', color: '#8a7a4a', keyframes: [
        { time: 0, pos: [-0.4, 0.76, -0.3], rot: [-30, 0, 0], scale: [1,1,1] },
        { time: 23, pos: [-0.4, 0.76, -0.3], rot: [-30, 0, 0], scale: [1.2,1.2,1.2] },
        { time: 32, pos: [-0.4, 0.76, -0.3], rot: [-30, 0, 0], scale: [1,1,1] },
      ], linkedPeriods: [] },
    ],
  },
];

// ── Active scene state ──
const SCENE = { fps: 24 };
let currentSceneIndex = 0;

function loadScene(index) {
  currentSceneIndex = index;
  const s = ALL_SCENES[index];
  SCENE.shots = s.shots;
  SCENE.tracks = s.tracks;
  SCENE.totalDuration = s.shots[s.shots.length - 1].end;
  SCENE.name = s.name;
  SCENE.location = s.location;
  playhead = 0;
  viewStart = 0;
  viewEnd = SCENE.totalDuration;
  previewCamera = null;
  selectedKeyframe = null;
  for (const k in activeCameraPerShot) delete activeCameraPerShot[k];
  SCENE.shots.forEach((_, i) => { activeCameraPerShot[i] = 0; });
  document.getElementById('shot-bar-container').style.height =
    (Math.max(...SCENE.shots.map(s => s.cameras.length)) * CAM_ROW_HEIGHT + 2) + 'px';
  renderSceneTabs();
  render();
  updateViewportFrame();
}

// ── Constants ──
const CAM_ROW_HEIGHT = 20;
const TRACK_ROW_HEIGHT = 28;
const SUB_TRACK_HEIGHT = 22;

const CAMERA_PROPS = ['Position X', 'Position Y', 'Position Z', 'Pan', 'Tilt', 'Roll', 'Focal Length'];
const OBJECT_PROPS = ['Position X', 'Position Y', 'Position Z', 'Scale', 'Rotation X', 'Rotation Y', 'Rotation Z'];
const INTERP_CURVES = ['linear', 'ease-in', 'ease-out', 'ease-in-out', 'bezier'];
const INTERP_SYMBOLS = { 'linear': '─', 'ease-in': '⌒', 'ease-out': '⌓', 'ease-in-out': '~', 'bezier': '∿' };

// Keyframe interpolation shapes (AE-style: linear ◇, smooth ○, hold □)
const KF_INTERP_TYPES = ['linear', 'smooth', 'hold'];
const kfInterpOverrides = {};
function getKfInterp(key) {
  if (kfInterpOverrides[key]) return kfInterpOverrides[key];
  // Default: camera kfs get smooth, single-kf objects get hold, multi-kf objects get linear
  const parts = key.split('-');
  if (parts[0] === 'cam') return 'smooth';
  const ti = parseInt(parts[1]);
  const track = SCENE.tracks[ti];
  return (track && track.keyframes.length <= 1) ? 'hold' : 'linear';
}
function cycleKfInterp(key) {
  const current = getKfInterp(key);
  const idx = KF_INTERP_TYPES.indexOf(current);
  kfInterpOverrides[key] = KF_INTERP_TYPES[(idx + 1) % KF_INTERP_TYPES.length];
}

const TOOL_MODES = [
  { key: 'Q', label: 'SELECT', icon: '◇' },
  { key: 'W', label: 'MOVE', icon: '✥' },
  { key: 'E', label: 'ROTATE', icon: '↻' },
  { key: 'R', label: 'SCALE', icon: '⬡' },
];
const ASPECT_RATIOS = [
  { name: '2.39:1', ratio: 2.39 },
  { name: '2.35:1', ratio: 2.35 },
  { name: '1.85:1', ratio: 1.85 },
  { name: '16:9', ratio: 16 / 9 },
  { name: '4:3', ratio: 4 / 3 },
  { name: '1:1', ratio: 1 },
  { name: '9:16', ratio: 9 / 16 },
  { name: 'None', ratio: null },
];

// ── State ──
let playhead = 0;
let viewStart = 0;
let viewEnd = 0;
let playing = false;
let selectedKeyframe = null;
let animFrame = null;
const activeCameraPerShot = {};
let previewCamera = null;
let shotBarLastClick = { shotIndex: -1, camIndex: -1, time: 0 };
let boundaryDragging = false;
let boundaryIndex = -1;
let boundarySnapshot = null;
const trackExpanded = {};
let currentToolMode = 0;
let directorView = false;
let cameraPathVisible = false;
let currentAspectIndex = 0; // 2.39:1 default
let guidesVisible = false;
let kfDragging = false;
let kfDragInfo = null;
let coverageDivDragging = false;
let coverageDivInfo = null;

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

// ── Feature visibility ──
function applyFeatureVisibility() {
  const featureElements = {
    'hud': ['viewport-hud'],
    'tool-mode': ['tool-mode-badge'],
    'shot-management': ['shot-mgmt-buttons'],
    'coverage-track': ['coverage-container'],
  };
  for (const [featId, elIds] of Object.entries(featureElements)) {
    const show = feat(featId);
    elIds.forEach(id => {
      const el = document.getElementById(id);
      if (el) el.style.display = show ? '' : 'none';
    });
  }
  const dirBadge = document.getElementById('director-badge');
  if (dirBadge) dirBadge.style.display = (feat('director-view') && directorView) ? '' : 'none';
  const pathBadge = document.getElementById('camera-path-badge');
  if (pathBadge) pathBadge.style.display = (feat('camera-path') && cameraPathVisible) ? '' : 'none';
  const guidesEl = document.getElementById('viewport-guides');
  if (guidesEl) guidesEl.style.display = (feat('frame-guides') && guidesVisible) ? '' : 'none';
  updateAspectMasks();
}

// ── Viewport frame sizing (maintains 16:9 aspect ratio) ──
function updateViewportFrame() {
  const viewport = document.getElementById('viewport');
  const frame = document.getElementById('viewport-frame');
  if (!viewport || !frame) return;
  const vw = viewport.clientWidth;
  const vh = viewport.clientHeight;
  const ar = 16 / 9;
  if (vw / vh > ar) {
    frame.style.height = vh + 'px';
    frame.style.width = Math.round(vh * ar) + 'px';
  } else {
    frame.style.width = vw + 'px';
    frame.style.height = Math.round(vw / ar) + 'px';
  }
  updateAspectMasks();
}

function updateAspectMasks() {
  const frame = document.getElementById('viewport-frame');
  if (!frame) return;
  const ar = ASPECT_RATIOS[currentAspectIndex];
  const top = document.getElementById('aspect-mask-top');
  const bottom = document.getElementById('aspect-mask-bottom');
  const left = document.getElementById('aspect-mask-left');
  const right = document.getElementById('aspect-mask-right');
  if (!feat('aspect-ratio') || !ar.ratio) {
    [top, bottom, left, right].forEach(el => { if (el) el.style.display = 'none'; });
    return;
  }
  const fw = frame.clientWidth;
  const fh = frame.clientHeight;
  if (!fw || !fh) return;
  const frameAR = fw / fh;
  if (ar.ratio < frameAR) {
    const cw = fh * ar.ratio;
    const barW = (fw - cw) / 2;
    if (top) top.style.display = 'none';
    if (bottom) bottom.style.display = 'none';
    if (left) { left.style.display = ''; left.style.width = barW + 'px'; }
    if (right) { right.style.display = ''; right.style.width = barW + 'px'; }
  } else if (ar.ratio > frameAR) {
    const ch = fw / ar.ratio;
    const barH = (fh - ch) / 2;
    if (left) left.style.display = 'none';
    if (right) right.style.display = 'none';
    if (top) { top.style.display = ''; top.style.height = barH + 'px'; }
    if (bottom) { bottom.style.display = ''; bottom.style.height = barH + 'px'; }
  } else {
    [top, bottom, left, right].forEach(el => { if (el) el.style.display = 'none'; });
  }
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
  return `${String(hh).padStart(2,'0')};${String(mm).padStart(2,'0')};${String(ss).padStart(2,'0')};${String(ff).padStart(2,'0')}`;
}
function formatRulerTime(t) {
  const totalFrames = Math.max(0, Math.round(t * SCENE.fps));
  const ff = totalFrames % 24;
  const totalSeconds = Math.floor(totalFrames / 24);
  const ss = totalSeconds % 60;
  const mm = Math.floor(totalSeconds / 60);
  const duration = viewEnd - viewStart;
  if (duration < 3) return `${mm};${String(ss).padStart(2,'0')};${String(ff).padStart(2,'0')}`;
  return `${mm};${String(ss).padStart(2,'0')}`;
}

function getCurrentShot() {
  for (let i = 0; i < SCENE.shots.length; i++) {
    if (playhead >= SCENE.shots[i].start && playhead < SCENE.shots[i].end) return i;
  }
  return SCENE.shots.length - 1;
}
function adjustAlpha(hex, factor) {
  const r = parseInt(hex.slice(1,3),16);
  const g = parseInt(hex.slice(3,5),16);
  const b = parseInt(hex.slice(5,7),16);
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

// Interpolate a property value between keyframes
function interpolateTrack(keyframes, time, prop) {
  if (!keyframes || keyframes.length === 0) return null;
  const first = keyframes[0][prop];
  if (first === undefined) return null;
  if (time <= keyframes[0].time) return first;
  if (time >= keyframes[keyframes.length-1].time) return keyframes[keyframes.length-1][prop];
  for (let i = 0; i < keyframes.length - 1; i++) {
    if (time >= keyframes[i].time && time <= keyframes[i+1].time) {
      const frac = (time - keyframes[i].time) / (keyframes[i+1].time - keyframes[i].time);
      const a = keyframes[i][prop];
      const b = keyframes[i+1][prop];
      if (a == null || b == null) return a;
      if (Array.isArray(a)) return a.map((v, j) => +(v + frac * (b[j] - v)).toFixed(2));
      return +(a + frac * (b - a)).toFixed(1);
    }
  }
  return keyframes[keyframes.length-1][prop];
}

// Format array as tuple string
function fmtTuple(arr) {
  if (!arr) return '—';
  return arr.map(v => v.toFixed(1)).join(', ');
}

// ── Scene tabs ──
function renderSceneTabs() {
  const tabs = document.getElementById('scene-tabs');
  if (!tabs) return;
  tabs.innerHTML = '';
  ALL_SCENES.forEach((scene, i) => {
    const btn = document.createElement('button');
    btn.className = 'scene-tab' + (i === currentSceneIndex ? ' active' : '');
    btn.textContent = scene.name;
    btn.addEventListener('click', () => loadScene(i));
    tabs.appendChild(btn);
  });
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
  renderTransportBar();
  if (feat('coverage-track')) renderCoverageTrack();
  if (feat('tool-mode')) renderToolMode();
  renderHUD();
}

function renderShotBar() {
  shotBar.querySelectorAll('.shot-cam, .shot-boundary-handle').forEach(el => el.remove());
  const currentShotIndex = getCurrentShot();
  if (previewCamera && previewCamera.shotIndex !== currentShotIndex) previewCamera = null;

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
        el.addEventListener('mouseleave', () => { shotTooltip.style.display = 'none'; });
      }

      el.addEventListener('mousedown', (e) => {
        e.stopPropagation(); e.preventDefault();
        const now = Date.now();
        const isDouble = shotBarLastClick.shotIndex === si && shotBarLastClick.camIndex === ci && now - shotBarLastClick.time < 350;
        if (isDouble) {
          // Double-click: activate camera AND zoom to shot
          activeCameraPerShot[si] = ci;
          previewCamera = null;
          const pad = (shot.end - shot.start) * 0.08;
          viewStart = Math.max(0, shot.start - pad);
          viewEnd = Math.min(SCENE.totalDuration, shot.end + pad);
          shotBarLastClick = { shotIndex: -1, camIndex: -1, time: 0 };
        } else {
          shotBarLastClick = { shotIndex: si, camIndex: ci, time: now };
          if (si !== currentShotIndex) playhead = shot.start;
          if (ci !== activeCamIdx || si !== currentShotIndex) {
            previewCamera = { shotIndex: si, cameraIndex: ci };
          } else { previewCamera = null; }
        }
        render();
      });
      shotBar.appendChild(el);
    });
  });

  // Boundary drag handles
  for (let i = 0; i < SCENE.shots.length - 1; i++) {
    const handle = document.createElement('div');
    handle.className = 'shot-boundary-handle';
    handle.style.left = timeToX(SCENE.shots[i].end) + 'px';
    const idx = i;
    handle.addEventListener('mousedown', (e) => {
      e.stopPropagation(); e.preventDefault();
      boundaryDragging = true;
      boundaryIndex = idx;
      boundarySnapshot = {
        shots: SCENE.shots.map(s => ({ start: s.start, end: s.end })),
        cameraTimes: SCENE.shots.map(s => s.cameras.map(c => c.keyframes.map(kf => kf.time))),
        objectTimes: SCENE.tracks.map(t => t.keyframes.map(kf => kf.time)),
        linkedPeriods: SCENE.tracks.map(t => (t.linkedPeriods||[]).map(lp => ({ start: lp.start, end: lp.end }))),
        coverageTimes: SCENE.shots.map(s => (s.coverage||[]).map(seg => ({ start: seg.start, end: seg.end }))),
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
  if (duration <= 2) interval = 1/SCENE.fps;
  else if (duration <= 5) interval = 0.5;
  else if (duration <= 15) interval = 1;
  else if (duration <= 40) interval = 2;
  else if (duration <= 60) interval = 5;
  else interval = 10;
  const startTick = Math.ceil(viewStart/interval)*interval;
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

  // Camera track
  const camLabel = document.createElement('div');
  camLabel.className = 'track-label camera-label';
  let camLabelHTML = '';
  if (feat('track-expand')) {
    const exp = trackExpanded['cam'] || false;
    camLabelHTML += `<span class="expand-arrow ${exp?'expanded':''}" data-track="cam">▶</span>`;
  }
  camLabelHTML += `<div class="dot" style="background:${currentShot.color}"></div><div class="name">Cam ${displayLetter}</div>`;
  camLabel.innerHTML = camLabelHTML;
  trackLabels.appendChild(camLabel);

  const camRow = document.createElement('div');
  camRow.className = 'track-row';
  camRow.style.top = yOffset + 'px';
  trackArea.appendChild(camRow);
  renderCameraRow(camRow, currentShotIndex);
  yOffset += TRACK_ROW_HEIGHT;

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
      if (display.cam) {
        const shotKfs = display.cam.keyframes.filter(kf => kf.time >= currentShot.start && kf.time <= currentShot.end);
        shotKfs.forEach((kf, ki) => {
          if (ki % CAMERA_PROPS.length === pi || pi < 3) {
            const el = document.createElement('div');
            el.className = 'keyframe';
            el.style.left = timeToX(kf.time) + 'px';
            el.style.background = currentShot.color;
            el.style.width = '7px'; el.style.height = '7px';
            subRow.appendChild(el);
          }
        });
        if (feat('interp-curves') && shotKfs.length > 1) {
          for (let k = 0; k < shotKfs.length - 1; k++) {
            if (k % CAMERA_PROPS.length === pi || pi < 3) {
              const x1 = timeToX(shotKfs[k].time);
              const x2 = timeToX(shotKfs[k+1].time);
              if (x2 - x1 > 30) {
                const ct = INTERP_CURVES[(k+pi)%INTERP_CURVES.length];
                const ind = document.createElement('div');
                ind.className = 'interp-indicator';
                ind.style.left = ((x1+x2)/2) + 'px';
                ind.textContent = INTERP_SYMBOLS[ct];
                subRow.appendChild(ind);
              }
            }
          }
        }
      }
      SCENE.shots.forEach(shot => { if (shot.start > 0) { const l = document.createElement('div'); l.className='shot-boundary'; l.style.left=timeToX(shot.start)+'px'; subRow.appendChild(l); }});
      yOffset += SUB_TRACK_HEIGHT;
    });
  }

  // Object tracks
  SCENE.tracks.forEach((track, ti) => {
    const label = document.createElement('div');
    label.className = 'track-label';
    let lHTML = '';
    if (feat('track-expand')) {
      const exp = trackExpanded['obj-'+ti] || false;
      lHTML += `<span class="expand-arrow ${exp?'expanded':''}" data-track="obj-${ti}">▶</span>`;
    }
    lHTML += `<div class="dot" style="background:${track.color}"></div><div class="name">${track.name}</div>`;
    label.innerHTML = lHTML;
    trackLabels.appendChild(label);
    const row = document.createElement('div');
    row.className = 'track-row';
    row.style.top = yOffset + 'px';
    trackArea.appendChild(row);
    renderObjectRow(row, ti, currentShotIndex);
    yOffset += TRACK_ROW_HEIGHT;

    if (feat('track-expand') && trackExpanded['obj-'+ti]) {
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
        track.keyframes.forEach((kf, ki) => {
          if (ki % OBJECT_PROPS.length === pi || pi < 3) {
            const el = document.createElement('div');
            el.className = 'keyframe';
            el.style.left = timeToX(kf.time) + 'px';
            el.style.background = track.color;
            el.style.width = '7px'; el.style.height = '7px';
            subRow.appendChild(el);
          }
        });
        SCENE.shots.forEach(shot => { if (shot.start > 0) { const l = document.createElement('div'); l.className='shot-boundary'; l.style.left=timeToX(shot.start)+'px'; subRow.appendChild(l); }});
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
  SCENE.shots.forEach((shot, si) => {
    const bg = document.createElement('div');
    bg.className = 'shot-bg' + (si === currentShotIndex ? ' active-shot' : '');
    const left = timeToX(shot.start);
    bg.style.left = left + 'px';
    bg.style.width = (timeToX(shot.end) - left) + 'px';
    bg.style.background = shot.color;
    row.appendChild(bg);
  });
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
  SCENE.shots.forEach(shot => {
    if (shot.start > 0) { const line = document.createElement('div'); line.className = 'shot-boundary'; line.style.left = timeToX(shot.start) + 'px'; row.appendChild(line); }
  });
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
      if (feat('interp-curves')) el.classList.add('kf-' + getKfInterp(key));
      el.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        if (e.altKey && feat('interp-curves')) {
          cycleKfInterp(key); render(); return;
        }
        if (feat('keyframe-drag')) {
          kfDragging = true;
          kfDragInfo = { type:'cam', shotIndex: currentShotIndex, camIndex: display.index, kfIndex: ki, startX: e.clientX, origTime: kf.time };
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
  SCENE.shots.forEach(shot => {
    if (shot.start > 0) { const line = document.createElement('div'); line.className = 'shot-boundary'; line.style.left = timeToX(shot.start) + 'px'; row.appendChild(line); }
  });
  if (track.linkedPeriods) {
    track.linkedPeriods.forEach(lp => {
      const x1 = timeToX(lp.start); const x2 = timeToX(lp.end);
      const region = document.createElement('div');
      region.className = 'linked-region';
      region.style.left = x1 + 'px'; region.style.width = (x2-x1) + 'px';
      const ll = document.createElement('div');
      ll.className = 'link-label';
      if (x2-x1 > 80) ll.textContent = `→ ${lp.parent}`;
      region.appendChild(ll);
      row.appendChild(region);
      [lp.start, lp.end].forEach(t => {
        const kf = document.createElement('div');
        kf.className = 'keyframe link-boundary';
        kf.style.left = timeToX(t) + 'px';
        row.appendChild(kf);
      });
    });
  }
  track.keyframes.forEach((kf, ki) => {
    const el = document.createElement('div');
    el.className = 'keyframe';
    el.style.left = timeToX(kf.time) + 'px';
    el.style.background = track.color;
    const key = `obj-${trackIndex}-${ki}`;
    if (selectedKeyframe === key) el.classList.add('selected');
    if (feat('interp-curves')) el.classList.add('kf-' + getKfInterp(key));
    el.addEventListener('mousedown', (e) => {
      e.stopPropagation();
      if (e.altKey && feat('interp-curves')) {
        cycleKfInterp(key); render(); return;
      }
      if (feat('keyframe-drag')) {
        kfDragging = true;
        kfDragInfo = { type:'obj', trackIndex, kfIndex: ki, startX: e.clientX, origTime: kf.time };
      }
      selectedKeyframe = (selectedKeyframe === key) ? null : key;
      playhead = kf.time;
      render();
    });
    row.appendChild(el);
  });
}

function renderPlayhead() { playheadEl.style.left = timeToX(playhead) + 'px'; }

function renderZoomBar() {
  const w = zoomBar.clientWidth;
  const tL = (viewStart/SCENE.totalDuration)*w;
  const tR = (viewEnd/SCENE.totalDuration)*w;
  zoomThumb.style.left = tL + 'px';
  zoomThumb.style.width = Math.max(30, tR-tL) + 'px';
  zoomPlayheadIndicator.style.left = ((playhead/SCENE.totalDuration)*w - 2) + 'px';
}

function renderMinimap() {
  if (!minimapEl) return;
  const w = minimapEl.clientWidth;
  const tToX = (t) => (t/SCENE.totalDuration)*w;
  const csi = getCurrentShot();
  const sc = document.getElementById('minimap-shots');
  sc.innerHTML = '';
  SCENE.shots.forEach((shot, i) => {
    const el = document.createElement('div');
    el.className = 'minimap-shot';
    const l = tToX(shot.start); const r = tToX(shot.end);
    el.style.left = l+'px'; el.style.width = (r-l-1)+'px';
    el.style.background = i===csi ? shot.color : adjustAlpha(shot.color,0.6);
    if (r-l > 30) el.textContent = shot.name;
    sc.appendChild(el);
  });
  const tc = document.getElementById('minimap-tracks');
  tc.innerHTML = '';
  const rc = 1 + SCENE.tracks.length;
  const tH = Math.max(4, (48-17)/rc);
  const camRow = document.createElement('div');
  camRow.className = 'minimap-track-row';
  camRow.style.top = '0px'; camRow.style.height = tH+'px';
  SCENE.shots.forEach((shot, si) => {
    const cam = shot.cameras[activeCameraPerShot[si]||0];
    cam.keyframes.forEach(kf => {
      const el = document.createElement('div');
      el.className='minimap-kf'; el.style.left=tToX(kf.time)+'px'; el.style.background=shot.color;
      camRow.appendChild(el);
    });
  });
  tc.appendChild(camRow);
  SCENE.tracks.forEach((track, ti) => {
    const row = document.createElement('div');
    row.className = 'minimap-track-row';
    row.style.top = ((ti+1)*tH)+'px'; row.style.height = tH+'px';
    if (track.linkedPeriods) {
      track.linkedPeriods.forEach(lp => {
        const region = document.createElement('div');
        region.className = 'minimap-link-region';
        region.style.left = tToX(lp.start)+'px'; region.style.width = (tToX(lp.end)-tToX(lp.start))+'px';
        row.appendChild(region);
      });
    }
    track.keyframes.forEach(kf => {
      const el = document.createElement('div');
      el.className='minimap-kf'; el.style.left=tToX(kf.time)+'px'; el.style.background=track.color;
      row.appendChild(el);
    });
    tc.appendChild(row);
  });
  const vw = document.getElementById('minimap-view-window');
  const vL = tToX(viewStart); const vR = tToX(viewEnd);
  vw.style.left = vL+'px'; vw.style.width = (vR-vL)+'px';
  vw.style.display = (viewEnd-viewStart >= SCENE.totalDuration-0.01) ? 'none' : 'block';
  document.getElementById('minimap-playhead').style.left = tToX(playhead)+'px';
}

function renderViewportInfo() {
  const si = getCurrentShot();
  const shot = SCENE.shots[si];
  const display = getDisplayCamera(si);
  const letter = String.fromCharCode(65 + display.index);
  const dur = (shot.end - shot.start).toFixed(1);
  viewportShotLabel.textContent = `Shot ${si+1}${letter}: ${shot.name} (${dur}s)`;
  // Sequence timecode
  viewportTimecode.textContent = formatTimecode(playhead);
}

function renderTransportBar() {
  // Local shot timecode (elapsed / duration)
  const si = getCurrentShot();
  const shot = SCENE.shots[si];
  const elapsed = Math.max(0, playhead - shot.start);
  const duration = shot.end - shot.start;
  document.getElementById('transport-time').textContent = formatTimecode(elapsed);
  document.getElementById('transport-duration').textContent = formatTimecode(duration);
  document.getElementById('transport-shot').textContent = shot.name;
  const playBtn = document.getElementById('transport-play');
  playBtn.textContent = playing ? '⏸' : '▶';
  playBtn.classList.toggle('playing', playing);
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
    // Highlight active segment (playhead is over it)
    const isActive = playhead >= seg.start && playhead < seg.end;
    el.classList.add(isActive ? 'active-coverage' : 'inactive-coverage');
    el.style.left = left + 'px';
    el.style.width = width + 'px';
    el.style.background = shot.color;
    const letter = String.fromCharCode(65 + seg.camera);
    if (width > 20) el.textContent = letter;
    el.title = `${cam.name}: ${(seg.end-seg.start).toFixed(1)}s`;

    // Alt+click to split
    el.addEventListener('mousedown', (e) => {
      if (e.altKey) {
        e.stopPropagation(); e.preventDefault();
        const rect = track.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const splitTime = Math.round(xToTime(x) * SCENE.fps) / SCENE.fps;
        if (splitTime > seg.start + 0.2 && splitTime < seg.end - 0.2) {
          const nextCam = (seg.camera + 1) % shot.cameras.length;
          coverage.splice(i + 1, 0, { camera: nextCam, start: splitTime, end: seg.end });
          seg.end = splitTime;
          render();
        }
      }
    });
    track.appendChild(el);

    // Divider between segments (draggable)
    if (i > 0) {
      const divider = document.createElement('div');
      divider.className = 'coverage-divider';
      divider.style.left = left + 'px';
      const divIdx = i;
      divider.addEventListener('mousedown', (e) => {
        e.stopPropagation(); e.preventDefault();
        coverageDivDragging = true;
        coverageDivInfo = { shotIndex: si, dividerIndex: divIdx, coverage };
      });
      track.appendChild(divider);
    }
  });
  const phCov = document.getElementById('playhead-coverage');
  if (phCov) phCov.style.left = timeToX(playhead) + 'px';
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

function renderHUD() {
  if (!feat('hud')) return;
  const si = getCurrentShot();
  const display = getDisplayCamera(si);
  if (!display.cam) return;
  const focal = interpolateTrack(display.cam.keyframes, playhead, 'focal');
  const pos = interpolateTrack(display.cam.keyframes, playhead, 'pos');
  if (focal != null) document.getElementById('hud-focal').textContent = focal.toFixed(0) + 'mm';
  if (pos != null) document.getElementById('hud-height').textContent = pos[1].toFixed(1) + 'm';
  // Approximate AOV from focal length (full-frame 35mm)
  if (focal != null) {
    const aov = 2 * Math.atan(36 / (2 * focal)) * (180 / Math.PI);
    document.getElementById('hud-aov').textContent = aov.toFixed(1) + '°';
  }
}

// ── Interactions ──

let trackDragging = false;
trackArea.addEventListener('mousedown', (e) => {
  if (e.button === 0) {
    trackDragging = true;
    previewCamera = null;
    const rect = trackArea.getBoundingClientRect();
    playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(e.clientX - rect.left)));
    selectedKeyframe = null;
    render();
  }
  if (e.button === 1) {
    e.preventDefault();
    panDragging = true; panStartX = e.clientX; panStartViewStart = viewStart;
    trackArea.style.cursor = 'grabbing';
  }
});

let rulerDragging = false;
ruler.addEventListener('mousedown', (e) => {
  if (e.button !== 0) return;
  rulerDragging = true; previewCamera = null;
  const rect = ruler.getBoundingClientRect();
  playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(e.clientX - rect.left)));
  render();
});

let panDragging = false, panStartX = 0, panStartViewStart = 0;
shotBar.addEventListener('mousedown', (e) => {
  if (e.button === 1) {
    e.preventDefault();
    panDragging = true; panStartX = e.clientX; panStartViewStart = viewStart;
    shotBar.style.cursor = 'grabbing';
  }
});

function handleZoomWheel(e) {
  e.preventDefault();
  const rect = e.currentTarget.getBoundingClientRect();
  const fraction = (e.clientX - rect.left) / rect.width;
  if (e.shiftKey) {
    const d = viewEnd - viewStart;
    const pan = (e.deltaY > 0 ? 0.1 : -0.1) * d;
    viewStart += pan; viewEnd += pan; clampView(); render(); return;
  }
  zoomAtPosition(fraction, e.deltaY);
}
trackArea.addEventListener('wheel', handleZoomWheel, { passive: false });
ruler.addEventListener('wheel', handleZoomWheel, { passive: false });
shotBar.addEventListener('wheel', handleZoomWheel, { passive: false });

// Timeline vertical resize
let timelineResizing = false, tlResizeStartY = 0, tlResizeStartH = 0;
const timelineResizeHandle = document.getElementById('timeline-resize-handle');
const timelineSection = document.getElementById('timeline-section');
timelineResizeHandle.addEventListener('mousedown', (e) => {
  timelineResizing = true;
  tlResizeStartY = e.clientY;
  tlResizeStartH = timelineSection.offsetHeight;
  e.preventDefault();
});

// Panel horizontal resize
let panelDragInfo = null;
document.querySelectorAll('.panel-drag-edge').forEach(edge => {
  edge.addEventListener('mousedown', (e) => {
    const panel = edge.closest('.side-panel');
    panelDragInfo = { panel, startX: e.clientX, startWidth: panel.offsetWidth, isLeft: panel.classList.contains('left') };
    e.preventDefault();
  });
});

// Track expand/collapse
trackLabels.addEventListener('click', (e) => {
  const arrow = e.target.closest('.expand-arrow');
  if (!arrow || !feat('track-expand')) return;
  const tk = arrow.dataset.track;
  trackExpanded[tk] = !trackExpanded[tk];
  render();
});

// Global mousemove
let zoomDragging = false, zoomDragStartX = 0, zoomDragStartViewStart = 0;
let minimapDragging = false;

document.addEventListener('mousemove', (e) => {
  // Timeline vertical resize
  if (timelineResizing) {
    const dy = tlResizeStartY - e.clientY;
    const newH = Math.max(80, Math.min(window.innerHeight - 150, tlResizeStartH + dy));
    timelineSection.style.height = newH + 'px';
    requestAnimationFrame(() => { render(); updateViewportFrame(); });
    return;
  }
  // Panel horizontal resize
  if (panelDragInfo) {
    const dx = e.clientX - panelDragInfo.startX;
    const mult = panelDragInfo.isLeft ? 1 : -1;
    const newW = Math.max(150, Math.min(500, panelDragInfo.startWidth + dx * mult));
    panelDragInfo.panel.style.width = newW + 'px';
    panelDragInfo.panel.style.minWidth = newW + 'px';
    requestAnimationFrame(() => { render(); updateViewportFrame(); });
    return;
  }
  // Coverage divider drag
  if (coverageDivDragging && coverageDivInfo) {
    const rect = document.getElementById('coverage-track').getBoundingClientRect();
    const x = e.clientX - rect.left;
    let newTime = Math.round(xToTime(x) * SCENE.fps) / SCENE.fps;
    const cov = coverageDivInfo.coverage;
    const di = coverageDivInfo.dividerIndex;
    const prev = cov[di - 1];
    const curr = cov[di];
    newTime = Math.max(prev.start + 0.2, Math.min(curr.end - 0.2, newTime));
    prev.end = newTime;
    curr.start = newTime;
    render();
    return;
  }
  // Keyframe drag
  if (kfDragging && kfDragInfo) {
    const rect = trackArea.getBoundingClientRect();
    let newTime = Math.round(xToTime(e.clientX - rect.left) * 10) / 10;
    newTime = Math.max(0, Math.min(SCENE.totalDuration, newTime));
    if (kfDragInfo.type === 'cam') {
      const shot = SCENE.shots[kfDragInfo.shotIndex];
      newTime = Math.max(shot.start, Math.min(shot.end, newTime));
      shot.cameras[kfDragInfo.camIndex].keyframes[kfDragInfo.kfIndex].time = newTime;
    } else {
      SCENE.tracks[kfDragInfo.trackIndex].keyframes[kfDragInfo.kfIndex].time = newTime;
    }
    playhead = newTime; render(); return;
  }
  // Boundary drag
  if (boundaryDragging && boundarySnapshot) {
    const rect = shotBar.getBoundingClientRect();
    let newTime = xToTime(e.clientX - rect.left);
    const snap = boundarySnapshot;
    newTime = Math.max(snap.shots[boundaryIndex].start + 1, newTime);
    newTime = Math.round(newTime * SCENE.fps) / SCENE.fps;
    const delta = newTime - snap.origBoundary;
    SCENE.shots[boundaryIndex].end = newTime;
    for (let j = boundaryIndex + 1; j < SCENE.shots.length; j++) {
      SCENE.shots[j].start = snap.shots[j].start + delta;
      SCENE.shots[j].end = snap.shots[j].end + delta;
      SCENE.shots[j].cameras.forEach((cam, ci) => {
        cam.keyframes.forEach((kf, ki) => { kf.time = snap.cameraTimes[j][ci][ki] + delta; });
      });
      if (SCENE.shots[j].coverage) {
        SCENE.shots[j].coverage.forEach((seg, si) => {
          const orig = snap.coverageTimes[j][si];
          if (orig) { seg.start = orig.start + delta; seg.end = orig.end + delta; }
        });
      }
    }
    if (!e.shiftKey) {
      SCENE.tracks.forEach((track, ti) => {
        track.keyframes.forEach((kf, ki) => { const ot = snap.objectTimes[ti][ki]; kf.time = ot >= snap.origBoundary ? ot + delta : ot; });
        (track.linkedPeriods||[]).forEach((lp, li) => { const ol = snap.linkedPeriods[ti][li]; lp.start = ol.start >= snap.origBoundary ? ol.start+delta : ol.start; lp.end = ol.end >= snap.origBoundary ? ol.end+delta : ol.end; });
      });
    } else {
      SCENE.tracks.forEach((track, ti) => {
        track.keyframes.forEach((kf, ki) => { kf.time = snap.objectTimes[ti][ki]; });
        (track.linkedPeriods||[]).forEach((lp, li) => { const ol = snap.linkedPeriods[ti][li]; lp.start = ol.start; lp.end = ol.end; });
      });
    }
    SCENE.totalDuration = SCENE.shots[SCENE.shots.length-1].end;
    if (viewEnd > SCENE.totalDuration) { const d = viewEnd-viewStart; viewEnd = SCENE.totalDuration; viewStart = Math.max(0, viewEnd-d); }
    const ls = SCENE.shots[boundaryIndex];
    const ld = ls.end - ls.start;
    resizeLabel.style.display = 'block';
    resizeLabel.style.left = (e.clientX+14)+'px'; resizeLabel.style.top = (e.clientY-24)+'px';
    resizeLabel.textContent = `${ls.name}: ${ld.toFixed(1)}s (${Math.round(ld*SCENE.fps)}f)  [${e.shiftKey?'shots only':'ripple'}]`;
    render(); return;
  }
  if (trackDragging) { const rect = trackArea.getBoundingClientRect(); playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(e.clientX-rect.left))); render(); return; }
  if (rulerDragging) { const rect = ruler.getBoundingClientRect(); playhead = Math.max(0, Math.min(SCENE.totalDuration, xToTime(e.clientX-rect.left))); render(); return; }
  if (panDragging) {
    const dx = e.clientX-panStartX; const w = trackArea.clientWidth; const d = viewEnd-viewStart;
    viewStart = panStartViewStart - (dx/w)*d; viewEnd = viewStart+d; clampView(); render(); return;
  }
  if (zoomDragging) {
    const dx = e.clientX-zoomDragStartX; const w = zoomBar.clientWidth;
    const d = viewEnd-viewStart; viewStart = zoomDragStartViewStart + (dx/w)*SCENE.totalDuration; viewEnd = viewStart+d; clampView(); render(); return;
  }
  if (minimapDragging) { handleMinimapClick(e); return; }
});

document.addEventListener('mouseup', () => {
  if (timelineResizing) timelineResizing = false;
  if (panelDragInfo) panelDragInfo = null;
  if (coverageDivDragging) { coverageDivDragging = false; coverageDivInfo = null; }
  if (kfDragging) { kfDragging = false; kfDragInfo = null; }
  if (boundaryDragging) { boundaryDragging = false; boundaryIndex = -1; boundarySnapshot = null; resizeLabel.style.display = 'none'; }
  trackDragging = false;
  rulerDragging = false;
  minimapDragging = false;
  if (panDragging) { panDragging = false; trackArea.style.cursor = 'crosshair'; shotBar.style.cursor = 'pointer'; }
  zoomDragging = false;
});

zoomThumb.addEventListener('mousedown', (e) => {
  zoomDragging = true; zoomDragStartX = e.clientX; zoomDragStartViewStart = viewStart; e.preventDefault();
});
zoomBar.addEventListener('click', (e) => {
  if (e.target === zoomThumb) return;
  const rect = zoomBar.getBoundingClientRect();
  const t = ((e.clientX-rect.left)/rect.width)*SCENE.totalDuration;
  const d = viewEnd-viewStart;
  viewStart = t-d/2; viewEnd = viewStart+d; clampView();
  playhead = Math.max(0, Math.min(SCENE.totalDuration, t)); render();
});

// Transport play
document.getElementById('transport-play').addEventListener('click', () => {
  playing = !playing;
  if (playing) animLoop();
  render();
});

// Add shot
const addShotBtn = document.getElementById('btn-add-shot');
if (addShotBtn) {
  addShotBtn.addEventListener('click', () => {
    if (!feat('shot-management')) return;
    const last = SCENE.shots[SCENE.shots.length-1];
    const ns = last.end; const ne = ns + 4;
    const hue = (SCENE.shots.length * 47) % 360;
    SCENE.shots.push({
      name: `NEW SHOT ${SCENE.shots.length+1}`, start: ns, end: ne,
      color: `hsl(${hue},40%,45%)`,
      cameras: [{ name: 'Cam A', keyframes: [
        { time: ns, pos: [0,1.5,0], rot: [0,0,0], focal: 50 },
        { time: ne-0.5, pos: [0,1.5,0], rot: [0,0,0], focal: 50 },
      ]}],
      coverage: [{ camera: 0, start: ns, end: ne }],
    });
    SCENE.totalDuration = ne;
    activeCameraPerShot[SCENE.shots.length-1] = 0;
    if (viewEnd < SCENE.totalDuration) viewEnd = SCENE.totalDuration;
    render();
  });
}

// Minimap
minimapEl.addEventListener('mousedown', (e) => {
  if (e.button !== 0) return;
  minimapDragging = true; previewCamera = null; handleMinimapClick(e);
});
function handleMinimapClick(e) {
  const rect = minimapEl.getBoundingClientRect();
  const t = ((e.clientX-rect.left)/rect.width)*SCENE.totalDuration;
  const d = viewEnd-viewStart;
  viewStart = t-d/2; viewEnd = viewStart+d; clampView();
  playhead = Math.max(0, Math.min(SCENE.totalDuration, t)); render();
}

trackArea.addEventListener('auxclick', (e) => e.preventDefault());
ruler.addEventListener('auxclick', (e) => e.preventDefault());
shotBar.addEventListener('auxclick', (e) => e.preventDefault());
minimapEl.addEventListener('auxclick', (e) => e.preventDefault());

// ── Keyboard ──
document.addEventListener('keydown', (e) => {
  if (e.target.tagName === 'INPUT') return;
  if (e.key === ' ') { e.preventDefault(); document.getElementById('transport-play').click(); return; }
  if (e.key === 'ArrowLeft') { playhead = Math.max(0, playhead - 1/SCENE.fps); render(); return; }
  if (e.key === 'ArrowRight') { playhead = Math.min(SCENE.totalDuration, playhead + 1/SCENE.fps); render(); return; }
  if (e.key === '\\') { viewStart = 0; viewEnd = SCENE.totalDuration; render(); return; }

  if (feat('tool-mode')) {
    const mi = TOOL_MODES.findIndex(m => m.key === e.key.toUpperCase());
    if (mi !== -1 && !e.shiftKey) { currentToolMode = mi; render(); return; }
  }
  if (feat('full-shortcuts')) {
    if (e.key === 'Home') { e.preventDefault(); playhead = 0; render(); return; }
    if (e.key === 'End') { e.preventDefault(); playhead = SCENE.totalDuration; render(); return; }
    const u = e.key.toUpperCase();
    if (u === 'G') { guidesVisible = !guidesVisible; applyFeatureVisibility(); return; }
    if (u === 'H') { const h = document.getElementById('viewport-hud'); if (h) h.style.display = h.style.display==='none'?'':'none'; return; }
    if (u === 'P' && feat('camera-path')) { cameraPathVisible = !cameraPathVisible; applyFeatureVisibility(); return; }
    if (u === 'D' && feat('director-view')) { directorView = !directorView; applyFeatureVisibility(); return; }
    if (u === 'A' && feat('aspect-ratio')) { currentAspectIndex = (currentAspectIndex+1)%ASPECT_RATIOS.length; updateAspectMasks(); return; }
  }
  if (feat('active-cam-keys') && e.shiftKey) {
    const num = parseInt(e.key);
    if (num >= 1 && num <= 4) {
      const si = getCurrentShot();
      if (num-1 < SCENE.shots[si].cameras.length) {
        activeCameraPerShot[si] = num-1; previewCamera = null; render();
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
      playhead += (ts - lastTime) / 1000;
      if (playhead >= SCENE.totalDuration) playhead = 0;
      if (playhead < viewStart || playhead > viewEnd) {
        const d = viewEnd-viewStart; viewStart = playhead; viewEnd = viewStart+d;
        if (viewEnd > SCENE.totalDuration) { viewEnd = SCENE.totalDuration; viewStart = viewEnd-d; }
      }
      render();
    }
    lastTime = ts; animLoop();
  });
}

// ── Multi-panel viewport layout ──
let currentLayout = 1;

const VIEW_PLACEHOLDERS = {
  '3d': { icon: '🎥', label: '3D Viewport', hint: 'Camera view would render here' },
  '2d': { icon: '✏️', label: '2D Designer', hint: 'Top-down scene layout and blocking' },
  'director': { icon: '🎬', label: 'Director Mode', hint: 'Storyboard overview with shot thumbnails' },
};

function setLayout(layout) {
  currentLayout = layout;
  const area = document.getElementById('viewport-area');
  area.className = 'layout-' + layout;
  document.querySelectorAll('.layout-btn').forEach(btn => {
    btn.classList.toggle('active', parseInt(btn.dataset.layout) === layout);
  });
  document.getElementById('view-panel-0').style.display = '';
  document.getElementById('view-panel-1').style.display = layout >= 2 ? '' : 'none';
  document.getElementById('view-panel-2').style.display = layout >= 3 ? '' : 'none';
  requestAnimationFrame(updateViewportFrame);
}

function updatePanelView(panelIdx, newViewType) {
  const panel = document.getElementById('view-panel-' + panelIdx);
  const content = panel.querySelector('.view-panel-content');
  const oldViewType = panel.dataset.view;

  if (newViewType === '3d') {
    // Find which panel currently has 3D and swap
    const current3d = document.querySelector('.view-panel[data-view="3d"]');
    if (current3d && current3d !== panel) {
      const currentContent = current3d.querySelector('.view-panel-content');
      // Move viewport out, put placeholder in old location
      const ph = VIEW_PLACEHOLDERS[oldViewType] || VIEW_PLACEHOLDERS['2d'];
      currentContent.innerHTML = `<div class="view-placeholder"><div class="view-placeholder-icon">${ph.icon}</div><div class="view-placeholder-label">${ph.label}</div><div class="view-placeholder-hint">${ph.hint}</div></div>`;
      current3d.dataset.view = oldViewType;
      const oldSel = current3d.querySelector('.view-selector');
      if (oldSel) oldSel.value = oldViewType;
    }
    // Move viewport into this panel
    const viewport = document.getElementById('viewport');
    content.innerHTML = '';
    content.appendChild(viewport);
  } else {
    // If this panel was 3D, move viewport to panel 0
    if (oldViewType === '3d' && panelIdx !== 0) {
      const p0 = document.getElementById('view-panel-0');
      const viewport = document.getElementById('viewport');
      p0.querySelector('.view-panel-content').innerHTML = '';
      p0.querySelector('.view-panel-content').appendChild(viewport);
      p0.dataset.view = '3d';
      const p0Sel = p0.querySelector('.view-selector');
      if (p0Sel) p0Sel.value = '3d';
    } else if (oldViewType === '3d' && panelIdx === 0) {
      // Moving 3D away from panel 0 — find another panel to host it
      const alt = currentLayout >= 2 ? document.getElementById('view-panel-1') : null;
      if (alt) {
        const viewport = document.getElementById('viewport');
        alt.querySelector('.view-panel-content').innerHTML = '';
        alt.querySelector('.view-panel-content').appendChild(viewport);
        alt.dataset.view = '3d';
        const altSel = alt.querySelector('.view-selector');
        if (altSel) altSel.value = '3d';
      }
    }
    // Set placeholder
    if (!content.querySelector('#viewport')) {
      const ph = VIEW_PLACEHOLDERS[newViewType] || VIEW_PLACEHOLDERS['2d'];
      content.innerHTML = `<div class="view-placeholder"><div class="view-placeholder-icon">${ph.icon}</div><div class="view-placeholder-label">${ph.label}</div><div class="view-placeholder-hint">${ph.hint}</div></div>`;
    }
  }
  panel.dataset.view = newViewType;
  requestAnimationFrame(updateViewportFrame);
}

// Layout chooser clicks
document.querySelectorAll('.layout-btn').forEach(btn => {
  btn.addEventListener('click', () => setLayout(parseInt(btn.dataset.layout)));
});

// View selector changes
document.querySelectorAll('.view-selector').forEach(sel => {
  sel.addEventListener('change', (e) => {
    updatePanelView(parseInt(sel.dataset.panelIdx), e.target.value);
  });
});

// ── Resize & init ──
window.addEventListener('resize', () => { render(); updateViewportFrame(); });

// Load first scene
loadScene(0);
applyFeatureVisibility();
requestAnimationFrame(updateViewportFrame);
