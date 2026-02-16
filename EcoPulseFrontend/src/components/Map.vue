<template>
  <div class="map-wrapper">
    <div ref="mapRoot" class="fullscreen-map"></div>

    <div class="layer-panel">
      <h3>Слои</h3>
      <label>
        <input
            type="checkbox"
            v-model="layersState.single.visible"
            @change="toggleLayer('single')"
        />
        Котельные
      </label>
      <label>
        <input
            type="checkbox"
            v-model="layersState.vehicleFlow.visible"
            @change="toggleLayer('vehicleFlow')"
        />
        Дороги
      </label>
      <label>
        <input
            type="checkbox"
            v-model="layersState.vehicleQueue.visible"
            @change="toggleLayer('vehicleQueue')"
        />
        Перекрестки
      </label>
      <label>
        <input
            type="checkbox"
            v-model="layersState.tileGrid.visible"
            @change="toggleLayer('tileGrid')"
        />
        Сетка
      </label>
      <label>
        <input
            type="checkbox"
            v-model="layersState.sanitaryArea.visible"
            @change="toggleLayer('sanitaryArea')"
        />
        СЗЗ
      </label>
    </div>

    <!--<div class="edit-tool-panel">
      <span class="edit-tool__label" @click="toggleEditPanel">
        Изменить
      </span>

      <div v-if="editPanelOpen" class="edit-tool-panel__body">
        <button class="edit-tool-panel__body__item" @click="startCreateModeFlow">
          Создать линию
        </button>
        <button class="edit-tool-panel__body__item" @click="startCreateModeQueue">
          Создать точку
        </button>

        <input
            type="text"
            v-model="streetName"
        />
      </div>

    </div>-->

    <div class="city-select">
      <span class="city-select__label" @click="toggleCityDropdown">
        {{ selectedCities.length ? selectedCities.map(c => c.name).join(', ') : 'Выберите города' }}
      </span>

      <div
          v-if="cityDropdownOpen"
          class="city-select-dropdown"
      >
        <label
            v-for="city in cities"
            :key="city.id"
            class="city-select-dropdown__item"
        >
          <input
              type="checkbox"
              :value="city"
              v-model="selectedCities"
          />
          <span>{{ city.name }}</span>
        </label>
      </div>
    </div>

    <div class="legend">
      <div class="legend-title">Уровни загрязнения</div>

      <div
          v-for="level in levels"
          :key="level.label"
          class="legend-item"
          @click="showInfo = true"
      >
    <span
        class="legend-color"
        :style="{ backgroundColor: level.color }"
    />
        <span class="legend-label">{{ level.label }}</span>
      </div>

      <div
          v-if="showInfo"
          class="legend-popup"
          @click.self="showInfo = false"
      >
        <div class="legend-popup-content">
          <img src="../assets/info.png" alt="Фото загрязнения"/>
        </div>
      </div>
    </div>

    <div v-if="currentRecommendation?.recommendationLevel" class="recommendation-title">
      {{ currentRecommendation.recommendationLevel }}: {{ currentRecommendation.recommendationText }}
    </div>
  </div>

  <WeatherInfo
      v-if="weather"
      :temperature="weather.temperature"
      :icon-class="weather.iconClass"
      :wind-speed="weather.windSpeed"
      :wind-direction="weather.windDirection"
  />

  <SimulationPanel
      v-model:data="simulationData"  
      :start-data="simulationStartData"
      v-if="showSimulationPanel"
      @buildSimulation="buildSimulation"
      @close="closeSimulationPanel"
  />
</template>

<script setup>
import {onMounted, reactive, ref} from 'vue'
import 'ol/ol.css'
import {defaults as defaultControls} from 'ol/control'
import Map from 'ol/Map'
import {fromLonLat, toLonLat} from 'ol/proj'
import View from 'ol/View'
import TileLayer from 'ol/layer/Tile'
import OSM from 'ol/source/OSM'

import VectorLayer from 'ol/layer/Vector'
import VectorSource from 'ol/source/Vector'
import {LineString, Point, Polygon} from 'ol/geom'
import Feature from 'ol/Feature'
import {Circle as CircleStyle, Fill, Icon, Stroke, Style} from 'ol/style'
import Modify from 'ol/interaction/Modify';

import {
  calculateSingleDangerZones, calculateSingleDangerZone, calculateVehicleFlowDangerZones,
  calculateTrafficLightQueueDangerZones
} from '../api/dangerZone.js';
import { calculateTileGrid } from '../api/tileGrid.js';
import { getAllEnterpriseSanitaryAreas } from '../api/enterprise.js';
import { getRecommendations } from '../api/recommendation.js';
import {getCurrentWeather} from '../api/weather.js';
import {
  addTrafficLightQueueEmissionSource,
  addVehicleFlowEmissionSource,
  getSingleEmissionSourceById, updateVehicleFlowEmissionSource
} from '../api/emissionSource.js';
import boilerIcon from '../icons/boiler.png';
import WeatherInfo from "../components/WeatherInfo.vue";
import SimulationPanel from '../components/SimulationPanel.vue'
import {asArray} from "ol/color";
import {altKeyOnly, singleClick} from 'ol/events/condition';
import Text from 'ol/style/Text.js';
import {getCityById} from "../api/city.js";
import Overlay from 'ol/Overlay'
import XYZ from 'ol/source/XYZ.js';

const mapRoot = ref(null)
const map = ref(null)
const weather = ref(null)
const showSimulationPanel = ref(false)
const simulationStartData = ref(null)
const simulationData = ref(null)

const createModeFlow = ref(false)
const createModeQueue = ref(false)
const createPoints = ref([])
const modifyFlow = ref(null);
const showInfo = ref(false);
const streetName = ref(null);

const singlePopup = ref(null)
const currentSingle = ref(null)
const userPosition = ref(null);
const currentRecommendation = ref(null)

const olLayers = reactive({
  single: null,
  vehicleFlow: null,
  vehicleQueue: null,
  tileGrid: null,
  sanitaryArea: null,
})

const layersState = reactive({
  single: {visible: true},
  vehicleFlow: {visible: false},
  vehicleQueue: {visible: false},
  tileGrid: {visible: false},
  sanitaryArea: {visible: false},
})

const levels = [
  {max: 9.0, color: 'rgba(171, 209, 98, 1)', label: 'Очень низкий'},
  {max: 35.4, color: 'rgba(248, 212, 97, 1)', label: 'Низкий'},
  {max: 55.4, color: 'rgba(251, 153, 86, 1)', label: 'Средний'},
  {max: 125.4, color: 'rgba(246, 104, 106, 1)', label: 'Высокий'},
  {max: 225.4, color: 'rgba(164, 125, 184, 1)', label: 'Очень высокий'},
  {max: 9999, color: 'rgba(138, 79, 163, 1)', label: 'Экстремальный'}
]

const cities = ref([
  {id: 1, name: 'Кемерово'},
  {id: 2, name: 'Новокузнецк'},
  {id: 3, name: 'Прокопьевск'},
  {id: 4, name: 'Киселевск'},
])

const selectedCities = ref(JSON.parse(localStorage.getItem("selectedCities") || 'null') ?? [cities.value[0]])
const cityDropdownOpen = ref(false)
const editPanelOpen = ref(false)

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
  }
}


const toggleEditPanel = () => {
  editPanelOpen.value = !editPanelOpen.value
}

function startCreateModeFlow() {
  createModeFlow.value = true
  createPoints.value = []
}

function startCreateModeQueue() {
  createModeQueue.value = true
}

function showSinglePopup(coordinate, feature) {
  currentSingle.value = feature.get('dangerData')
  
  const popupElement = createSinglePopupElement()
  
  if (!singlePopup.value) {
    singlePopup.value = new Overlay({
      element: popupElement,
      positioning: 'bottom-center',
      stopEvent: false,
      insertFirst: false,
    })
    map.value.addOverlay(singlePopup.value)
  } else {
    singlePopup.value.setElement(popupElement)
  }
  
  singlePopup.value.setPosition(coordinate)
}

function hideSinglePopup() {
  if (singlePopup.value) {
    map.value.removeOverlay(singlePopup.value)
    singlePopup.value = null
  }
  currentSingle.value = null
}

function createSinglePopupElement() {
  const popup = document.createElement('div')
  popup.className = 'single-popup'
  
  popup.innerHTML = `
    <div class="popup-content">
      <div class="popup-text">
        <div class="average-concentration">
          <strong><label>Концентрация: </label></strong>
          <span>${currentSingle.value.averageConcentration} мкг/м3</span>
        </div>

        <div class="legend-item">
          <strong><label>Уровень загрязнения -&nbsp;</label></strong> 
          <span>${currentSingle.value.pollutionLevel}&nbsp;</span> 
          <span 
            class="legend-color" 
            style="background-color: ${currentSingle.value.color}"
          ></span>
        </div>
      </div>
    </div>
  `
  return popup
}

async function handleTwoPointsSelected(p1, p2) {
  const selectedCityId = selectedCities.value.length > 0
      ? selectedCities.value[0].id
      : null;

  await addVehicleFlowEmissionSource({
    cityId: selectedCityId,
    points: {
      type: "LineString",
      coordinates: [p1, p2]
    },
    vehicleType: 1,
    maxTrafficIntensity: 35 / 2,
    averageSpeed: 40 / 2,
    streetName: streetName.value
  });

  await updateVehicleFlowLayer();
}

async function buildSimulation(data) {
  const dangerZone = await calculateSingleDangerZone({
    pollutant: 2,
    ejectedTemp: data.ejectedTemp,
    airTemp: data.airTemp,
    avgExitSpeed: data.avgExitSpeed,
    heightSource: data.heightSource,
    diameterSource: data.diameterSource,
    tempStratificationRatio: data.tempStratificationRatio,
    sedimentationRateRatio: data.sedimentationRateRatio,
    windSpeed: data.windSpeed,
    windDirection: data.windDirection,
    distance: 10000,
    sourceLocation: data.location
  });

  dangerZone.emissionSourceId = data.emissionSourceId;
  dangerZone.location = data.location;

  const singleLayer = olLayers.single;
  const source = singleLayer.getSource();

  const featuresToRemove = source.getFeatures().filter(f => {
    return f.get('emissionSourceId') === dangerZone.emissionSourceId;
  });

  source.removeFeatures(featuresToRemove);

  const ellipse = createEllipse(dangerZone);
  ellipse.set('emissionSourceId', dangerZone.emissionSourceId);
  source.addFeature(ellipse);

  const pointFeature = new Feature({
    geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
    type: 'boiler'
  });
  pointFeature.set('emissionSourceId', dangerZone.emissionSourceId);
  source.addFeature(pointFeature);

  simulationData.value = {
    averageConcentration: dangerZone.averageConcentration,
    pollutionLevel: dangerZone.pollutionLevel,
    color: dangerZone.color
  }
}

async function updateSingleLayer() {
  const singleDangerZones = await calculateSingleDangerZones({
    pollutant: 2, // solid particles
    airTemp: weather.value.temperature,
    windSpeed: weather.value.windSpeed,
    windDirection: weather.value.windDirection,
    cityIds: selectedCities.value.map(c => c.id)
  });

  const layer = olLayers.single;
  if (!layer) return singleDangerZones;

  const source = layer.getSource();
  if (!source) return singleDangerZones;

  source.clear();

  singleDangerZones.forEach(dangerZone => {
    const ellipse = createEllipse(dangerZone);
    ellipse.set('emissionSourceId', dangerZone.emissionSourceId);
    source.addFeature(ellipse);

    const pointFeature = new Feature({
      geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
      type: 'boiler'
    })
    pointFeature.set('dangerColor', dangerZone.color);
    pointFeature.set('emissionSourceId', dangerZone.emissionSourceId);

    source.addFeature(pointFeature);
  })

  layer.changed();

  await updateModifyFlow();

  return singleDangerZones;
}

async function updateVehicleFlowLayer() {
  const vehicleFlowDangerZones = await calculateVehicleFlowDangerZones({
    cityIds: selectedCities.value.map(c => c.id)
  });

  const layer = olLayers.vehicleFlow;
  if (!layer) return vehicleFlowDangerZones;

  const source = layer.getSource();
  if (!source) return vehicleFlowDangerZones;

  source.clear();

  vehicleFlowDangerZones.forEach(dz => {
    const coords = dz.points.coordinates.map(([lon, lat]) => fromLonLat([lon, lat]));

    const lineFeature = new Feature({
      geometry: new LineString(coords),
      type: 'flow'
    });

    lineFeature.set('dangerColor', dz.color);
    lineFeature.set('emissionSourceId', dz.emissionSourceId);

    source.addFeature(lineFeature);
  });

  layer.changed();

  await updateModifyFlow();

  return vehicleFlowDangerZones;
}

async function updateVehicleQueueLayer() {
  const vehicleQueueDangerZones = await calculateTrafficLightQueueDangerZones({
    cityIds: selectedCities.value.map(c => c.id)
  });

  const layer = olLayers.vehicleQueue;
  if (!layer) return vehicleQueueDangerZones;

  const source = layer.getSource();
  if (!source) return vehicleQueueDangerZones;

  source.clear();

  vehicleQueueDangerZones.forEach(dangerZone => {
    const pointFeature = new Feature({
      geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
      type: 'queue'
    })
    pointFeature.set('dangerColor', dangerZone.color);
    pointFeature.set('emissionSourceId', dangerZone.emissionSourceId);

    source.addFeature(pointFeature)
  })

  layer.changed();

  return vehicleQueueDangerZones;
}

async function updateTileGridLayer(singleDangerZones, vehicleFlowDangerZones, vehicleQueueDangerZones) {
  const tileGridResult = await calculateTileGrid({
    cityIds: selectedCities.value.map(c => c.id),
    tileSize: 750,
    singleDangerZones: singleDangerZones,
    vehicleFlowDangerZones: vehicleFlowDangerZones,
    trafficLightQueueDangerZones: vehicleQueueDangerZones
  });

  const layer = olLayers.tileGrid;
  if (!layer) return;

  const source = layer.getSource();
  if (!source) return;

  source.clear();

  const allTiles = tileGridResult.flatMap(city => city.tiles);
  allTiles.forEach(tileInfo => {
    const polygonCoords = tileInfo.tile.coordinates[0]; 
    
    const polygonFeature = new Feature({
      geometry: new Polygon([
        polygonCoords.map(coord => fromLonLat(coord))
      ]),
      color: tileInfo.color,
      cityId: tileInfo.cityId
    });
    
    source.addFeature(polygonFeature);
  });

  layer.changed();
}

async function updateLayers() {
  const singleDangerZones = await updateSingleLayer();
  const vehicleFlowDangerZones = await updateVehicleFlowLayer();
  const vehicleQueueDangerZones = await updateVehicleQueueLayer();
  await updateTileGridLayer(singleDangerZones, vehicleFlowDangerZones, vehicleQueueDangerZones);
}

async function closeSimulationPanel() {
  await buildSimulation(simulationStartData.value);
  showSimulationPanel.value = false;
  simulationStartData.value = null;
}

function createEllipse(dangerZone) {
  const polygonCoords = dangerZone.polygon.coordinates[0];
  const coords = polygonCoords.map(coord => 
    fromLonLat(coord)
  );
  
  const ellipseFeature = new Feature({
    geometry: new Polygon([coords])
  });

  ellipseFeature.set('dangerData', dangerZone);
  ellipseFeature.set('emissionSourceId', dangerZone.emissionSourceId);
  ellipseFeature.set('dangerColor', dangerZone.color);
  ellipseFeature.set('averageConcentration', dangerZone.averageConcentration || 'N/A'); 

  return ellipseFeature;
}

function getColorWithAlpha(baseColor, alpha = 0.75) {
  if (!baseColor) {
    return [0, 0, 0, alpha];
  }

  return Array.isArray(baseColor)
      ? [baseColor[0], baseColor[1], baseColor[2], alpha]
      : asArray(baseColor).slice(0, 3).concat(alpha);
}

function createSingleLayer(dangerZones) {
  const singleSource = new VectorSource();

  dangerZones.forEach(dangerZone => {
    const ellipse = createEllipse(dangerZone);
    ellipse.set('emissionSourceId', dangerZone.emissionSourceId);
    singleSource.addFeature(ellipse);

    const pointFeature = new Feature({
      geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
      type: 'boiler'
    });
    pointFeature.set('dangerColor', dangerZone.color);
    pointFeature.set('emissionSourceId', dangerZone.emissionSourceId);
    pointFeature.set('averageConcentration', dangerZone.averageConcentration);

    singleSource.addFeature(pointFeature);
  });

  const pointStyle = new Style({
    image: new Icon({
      src: boilerIcon,
      scale: 0.055,
      anchor: [0.5, 1],
      anchorXUnits: 'fraction',
      anchorYUnits: 'fraction'
    })
  });

  const ellipseFillStyle = new Style({
    fill: new Fill({
      color: 'black'
    }),
    stroke: new Stroke({
      color: 'rgba(0, 0, 0, 0.5)'
    })
  });

  const textStyle = new Style({
    text: new Text({
      font: 'bold 18px roboto',
      fill: new Fill({
        color: 'black'
      }),
      textAlign: 'center',
      textBaseline: 'middle'
    })
  });

  return new VectorLayer({
    source: singleSource,
    visible: true,
    zIndex: 3,
    style: feature => {
      const geom = feature.getGeometry();
      const color = getColorWithAlpha(feature.get('dangerColor'), 0.6);

      if (geom.getType() === 'Polygon') {
        ellipseFillStyle.getFill().setColor(color);
        
        /*const avgConc = feature.get('averageConcentration');
        if (avgConc && avgConc !== 'N/A') {
          textStyle.getText().setText(avgConc.toString());
          return [ellipseFillStyle, textStyle];
        }*/
        
        return ellipseFillStyle;
      }

      if (geom.getType() === 'Point') {
        return pointStyle;
      }

      return null;
    }
  });
}

function createVehicleFlowLayer(dangerZones) {
  const vehicleFlowSource = new VectorSource();

  dangerZones.forEach(dz => {
    const coords = dz.points.coordinates.map(([lon, lat]) => fromLonLat([lon, lat]));

    const lineFeature = new Feature({
      geometry: new LineString(coords),
      type: 'flow'
    });
    lineFeature.set('dangerColor', dz.color);
    lineFeature.set('emissionSourceId', dz.emissionSourceId);

    vehicleFlowSource.addFeature(lineFeature);
  });

  const lineStyle = new Style({
    stroke: new Stroke({
      color: 'black',
      width: 5,
    }),
  });

  const pointStyle = new Style({
    image: new CircleStyle({
      radius: 3,
      fill: new Fill({color: 'black'}),
      stroke: new Stroke({
        color: 'rgba(0, 0, 0, 0.5)',
      })
    })
  });

  return new VectorLayer({
    source: vehicleFlowSource,
    visible: false,
    zIndex: 1,
    style: feature => {
      const geomType = feature.getGeometry().getType();
      const color = getColorWithAlpha(feature.get('dangerColor'), 0.75);

      if (geomType === 'LineString') {
        lineStyle.getStroke().setColor(color);
        return lineStyle;
      }

      if (geomType === 'Point') {
        pointStyle.getImage().getFill().setColor(color);
        return pointStyle;
      }

      return null;
    }
  });
}

function createVehicleQueueLayer(dangerZones) {
  const vehicleQueueSource = new VectorSource()

  dangerZones.forEach(dangerZone => {
    const pointFeature = new Feature({
      geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
      type: 'queue'
    })
    pointFeature.set('dangerColor', dangerZone.color);
    pointFeature.set('emissionSourceId', dangerZone.emissionSourceId);

    vehicleQueueSource.addFeature(pointFeature)
  })

  return new VectorLayer({
    source: vehicleQueueSource,
    visible: false,
    zIndex: 2,
    style: feature => {
      const geomType = feature.getGeometry().getType();
      const color = getColorWithAlpha(feature.get('dangerColor'), 0.75);

      if (geomType === 'Point') {
        return new Style({
          image: new CircleStyle({
            radius: 5,
            fill: new Fill({color: color}),
            stroke: new Stroke({
              color: 'rgba(0, 0, 0, 0.5)',
            })
          }),
        });
      }

      return null;
    }
  });
}

function createTileGridLayer(tileGridResult) {
  const tileGridSource = new VectorSource();

  const allTiles = tileGridResult.flatMap(city => city.tiles);
  allTiles.forEach(tileInfo => {
    const polygonCoords = tileInfo.tile.coordinates[0]; 
    
    const polygonFeature = new Feature({
      geometry: new Polygon([
        polygonCoords.map(coord => fromLonLat(coord))
      ]),
      color: tileInfo.color,
      cityId: tileInfo.cityId
    });
    
    tileGridSource.addFeature(polygonFeature);
  });

  return new VectorLayer({
    source: tileGridSource,
    visible: false,
    zIndex: 3,
    style: feature => {
      const color = getColorWithAlpha(feature.get('color'), 0.6);
      
      return new Style({
        fill: new Fill({
          color: color 
        }),
        stroke: new Stroke({
          color: 'black', 
          width: 0.5
        })
      });
    }
  });
}

function createSanitaryAreaLayer(sanitaryAreas) {
  const sanitaryAreaSource = new VectorSource();

  sanitaryAreas.forEach(area => {
    const polygonCoords = area.coordinates[0]; 
    
    const polygonFeature = new Feature({
      geometry: new Polygon([
        polygonCoords.map(coord => fromLonLat(coord))
      ]),
    });
    
    sanitaryAreaSource.addFeature(polygonFeature);
  });

  const hatchPattern = getHatchPattern(24);

  return new VectorLayer({
    source: sanitaryAreaSource,
    visible: false,
    zIndex: 3,
    style: feature => {
      return new Style({
        fill: new Fill({ 
          color: hatchPattern 
        }),
        stroke: new Stroke({
          color: "black",
          width: 1
        })
      });
    }
  });
}

function createLayers(singleDangerZones, vehicleFlowDangerZones, vehicleQueueDangerZones, tileGridResult, sanitaryAreas, recommendationResult) {
  const singleLayer = createSingleLayer(singleDangerZones);
  const vehicleFlowLayer = createVehicleFlowLayer(vehicleFlowDangerZones);
  const vehicleQueueLayer = createVehicleQueueLayer(vehicleQueueDangerZones);
  const tileGridLayer = createTileGridLayer(tileGridResult);
  const sanitaryAreaLayer = createSanitaryAreaLayer(sanitaryAreas);

  olLayers.single = singleLayer;
  olLayers.vehicleFlow = vehicleFlowLayer;
  olLayers.vehicleQueue = vehicleQueueLayer;
  olLayers.tileGrid = tileGridLayer;
  olLayers.sanitaryArea = sanitaryAreaLayer;

  return {singleLayer, vehicleFlowLayer, vehicleQueueLayer, tileGridLayer, sanitaryAreaLayer};
}

onMounted(async () => {
  const baseLayer = new TileLayer({
    source: new OSM()
  })

  const currentWeather = await getCurrentWeather();
  weather.value = {
    temperature: currentWeather.temperature,
    iconClass: currentWeather.iconClass,
    windSpeed: currentWeather.windSpeed,
    windDirection: currentWeather.windDirection,
  }

  await updateLayers();

  const singleDangerZones = await calculateSingleDangerZones({
    pollutant: 2, // solid particles
    airTemp: currentWeather.temperature,
    windSpeed: currentWeather.windSpeed,
    windDirection: currentWeather.windDirection,
    cityIds: selectedCities.value.map(c => c.id)
  });

  const vehicleFlowDangerZones = await calculateVehicleFlowDangerZones({
    cityIds: selectedCities.value.map(c => c.id)
  });

  const vehicleQueueDangerZones = await calculateTrafficLightQueueDangerZones({
    cityIds: selectedCities.value.map(c => c.id)
  });

  const tileGridResult = await calculateTileGrid({
    cityIds: selectedCities.value.map(c => c.id),
    tileSize: 750,
    singleDangerZones: singleDangerZones,
    vehicleFlowDangerZones: vehicleFlowDangerZones,
    trafficLightQueueDangerZones: vehicleQueueDangerZones
  });

  const sanitaryAreas = await getAllEnterpriseSanitaryAreas(selectedCities.value.map(c => c.id));

  const userPos = await getUserPosition();
  
  const recommendationResult = await getRecommendations({
    tiles: tileGridResult.flatMap(result => result.tiles),
    userLocation: userPosition.value
  });

  currentRecommendation.value = {
    recommendationLevel: recommendationResult.recommendationLevel,
    recommendationText: recommendationResult.recommendationText
  };

  const {
    singleLayer,
    vehicleFlowLayer,
    vehicleQueueLayer,
    tileGridLayer,
    sanitaryAreaLayer
  } = createLayers(singleDangerZones, vehicleFlowDangerZones, vehicleQueueDangerZones, tileGridResult, sanitaryAreas);

  let coords = [86.0833, 55.3333]
  if (selectedCities.value.length > 0) {
    const selectedCity = await getCityById(selectedCities.value[0].id)
    coords = selectedCity.location.coordinates;
  }

  map.value = new Map({
    target: mapRoot.value,
    layers: [baseLayer, singleLayer, vehicleFlowLayer, vehicleQueueLayer, tileGridLayer,  sanitaryAreaLayer],
    view: new View({
      center: fromLonLat(coords),
      zoom: 12
    }),
    controls: defaultControls({
      zoom: false
    })
  })

  initLayersState();

  map.value.on('singleclick', async (evt) => {
    const pixel = evt.pixel;

    let found = null;

    map.value.forEachFeatureAtPixel(pixel, (feature) => {
      if (feature.getGeometry().getType() === 'Polygon') {
        const dangerData = feature.get('dangerData');
        if (dangerData) {
          found = dangerData;
        }
      }
    });

    if (createModeFlow.value) {
      const coord3857 = evt.coordinate
      const [lon, lat] = toLonLat(coord3857)

      createPoints.value.push([lon, lat])

      if (createPoints.value.length === 2) {
        const [p1, p2] = createPoints.value
        await handleTwoPointsSelected(p1, p2)

        createModeFlow.value = false
        createPoints.value = []
      }

      return
    }

    if (createModeQueue.value) {
      const coord3857 = evt.coordinate
      const [lon, lat] = toLonLat(coord3857)

      const selectedCityId = selectedCities.value.length > 0
      ? selectedCities.value[0].id
      : null;

      await addTrafficLightQueueEmissionSource({
        location: {
          type: "Point",
          coordinates: [lon, lat]
        },
        vehicleType: 1,
        vehiclesCount: randomInt(1, 5),
        trafficLightCycles: 12,
        trafficLightStopTime: 60,
        cityId: selectedCityId
      })

      await updateVehicleQueueLayer()

      createModeQueue.value = false

      return
    }

    if (showSimulationPanel.value && found == null) {
      await closeSimulationPanel();
    } else if (found != null) {
      if (simulationStartData.value != null) {
        await closeSimulationPanel();
      }

      showSimulationPanel.value = true;

      const emissionSource = await getSingleEmissionSourceById(found.emissionSourceId);

      simulationStartData.value = {
        emissionSourceId: emissionSource.id,
        location: emissionSource.location,
        ejectedTemp: emissionSource.ejectedTemp,
        airTemp: weather.value.temperature,
        avgExitSpeed: emissionSource.avgExitSpeed,
        heightSource: emissionSource.heightSource,
        diameterSource: emissionSource.diameterSource,
        windSpeed: weather.value.windSpeed,
        windDirection: weather.value.windDirection,
        tempStratificationRatio: emissionSource.tempStratificationRatio,
        sedimentationRateRatio: emissionSource.sedimentationRateRatio,
      }

      simulationData.value = {
        averageConcentration: found.averageConcentration,
        pollutionLevel: found.pollutionLevel,
        color: found.color
      }
    }
  });

  map.value.on('pointermove', (evt) => {
    if (evt.dragging) return
    
    const pixel = map.value.getEventPixel(evt.originalEvent)
    let singleFeature = null
    
    map.value.forEachFeatureAtPixel(pixel, (feature, layer) => {
      if (layer === olLayers.single) {
        singleFeature = feature
        return false
      }
    })

    if (singleFeature) {
      const coordinate = evt.coordinate
      showSinglePopup(coordinate, singleFeature)
      
      const mapElement = map.value.getTargetElement()
      mapElement.style.cursor = 'pointer'
    } else {
      hideSinglePopup()
    }

    const allHit = map.value.hasFeatureAtPixel(pixel)
    const mapElement = map.value.getTargetElement()
    mapElement.style.cursor = allHit ? 'pointer' : ''
  })

  await updateModifyFlow();
})

async function updateModifyFlow() {
  if (modifyFlow.value) {
    map.value.removeInteraction(modifyFlow.value);
  }

  /*const mf = new Modify({
    source: olLayers.vehicleFlow.getSource(),
    filter: feature => feature.getGeometry().getType() === 'LineString',
    deleteCondition: event => altKeyOnly(event) && singleClick(event),
  });

  mf.on('modifyend', async evt => {
    const feature = evt.features.item(0);
    if (!feature) return;

    const geom = feature.getGeometry();
    const coords3857 = geom.getCoordinates();

    const emissionSourceId = feature.get('emissionSourceId');
    const coords4326 = coords3857.map(([x, y]) => toLonLat([x, y]));

    await updateVehicleFlowEmissionSource({
      id: emissionSourceId,
      points: {
        type: "LineString",
        coordinates: coords4326
      },
      streetName: streetName.value
    });

    feature.changed();
  });

  map.value.addInteraction(mf);
  modifyFlow.value = mf;*/

  //modifyFlow.value.setActive(layersState.vehicleFlow.visible);
}

function randomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

const initLayersState = () => {
  const savedState = localStorage.getItem('layersState');
  if (savedState) {
    const parsedState = JSON.parse(savedState);
    Object.keys(layersState).forEach(key => {
      if (parsedState[key]) {
        layersState[key].visible = parsedState[key].visible ?? true;
      }
    });
  }

  Object.keys(olLayers).forEach(key => {
    if (olLayers[key]) {
      olLayers[key].setVisible(layersState[key].visible);
    }
  });
};

const toggleLayer = key => {
  if (!olLayers[key]) return;

  const visible = layersState[key].visible;
  olLayers[key].setVisible(visible);

  saveLayersState();

  if (key === 'vehicleFlow' && modifyFlow.value) {
    modifyFlow.value.setActive(visible);
  }
};

const saveLayersState = () => {
  const stateToSave = {};
  Object.keys(layersState).forEach(key => {
    stateToSave[key] = { visible: layersState[key].visible };
  });
  localStorage.setItem('layersState', JSON.stringify(stateToSave));
};

function getHatchPattern(size) {
  const canvas = document.createElement('canvas');
  const context = canvas.getContext('2d');
  
  canvas.width = size;
  canvas.height = size;
    
  context.strokeStyle = 'black';
  context.lineWidth = 1.5;
    
  context.beginPath();
  context.moveTo(0, size);
  context.lineTo(size, 0);
  context.stroke();
    
  return context.createPattern(canvas, 'repeat');
}

</script>

<style>
.map-wrapper {
  position: relative;
}

.fullscreen-map {
  width: 100%;
  height: 100vh;
}

.layer-panel {
  position: absolute;
  top: 24px;
  left: 24px;
  padding: 8px 12px;
  background: white;
  border-radius: 4px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  font-size: 14px;
}

.layer-panel h3 {
  margin: 0 0 4px;
  font-size: 14px;
}

.layer-panel label {
  display: block;
}

.layer-panel .create-btn {
  margin-top: 8px;
  width: 100%;
  padding: 6px 8px;
  font-size: 14px;
  cursor: pointer;
}

.legend {
  position: absolute;
  left: 24px;
  bottom: 24px;
  background: rgba(255, 255, 255, 0.9);
  padding: 8px 10px;
  border-radius: 4px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  font-size: 12px;
  z-index: 10;
}

.legend-title {
  font-weight: 600;
  margin-bottom: 4px;
}

.legend-item {
  display: flex;
  align-items: center;
  margin: 2px 0;
  cursor: pointer;
}

.legend-color {
  width: 14px;
  height: 14px;
  border-radius: 2px;
  margin-right: 6px;
  border: 1px solid rgba(0, 0, 0, 0.2);
}

.pollution-level {
  font-size: 14px;
  color: #444;
  display: flex;        
  align-items: center;  
  gap: 6px;            
}

.pollution-level-color {
    width: 14px;
    height: 14px;
    border-radius: 2px;
    margin-right: 6px;
    border: 1px solid rgba(0, 0, 0, 0.2);
}

.legend-popup {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.legend-popup-content {
  background: #fff;
  padding: 12px 16px;
  border-radius: 4px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
  max-width: 80vw;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.legend-popup-content img {
  max-width: 100%;
  max-height: 70vh;
  display: block;
}

.city-select {
  position: absolute;
  top: 24px;
  left: 160px;
  font-family: sans-serif;
  font-size: 14px;
}

.city-select__label {
  display: inline-block;
  padding: 6px 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
}

.city-select-dropdown {
  position: absolute;
  margin-top: 4px;
  padding: 6px 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
  z-index: 10;
}

.city-select-dropdown__item {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 4px;
}

.city-select-dropdown__item input[type="checkbox"] {
  margin: 0;
}

.edit-tool-panel {
  position: absolute;
  top: 24px;
  left: 256px;
  font-family: sans-serif;
  font-size: 14px;
}

.edit-tool-panel {
  display: inline-block;
  padding: 6px 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
}

.edit-tool-panel__body {
  position: absolute;
  margin-top: 12px;
  left: 0;
  padding: 8px 12px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
  z-index: 10;
}

.edit-tool-panel__body__item {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 4px;
}

.single-popup {
  background: rgba(255, 255, 255, 0.95);
  border-radius: 6px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  padding: 8px;
  font-size: 12px;
  min-width: 180px;
  pointer-events: none;
}

.popup-content {
  line-height: 1.4;
}

.popup-title {
  font-weight: 600;
  margin-bottom: 4px;
  color: #333;
}

.popup-text {
  color: #555;
}

.recommendation-title {
  position: absolute;
  top: 24px;
  left: 50%;
  transform: translateX(-50%);
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.95);
  border-radius: 4px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  font-size: 14px;
  z-index: 10;
  white-space: nowrap;
  max-width: 90vw;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>