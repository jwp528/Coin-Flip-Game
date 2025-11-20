// Premium Particle Effects System
class ParticleSystem {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) {
            this.canvas = document.createElement('canvas');
            this.canvas.id = canvasId;
            this.canvas.style.position = 'fixed';
            this.canvas.style.top = '0';
            this.canvas.style.left = '0';
            this.canvas.style.width = '100%';
            this.canvas.style.height = '100%';
            this.canvas.style.pointerEvents = 'none';
            this.canvas.style.zIndex = '100';
            document.body.appendChild(this.canvas);
        }
        
        this.ctx = this.canvas.getContext('2d');
        this.particles = [];
        this.animationFrameId = null;  // Track animation frame for cleanup
        this.isAnimating = false;      // Track animation state
        this.resize();
        
        window.addEventListener('resize', () => this.resize());
        // Don't start animation loop yet - wait for particles
    }
    
    resize() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }
    
    createParticle(x, y, options = {}) {
        const particle = {
            x: x,
            y: y,
            vx: (Math.random() - 0.5) * (options.velocityX || 4),
            vy: (Math.random() - 0.5) * (options.velocityY || 4) - 2,
            life: 1,
            decay: options.decay || 0.015,
            size: options.size || (Math.random() * 3 + 2),
            color: options.color || `hsl(${Math.random() * 60 + 30}, 100%, 50%)`,
            gravity: options.gravity !== undefined ? options.gravity : 0.1,
            bounce: options.bounce || 0.6,
            rotation: Math.random() * Math.PI * 2,
            rotationSpeed: (Math.random() - 0.5) * 0.2,
            shape: options.shape || 'circle'
        };
        this.particles.push(particle);
    }
    
    burst(x, y, count = 30, options = {}) {
        for (let i = 0; i < count; i++) {
            this.createParticle(x, y, options);
        }
        
        // Start animation only when particles are added
        if (!this.isAnimating) {
            this.isAnimating = true;
            this.animate();
        }
    }
    
    confetti(x, y, count = 50) {
        const colors = ['#ff0000', '#00ff00', '#0000ff', '#ffff00', '#ff00ff', '#00ffff', '#ffa500'];
        for (let i = 0; i < count; i++) {
            this.createParticle(x, y, {
                velocityX: 8,
                velocityY: 8,
                decay: 0.01,
                size: Math.random() * 8 + 4,
                color: colors[Math.floor(Math.random() * colors.length)],
                gravity: 0.15,
                bounce: 0.7,
                shape: Math.random() > 0.5 ? 'square' : 'circle'
            });
        }
        
        // Start animation when particles are added
        if (!this.isAnimating) {
            this.isAnimating = true;
            this.animate();
        }
    }
    
    sparkle(x, y, count = 20) {
        for (let i = 0; i < count; i++) {
            this.createParticle(x, y, {
                velocityX: 3,
                velocityY: 3,
                decay: 0.02,
                size: Math.random() * 4 + 1,
                color: `hsl(45, 100%, ${Math.random() * 30 + 70}%)`,
                gravity: 0.05
            });
        }
        
        // Start animation when particles are added
        if (!this.isAnimating) {
            this.isAnimating = true;
            this.animate();
        }
    }
    
    animate() {
        // Stop animation if no particles
        if (this.particles.length === 0) {
            this.isAnimating = false;
            return; // Don't schedule next frame
        }
        
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        for (let i = this.particles.length - 1; i >= 0; i--) {
            const p = this.particles[i];
            
            // Update position
            p.x += p.vx;
            p.y += p.vy;
            p.vy += p.gravity;
            p.rotation += p.rotationSpeed;
            
            // Bounce off bottom
            if (p.y > this.canvas.height - p.size) {
                p.y = this.canvas.height - p.size;
                p.vy *= -p.bounce;
                p.vx *= 0.9;
            }
            
            // Bounce off sides
            if (p.x < p.size || p.x > this.canvas.width - p.size) {
                p.vx *= -1;
            }
            
            // Update life
            p.life -= p.decay;
            
            // Remove dead particles
            if (p.life <= 0) {
                this.particles.splice(i, 1);
                continue;
            }
            
            // Draw particle
            this.ctx.save();
            this.ctx.globalAlpha = p.life;
            this.ctx.translate(p.x, p.y);
            this.ctx.rotate(p.rotation);
            
            if (p.shape === 'square') {
                this.ctx.fillStyle = p.color;
                this.ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size);
            } else {
                this.ctx.beginPath();
                this.ctx.arc(0, 0, p.size, 0, Math.PI * 2);
                this.ctx.fillStyle = p.color;
                this.ctx.fill();
            }
            
            this.ctx.restore();
        }
        
        // Only continue animation if particles exist
        if (this.particles.length > 0) {
            this.animationFrameId = requestAnimationFrame(() => this.animate());
        } else {
            this.isAnimating = false;
        }
    }
    
    clear() {
        this.particles = [];
    }
}

// Initialize global particle system
window.particleSystem = null;

window.initParticleSystem = function() {
    if (!window.particleSystem) {
        window.particleSystem = new ParticleSystem('particle-canvas');
    }
    return window.particleSystem;
};

window.triggerParticleBurst = function(x, y, count, options) {
    const ps = window.initParticleSystem();
    // Handle null from C# - convert to empty object
    ps.burst(x, y, count, options || {});
};

window.triggerConfetti = function(x, y, count) {
    const ps = window.initParticleSystem();
    ps.confetti(x, y, count);
};

window.triggerSparkle = function(x, y, count) {
    const ps = window.initParticleSystem();
    ps.sparkle(x, y, count);
};

// Clear all particles (e.g., when drawer closes)
window.clearParticles = function() {
    const ps = window.particleSystem;
    if (ps) {
        ps.particles = [];
        ps.isAnimating = false;
        if (ps.animationFrameId) {
            cancelAnimationFrame(ps.animationFrameId);
            ps.animationFrameId = null;
        }
    }
};
