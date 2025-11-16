// Advanced Coin Physics System
window.coinPhysics = {
    coinElement: null,
    isDragging: false,
    momentumX: 0,
    momentumY: 0,
    
    startDrag: function(centerX, centerY) {
        this.coinElement = document.querySelector('.coin');
        this.isDragging = true;
        this.momentumX = 0;
        this.momentumY = 0;
        
        if (this.coinElement) {
            this.coinElement.style.transition = 'none';
        }
    },
    
    updateDrag: function(rotationX, rotationY, offsetX, offsetY) {
        if (this.coinElement && this.isDragging) {
            requestAnimationFrame(() => {
                if (this.coinElement) {
                    const scale = 1 + Math.abs(offsetY) / 1000;
                    this.coinElement.style.transform = `
                        rotateX(${rotationX.toFixed(2)}deg) 
                        rotateY(${rotationY.toFixed(2)}deg)
                        scale(${Math.min(scale, 1.15)})
                    `;
                }
            });
        }
    },
    
    resetTransform: function() {
        this.isDragging = false;
        if (this.coinElement) {
            this.coinElement.style.transition = 'transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1)';
            this.coinElement.style.transform = 'rotateX(0deg) rotateY(0deg) scale(1)';
        }
    },
    
    clearTransform: function() {
        this.isDragging = false;
        if (this.coinElement) {
            this.coinElement.style.transform = '';
            this.coinElement.style.transition = '';
        }
    }
};

window.initCoinPhysics = function() {
    return true;
};

// Haptic Feedback (for mobile devices)
window.triggerHaptic = function(intensity) {
    if ('vibrate' in navigator) {
        switch(intensity) {
            case 'light':
                navigator.vibrate(10);
                break;
            case 'medium':
                navigator.vibrate(20);
                break;
            case 'heavy':
                navigator.vibrate([30, 10, 30]);
                break;
        }
    }
};

// Sound Effects Placeholders (ready for audio implementation)
window.playFlipSound = function() {
    // TODO: Add actual audio implementation
    console.log('?? Flip sound');
};

window.playLandSound = function() {
    // TODO: Add actual audio implementation
    console.log('?? Land sound');
};

window.playAchievementSound = function() {
    // TODO: Add actual audio implementation
    console.log('?? Achievement sound');
};

// Window inner dimensions helper
window.getWindowDimensions = function() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};
