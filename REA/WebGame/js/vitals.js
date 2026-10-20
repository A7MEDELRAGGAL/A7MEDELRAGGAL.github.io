(function (global) {
  'use strict';

  var G = global.G;

  G.Vitals = function (opts) {
    opts = opts || {};

    this.maxHealth = opts.maxHealth || 100;
    this.maxFull = opts.maxFull || 100;
    this.maxStamina = opts.maxStamina || 100;

    this.health = this.maxHealth;
    this.full = this.maxFull;
    this.stamina = this.maxStamina;
    this.poison = 0;
    this.fire = 0;

    this.iframes = 0;
    this.dead = false;
    this.hurtFlash = 0;

    this.fullDrain = opts.fullDrain || 0.055;
    this.regenRate = opts.regenRate || 0.9;
    this.staminaDrain = opts.staminaDrain || 4.5;
    this.staminaRegen = opts.staminaRegen || 12;
    this.tickAccum = 0;
    this.staminaAccum = 0;

    this.onDeath = opts.onDeath || null;
    this.onRespawn = opts.onRespawn || null;
  };

  G.Vitals.prototype.update = function (dt) {
    if (this.dead) return;

    this.tickAccum += dt;
    if (this.tickAccum < 1) return;
    this.tickAccum -= 1;

    this.full = Math.max(0, this.full - this.fullDrain);

    if (this.full <= 0) this.damage(0.6);
    else if (this.full >= 99 && this.health < this.maxHealth) this.heal(this.regenRate);

    if (this.fire > 0) {
      this.fire = Math.max(0, this.fire - 1);
      this.damage(2.2);
    }

    if (this.poison > 0) {
      this.poison = Math.max(0, this.poison - 1);
      this.damage(1.1);
    }
  };

  G.Vitals.prototype.damage = function (amount) {
    if (this.dead || amount <= 0) return false;

    this.health = Math.max(0, this.health - amount);
    this.hurtFlash = 0.28;

    if (this.health <= 0) {
      this.dead = true;
      if (this.onDeath) this.onDeath();
    }

    return true;
  };

  G.Vitals.prototype.hurt = function (amount, iframes) {
    if (this.iframes > 0) return false;

    var applied = this.damage(amount);
    if (applied) this.iframes = iframes || 0.45;
    return applied;
  };

  G.Vitals.prototype.heal = function (amount) {
    if (this.dead) return;
    this.health = Math.min(this.maxHealth, this.health + amount);
  };

  G.Vitals.prototype.feed = function (amount) {
    this.full = Math.min(this.maxFull, this.full + amount);
  };

  G.Vitals.prototype.cure = function (poison, fire) {
    if (poison > 0) this.poison = Math.max(0, this.poison - poison);
    if (fire > 0) this.fire = Math.max(0, this.fire - fire);
  };

  G.Vitals.prototype.applyPoison = function (amount) {
    this.poison = Math.min(100, this.poison + amount);
  };

  G.Vitals.prototype.applyFire = function (amount) {
    this.fire = Math.min(100, this.fire + amount);
  };

  G.Vitals.prototype.resurrect = function () {
    this.dead = false;
    this.health = this.maxHealth * 0.6;
    this.full = Math.max(this.full, this.maxFull * 0.5);
    this.stamina = this.maxStamina;
    this.poison = 0;
    this.fire = 0;
    this.iframes = 1.2;
    if (this.onRespawn) this.onRespawn();
  };

  G.Vitals.prototype.canJump = function () {
    return !this.dead && this.stamina > 0;
  };

  G.Vitals.prototype.spendStamina = function (amount) {
    this.stamina = Math.max(0, this.stamina - amount);
  };

  G.Vitals.prototype.drainStamina = function (amount) {
    this.stamina = Math.max(0, this.stamina - amount);
  };

  G.Vitals.prototype.regenStamina = function (amount) {
    this.stamina = Math.min(this.maxStamina, this.stamina + amount);
  };

  G.Vitals.prototype.tick = function (dt) {
    if (this.iframes > 0) this.iframes -= dt;
    if (this.hurtFlash > 0) this.hurtFlash -= dt;

    this.staminaAccum += dt;
    if (this.staminaAccum >= 0.25) {
      this.staminaAccum = 0;
      if (this.sprinting) this.drainStamina(this.staminaDrain);
      else this.regenStamina(this.staminaRegen);
    }

    this.update(dt);
  };

})(window);
