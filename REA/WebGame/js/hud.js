(function (global) {
  'use strict';

  var G = global.G;

  var el = {};

  function byId(id) { return document.getElementById(id); }

  G.Hud = function () {
    el.hint = byId('hint');
    el.prompt = byId('prompt');
    el.hotbar = byId('hotbar');
    el.status = byId('status');
    el.rodstrip = byId('rodstrip');
    el.rodLabel = byId('rodLabel');
    el.barBite = byId('barBite');
    el.rodLine = byId('rodLine');

    el.barHealth = byId('barHealth');
    el.numHealth = byId('numHealth');
    el.barFull = byId('barFull');
    el.numFull = byId('numFull');
    el.barPoison = byId('barPoison');
    el.numPoison = byId('numPoison');
    el.barFire = byId('barFire');
    el.numFire = byId('numFire');

    el.barWeight = byId('barWeight');
    el.numWeight = byId('numWeight');

    el.journal = byId('journal');
    el.journalList = byId('journalList');
    el.journalSearch = byId('journalSearch');
    el.journalStats = byId('journalStats');
    el.journalPage = byId('journalPage');

    this.slots = [];
    this.lastVersion = -1;
    this.buildHotbar(9);
  };

  G.Hud.prototype.buildHotbar = function (count) {
    var html = '';
    for (var i = 0; i < count; i++) {
      html += '<div class="slot" data-i="' + i + '">'
        + '<span class="key">' + (i + 1) + '</span>'
        + '<span class="glyph">\u{2753}</span>'
        + '<span class="nm">empty</span>'
        + '<span class="ct"></span>'
        + '</div>';
    }
    el.hotbar.innerHTML = html;

    this.slots = [];
    for (var j = 0; j < count; j++) {
      this.slots.push({
        root: el.hotbar.children[j],
        glyph: el.hotbar.children[j].querySelector('.glyph'),
        nm: el.hotbar.children[j].querySelector('.nm'),
        ct: el.hotbar.children[j].querySelector('.ct')
      });
    }
  };

  G.Hud.prototype.refreshJournal = function (journal) {
    if (!el.journalList) return;

    var entries = journal.pageEntries();
    var stats = journal.stats();
    var pages = journal.pageCount();

    if (el.journalStats) {
      el.journalStats.textContent = stats.found + ' found - ' + stats.defeated + ' defeated - ' +
        stats.worth + ' coins';
    }

    if (el.journalPage) {
      el.journalPage.textContent = (pages === 0 ? 0 : journal.page + 1) + ' / ' + pages;
    }

    var html = '';

    if (!entries.length) {
      html = '<div class="journal-empty">' +
        (journal.query ? 'nothing matches "' + escapeHtml(journal.query) + '"' : 'no records yet') +
        '</div>';
    } else {
      for (var i = 0; i < entries.length; i++) {
        var e = entries[i];
        html += '<div class="journal-entry' + (e.defeated ? '' : ' unseen') + '">' +
          '<div class="row1"><span class="nm">' + escapeHtml(e.name) + '</span>' +
          '<span class="ct">' + (e.count > 1 ? 'x' + e.count : '') + '</span></div>' +
          '<div class="meta">' + escapeHtml(journal.describe(e)) + '</div></div>';
      }
    }

    el.journalList.innerHTML = html;
  };

  function escapeHtml(s) {
    return String(s)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }

  G.Hud.prototype.setHint = function (text) {
    if (el.hint) el.hint.textContent = text;
  };

  G.Hud.prototype.setPrompt = function (text) {
    if (!el.prompt) return;
    if (!text) {
      el.prompt.classList.add('hidden');
      return;
    }
    el.prompt.textContent = text;
    el.prompt.classList.remove('hidden');
  };

  G.Hud.prototype.refreshHotbar = function (inv) {
    if (inv.version === this.lastVersion) return;
    this.lastVersion = inv.version;

    for (var i = 0; i < this.slots.length; i++) {
      var slot = inv.get(i);
      var view = this.slots[i];

      if (!slot || slot.n <= 0) {
        view.glyph.textContent = '\u{2753}';
        view.nm.textContent = i === inv.selected ? 'empty' : '';
        view.ct.textContent = '';
        view.root.classList.toggle('active', i === inv.selected);
        continue;
      }

      view.glyph.textContent = G.Catalog.glyph(slot.id);
      view.nm.textContent = G.Catalog.name(slot.id);
      view.ct.textContent = slot.n > 1 ? String(slot.n) : '';
      view.root.classList.toggle('active', i === inv.selected);
      view.root.style.borderColor = slot.id === inv.baitId ? 'var(--warn)' : '';
    }
  };

  G.Hud.prototype.refreshVitals = function (v) {
    setBar(el.barHealth, v.health / v.maxHealth);
    setNum(el.numHealth, Math.ceil(v.health));
    setBar(el.barFull, v.full / v.maxFull);
    setNum(el.numFull, Math.ceil(v.full));
    setBar(el.barPoison, v.poison / 100);
    setNum(el.numPoison, Math.ceil(v.poison));
    setBar(el.barFire, v.fire / 100);
    setNum(el.numFire, Math.ceil(v.fire));
  };

  G.Hud.prototype.refreshWeight = function (inv) {
    var ratio = G.clamp01(inv.weight() / inv.maxWeight);
    if (el.barWeight) el.barWeight.style.width = (ratio * 100).toFixed(1) + '%';
    if (el.numWeight) el.numWeight.textContent = inv.weight().toFixed(1);
  };

  G.Hud.prototype.refreshRod = function (fishing, inv) {
    var held = inv.held();
    var isRod = held && G.Catalog.isRod(held.id);

    if (!isRod) {
      el.rodstrip.classList.add('hidden');
      return;
    }

    el.rodstrip.classList.remove('hidden');
    var frac = fishing.biteFraction();
    el.barBite.style.width = (frac * 100).toFixed(1) + '%';

    var label = G.Catalog.name(held.id);
    if (fishing.state !== 'idle') label += ' - ' + fishing.state;
    el.rodLabel.textContent = label;
  };

  G.Hud.prototype.refreshRodDetail = function (fishing) {
    if (!el.rodLine) return;

    var bits = [];
    bits.push('line ' + fishing.lineLength.toFixed(1) + ' m');
    if (fishing.tension > 0.01) bits.push('tension ' + Math.round(fishing.tension * 100) + '%');
    if (fishing.baitId > 0) bits.push(G.Bait.get(fishing.baitId).name);
    else bits.push('no bait');

    el.rodLine.textContent = bits.join('  -  ');
  };

  G.Hud.prototype.refreshStatus = function (ctx) {
    var rows = [];

    rows.push(row('Mode', ctx.mode));
    if (ctx.speed !== null) rows.push(row('Speed', ctx.speed.toFixed(1) + ' km/h'));
    if (ctx.draft !== null) rows.push(row('Draft', ctx.draft.toFixed(2) + ' m'));
    rows.push(row('Pos', ctx.pos));
    rows.push(row('Bait', ctx.bait));
    rows.push(row('Depth', ctx.depth.toFixed(1) + ' m'));
    rows.push(row('State', ctx.state));
    if (ctx.fps !== undefined) rows.push(row('FPS', String(ctx.fps)));
    if (ctx.errors) rows.push(row('Faults', String(ctx.errors)));

    if (el.status) el.status.innerHTML = rows.join('');
  };

  function row(k, v) {
    return '<div class="row"><span>' + k + '</span><b>' + v + '</b></div>';
  }

  function setBar(node, ratio) {
    if (!node) return;
    node.style.width = (G.clamp01(ratio) * 100).toFixed(1) + '%';
  }

  function setNum(node, value) {
    if (!node) return;
    node.textContent = String(value);
  }

})(window);
