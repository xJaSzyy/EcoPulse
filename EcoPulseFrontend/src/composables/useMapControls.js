import { ref } from 'vue'
import { fromLonLat } from 'ol/proj'
import { getCityById } from '../api/city.js';

export function useMapControls({
  map,
  selectedCities,
  updateLayers,
  preloadTileGrids,
  refreshTileGrid
}) {
  const cityDropdownOpen = ref(false)
  const editPanelOpen = ref(false)

  const toggleCityDropdown = async () => {
    if (!cityDropdownOpen.value) {
      cityDropdownOpen.value = true
      return
    }

    if (cityDropdownOpen.value) {
      const lastCities = JSON.parse(localStorage.getItem("selectedCities") || '[]')
      localStorage.setItem("selectedCities", JSON.stringify(selectedCities.value))

      if (selectedCities.value.length === 1) {
        try {
          const selectedCity = await getCityById(selectedCities.value[0].id)
          if (selectedCity?.location) {
            const view = map.value.getView()
            view.setCenter(fromLonLat(selectedCity.location.coordinates))
            view.setZoom(12)
          }
        } catch (error) {
          console.error('Ошибка загрузки города:', error)
        }
      }

      cityDropdownOpen.value = false
      await updateLayers()

      const {
        singleDangerZones,
        vehicleFlowDangerZones,
        vehicleQueueDangerZones
      } = await updateLayers()

      await preloadTileGrids(
        singleDangerZones,
        vehicleFlowDangerZones,
        vehicleQueueDangerZones,
        true
      )

      await refreshTileGrid()
    }
  }

  const toggleEditPanel = () => {
    editPanelOpen.value = !editPanelOpen.value
  }

  return {
    cityDropdownOpen,
    editPanelOpen,
    toggleCityDropdown,
    toggleEditPanel
  }
}
