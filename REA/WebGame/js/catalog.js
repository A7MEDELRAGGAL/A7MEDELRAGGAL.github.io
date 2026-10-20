(function (global) {
  'use strict';

  var G = global.G;

  var ITEMS = [
    { id: 1, name: 'Cedar Rod', cat: 'rod', weight: 1.6, stack: 1, cast: 22, reel: 6.5, biteMin: 1.4, biteMax: 5.5, hook: 1.35, glyph: '\u{1F3A3}' },
    { id: 2, name: 'Reed Rod', cat: 'rod', weight: 1.1, stack: 1, cast: 14, reel: 4.6, biteMin: 2.2, biteMax: 8, hook: 1.05, glyph: '\u{1F41F}' },
    { id: 3, name: 'Stone Spear', cat: 'melee', weight: 2.4, stack: 1, damage: 26, knock: 7, reach: 3.0, glyph: '\u{1F5E1}' },
    { id: 4, name: 'Iron Knife', cat: 'melee', weight: 0.7, stack: 1, damage: 17, knock: 3.5, reach: 2.1, glyph: '\u{1F52A}' },
    { id: 5, name: 'Stone Pick', cat: 'tool', weight: 2.0, stack: 1, glyph: '\u{2692}' },
    { id: 6, name: 'Torch', cat: 'tool', weight: 0.9, stack: 1, glyph: '\u{1F526}' },
    { id: 7, name: 'Rope Coil', cat: 'tool', weight: 0.8, stack: 3, glyph: '\u{1FA9D}' },
    { id: 8, name: 'Worm Bait', cat: 'bait', weight: 0.05, stack: 40, glyph: '\u{1F41B}' },
    { id: 9, name: 'Dough Bait', cat: 'bait', weight: 0.06, stack: 40, glyph: '\u{1F35E}' },
    { id: 10, name: 'Kelp Wrack', cat: 'resource', weight: 0.3, stack: 12, glyph: '\u{1F33F}' },
    { id: 11, name: 'Mackerel', cat: 'resource', weight: 0.9, stack: 6, food: 14, heal: 2, glyph: '\u{1F41F}' },
    { id: 12, name: 'Pike', cat: 'resource', weight: 2.6, stack: 4, food: 26, heal: 5, glyph: '\u{1F420}' },
    { id: 13, name: 'Eel', cat: 'resource', weight: 2.1, stack: 4, food: 20, heal: 4, glyph: '\u{1F40D}' },
    { id: 14, name: 'Clam', cat: 'resource', weight: 0.4, stack: 12, food: 8, glyph: '\u{1F41A}' },
    { id: 15, name: 'Crab', cat: 'resource', weight: 0.6, stack: 8, food: 10, heal: 3, glyph: '\u{1F980}' },
    { id: 16, name: 'Bread Loaf', cat: 'consumable', weight: 0.5, stack: 5, food: 28, glyph: '\u{1F35E}' },
    { id: 17, name: 'Wild Berry', cat: 'consumable', weight: 0.1, stack: 20, food: 5, glyph: '\u{1F353}' },
    { id: 18, name: 'Linen Bandage', cat: 'consumable', weight: 0.15, stack: 6, heal: 25, glyph: '\u{1FA7A}' },
    { id: 19, name: 'Charcoal Draught', cat: 'consumable', weight: 0.4, stack: 4, heal: 4, cure: 100, glyph: '\u{1F9EA}' }
  ];

  var BY_ID = {};
  var CATCH_POOL = [11, 11, 11, 12, 13, 14, 15, 10];

  for (var i = 0; i < ITEMS.length; i++) BY_ID[ITEMS[i].id] = ITEMS[i];

  G.Catalog = {
    all: ITEMS,

    get: function (id) { return BY_ID[id] || null; },

    isRod: function (id) { var d = BY_ID[id]; return !!d && d.cat === 'rod'; },
    isBait: function (id) { var d = BY_ID[id]; return !!d && d.cat === 'bait'; },
    isMelee: function (id) { var d = BY_ID[id]; return !!d && d.cat === 'melee'; },
    isEdible: function (id) {
      var d = BY_ID[id];
      return !!d && ((d.food || 0) > 0 || (d.heal || 0) > 0 || (d.cure || 0) > 0);
    },
    isStackable: function (id) { var d = BY_ID[id]; return !!d && d.stack > 1; },
    stackMax: function (id) { var d = BY_ID[id]; return d ? d.stack : 1; },
    weightOf: function (id) { var d = BY_ID[id]; return d ? d.weight : 0.1; },
    glyph: function (id) { var d = BY_ID[id]; return d ? d.glyph : '\u{2753}'; },
    name: function (id) { var d = BY_ID[id]; return d ? d.name : 'Nothing'; },

    worth: function (id) {
      var d = BY_ID[id];
      if (!d) return 0;
      if (d.cat === 'resource') return Math.round(d.weight * 60);
      if (d.cat === 'consumable') return Math.round(d.weight * 30);
      if (d.cat === 'bait') return Math.round(d.weight * 20);
      return Math.round(d.weight * 40);
    },

    rollCatch: function (depthBias) {
      var pool = CATCH_POOL;
      if (depthBias > 0.6) pool = [12, 12, 13, 13, 11];
      return pool[Math.floor(Math.random() * pool.length)];
    },

    starterKit: function () {
      return [
        { id: 1, n: 1 },
        { id: 8, n: 12 },
        { id: 3, n: 1 },
        { id: 16, n: 2 },
        { id: 10, n: 3 }
      ];
    }
  };

})(window);
