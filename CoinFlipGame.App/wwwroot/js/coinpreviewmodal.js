// Coin Preview Modal - Document-level pointer event handling
window.coinPreviewModal = {
    dotNetRef: null,
    
    setupEvents: function(dotNetReference) {
        this.dotNetRef = dotNetReference;
        
        // Add document-level event listeners
        document.addEventListener('pointermove', this.handlePointerMove.bind(this));
        document.addEventListener('pointerup', this.handlePointerUp.bind(this));
        document.addEventListener('pointercancel', this.handlePointerUp.bind(this));
    },
    
    handlePointerMove: function(e) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('HandlePointerMove', e.clientX, e.clientY);
        }
    },
    
    handlePointerUp: function(e) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('HandlePointerUp');
        }
    },
    
    cleanup: function() {
        document.removeEventListener('pointermove', this.handlePointerMove);
        document.removeEventListener('pointerup', this.handlePointerUp);
        document.removeEventListener('pointercancel', this.handlePointerUp);
        this.dotNetRef = null;
    }
};
