import { ref } from 'vue'
import VectorSource from 'ol/source/Vector'
import Draw from 'ol/interaction/Draw'
import { toLonLat } from 'ol/proj'

export function useMapCreateModes({
  map,
  selectedCities,
  createModeSingle,
  setSingleSourceLocation,
  setVehicleFlowData,
  setTrafficLightQueueLocation,
  stopDeleteMode
}) {
  const createModeFlow = ref(false)
  const createModeQueue = ref(false)
  const createPoints = ref([])

  function removeAllDrawInteractions() {
    if (!map.value) return

    const interactions = map.value.getInteractions().getArray()

    interactions
      .filter(interaction => interaction instanceof Draw)
      .forEach(interaction => {
        interaction.abortDrawing()
        map.value.removeInteraction(interaction)
      })
  }

  function startCreateModeSingle() {
    if (!map.value) return

    cancelCreateModeFlow()
    cancelCreateModeSingle()

    createModeQueue.value = false
    createModeSingle.value = true

    const drawSource = new VectorSource()

    const drawInteraction = new Draw({
      source: drawSource,
      type: 'Point'
    })

    map.value.addInteraction(drawInteraction)

    drawInteraction.once('drawend', (event) => {
      const coordinates = toLonLat(
        event.feature.getGeometry().getCoordinates()
      )

      const cityId = selectedCities.value.length > 0
        ? selectedCities.value[0].id
        : null

      createModeSingle.value = false

      removeAllDrawInteractions()

      setSingleSourceLocation(coordinates, cityId)
    })
  }

  function cancelCreateModeSingle() {
    removeAllDrawInteractions()
    createModeSingle.value = false
  }

  function startCreateModeFlow() {
    if (!map.value) return

    cancelCreateModeSingle()
    cancelCreateModeFlow()
    stopDeleteMode()

    createModeQueue.value = false
    createModeFlow.value = true

    const drawSource = new VectorSource()

    const drawInteraction = new Draw({
      source: drawSource,
      type: 'LineString'
    })

    map.value.addInteraction(drawInteraction)

    drawInteraction.once('drawend', (event) => {
      const coordinates = event.feature
        .getGeometry()
        .getCoordinates()
        .map(coord => toLonLat(coord))

      const selectedCityId = selectedCities.value.length > 0
        ? selectedCities.value[0].id
        : null

      createModeFlow.value = false

      removeAllDrawInteractions()

      setVehicleFlowData(coordinates, selectedCityId)
    })
  }

  function cancelCreateModeFlow() {
    removeAllDrawInteractions()

    createModeFlow.value = false
    createPoints.value = []
  }

  function cancelCreateModeQueue() {
    createModeQueue.value = false
    removeAllDrawInteractions()
  }

  function startCreateModeQueue() {
    if (!map.value) return

    cancelCreateModeQueue()

    createModeQueue.value = true

    const drawSource = new VectorSource()

    const drawInteraction = new Draw({
      source: drawSource,
      type: 'Point'
    })

    map.value.addInteraction(drawInteraction)

    drawInteraction.once('drawend', (event) => {
      const coordinates = toLonLat(
        event.feature.getGeometry().getCoordinates()
      )

      const cityId = selectedCities.value.length > 0
        ? selectedCities.value[0].id
        : null

      createModeQueue.value = false

      removeAllDrawInteractions()

      setTrafficLightQueueLocation(coordinates, cityId)
    })
  }

  return {
    createModeSingle,
    createModeFlow,
    createModeQueue,
    createPoints,
    startCreateModeSingle,
    cancelCreateModeSingle,
    startCreateModeFlow,
    cancelCreateModeFlow,
    startCreateModeQueue,
    cancelCreateModeQueue,
    removeAllDrawInteractions
  }
}
