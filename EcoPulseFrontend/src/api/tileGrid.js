import {API_BASE_URL} from "./config.js";

export async function calculateTileGridInfo(id) {
    const response = await fetch(API_BASE_URL + '/tile-grid/city/' + id, {
        method: 'GET', headers: {
            'Content-Type': 'application/json', 'Accept': 'application/json'
        }
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Request failed: ${response.status} ${errorText}`);
    }

    return await response.json();
}