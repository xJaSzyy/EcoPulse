import {API_BASE_URL} from "./config.js";

export async function calculateTileGrid(payload) {
    const response = await fetch(API_BASE_URL + '/tile-grid/calculate', {
        method: 'POST', headers: {
            'Content-Type': 'application/json', 'Accept': 'application/json'
        }, body: JSON.stringify(payload)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Request failed: ${response.status} ${errorText}`);
    }

    return await response.json();
}