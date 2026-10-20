(function (global) {
  'use strict';

  var G = global.G;

  // Journal store, modelled on the original's per-player creature records:
  // a linear scan keyed by a compact id, null when a record is absent, and
  // the journal being a projection of the store rather than the source of truth.
  function Entry() {
    this.id = 0;
    this.name = '';
    this.count = 0;
    this.worth = 0;
    this.discovered = false;
    this.defeated = false;
    this.note = '';
    this.firstFound = 0;
  }

  G.Journal = function (opts) {
    opts = opts || {};

    this.entries = [];
    this.page = 0;
    this.pageSize = opts.pageSize || 8;
    this.query = '';
    this.version = 0;
    this.onChange = opts.onChange || null;
  };

  G.Journal.prototype.find = function (id) {
    for (var i = 0; i < this.entries.length; i++) {
      if (this.entries[i].id === id) return this.entries[i];
    }
    return null;
  };

  G.Journal.prototype.has = function (id) {
    return this.find(id) !== null;
  };

  G.Journal.prototype.record = function (id, name, opts) {
    opts = opts || {};

    if (!id) return null;

    var entry = this.find(id);

    if (!entry) {
      entry = new Entry();
      entry.id = id;
      entry.name = name || ('Entry ' + id);
      entry.firstFound = opts.now || 0;
      this.entries.push(entry);
    }

    if (name) entry.name = name;
    entry.count += opts.count || 0;
    entry.worth = opts.worth !== undefined ? opts.worth : entry.worth;
    entry.discovered = true;
    if (opts.defeated) entry.defeated = true;
    if (opts.note) entry.note = opts.note;

    this.version++;
    if (this.onChange) this.onChange();
    return entry;
  };

  G.Journal.prototype.describe = function (entry) {
    if (!entry) return '';

    var parts = [];
    if (entry.count > 1) parts.push('x' + entry.count);
    if (entry.worth > 0) parts.push(entry.worth + ' coins');
    if (entry.defeated) parts.push('defeated');
    else if (entry.discovered) parts.push('spotted');
    if (entry.note) parts.push(entry.note);

    return parts.join(' - ');
  };

  G.Journal.prototype.matches = function (entry, query) {
    if (!query) return true;
    var q = query.toLowerCase();

    if (String(entry.name).toLowerCase().indexOf(q) >= 0) return true;
    if (String(entry.note).toLowerCase().indexOf(q) >= 0) return true;

    if (String(entry.id) === q) return true;
    if (String(entry.count) === q) return true;

    return false;
  };

  G.Journal.prototype.search = function (query, limit) {
    var q = (query === undefined || query === null) ? '' : String(query);
    var max = limit || this.pageSize * 4;
    var out = [];

    for (var i = 0; i < this.entries.length; i++) {
      if (!this.matches(this.entries[i], q)) continue;
      out.push(this.entries[i]);
      if (out.length >= max) break;
    }

    return out;
  };

  G.Journal.prototype.setQuery = function (query) {
    this.query = query || '';
    this.page = 0;
    this.version++;
    if (this.onChange) this.onChange();
  };

  G.Journal.prototype.results = function () {
    return this.search(this.query, this.entries.length);
  };

  G.Journal.prototype.pageCount = function () {
    var total = this.results().length;
    if (total === 0) return 0;
    return Math.ceil(total / this.pageSize);
  };

  G.Journal.prototype.pageEntries = function (page) {
    var all = this.results();
    var index = page === undefined ? this.page : page;
    if (index < 0) index = 0;
    if (index >= this.pageCount()) index = this.pageCount() - 1;
    if (index < 0) return [];

    this.page = index;
    return all.slice(index * this.pageSize, index * this.pageSize + this.pageSize);
  };

  G.Journal.prototype.scrollPage = function (delta) {
    var count = this.pageCount();
    if (count === 0) return 0;

    var next = this.page + delta;
    if (next < 0) next = count - 1;
    if (next >= count) next = 0;

    this.page = next;
    this.version++;
    if (this.onChange) this.onChange();
    return this.page;
  };

  G.Journal.prototype.stats = function () {
    var found = 0;
    var defeated = 0;
    var worth = 0;

    for (var i = 0; i < this.entries.length; i++) {
      if (this.entries[i].discovered) found++;
      if (this.entries[i].defeated) defeated++;
      worth += this.entries[i].worth;
    }

    return { found: found, defeated: defeated, worth: worth, total: this.entries.length };
  };

})(window);
