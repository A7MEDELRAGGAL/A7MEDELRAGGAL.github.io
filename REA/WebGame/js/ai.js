(function (global) {
  'use strict';

  var G = global.G;

  var DEFAULT_BASE = 'https://api.beatapi.io';
  var STORAGE_KEY = 'tidebreak.aiKey';
  var memoryKey = '';

  var FREE_MODELS = [
    'model:gemini-3.8-flash-free',
    'model:deepseek-v4.1-flash-free',
    'model:deepseek-v4-flash-0731-free'
  ];

  var SYSTEM = 'You are the dockhand NPC of Tidebreak, a sunny open-water fishing sandbox game. ' +
    'Reply in short, friendly chat lines (max 2 sentences). Stay in character as a local sailor. ' +
    'Never break character or mention being an AI.';

  function config() {
    var local = G.AI_CONFIG || {};
    var base = local.base || DEFAULT_BASE;
    var key = local.key || memoryKey;

    if (!key) {
      try {
        key = global.localStorage.getItem(STORAGE_KEY) || '';
      } catch (e) {
        key = '';
      }
    }

    return { base: base, key: key };
  }

  function post(path, body) {
    var cfg = config();
    if (!cfg.key) {
      return Promise.reject(new Error('no API key set'));
    }

    return global.fetch(cfg.base + path, {
      method: 'POST',
      headers: {
        'Authorization': 'Bearer ' + cfg.key,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(body)
    }).then(function (res) {
      if (!res.ok) {
        return res.text().then(function (t) {
          throw new Error('HTTP ' + res.status + ': ' + String(t).slice(0, 120));
        });
      }
      return res.json();
    });
  }

  function chatOnce(model, messages) {
    return post('/v1/chat/completions', {
      model: model,
      messages: messages,
      max_tokens: 128,
      stream: false
    }).then(function (data) {
      var c = data && data.choices && data.choices[0];
      var msg = c && c.message && c.message.content;
      if (!msg) throw new Error('empty reply from ' + model);
      return { model: model, text: msg };
    });
  }

  function chat(messages, opts) {
    opts = opts || {};
    var list = opts.models || FREE_MODELS;
    var i = 0;

    function next() {
      if (i >= list.length) return Promise.reject(new Error('all models failed'));
      var model = list[i++];
      return chatOnce(model, messages).catch(function (e) {
        if (opts.signal && opts.signal.aborted) throw e;
        return next();
      });
    }

    return next();
  }

  function byId(id) { return document.getElementById(id); }

  G.AI = {
    BASE: DEFAULT_BASE,
    STORAGE_KEY: STORAGE_KEY,
    FREE_MODELS: FREE_MODELS.slice(),
    DEFAULT_MODEL: FREE_MODELS[0],
    chat: chat,
    post: post,
    hasKey: function () { return !!config().key; },
    setKey: function (key) {
      memoryKey = key || '';

      try {
        if (key) global.localStorage.setItem(STORAGE_KEY, key);
        else global.localStorage.removeItem(STORAGE_KEY);
      } catch (e) {
        return !!memoryKey;
      }
      return true;
    }
  };

  G.AiChat = function () {
    this.root = byId('aiChat');
    this.log = byId('aiChatLog');
    this.form = byId('aiChatForm');
    this.input = byId('aiChatInput');
    this.status = byId('aiChatStatus');
    this.sel = byId('aiChatModel');
    this.keyField = byId('aiChatKey');
    this.keySave = byId('aiChatKeySave');
    this.busy = false;
    this.open = false;
    this.history = [];

    if (!this.root || !this.form || !this.input) {
      this.disabled = true;
      return;
    }

    if (this.sel) {
      for (var i = 0; i < FREE_MODELS.length; i++) {
        var opt = document.createElement('option');
        opt.value = FREE_MODELS[i];
        opt.textContent = FREE_MODELS[i].replace(/^model:/, '');
        this.sel.appendChild(opt);
      }
    }

    if (this.keyField) this.keyField.value = '';
    this.refreshKeyState();

    var self = this;

    if (this.keySave) {
      this.keySave.addEventListener('click', function () {
        G.AI.setKey((self.keyField.value || '').trim());
        self.keyField.value = '';
        self.refreshKeyState();
      });
    }

    this.form.addEventListener('submit', function (e) {
      e.preventDefault();
      self.send();
    });
  };

  G.AiChat.prototype.refreshKeyState = function () {
    var has = G.AI.hasKey();
    if (this.keyField) this.keyField.placeholder = has ? 'key saved — type to replace' : 'paste your API key';
    this.setStatus(has ? '' : 'no API key yet — paste one below to talk to the dockhand');
  };

  G.AiChat.prototype.toggle = function () {
    if (this.disabled) return;
    if (this.open) this.close();
    else this.show();
  };

  G.AiChat.prototype.show = function () {
    if (this.disabled) return;
    this.open = true;
    this.root.classList.remove('hidden');
    if (this.input) this.input.focus();
    if (global.Tidebreak && global.Tidebreak.onChatOpen) global.Tidebreak.onChatOpen(true);
  };

  G.AiChat.prototype.close = function () {
    if (this.disabled) return;
    this.open = false;
    this.root.classList.add('hidden');
    if (this.input) this.input.blur();
    if (global.Tidebreak && global.Tidebreak.onChatOpen) global.Tidebreak.onChatOpen(false);
  };

  G.AiChat.prototype.append = function (who, text) {
    if (!this.log) return;
    var row = document.createElement('div');
    row.className = 'line ' + (who === 'you' ? 'you' : 'ai');
    row.textContent = (who === 'you' ? '> ' : '* ') + text;
    this.log.appendChild(row);
    this.log.scrollTop = this.log.scrollHeight;
  };

  G.AiChat.prototype.setStatus = function (t) {
    if (this.status) this.status.textContent = t || '';
  };

  G.AiChat.prototype.send = function () {
    if (this.disabled || this.busy) return;

    var text = (this.input.value || '').trim();
    if (!text) return;

    this.input.value = '';
    this.append('you', text);
    this.history.push({ role: 'user', content: text });
    if (this.history.length > 20) this.history.shift();

    var self = this;
    var model = this.sel ? this.sel.value : G.AI.DEFAULT_MODEL;
    this.busy = true;
    this.setStatus('thinking...');

    var msgs = [{ role: 'system', content: SYSTEM }].concat(this.history);

    G.AI.chat(msgs, { models: [model] }).then(function (r) {
      self.append('ai', String(r.text).trim());
      self.history.push({ role: 'assistant', content: r.text });
      self.setStatus('');
    }).catch(function (e) {
      self.append('ai', '...signal lost. (' + (e && e.message ? e.message : 'error') + ')');
      self.setStatus('');
    }).then(function () {
      self.busy = false;
    });
  };

})(window);
