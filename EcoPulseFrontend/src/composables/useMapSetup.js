import { onMounted } from 'vue'
import Map from 'ol/Map'
import View from 'ol/View'
import { defaults as defaultControls } from 'ol/control'
import { fromLonLat } from 'ol/proj'

import {
  calculateSingleDangerZones,
  calculateVehicleFlowDangerZones,
  calculateTrafficLightQueueDangerZones
} from '../api/dangerZone.js'
import { calculateTileArea } from '../api/grid.js'
import { getAllEnterpriseSanitaryAreas } from '../api/enterprise.js'
import { getCurrentWeather } from '../api/weather.js'
import { getCityById } from '../api/city.js'
import { getSingleEmissionSourceById } from '../api/emissionSource.js'

export function useMapSetup({
  map,
  mapRoot,
  baseLayers,
  selectedBaseMap,
  weather,
  selectedCities,
  olLayers,
  measureLayer,
  loadMapCities,
  createTileGridLayer,
  createSingleLayer,
  createVehicleFlowLayer,
  createVehicleQueueLayer,
  createSanitaryAreaLayer,
  withCache,
  setLatestDangerZones,
  preloadTileGrids,
  tileGridCache,
  refreshTileGridByZoom,
  initLayersState,
  showSimulationPanel,
  simulationStartData,
  closeSimulationPanel,
  openSimulationPanel,
  isDeleteMode,
  createModeFlow,
  createModeSingle,
  isMeasuringArea,
  changeBaseMap,
  showPopup,
  hidePopup
}) {
  function createLayers(
    singleDangerZones,
    vehicleFlowDangerZones,
    vehicleQueueDangerZones,
    sanitaryAreas,
    tileGridResult,
    areaGridResult
  ) {
    const singleLayer = createSingleLayer(singleDangerZones);
    const vehicleFlowLayer = createVehicleFlowLayer(vehicleFlowDangerZones);
    const vehicleQueueLayer = createVehicleQueueLayer(vehicleQueueDangerZones);
    const sanitaryAreaLayer = createSanitaryAreaLayer(sanitaryAreas);
    const tileGridLayer = createTileGridLayer(tileGridResult);
    const areaGridLayer = createTileGridLayer(areaGridResult);

    olLayers.single = singleLayer;
    olLayers.vehicleFlow = vehicleFlowLayer;
    olLayers.vehicleQueue = vehicleQueueLayer;
    olLayers.sanitaryArea = sanitaryAreaLayer;
    olLayers.tile = tileGridLayer;
    olLayers.area = areaGridLayer;

    return { singleLayer, vehicleFlowLayer, vehicleQueueLayer, sanitaryAreaLayer, tileGridLayer, areaGridLayer };
  }

  onMounted(async () => {
    await loadMapCities()

    const baseLayer = baseLayers.osm

    const currentWeather = await getCurrentWeather();
    weather.value = {
      temperature: currentWeather.temperature,
      iconClass: currentWeather.iconClass,
      windSpeed: currentWeather.windSpeed,
      windDirection: currentWeather.windDirection,
    }

    const singleDangerZones = await calculateSingleDangerZones({
      pollutant: 2, // solid particles
      airTemp: currentWeather.temperature,
      windSpeed: currentWeather.windSpeed,
      windDirection: currentWeather.windDirection,
      cityIds: selectedCities.value.map(c => c.id)
    });

    const cityIds = selectedCities.value.map(c => c.id);

    const vehicleFlowDangerZones = await withCache(
      `vehicleFlow_${cityIds.join('_')}`,
      () => calculateVehicleFlowDangerZones({ cityIds })
    );

    const vehicleQueueDangerZones = await withCache(
      `vehicleQueue_${cityIds.join('_')}`,
      () => calculateTrafficLightQueueDangerZones({ cityIds })
    );

    setLatestDangerZones({
      single: singleDangerZones,
      vehicleFlow: vehicleFlowDangerZones,
      vehicleQueue: vehicleQueueDangerZones
    })

    await preloadTileGrids(
      singleDangerZones,
      vehicleFlowDangerZones,
      vehicleQueueDangerZones
    );

    const tileGridResult = tileGridCache.value[750];

    const areaGridResult = await withCache(
      `areaGrid_${cityIds.join('_')}_750`,
      () =>
        calculateTileArea({
          cityIds,
          tileSize: 750,
          singleDangerZones,
          vehicleFlowDangerZones,
          trafficLightQueueDangerZones: vehicleQueueDangerZones
        })
    );

    const sanitaryAreas = await getAllEnterpriseSanitaryAreas(selectedCities.value.map(c => c.id));

    const {
      singleLayer,
      vehicleFlowLayer,
      vehicleQueueLayer,
      sanitaryAreaLayer,
      tileGridLayer,
      areaGridLayer
    } = createLayers(singleDangerZones, vehicleFlowDangerZones, vehicleQueueDangerZones, sanitaryAreas, tileGridResult, areaGridResult);

    let coords = [86.811102, 53.848768]
    if (selectedCities.value.length > 0) {
      const selectedCity = await getCityById(selectedCities.value[0].id)
      coords = selectedCity.location.coordinates;
    }

    map.value = new Map({
      target: mapRoot.value,
      layers: [
        baseLayers.osm,
        baseLayers.esri,
        baseLayers.cartoVoyager,
        baseLayers.cartoLight,
        baseLayers.cartoDark,

        singleLayer,
        vehicleFlowLayer,
        vehicleQueueLayer,
        sanitaryAreaLayer,
        tileGridLayer,
        areaGridLayer,

        measureLayer
      ],
      view: new View({
        center: fromLonLat(coords),
        zoom: 12
      }),
      controls: defaultControls({
        zoom: false
      })
    });

    if (selectedBaseMap.value !== 'osm') {
      changeBaseMap()
    }

    initLayersState();

    map.value.on('moveend', refreshTileGridByZoom);

    await refreshTileGridByZoom();

    map.value.on('singleclick', async (evt) => {
      if (isDeleteMode.value) return

      const pixel = evt.pixel;

      let found = null;

      map.value.forEachFeatureAtPixel(pixel, (feature, layer) => {
        if (feature.getGeometry().getType() === 'Polygon' && layer === olLayers.single) {
          const dangerData = feature.get('dangerData');
          if (dangerData) {
            found = dangerData;
          }
        }
      });

      if (isMeasuringArea.value) {
        return
      }

      if (showSimulationPanel.value && found == null) {
        await closeSimulationPanel();
      } else if (found != null) {
        if (simulationStartData.value != null) {
          await closeSimulationPanel();
        }

        const emissionSource = await getSingleEmissionSourceById(
          found.emissionSourceId
        )

        openSimulationPanel(emissionSource, found)
      }
    });

    map.value.on('pointermove', (evt) => {
      if (createModeFlow.value || createModeSingle.value) {
        hidePopup()
        return
      }

      if (evt.dragging || isMeasuringArea.value) {
        hidePopup()
        return
      }

      const pixel = evt.pixel

      let popupFeature = null
      let sourceType = null

      map.value.forEachFeatureAtPixel(
        pixel,
        (feature, layer) => {
          if (layer === olLayers.single) {
            popupFeature = feature
            sourceType = 'котельная'
            return true
          }

          if (layer === olLayers.vehicleFlow) {
            popupFeature = feature
            sourceType = 'дорога'
            return true
          }

          if (layer === olLayers.vehicleQueue) {
            popupFeature = feature
            sourceType = 'перекресток'
            return true
          }

          return false
        },
        {
          hitTolerance: 8
        }
      )

      if (popupFeature) {
        showPopup(evt.coordinate, popupFeature, sourceType)
        map.value.getTargetElement().style.cursor = 'pointer'
      } else {
        hidePopup()
        map.value.getTargetElement().style.cursor = ''
      }
    })
  })
}
