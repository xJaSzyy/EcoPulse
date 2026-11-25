import { ref } from 'vue'
import { getAllCities } from '../api/city.js';

export function useMapCities() {
  const cities = ref([])
  const selectedCities = ref([])
  const userPosition = ref(null);

  async function loadMapCities() {
    try {
      cities.value = await getAllCities()

      const savedCities = JSON.parse(
        localStorage.getItem('selectedCities') || 'null'
      )

      const validSavedCities = Array.isArray(savedCities)
        ? savedCities
          .map(saved => cities.value.find(city => city.id === saved.id))
          .filter(Boolean)
        : []

      selectedCities.value = validSavedCities.length
        ? validSavedCities
        : (cities.value.length ? [cities.value[0]] : [])

    } catch (error) {
      console.error('Ошибка загрузки городов:', error)
    }
  }

  const getUserPosition = () => {
    return new Promise((resolve) => {
      if (!navigator.geolocation) {
        resolve(null);
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          userPosition.value =
          {
            "type": "Point",
            "coordinates": [position.coords.longitude, position.coords.latitude]
          };
          resolve(userPosition.value);
        },
        (error) => {
          resolve(null);
        },
        { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
      );
    });
  };

  return {
    cities,
    selectedCities,
    loadMapCities,
    userPosition,
    getUserPosition
  }
}
