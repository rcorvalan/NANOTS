import { Vector2 } from './Vector2.js';

export class Predator {
  constructor(x, y, maxSpeed = 3.5, maxForce = 0.15) {
    this.position = new Vector2(x, y);
    this.velocity = new Vector2(Math.random() * 2 - 1, Math.random() * 2 - 1).setMag(maxSpeed);
    this.acceleration = new Vector2(0, 0);
    this.maxSpeed = maxSpeed;
    this.maxForce = maxForce;
    
    this.size = 8;
    this.energy = 1000;
  }

  applyForce(force) {
    this.acceleration.add(force);
  }

  seek(nanots) {
    let closestId = null;
    let closestDistSq = Infinity;
    
    // Persigue al nanot más cercano que genere ruido electromagnético
    for (let n of nanots) {
      if (n.dead) continue;
      let dSq = this.position.distance(n.position);
      if (dSq < 200) { 
        if (dSq < closestDistSq) {
          closestDistSq = dSq;
          closestId = n;
        }
      }
    }
    
    if (closestId) {
      let desired = Vector2.sub(closestId.position, this.position);
      desired.setMag(this.maxSpeed);
      let steer = Vector2.sub(desired, this.velocity);
      steer.limit(this.maxForce);
      this.applyForce(steer);
    }
  }

  update(env, nanots) {
    this.seek(nanots);
    
    this.velocity.add(this.acceleration);
    this.velocity.limit(this.maxSpeed);
    this.position.add(this.velocity);
    this.acceleration.mult(0);

    // Bouncing edges
    if (this.position.x < 0) { this.position.x = 0; this.velocity.x *= -1; }
    if (this.position.x > env.width) { this.position.x = env.width; this.velocity.x *= -1; }
    if (this.position.y < 0) { this.position.y = 0; this.velocity.y *= -1; }
    if (this.position.y > env.height) { this.position.y = env.height; this.velocity.y *= -1; }
    
    this.energy -= 1; // Consume energy fast

    // Kill collision
    for (let n of nanots) {
        if (n.dead) continue;
        let d = this.position.distance(n.position);
        if (d < this.size + n.size) {
            n.dead = true;
            this.energy += 300; 
            window.logEvent("Depredador devoró un NANOT.", "red");
        }
    }
  }

  draw(ctx) {
    let theta = Math.atan2(this.velocity.y, this.velocity.x) + Math.PI / 2;
    ctx.save();
    ctx.translate(this.position.x, this.position.y);
    ctx.rotate(theta);
    ctx.beginPath();
    ctx.moveTo(0, -this.size * 2);
    ctx.lineTo(-this.size, this.size);
    ctx.lineTo(this.size, this.size);
    ctx.closePath();
    ctx.fillStyle = "rgba(220, 38, 38, 0.9)";
    ctx.shadowBlur = 10;
    ctx.shadowColor = "red";
    ctx.fill();
    ctx.restore();
  }
}
