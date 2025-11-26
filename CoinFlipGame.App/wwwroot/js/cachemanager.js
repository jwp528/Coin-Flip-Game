// Cache Version Manager
// Helps prevent cache corruption issues in Blazor WebAssembly apps

window.CacheVersionManager = {
    CURRENT_VERSION: '1.4.1',
    VERSION_KEY: 'app_cache_version',
    
    /**
     * Check if cache version matches current version
     * Returns true if cache is valid, false if needs clearing
     */
    validateCache: function() {
        try {
            const cachedVersion = localStorage.getItem(this.VERSION_KEY);
            
            if (!cachedVersion) {
                console.log('[CacheVersion] No cached version found');
                return false;
            }
            
            if (cachedVersion !== this.CURRENT_VERSION) {
                console.log(`[CacheVersion] Version mismatch: cached=${cachedVersion}, current=${this.CURRENT_VERSION}`);
                return false;
            }
            
            console.log(`[CacheVersion] Cache version validated: ${this.CURRENT_VERSION}`);
            return true;
        } catch (error) {
            console.error('[CacheVersion] Error validating cache:', error);
            return false;
        }
    },
    
    /**
     * Update cache version in localStorage
     */
    updateVersion: function() {
        try {
            localStorage.setItem(this.VERSION_KEY, this.CURRENT_VERSION);
            console.log(`[CacheVersion] Updated to version: ${this.CURRENT_VERSION}`);
        } catch (error) {
            console.error('[CacheVersion] Error updating version:', error);
        }
    },
    
    /**
     * Clear all caches if version doesn't match
     */
    clearIfOutdated: async function() {
        if (!this.validateCache()) {
            console.log('[CacheVersion] Clearing outdated cache...');
            
            try {
                // Clear browser caches
                if ('caches' in window) {
                    const cacheNames = await caches.keys();
                    await Promise.all(cacheNames.map(name => caches.delete(name)));
                    console.log('[CacheVersion] Cleared all caches');
                }
                
                // Update version marker
                this.updateVersion();
                
                return true;
            } catch (error) {
                console.error('[CacheVersion] Error clearing caches:', error);
                return false;
            }
        }
        
        return false;
    },
    
    /**
     * Initialize cache version management
     * Call this before Blazor starts
     */
    init: async function() {
        console.log('[CacheVersion] Initializing...');
        const wasCleared = await this.clearIfOutdated();
        
        if (wasCleared) {
            console.log('[CacheVersion] Cache was cleared due to version change');
        } else {
            console.log('[CacheVersion] Cache is up to date');
        }
        
        // Ensure version is set
        this.updateVersion();
    }
};

// Auto-initialize on script load
(async function() {
    await window.CacheVersionManager.init();
})();
