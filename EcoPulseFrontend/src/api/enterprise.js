import {API_BASE_URL} from "./config.js";

export async function getAllEnterpriseSanitaryAreas(payload) {
    const response = await fetch(API_BASE_URL + '/enterprise/sanitary-area', {
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

export default {
  getAll: async () => {
    const res = await fetch(API_BASE_URL + '/enterprise')
    return res.json()
  },
  create: async (payload) => {
    const response = await fetch(API_BASE_URL + '/enterprise', {
        method: 'POST',
        headers: {
        'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload)  
    });
    return response.json();
  },
  delete: async (id) => {
    await fetch(API_BASE_URL + '/enterprise/' + id, { method: 'DELETE' })
  }
}