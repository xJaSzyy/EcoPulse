import { ref } from 'vue'
import { calculateTileGrid } from '../api/grid.js'
import Feature from 'ol/Feature'
import { fromLonLat } from 'ol/proj'
import { Polygon, MultiPolygon } from 'ol/geom'

export function useTileGrid({
  map,
  selectedCities,
  olLayers,
  tileSize,
  autoTileSizeByZoom,
  withCache,
  geoJsonToOlGeometry
}) {
  const tileGridCache = ref({})
  const currentTileSize = ref(null)

  const TILE_SIZES = [3000, 1500, 1000, 750, 400, 200, 100]

  let latestSingleDangerZones = []
  let latestVehicleFlowDangerZones = []
  let latestVehicleQueueDangerZones = []

  async function preloadTileGrids(
    singleDangerZones,
    vehicleFlowDangerZones,
    vehicleQueueDangerZones,
    forceRecalculate = false
  ) {
    latestSingleDangerZones = singleDangerZones
    latestVehicleFlowDangerZones = vehicleFlowDangerZones
    latestVehicleQueueDangerZones = vehicleQueueDangerZones

    const cityIds = selectedCities.value
      .map(city => city.id)
      .sort((a, b) => a - b)

    const results = await Promise.all(
      TILE_SIZES.map(async size => {
        const cacheKey = `tileGrid_${cityIds.join('_')}_${size}`

        const calculate = () => calculateTileGrid({
          cityIds,
          tileSize: size,
          singleDangerZones,
          vehicleFlowDangerZones,
          trafficLightQueueDangerZones: vehicleQueueDangerZones
        })

        const result = forceRecalculate
          ? await calculate()
          : await withCache(cacheKey, calculate)

        return [size, result]
      })
    )

    tileGridCache.value = Object.fromEntries(results)
  }

  function getTileSizeForZoom(zoom) {
    if (zoom <= 10.25) return 3000
    if (zoom <= 10.75) return 1500
    if (zoom <= 11.5) return 1000
    if (zoom <= 12.5) return 750
    if (zoom <= 13) return 400
    if (zoom <= 14) return 200

    return 100
  }

  async function applyTileSize() {
    const size = Number(tileSize.value)

    if (!Number.isFinite(size)) return

    const normalizedSize = Math.min(
      3000,
      Math.max(100, Math.round(size / 25) * 25)
    )

    tileSize.value = normalizedSize
    currentTileSize.value = normalizedSize

    await updateTileGridLayer(normalizedSize)
  }

  async function onAutoTileSizeChange() {
    if (autoTileSizeByZoom.value) {
      await refreshTileGridByZoom()
    } else {
      await applyTileSize()
    }
  }

  async function refreshTileGridByZoom() {
    if (!map.value || !autoTileSizeByZoom.value) return

    const zoom = map.value.getView().getZoom()

    if (zoom == null) return

    const size = getTileSizeForZoom(zoom)

    if (size === currentTileSize.value) return

    currentTileSize.value = size

    await updateTileGridLayer(size)
  }

  async function refreshTileGrid() {
    const zoom = map.value?.getView().getZoom() ?? 12

    const size = autoTileSizeByZoom.value
      ? getTileSizeForZoom(zoom)
      : tileSize.value

    currentTileSize.value = size

    await updateTileGridLayer(size)
  }

  async function updateTileGridLayer(size) {
    let tileGridResult = tileGridCache.value[size]

    if (!tileGridResult) {
      console.log(`Сетка ${size} м отсутствует в кеше. Загружаем...`)

      const cityIds = selectedCities.value.map(city => city.id)

      tileGridResult = await calculateTileGrid({
        cityIds,
        tileSize: size,
        singleDangerZones: latestSingleDangerZones,
        vehicleFlowDangerZones: latestVehicleFlowDangerZones,
        trafficLightQueueDangerZones: latestVehicleQueueDangerZones
      })

      tileGridCache.value = {
        ...tileGridCache.value,
        [size]: tileGridResult
      }
    }

    const layer = olLayers.tile
    if (!layer) return

    const source = layer.getSource()
    if (!source) return

    source.clear()

    const allTiles = tileGridResult.flatMap(city => city.tiles)

    allTiles.forEach(tileInfo => {
      const geometry = geoJsonToOlGeometry(tileInfo.tile)
      if (!geometry) return

      const feature = new Feature({
        geometry,
        color: tileInfo.color,
        cityId: tileInfo.cityId
      })

      feature.set('dangerData', tileInfo)
      source.addFeature(feature)
    })

    layer.changed()
  }

  return {
    tileGridCache,
    currentTileSize,
    preloadTileGrids,
    getTileSizeForZoom,
    applyTileSize,
    onAutoTileSizeChange,
    refreshTileGridByZoom,
    refreshTileGrid,
    updateTileGridLayer
  }
}