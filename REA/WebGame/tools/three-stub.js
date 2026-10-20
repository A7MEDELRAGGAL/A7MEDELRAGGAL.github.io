'use strict';

const T = Math.PI * 2;

class V3 {
  constructor(x = 0, y = 0, z = 0) { this.x = x; this.y = y; this.z = z; }
  set(x, y, z) { this.x = x; this.y = y; this.z = z; return this; }
  copy(v) { this.x = v.x; this.y = v.y; this.z = v.z; return this; }
  clone() { return new V3(this.x, this.y, this.z); }
  add(v) { this.x += v.x; this.y += v.y; this.z += v.z; return this; }
  addScaledVector(v, s) { this.x += v.x * s; this.y += v.y * s; this.z += v.z * s; return this; }
  sub(v) { this.x -= v.x; this.y -= v.y; this.z -= v.z; return this; }
  subVectors(a, b) { this.x = a.x - b.x; this.y = a.y - b.y; this.z = a.z - b.z; return this; }
  multiplyScalar(s) { this.x *= s; this.y *= s; this.z *= s; return this; }
  dot(v) { return this.x * v.x + this.y * v.y + this.z * v.z; }
  cross(v) {
    const x = this.y * v.z - this.z * v.y;
    const y = this.z * v.x - this.x * v.z;
    const z = this.x * v.y - this.y * v.x;
    return this.set(x, y, z);
  }
  lengthSq() { return this.x * this.x + this.y * this.y + this.z * this.z; }
  length() { return Math.sqrt(this.lengthSq()); }
  distanceTo(v) { const dx = this.x - v.x, dy = this.y - v.y, dz = this.z - v.z; return Math.sqrt(dx * dx + dy * dy + dz * dz); }
  normalize() { const l = this.length() || 1; return this.multiplyScalar(1 / l); }
  lerp(v, t) { this.x += (v.x - this.x) * t; this.y += (v.y - this.y) * t; this.z += (v.z - this.z) * t; return this; }
  setScalar(s) { return this.set(s, s, s); }
  setY(y) { this.y = y; return this; }
}

class Quat {
  constructor(x = 0, y = 0, z = 0, w = 1) { this.x = x; this.y = y; this.z = z; this.w = w; }
  copy(q) { this.x = q.x; this.y = q.y; this.z = q.z; this.w = q.w; return this; }
  clone() { return new Quat(this.x, this.y, this.z, this.w); }
  setFromAxisAngle(axis, angle) {
    const half = angle / 2;
    const s = Math.sin(half);
    this.x = axis.x * s;
    this.y = axis.y * s;
    this.z = axis.z * s;
    this.w = Math.cos(half);
    return this;
  }
}

class Matrix4 {
  constructor() {
    this.elements = new Array(16).fill(0);
    this.elements[0] = this.elements[5] = this.elements[10] = this.elements[15] = 1;
  }
  compose(pos, quat, scale) {
    const e = this.elements;
    e[0] = (1 - 2 * (quat.y * quat.y + quat.z * quat.z)) * scale.x;
    e[1] = (2 * (quat.x * quat.y + quat.z * quat.w)) * scale.x;
    e[2] = (2 * (quat.x * quat.z - quat.y * quat.w)) * scale.x;
    e[4] = (2 * (quat.x * quat.y - quat.z * quat.w)) * scale.y;
    e[5] = (1 - 2 * (quat.x * quat.x + quat.z * quat.z)) * scale.y;
    e[6] = (2 * (quat.y * quat.z + quat.x * quat.w)) * scale.y;
    e[8] = (2 * (quat.x * quat.z + quat.y * quat.w)) * scale.z;
    e[9] = (2 * (quat.y * quat.z - quat.x * quat.w)) * scale.z;
    e[10] = (1 - 2 * (quat.x * quat.x + quat.y * quat.y)) * scale.z;
    e[12] = pos.x; e[13] = pos.y; e[14] = pos.z;
    return this;
  }
  setPosition(v) { this.elements[12] = v.x; this.elements[13] = v.y; this.elements[14] = v.z; return this; }
  scale(v) {
    this.elements[0] *= v.x; this.elements[1] *= v.x; this.elements[2] *= v.x;
    this.elements[4] *= v.y; this.elements[5] *= v.y; this.elements[6] *= v.y;
    this.elements[8] *= v.z; this.elements[9] *= v.z; this.elements[10] *= v.z;
    return this;
  }
}

class Euler {
  constructor(x = 0, y = 0, z = 0) { this.x = x; this.y = y; this.z = z; }
  set(x, y, z) { this.x = x; this.y = y; this.z = z; return this; }
}

class Color {
  constructor(c) {
    if (typeof c === 'number') this.setHex(c);
    else if (c && typeof c === 'object') this.copy(c);
    else this.setHex(0xffffff);
  }
  setHex(hex) {
    this.r = ((hex >> 16) & 255) / 255;
    this.g = ((hex >> 8) & 255) / 255;
    this.b = (hex & 255) / 255;
    return this;
  }
  copy(c) { this.r = c.r; this.g = c.g; this.b = c.b; return this; }
  lerp(c, t) {
    this.r += (c.r - this.r) * t;
    this.g += (c.g - this.g) * t;
    this.b += (c.b - this.b) * t;
    return this;
  }
  multiplyScalar(s) { this.r *= s; this.g *= s; this.b *= s; return this; }
}

class Attr {
  constructor(array, itemSize) {
    this.array = array || [];
    this.itemSize = itemSize || 3;
    this.count = Math.floor(this.array.length / this.itemSize);
    this.needsUpdate = false;
  }
  getX(i) { return this.array[i * this.itemSize]; }
  getY(i) { return this.array[i * this.itemSize + 1]; }
  getZ(i) { return this.array[i * this.itemSize + 2]; }
  setX(i, v) { this.array[i * this.itemSize] = v; }
  setY(i, v) { this.array[i * this.itemSize + 1] = v; }
  setZ(i, v) { this.array[i * this.itemSize + 2] = v; }
  setXYZ(i, x, y, z) {
    this.array[i * this.itemSize] = x;
    this.array[i * this.itemSize + 1] = y;
    this.array[i * this.itemSize + 2] = z;
  }
}

function gridGeometry(cols, rows) {
  const array = [];
  for (let i = 0; i < cols * rows; i++) array.push(0, 0, 0);
  const geo = new Geo();
  geo.attributes.position = new Attr(array, 3);
  geo.parameters = { cols, rows };
  return geo;
}

class Geo {
  constructor() {
    this.attributes = {};
    this.userData = {};
  }
  setAttribute(name, attr) { this.attributes[name] = attr; return this; }
  getAttribute(name) { return this.attributes[name]; }
  rotateX() { return this; }
  rotateY() { return this; }
  computeVertexNormals() { return this; }
  computeBoundingSphere() { this.boundingSphere = { radius: 1000 }; return this; }
  setFromPoints(points) {
    const array = [];
    for (const p of points) array.push(p.x, p.y, p.z);
    this.attributes.position = new Attr(array, 3);
    return this;
  }
  dispose() {}
}

class Object3D {
  constructor() {
    this.position = new V3();
    this.rotation = new Euler();
    this.quaternion = new Quat();
    this.scale = new V3(1, 1, 1);
    this.children = [];
    this.parent = null;
    this.userData = {};
    this.visible = true;
    this.renderOrder = 0;
    this.frustumCulled = true;
    this.positionArrived = true;
  }
  add(child) { child.parent = this; this.children.push(child); return this; }
  remove(child) {
    const i = this.children.indexOf(child);
    if (i >= 0) { this.children.splice(i, 1); child.parent = null; }
    return this;
  }
  updateMatrixWorld() { return this; }
  updateMatrix() { return this; }
  localToWorld(v) {
    let obj = this;
    while (obj) {
      v.x += obj.position.x;
      v.y += obj.position.y;
      v.z += obj.position.z;
      obj = obj.parent;
    }
    return v;
  }
  getWorldPosition(out) { out = out || new V3(); return out.copy(this.position); }
  traverse(fn) { fn(this); for (const c of this.children) c.traverse(fn); }
  clone() {
    const copy = new this.constructor();
    copy.position.copy(this.position);
    copy.rotation.set(this.rotation.x, this.rotation.y, this.rotation.z);
    copy.scale.copy(this.scale);
    copy.userData = Object.assign({}, this.userData);
    copy.visible = this.visible;
    for (const c of this.children) copy.add(c.clone());
    return copy;
  }
}

class Mesh extends Object3D {
  constructor(geometry, material) {
    super();
    this.geometry = geometry || new Geo();
    this.material = material || null;
    this.receiveShadow = false;
    this.castShadow = false;
  }
}

class Group extends Object3D {}

class InstancedMesh extends Mesh {
  constructor(geometry, material, count) {
    super(geometry, material);
    this.count = count;
    this.instanceMatrix = { needsUpdate: false, array: new Array(count * 16).fill(0) };
  }
  setMatrixAt(i, m) { this.instanceMatrix.array[i * 16] = m.elements[12]; }
}

class Line extends Object3D {
  constructor(geometry, material) {
    super();
    this.geometry = geometry || new Geo();
    this.material = material || null;
  }
}

class Material {
  constructor(params) { Object.assign(this, params || {}); this.opacity = (params && params.opacity) || 1; }
  dispose() {}
}

class PlaneGeometry extends Geo {
  constructor(w, h, wSeg, hSeg) {
    super();
    const cols = (wSeg || 1) + 1;
    const rows = (hSeg || 1) + 1;
    this.attributes.position = new Attr(new Array(cols * rows * 3).fill(0), 3);
    this.parameters = { cols, rows };
  }
}

class BoxGeometry extends Geo { constructor() { super(); this.attributes.position = new Attr([0, 0, 0, 0, 0, 0], 3); } }
class SphereGeometry extends BoxGeometry {}
class CylinderGeometry extends BoxGeometry {}
class ConeGeometry extends BoxGeometry {}
class TorusGeometry extends BoxGeometry {}
class DodecahedronGeometry extends BoxGeometry {}

class RingGeometry extends BoxGeometry {}

class CircleGeometry extends BoxGeometry {
  constructor(r) { super(); this.parameters = { radius: r }; }
}

class Light extends Object3D {
  constructor(color, intensity) { super(); this.color = new Color(color || 0xffffff); this.intensity = intensity || 1; }
}
class HemisphereLight extends Light {}
class DirectionalLight extends Light {}

class Scene extends Object3D {
  constructor() { super(); this.fog = null; this.background = null; }
}

class PerspectiveCamera extends Object3D {
  constructor(fov, aspect, near, far) {
    super();
    this.fov = fov; this.aspect = aspect; this.near = near; this.far = far;
  }
  updateProjectionMatrix() {}
  lookAt(v) { this.lookTarget = v.clone(); }
}

class FogExp2 { constructor(color, density) { this.color = new Color(color); this.density = density; } }

class Texture { constructor() {} }
class CanvasTexture extends Texture {}

class WebGLRenderer {
  constructor(params) {
    this.domElement = (params && params.domElement) || null;
    this.outputEncoding = 0;
    this.shadowMap = { enabled: false };
    this.renderCount = 0;
    this.lastScene = null;
    this.lastCamera = null;
  }
  setPixelRatio() {}
  setSize() {}
  render(scene, camera) {
    this.renderCount++;
    this.lastScene = scene;
    this.lastCamera = camera;
  }
}

module.exports = {
  Vector3: V3,
  Quaternion: Quat,
  Matrix4: Matrix4,
  Euler: Euler,
  InstancedMesh: InstancedMesh,
  ACESFilmicToneMapping: 'ACESFilmicToneMapping',
  Color: Color,
  Object3D: Object3D,
  Group: Group,
  Mesh: Mesh,
  Line: Line,
  BufferGeometry: Geo,
  BufferAttribute: Attr,
  PlaneGeometry: PlaneGeometry,
  BoxGeometry: BoxGeometry,
  SphereGeometry: SphereGeometry,
  CylinderGeometry: CylinderGeometry,
  ConeGeometry: ConeGeometry,
  TorusGeometry: TorusGeometry,
  RingGeometry: RingGeometry,
  DodecahedronGeometry: DodecahedronGeometry,
  CircleGeometry: CircleGeometry,
  MeshStandardMaterial: Material,
  MeshBasicMaterial: Material,
  ShaderMaterial: Material,
  LineBasicMaterial: Material,
  HemisphereLight: HemisphereLight,
  DirectionalLight: DirectionalLight,
  Scene: Scene,
  PerspectiveCamera: PerspectiveCamera,
  FogExp2: FogExp2,
  Texture: Texture,
  CanvasTexture: CanvasTexture,
  WebGLRenderer: WebGLRenderer,
  EquirectangularReflectionMapping: 'EquirectangularReflectionMapping',
  sRGBEncoding: 'sRGBEncoding',
  DoubleSide: 'DoubleSide',
  REVISION: '128-stub',
  __tau: T,
  __gridGeometry: gridGeometry
};
