// Helper to get window dimensions
window.getWindowDimensions = function() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};

// Preload coin face URLs so CSS backgrounds never paint half-decoded.
window.preloadCoinArt = function (urls) {
  const list = (Array.isArray(urls) ? urls : [urls])
    .filter(u => typeof u === 'string' && u.length > 0)
    .map(u => u.replace(/^url\(["']?/, '').replace(/["']?\)$/, ''));
  const unique = [...new Set(list)];
  return Promise.all(unique.map(src => new Promise(resolve => {
    const img = new Image();
    img.decoding = 'async';
    img.onload = img.onerror = () => resolve(src);
    img.src = src;
  })));
};