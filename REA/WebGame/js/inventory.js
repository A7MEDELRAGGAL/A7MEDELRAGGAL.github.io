(function (global) {
  'use strict';

  var G = global.G;

  function Slot() {
    this.id = -1;
    this.n = 0;
    this.cooked = 0;
  }

  G.Inventory = function (opts) {
    opts = opts || {};

    this.slots = [];
    this.slotCount = opts.slots || 24;
    this.hotbar = opts.hotbar || 9;
    this.maxWeight = opts.maxWeight || 140;

    for (var i = 0; i < this.slotCount; i++) this.slots.push(new Slot());

    this.selected = 0;
    this.baitId = -1;
    this.version = 0;
  };

  G.Inventory.prototype.get = function (index) {
    return index >= 0 && index < this.slots.length ? this.slots[index] : null;
  };

  G.Inventory.prototype.selectedSlot = function () {
    return this.get(this.selected);
  };

  G.Inventory.prototype.held = function () {
    var s = this.selectedSlot();
    return s && s.n > 0 ? s : null;
  };

  G.Inventory.prototype.weight = function () {
    var total = 0;
    for (var i = 0; i < this.slots.length; i++) {
      var s = this.slots[i];
      if (s.n > 0) total += G.Catalog.weightOf(s.id) * s.n;
    }
    return total;
  };

  G.Inventory.prototype.overweight = function () {
    return this.weight() > this.maxWeight;
  };

  G.Inventory.prototype.add = function (id, amount) {
    if (!id || amount <= 0) return 0;

    var max = G.Catalog.stackMax(id);
    var unit = G.Catalog.weightOf(id);
    var cap = this.maxWeight;
    var running = this.weight();
    var added = 0;
    var i, s, moved, space, put;

    function room(count) {
      return running + unit * count <= cap;
    }

    for (i = 0; i < this.slots.length && added < amount; i++) {
      s = this.slots[i];
      if (s.n <= 0 || s.id !== id || max <= 1) continue;

      space = max - s.n;
      if (space <= 0) continue;

      moved = Math.min(space, amount - added);
      if (!room(moved)) break;

      s.n += moved;
      running += unit * moved;
      added += moved;
    }

    for (i = 0; i < this.slots.length && added < amount; i++) {
      s = this.slots[i];
      if (s.n > 0) continue;

      put = Math.min(max, amount - added);
      if (!room(put)) break;

      s.id = id;
      s.n = put;
      s.cooked = 0;
      running += unit * put;
      added += put;
    }

    if (added > 0) this.version++;
    return added;
  };

  G.Inventory.prototype.count = function (id) {
    var total = 0;
    for (var i = 0; i < this.slots.length; i++) {
      if (this.slots[i].n > 0 && this.slots[i].id === id) total += this.slots[i].n;
    }
    return total;
  };

  G.Inventory.prototype.removeFrom = function (index, amount) {
    var s = this.get(index);
    if (!s || s.n <= 0) return 0;

    var taken = Math.min(s.n, amount);
    s.n -= taken;
    if (s.n <= 0) {
      s.id = -1;
      s.n = 0;
      s.cooked = 0;
    }

    this.version++;
    return taken;
  };

  G.Inventory.prototype.consume = function (index) {
    var s = this.get(index);
    if (!s || s.n <= 0) return null;

    var def = G.Catalog.get(s.id);
    if (!G.Catalog.isEdible(s.id)) return null;

    this.removeFrom(index, 1);
    return def;
  };

  G.Inventory.prototype.select = function (index) {
    if (index < 0 || index >= this.slots.length) return;
    if (this.selected === index) return;
    this.selected = index;
    this.version++;
  };

  G.Inventory.prototype.cycle = function (dir) {
    var next = (this.selected + dir + this.slotCount) % this.slotCount;
    this.select(next);
  };

  G.Inventory.prototype.setBaitFrom = function (index) {
    var s = this.get(index);
    if (!s || s.n <= 0) {
      this.baitId = -1;
      this.version++;
      return;
    }

    this.baitId = G.Catalog.isBait(s.id) ? s.id : -1;
    this.version++;
  };

  G.Inventory.prototype.useBait = function () {
    if (!this.baitId) return false;

    for (var i = 0; i < this.slots.length; i++) {
      var s = this.slots[i];
      if (s.n <= 0 || s.id !== this.baitId) continue;
      this.removeFrom(i, 1);
      if (this.count(this.baitId) <= 0) this.baitId = -1;
      return true;
    }

    this.baitId = -1;
    return false;
  };

  G.Inventory.prototype.grantStarter = function () {
    var kit = G.Catalog.starterKit();
    for (var i = 0; i < kit.length; i++) this.add(kit[i].id, kit[i].n);
    this.baitId = 8;
    this.select(0);
  };

})(window);
