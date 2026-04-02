export class Vector2 {
  constructor(x = 0, y = 0) {
    this.x = x;
    this.y = y;
  }

  add(v) {
    this.x = (isNaN(this.x + v.x)) ? 0 : this.x + v.x;
    this.y = (isNaN(this.y + v.y)) ? 0 : this.y + v.y;
    return this;
  }

  sub(v) {
    this.x -= v.x;
    this.y -= v.y;
    return this;
  }

  static sub(v1, v2) {
    return new Vector2(v1.x - v2.x, v1.y - v2.y);
  }

  mult(n) {
    this.x = (isNaN(this.x * n)) ? 0 : this.x * n;
    this.y = (isNaN(this.y * n)) ? 0 : this.y * n;
    return this;
  }

  div(n) {
    if (n === 0 || isNaN(n)) return this;
    this.x /= n;
    this.y /= n;
    return this;
  }

  magSq() {
    return this.x * this.x + this.y * this.y;
  }

  mag() {
    return Math.sqrt(this.magSq());
  }

  normalize() {
    const len = this.mag();
    if (len !== 0) {
      this.mult(1 / len);
    }
    return this;
  }

  limit(max) {
    const mSq = this.magSq();
    if (isNaN(mSq)) { this.x = 0; this.y = 0; return this; }
    if (mSq > max * max) {
      this.normalize().mult(max);
    }
    return this;
  }

  setMag(n) {
    return this.normalize().mult(n);
  }

  distance(v) {
    const dx = this.x - v.x;
    const dy = this.y - v.y;
    return Math.sqrt(dx * dx + dy * dy);
  }

  copy() {
    return new Vector2(this.x, this.y);
  }
}
