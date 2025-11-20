// Coin filesystem scanner for BuildABoi
window.scanCoinDirectory = async function() {
    try {
        // In WebAssembly, we can't directly access the filesystem
        // Instead, we'll make HTTP requests to discover coins
        const basePath = '/img/coins';
        const foundPaths = [];
        
        // Known subdirectories to scan
        const subdirs = [
            '',  // Root coins directory
            'AI/Zodiak',
            'AI/Achievement',
            'AI/Leather',
            'Powers',
            'Combo'
        ];
        
        // For each subdirectory, try to fetch an index or scan known patterns
        for (const subdir of subdirs) {
            const dirPath = subdir ? `${basePath}/${subdir}` : basePath;
            
            // Try to fetch a directory listing JSON if it exists
            // This would need to be generated during build
            try {
                const response = await fetch(`${dirPath}/_files.json`);
                if (response.ok) {
                    const files = await response.json();
                    files.forEach(file => {
                        if (file.endsWith('.png') || file.endsWith('.jpg') || file.endsWith('.jpeg')) {
                            foundPaths.push(subdir ? `${basePath}/${subdir}/${file}` : `${basePath}/${file}`);
                        }
                    });
                    continue;
                }
            } catch (e) {
                // No index file, continue to pattern matching
            }
            
            // Fallback: try common coin names (this is a workaround for WASM)
            // In a real implementation, you'd pre-generate a manifest during build
            const commonNames = [
                'logo.png', 'Random.png',
                // Zodiak
                'Gemini.png', 'Rat.png', 'Ram.png', 'Dog.png', 'Tauros.png', 
                'Rabbit.png', 'Rooster.png', 'Pig.png', 'Dragon.png', 'Dragon_Rare.png',
                // Achievement
                'Completionist.png',
                // Leather
                'Black.png', 'Brown.png',
                // Powers
                'Weighted_Heads.png', 'Weighted_Tails.png', 'Shaved_Heads.png', 'Shaved_Tails.png',
                'AutoClicker_Heads.png', 'AutoClicker_Tails.png',
                // Combo
                'DualSided_Green.png', 'DualSided_Purple.png', 'DualSided_Red.png'
            ];
            
            for (const name of commonNames) {
                const testPath = subdir ? `${basePath}/${subdir}/${name}` : `${basePath}/${name}`;
                try {
                    const response = await fetch(testPath, { method: 'HEAD' });
                    if (response.ok && !foundPaths.includes(testPath)) {
                        foundPaths.push(testPath);
                    }
                } catch (e) {
                    // File doesn't exist, skip
                }
            }
        }
        
        return foundPaths;
    } catch (error) {
        console.error('Error scanning coin directory:', error);
        return [];
    }
};
