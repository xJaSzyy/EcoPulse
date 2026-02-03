import {API_BASE_URL} from "./config.js";

export async function calculateSingleDangerZone(payload) {
    const response = await fetch(API_BASE_URL + '/danger-zone/single', {
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

export async function calculateSingleDangerZones(payload) {
    const response = await fetch(API_BASE_URL + '/danger-zones/single', {
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

export async function calculateVehicleFlowDangerZones(payload) {
    const response = await fetch(API_BASE_URL + '/danger-zones/vehicle-flow', {
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

export async function calculateTrafficLightQueueDangerZones(payload) {
    const response = await fetch(API_BASE_URL + '/danger-zones/traffic-light-queue', {
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
