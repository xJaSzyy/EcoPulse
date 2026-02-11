import {API_BASE_URL} from "./config.js";

export async function getCityById(id) {
    const response = await fetch(API_BASE_URL + '/city/' + id, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Request failed: ${response.status} ${errorText}`);
    }

    return await response.json();
}

export default {
  getAll: async () => {
    const res = await fetch(API_BASE_URL + '/city')
    return res.json()
  },
  create: async (payload) => {
    const response = await fetch(API_BASE_URL + '/city', {
        method: 'POST',
        headers: {
        'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload)  
    });
    return response.json();
  },
  delete: async (id) => {
    await fetch(API_BASE_URL + '/city/' + id, { method: 'DELETE' })
  }
}