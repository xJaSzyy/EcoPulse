import {API_BASE_URL} from "./config.js";

export async function calculateTileGridInfo(id, tileSize = 1000) {
    const endpoint = `${API_BASE_URL}/tile-grid/city/${id}`;
    const url = `${endpoint}?tileSize=${tileSize}`;
    
    const response = await fetch(url, {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    return await response.json();
}

export async function calculateTileGridDangerOverlay(id, dangerZones, tileSize = 1000) {
    const endpoint = `${API_BASE_URL}/tile-grid/city/${id}/danger-overlay`;
    const url = `${endpoint}?tileSize=${tileSize}`;
    
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(dangerZones)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    return await response.json();
}