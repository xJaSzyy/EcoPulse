import { reactive } from 'vue'
import Feature from 'ol/Feature'
import { LineString, Point } from 'ol/geom'
import { fromLonLat } from 'ol/proj'

import {
  calculateSingleDangerZones,
  calculateVehicleFlowDangerZones,
  calculateTrafficLightQueueDangerZones
} from '../api/dangerZone.js'

export function useDangerLayers({
  olLayers,
  selectedCities,
  weather,
  createEllipse,
  updateModifyFlow
}) {
  const latestDangerZones = reactive({
    single: [],
    vehicleFlow: [],
    vehicleQueue: []
  })

  async function updateSingleLayer() {
    const zones = await calculateSingleDangerZones({
      pollutant: 2,
      airTemp: weather.value.temperature,
      windSpeed: weather.value.windSpeed,
      windDirection: weather.value.windDirection,
      cityIds: selectedCities.value.map(c => c.id)
    })

    latestDangerZones.single = zones

    const layer = olLayers.single
    const source = layer?.getSource()

    if (!source) return zones

    source.clear()

    for (const dangerZone of zones) {
      const ellipse = await createEllipse(dangerZone)
      ellipse.set('emissionSourceId', dangerZone.emissionSourceId)
      source.addFeature(ellipse)

      const pointFeature = new Feature({
        geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
        type: 'boiler'
      })

      pointFeature.set('dangerColor', dangerZone.color)
      pointFeature.set('emissionSourceId', dangerZone.emissionSourceId)

      source.addFeature(pointFeature)
    }

    layer.changed()
    await updateModifyFlow()

    return zones
  }

  async function updateVehicleFlowLayer() {
    const zones = await calculateVehicleFlowDangerZones({
      cityIds: selectedCities.value.map(c => c.id)
    })

    latestDangerZones.vehicleFlow = zones

    const layer = olLayers.vehicleFlow
    const source = layer?.getSource()

    if (!source) return zones

    source.clear()

    for (const zone of zones) {
      const coords = zone.points.coordinates.map(
        ([lon, lat]) => fromLonLat([lon, lat])
      )

      const feature = new Feature({
        geometry: new LineString(coords),
        type: 'flow'
      })

      feature.set('dangerColor', zone.color)
      feature.set('emissionSourceId', zone.emissionSourceId)

      source.addFeature(feature)
    }

    layer.changed()
    await updateModifyFlow()

    return zones
  }

  async function updateVehicleQueueLayer() {
    const zones = await calculateTrafficLightQueueDangerZones({
      cityIds: selectedCities.value.map(c => c.id)
    })

    latestDangerZones.vehicleQueue = zones

    const layer = olLayers.vehicleQueue
    const source = layer?.getSource()

    if (!source) return zones

    source.clear()

    for (const zone of zones) {
      const feature = new Feature({
        geometry: new Point(fromLonLat(zone.location.coordinates)),
        type: 'queue'
      })

      feature.set('dangerColor', zone.color)
      feature.set('emissionSourceId', zone.emissionSourceId)

      source.addFeature(feature)
    }

    layer.changed()

    return zones
  }

  async function updateLayers() {
    const singleDangerZones = await updateSingleLayer()
    const vehicleFlowDangerZones = await updateVehicleFlowLayer()
    const vehicleQueueDangerZones = await updateVehicleQueueLayer()

    return {
      singleDangerZones,
      vehicleFlowDangerZones,
      vehicleQueueDangerZones
    }
  }

  function setLatestDangerZones({
    single,
    vehicleFlow,
    vehicleQueue
  }) {
    if (single !== undefined) latestDangerZones.single = single
    if (vehicleFlow !== undefined) latestDangerZones.vehicleFlow = vehicleFlow
    if (vehicleQueue !== undefined) latestDangerZones.vehicleQueue = vehicleQueue
  }

  return {
    updateSingleLayer,
    updateVehicleFlowLayer,
    updateVehicleQueueLayer,
    updateLayers,
    latestDangerZones,
    setLatestDangerZones
  }
}