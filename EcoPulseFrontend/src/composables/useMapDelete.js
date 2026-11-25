import { ref, toRaw } from 'vue'
import {
  deleteSingleEmissionSource,
  deleteVehicleFlowEmissionSource,
  deleteTrafficLightQueueEmissionSource
} from '../api/emissionSource.js'

export function useMapDelete({
  map,
  olLayers,
  selectedCities,
  updateLayers,
  preloadTileGrids,
  refreshTileGrid,
  cancelCreateModes
}) {
  const isDeleteMode = ref(false)
  const selectInteraction = ref(null)
  const selectedFeature = ref(null)
  const deleting = ref(false)

  let deleteClickHandler = null

  function toggleDeleteMode() {
    if (isDeleteMode.value) {
      stopDeleteMode()
    } else {
      startDeleteMode()
    }
  }

  function startDeleteMode() {
    if (!map.value) {
      console.error('Карта ещё не инициализирована')
      return
    }

    const {
      cancelCreateModeSingle,
      cancelCreateModeFlow,
      createModeQueue
    } = cancelCreateModes()
    cancelCreateModeSingle()
    cancelCreateModeFlow()
    createModeQueue.value = false

    stopDeleteMode()

    isDeleteMode.value = true

    deleteClickHandler = async (event) => {
      let selectedFeature = null
      let selectedLayer = null

      map.value.forEachFeatureAtPixel(
        event.pixel,
        (feature, layer) => {
          if (
            toRaw(layer) === toRaw(olLayers.single) ||
            toRaw(layer) === toRaw(olLayers.vehicleFlow) ||
            toRaw(layer) === toRaw(olLayers.vehicleQueue)
          ) {
            selectedFeature = feature
            selectedLayer = layer
            return true
          }

          return false
        },
        {
          hitTolerance: 10
        }
      )

      if (!selectedFeature) {
        return
      }

      await deleteSelectedFeature(selectedFeature, selectedLayer)
    }

    map.value.on('singleclick', deleteClickHandler)
  }

  function stopDeleteMode() {
    if (map.value && deleteClickHandler) {
      map.value.un('singleclick', deleteClickHandler)
      deleteClickHandler = null
    }

    selectInteraction.value = null
    selectedFeature.value = null
    isDeleteMode.value = false
  }

  async function deleteSelectedFeature(feature, layer) {
    if (deleting.value) return

    if (!layer) {
      console.error('Слой объекта не определён')
      return
    }

    let sourceType = null

    if (layer === olLayers.single) {
      sourceType = 'single'
    } else if (layer === olLayers.vehicleFlow) {
      sourceType = 'vehicleFlow'
    } else if (layer === olLayers.vehicleQueue) {
      sourceType = 'vehicleQueue'
    }

    if (!sourceType) {
      console.error('Неизвестный тип слоя:', layer)
      return
    }

    const sourceId =
      feature.get('emissionSourceId') ??
      feature.get('dangerData')?.emissionSourceId ??
      feature.getId()

    console.log('Удаление объекта:', {
      sourceType,
      sourceId,
      properties: feature.getProperties()
    })

    if (sourceId == null) {
      console.error('Не найден ID источника!', feature.getProperties())
      return
    }

    const objectName = {
      single: 'котельную',
      vehicleFlow: 'дорогу',
      vehicleQueue: 'перекрёсток'
    }[sourceType]

    if (!confirm(`Вы действительно хотите удалить ${objectName}?`)) {
      return
    }

    try {
      deleting.value = true

      switch (sourceType) {
        case 'single':
          await deleteSingleEmissionSource(sourceId)
          break

        case 'vehicleFlow':
          await deleteVehicleFlowEmissionSource(sourceId)

          const cityIds = selectedCities.value.map(c => c.id);
          const cacheKey = `vehicleFlow_${cityIds.join('_')}`;

          localStorage.removeItem(cacheKey);

          break

        case 'vehicleQueue':
          await deleteTrafficLightQueueEmissionSource(sourceId)
          break
      }

      const {
        singleDangerZones,
        vehicleFlowDangerZones,
        vehicleQueueDangerZones
      } = await updateLayers()

      stopDeleteMode()

      await preloadTileGrids(
        singleDangerZones,
        vehicleFlowDangerZones,
        vehicleQueueDangerZones,
        true
      )

      await refreshTileGrid()

    } catch (error) {
      console.error('Ошибка удаления:', error)
    } finally {
      deleting.value = false
    }
  }

  return {
    isDeleteMode,
    toggleDeleteMode,
    startDeleteMode,
    stopDeleteMode,
    deleteSelectedFeature
  }
}
