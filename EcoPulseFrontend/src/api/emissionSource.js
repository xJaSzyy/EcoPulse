import {API_BASE_URL} from "./config.js";

export async function getSingleEmissionSourceById(id) {
    const response = await fetch(API_BASE_URL + '/emission-source/single/' + id, {
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

export async function addVehicleFlowEmissionSource(payload) {
    const response = await fetch(API_BASE_URL + '/emission-source/vehicle-flow', {
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

export async function addTrafficLightQueueEmissionSource(payload) {
    const response = await fetch(API_BASE_URL + '/emission-source/traffic-light-queue', {
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

export async function updateVehicleFlowEmissionSource(payload) {
    const response = await fetch(API_BASE_URL + '/emission-source/vehicle-flow', {
        method: 'PUT', headers: {
            'Content-Type': 'application/json', 'Accept': 'application/json'
        }, body: JSON.stringify(payload)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Request failed: ${response.status} ${errorText}`);
    }

    return await response.json();
}
