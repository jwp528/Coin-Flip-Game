// Premium Audio System for Coin Flip Game
class AudioSystem {
    constructor() {
        this.audioContext = null;
        this.sounds = {};
        this.isMuted = false;
        this.volume = 0.7;
        
        // Initialize Web Audio API (user gesture required)
        this.initAudioContext();
    }
    
    initAudioContext() {
        try {
            window.AudioContext = window.AudioContext || window.webkitAudioContext;
            this.audioContext = new AudioContext();
            this.masterGain = this.audioContext.createGain();
            this.masterGain.connect(this.audioContext.destination);
            this.masterGain.gain.value = this.volume;
        } catch (e) {
            console.warn('Web Audio API not supported:', e);
        }
    }
    
    // Resume audio context on user interaction (required by browsers)
    async resumeContext() {
        if (this.audioContext && this.audioContext.state === 'suspended') {
            await this.audioContext.resume();
        }
    }
    
    // Load and play coin flip sound from audio file
    async playFlip() {
        if (this.isMuted || !this.audioContext) return;
        
        await this.resumeContext();
        
        try {
            // Load the coin-flip.mp3 file if not already loaded
            if (!this.sounds['coinFlip']) {
                const response = await fetch('/sounds/coin-flip.mp3');
                const arrayBuffer = await response.arrayBuffer();
                this.sounds['coinFlip'] = await this.audioContext.decodeAudioData(arrayBuffer);
            }
            
            // Create and play the sound
            const source = this.audioContext.createBufferSource();
            source.buffer = this.sounds['coinFlip'];
            
            const gainNode = this.audioContext.createGain();
            gainNode.gain.value = 0.7; // Adjust volume as needed
            
            source.connect(gainNode);
            gainNode.connect(this.masterGain);
            
            source.start(0);
        } catch (error) {
            console.warn('Failed to play coin flip sound:', error);
            // Fallback to synthesized sound if file fails to load
            this.playFlipSynthesized();
        }
    }
    
    // Load and play coin unlock sound from audio file
    async playCoinUnlock() {
        if (this.isMuted || !this.audioContext) return;
        
        await this.resumeContext();
        
        try {
            // Load the coin-unlocked.mp3 file if not already loaded
            if (!this.sounds['coinUnlock']) {
                const response = await fetch('/sounds/coin-unlocked.mp3');
                const arrayBuffer = await response.arrayBuffer();
                this.sounds['coinUnlock'] = await this.audioContext.decodeAudioData(arrayBuffer);
            }
            
            // Create and play the sound
            const source = this.audioContext.createBufferSource();
            source.buffer = this.sounds['coinUnlock'];
            
            const gainNode = this.audioContext.createGain();
            gainNode.gain.value = 0.8; // Slightly louder for celebration
            
            source.connect(gainNode);
            gainNode.connect(this.masterGain);
            
            source.start(0);
        } catch (error) {
            console.warn('Failed to play coin unlock sound:', error);
            // Fallback to sparkle sound if file fails to load
            this.playSparkle();
        }
    }
    
    // Fallback: Synthesize coin flip sound (whoosh with metallic ringing)
    playFlipSynthesized() {
        if (this.isMuted || !this.audioContext) return;
        
        const now = this.audioContext.currentTime;
        
        // Whoosh sound (filtered noise)
        const noiseBuffer = this.createNoiseBuffer(0.3);
        const noiseSource = this.audioContext.createBufferSource();
        noiseSource.buffer = noiseBuffer;
        
        const noiseFilter = this.audioContext.createBiquadFilter();
        noiseFilter.type = 'bandpass';
        noiseFilter.frequency.value = 800;
        noiseFilter.Q.value = 1;
        
        const noiseGain = this.audioContext.createGain();
        noiseGain.gain.setValueAtTime(0, now);
        noiseGain.gain.linearRampToValueAtTime(0.15, now + 0.05);
        noiseGain.gain.exponentialRampToValueAtTime(0.01, now + 0.3);
        
        noiseSource.connect(noiseFilter);
        noiseFilter.connect(noiseGain);
        noiseGain.connect(this.masterGain);
        
        noiseSource.start(now);
        noiseSource.stop(now + 0.3);
        
        // Metallic ring (oscillators)
        this.playMetallicRing(now, [800, 1200, 1600], 0.08, 0.4);
    }
    
    // Synthesize landing sound (thud with metallic resonance)
    playLand() {
        if (this.isMuted || !this.audioContext) return;
        
        this.resumeContext();
        
        const now = this.audioContext.currentTime;
        
        // Thud (low frequency impact)
        const thudOsc = this.audioContext.createOscillator();
        thudOsc.type = 'sine';
        thudOsc.frequency.setValueAtTime(120, now);
        thudOsc.frequency.exponentialRampToValueAtTime(40, now + 0.1);
        
        const thudGain = this.audioContext.createGain();
        thudGain.gain.setValueAtTime(0.3, now);
        thudGain.gain.exponentialRampToValueAtTime(0.01, now + 0.15);
        
        thudOsc.connect(thudGain);
        thudGain.connect(this.masterGain);
        
        thudOsc.start(now);
        thudOsc.stop(now + 0.15);
        
        // Metallic bounce
        this.playMetallicRing(now + 0.05, [1000, 1500, 2000], 0.06, 0.3);
        this.playMetallicRing(now + 0.15, [1200, 1800, 2400], 0.04, 0.2);
    }
    
    // Achievement fanfare
    playAchievement() {
        if (this.isMuted || !this.audioContext) return;
        
        this.resumeContext();
        
        const now = this.audioContext.currentTime;
        const notes = [523.25, 659.25, 783.99, 1046.50]; // C5, E5, G5, C6
        
        notes.forEach((freq, i) => {
            const osc = this.audioContext.createOscillator();
            osc.type = 'sine';
            osc.frequency.value = freq;
            
            const gain = this.audioContext.createGain();
            const startTime = now + (i * 0.1);
            gain.gain.setValueAtTime(0, startTime);
            gain.gain.linearRampToValueAtTime(0.15, startTime + 0.05);
            gain.gain.exponentialRampToValueAtTime(0.01, startTime + 0.4);
            
            osc.connect(gain);
            gain.connect(this.masterGain);
            
            osc.start(startTime);
            osc.stop(startTime + 0.4);
        });
        
        // Add sparkle
        setTimeout(() => this.playSparkle(), 200);
    }
    
    // Sparkle/shimmer sound
    playSparkle() {
        if (this.isMuted || !this.audioContext) return;
        
        this.resumeContext();
        
        const now = this.audioContext.currentTime;
        const frequencies = [2000, 2500, 3000, 3500, 4000];
        
        frequencies.forEach((freq, i) => {
            const osc = this.audioContext.createOscillator();
            osc.type = 'sine';
            osc.frequency.value = freq;
            
            const gain = this.audioContext.createGain();
            const startTime = now + (i * 0.02);
            gain.gain.setValueAtTime(0, startTime);
            gain.gain.linearRampToValueAtTime(0.08, startTime + 0.01);
            gain.gain.exponentialRampToValueAtTime(0.01, startTime + 0.15);
            
            osc.connect(gain);
            gain.connect(this.masterGain);
            
            osc.start(startTime);
            osc.stop(startTime + 0.15);
        });
    }
    
    // Hover sound (subtle)
    playHover() {
        if (this.isMuted || !this.audioContext) return;
        
        this.resumeContext();
        
        const now = this.audioContext.currentTime;
        const osc = this.audioContext.createOscillator();
        osc.type = 'sine';
        osc.frequency.value = 1200;
        
        const gain = this.audioContext.createGain();
        gain.gain.setValueAtTime(0, now);
        gain.gain.linearRampToValueAtTime(0.03, now + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.1);
        
        osc.connect(gain);
        gain.connect(this.masterGain);
        
        osc.start(now);
        osc.stop(now + 0.1);
    }
    
    // Helper: Create metallic ring
    playMetallicRing(startTime, frequencies, volume, duration) {
        frequencies.forEach(freq => {
            const osc = this.audioContext.createOscillator();
            osc.type = 'sine';
            osc.frequency.value = freq;
            
            const gain = this.audioContext.createGain();
            gain.gain.setValueAtTime(0, startTime);
            gain.gain.linearRampToValueAtTime(volume, startTime + 0.01);
            gain.gain.exponentialRampToValueAtTime(0.01, startTime + duration);
            
            osc.connect(gain);
            gain.connect(this.masterGain);
            
            osc.start(startTime);
            osc.stop(startTime + duration);
        });
    }
    
    // Helper: Create noise buffer
    createNoiseBuffer(duration) {
        const sampleRate = this.audioContext.sampleRate;
        const bufferSize = sampleRate * duration;
        const buffer = this.audioContext.createBuffer(1, bufferSize, sampleRate);
        const data = buffer.getChannelData(0);
        
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }
        
        return buffer;
    }
    
    // Volume control
    setVolume(value) {
        this.volume = Math.max(0, Math.min(1, value));
        if (this.masterGain) {
            this.masterGain.gain.value = this.volume;
        }
    }
    
    // Mute toggle
    toggleMute() {
        this.isMuted = !this.isMuted;
        return this.isMuted;
    }
    
    // Set mute state directly
    setMuted(muted) {
        this.isMuted = muted;
    }
}

// Initialize global audio system
window.audioSystem = null;

window.initAudioSystem = function() {
    if (!window.audioSystem) {
        window.audioSystem = new AudioSystem();
    }
    return window.audioSystem;
};

// Global sound functions
window.playFlipSound = function() {
    const audio = window.initAudioSystem();
    audio.playFlip();
};

window.playLandSound = function() {
    const audio = window.initAudioSystem();
    audio.playLand();
};

window.playAchievementSound = function() {
    const audio = window.initAudioSystem();
    audio.playAchievement();
};

window.playSparkleSound = function() {
    const audio = window.initAudioSystem();
    audio.playSparkle();
};

window.playHoverSound = function() {
    const audio = window.initAudioSystem();
    audio.playHover();
};

window.playCoinUnlockSound = function() {
    const audio = window.initAudioSystem();
    audio.playCoinUnlock();
};

window.setAudioVolume = function(volume) {
    const audio = window.initAudioSystem();
    audio.setVolume(volume);
};

window.toggleAudioMute = function() {
    const audio = window.initAudioSystem();
    return audio.toggleMute();
};

// Set sound enabled/disabled
window.setSoundEnabled = function(enabled) {
    const audio = window.initAudioSystem();
    audio.setMuted(!enabled);
};
