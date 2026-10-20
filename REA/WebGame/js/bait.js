(function (global) {
  'use strict';

  var G = global.G;

  // A bait is a species preference table plus the timing that governs a bite.
  // The roles mirror the original's bait record: a catch window, the chance the
  // bait is lost, whether the catch has to be reeled in, and the weight of each
  // fish it can draw.
  var BAITS = [
    {
      id: 8, name: 'Worm Bait', level: 1, glyph: '\u{1F41B}', weight: 0.05, stack: 40,
      catchTimeMin: 1.2, catchTimeMax: 4.0, lostOnBiteChance: 0.15, requireReeling: false,
      fish: { 11: 3, 14: 1, 15: 1, 10: 1 }
    },
    {
      id: 9, name: 'Dough Bait', level: 1, glyph: '\u{1F35E}', weight: 0.06, stack: 40,
      catchTimeMin: 2.0, catchTimeMax: 6.5, lostOnBiteChance: 0.08, requireReeling: false,
      fish: { 12: 2, 11: 1, 13: 1 }
    },
    {
      id: 21, name: 'Minnow', level: 2, glyph: '\u{1F41F}', weight: 0.12, stack: 20,
      catchTimeMin: 1.0, catchTimeMax: 3.2, lostOnBiteChance: 0.18, requireReeling: true,
      fish: { 12: 3, 11: 2, 13: 1 }
    },
    {
      id: 22, name: 'Shrimp', level: 2, glyph: '\u{1F990}', weight: 0.1, stack: 20,
      catchTimeMin: 1.4, catchTimeMax: 4.5, lostOnBiteChance: 0.12, requireReeling: true,
      fish: { 13: 3, 15: 2, 12: 1 }
    },
    {
      id: 23, name: 'Sea Grape', level: 3, glyph: '\u{1F347}', weight: 0.15, stack: 15,
      catchTimeMin: 2.5, catchTimeMax: 7.5, lostOnBiteChance: 0.05, requireReeling: true,
      fish: { 10: 3, 14: 2, 11: 1 }
    }
  ];

  var BY_ID = {};
  for (var i = 0; i < BAITS.length; i++) BY_ID[BAITS[i].id] = BAITS[i];

  G.Bait = {
    all: BAITS,

    get: function (id) { return BY_ID[id] || null; },

    isBait: function (id) { return !!BY_ID[id]; },

    catchWindow: function (id) {
      var bait = BY_ID[id];
      if (!bait) return { min: 2, max: 6 };
      return { min: bait.catchTimeMin, max: bait.catchTimeMax };
    },

    // the fish this bait can draw, as a weighted list
    candidates: function (id) {
      var bait = BY_ID[id];
      if (!bait) return [];

      var out = [];
      for (var key in bait.fish) {
        if (!Object.prototype.hasOwnProperty.call(bait.fish, key)) continue;
        out.push({ itemId: parseInt(key, 10), weight: bait.fish[key] });
      }

      return out;
    },

    rollFish: function (id) {
      var pool = this.candidates(id);
      if (!pool.length) return 11;

      var total = 0;
      var i;
      for (i = 0; i < pool.length; i++) total += pool[i].weight;

      var roll = Math.random() * total;
      for (i = 0; i < pool.length; i++) {
        roll -= pool[i].weight;
        if (roll <= 0) return pool[i].itemId;
      }

      return pool[pool.length - 1].itemId;
    },

    // a bait the player has can still attract a fish above its own level
    meetsLevel: function (id, level) {
      var bait = BY_ID[id];
      if (!bait) return false;
      return (level || 1) <= bait.level + 1;
    },

    lostOnBiteChance: function (id) {
      var bait = BY_ID[id];
      return bait ? bait.lostOnBiteChance : 0.1;
    },

    requiresReeling: function (id) {
      var bait = BY_ID[id];
      return bait ? bait.requireReeling : false;
    }
  };

})(window);
