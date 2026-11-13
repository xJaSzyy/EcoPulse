import { ref } from 'vue'
import VectorSource from 'ol/source/Vector'
import VectorLayer from 'ol/layer/Vector'
import Draw from 'ol/interaction/Draw'
import { getArea } from 'ol/sphere'
import { Fill, Stroke, Style, Text } from 'ol/style'

export function useAreaMeasurement(map) {
  const isMeasuringArea = ref(false)
  const measuredArea = ref(null)

  const measureSource = new VectorSource()

  const measureLayer = new VectorLayer({
    source: measureSource,
    visible: true,
    zIndex: 100,
    style: (feature) => {
      const geometry = feature.getGeometry()

      const area = getArea(geometry, {
        projection: map.value.getView().getProjection()
      })

      return new Style({
        fill: new Fill({
          color: 'rgba(59, 130, 246, 0.2)'
        }),
        stroke: new Stroke({
          color: '#2563eb',
          width: 3
        }),
        text: new Text({
          text: formatArea(area),
          font: 'bold 15px Arial',
          fill: new Fill({
            color: '#ffffff'
          }),
          backgroundFill: new Fill({
            color: 'rgba(30, 64, 175, 0.9)'
          }),
          padding: [6, 10, 6, 10],
          textAlign: 'center',
          overflow: true
        })
      })
    }
  })

  let drawInteraction = null

  function startAreaMeasurement() {
    if (!map.value || isMeasuringArea.value) return

    removeAllDrawInteractions()

    isMeasuringArea.value = true

    drawInteraction = new Draw({
      source: measureSource,
      type: 'Polygon'
    })

    drawInteraction.on('drawend', (event) => {
      if (!isMeasuringArea.value) return

      const geometry = event.feature.getGeometry()

      measuredArea.value = getArea(geometry, {
        projection: map.value.getView().getProjection()
      })
    })

    map.value.addInteraction(drawInteraction)
  }

  function removeAllDrawInteractions() {
    if (!map.value) {
      drawInteraction = null
      return
    }

    const interactions = map.value.getInteractions().getArray()

    interactions
      .filter(interaction => interaction instanceof Draw)
      .forEach(interaction => {
        interaction.abortDrawing()
        map.value.removeInteraction(interaction)
      })

    drawInteraction = null
  }

  function stopAreaMeasurement() {
    isMeasuringArea.value = false

    removeAllDrawInteractions()

    if (map.value) {
      map.value.getTargetElement().style.cursor = ''
    }
  }

  function clearAreaMeasurement() {
    stopAreaMeasurement()

    measureSource.clear()
    measuredArea.value = null
  }

  function formatArea(area) {
    if (area >= 1000000) {
      return `${(area / 1000000).toFixed(3)} км²`
    }

    return `${area.toFixed(2)} м²`
  }

  return {
    isMeasuringArea,
    measuredArea,
    measureLayer,
    startAreaMeasurement,
    stopAreaMeasurement,
    clearAreaMeasurement
  }
}