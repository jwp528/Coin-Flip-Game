// Coin file system helpers
window.listCoinFiles = async function(basePath) {
    // Since we can't actually list files in a browser environment,
    // we'll maintain a static registry of available coins
    // This should be updated when new coin folders are added
    
    const coinRegistry = {
        "/img/coins": ["logo.png"],
        "/img/coins/AI/Zodiak": ["Gemini.png", "Ram.png", "Tauros.png"],
        "/img/coins/AI/Cartoon": [] // Placeholder for future
    };
    
    return coinRegistry[basePath] || [];
};

// Helper to get window dimensions
window.getWindowDimensions = function() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};
