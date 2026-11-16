<template>
  <div class="map-wrapper">
    <div ref="mapRoot" class="fullscreen-map"></div>

    <div v-if="createModeSingle" class="map-pick-notification">
      <div class="map-pick-notification__icon">📍</div>

      <div class="map-pick-notification__content">
        <strong>Выбор местоположения</strong>
        <span>Нажмите на карту, чтобы указать точку размещения источника</span>
      </div>

      <button class="map-pick-notification__cancel" @click="cancelCreateModeSingle">
        Отмена
      </button>
    </div>

    <div class="map-controls">

      <div class="base-map-selector">
        <label for="baseMap">Карта:</label>

        <select id="baseMap" v-model="selectedBaseMap" @change="changeBaseMap">
          <option value="osm">Обычная</option>
          <option value="esri">Спутник</option>
          <option value="cartoVoyager">Вояджер</option>
          <option value="cartoLight">Светлая</option>
          <option value="cartoDark">Темная</option>
        </select>
      </div>

      <div class="layer-panel">
        <h3>Слои</h3>

        <label>
          <input type="checkbox" v-model="layersState.single.visible" @change="toggleLayer('single')" />
          <span>Котельные</span>
          <input class="z-index-input" type="number" min="0" v-model.number="layersState.single.zIndex"
            @change="updateLayerZIndex('single')" />
        </label>

        <label>
          <input type="checkbox" v-model="layersState.vehicleFlow.visible" @change="toggleLayer('vehicleFlow')" />
          <span>Дороги</span>
          <input class="z-index-input" type="number" min="0" v-model.number="layersState.vehicleFlow.zIndex"
            @change="updateLayerZIndex('vehicleFlow')" />
        </label>

        <label>
          <input type="checkbox" v-model="layersState.vehicleQueue.visible" @change="toggleLayer('vehicleQueue')" />
          <span>Перекрестки</span>
          <input class="z-index-input" type="number" min="0" v-model.number="layersState.vehicleQueue.zIndex"
            @change="updateLayerZIndex('vehicleQueue')" />
        </label>

        <label>
          <input type="checkbox" v-model="layersState.tileGrid.visible" @change="toggleLayer('tileGrid')" />
          <span>Сетка</span>
          <input class="z-index-input" type="number" min="0" v-model.number="layersState.tileGrid.zIndex"
            @change="updateLayerZIndex('tileGrid')" />
        </label>

        <label>
          <input type="checkbox" v-model="layersState.sanitaryArea.visible" @change="toggleLayer('sanitaryArea')" />
          <span>СЗЗ</span>
          <input class="z-index-input" type="number" min="0" v-model.number="layersState.sanitaryArea.zIndex"
            @change="updateLayerZIndex('sanitaryArea')" />
        </label>
      </div>

      <div class="city-select">
        <span class="city-select__label" @click="toggleCityDropdown">
          {{selectedCities.length ? selectedCities.map(c => c.name).join(', ') : 'Выберите города'}}
        </span>

        <div v-if="cityDropdownOpen" class="city-select-dropdown">
          <label v-for="city in cities" :key="city.id" class="city-select-dropdown__item">
            <input type="checkbox" :value="city" v-model="selectedCities" />
            <span>{{ city.name }}</span>
          </label>
        </div>
      </div>

      <div class="tileLayer-panel" v-if="layersState.tileGrid.visible">
        <label>
          <input type="radio" v-model="selectedGridLayer" value="tile" @change="toggleLayer('tile')" />
          Ячейки
        </label>
        <label>
          <input type="radio" v-model="selectedGridLayer" value="area" @change="toggleLayer('area')" />
          Районы
        </label>
      </div>

      <div v-if="layersState.tileGrid.visible" class="tile-size-control">
        <label class="tile-size-auto">
          <input type="checkbox" v-model="autoTileSizeByZoom" @change="onAutoTileSizeChange" />
          Автоматически
        </label>

        <div class="tile-size-header">
          <label for="tileSize">Размер тайла</label>

          <div class="tile-size-value">
            {{ autoTileSizeByZoom ? (currentTileSize ?? tileSize) : tileSize }} м
          </div>
        </div>

        <input id="tileSize" :value="autoTileSizeByZoom ? (currentTileSize ?? tileSize) : tileSize"
          class="tile-size-slider" type="range" min="100" max="3000" step="25" :disabled="autoTileSizeByZoom"
          @input="tileSize = Number($event.target.value)" />

        <div class="tile-size-range">
          <span>100 м</span>
          <span>3000 м</span>
        </div>

        <button v-if="!autoTileSizeByZoom" class="tile-size-apply" @click="applyTileSize">
          Применить
        </button>
      </div>

      <div class="area-measure-panel">
        <button v-if="!isMeasuringArea" @click="startAreaMeasurement">
          Измерить площадь
        </button>

        <button v-if="isMeasuringArea" @click="stopAreaMeasurement">
          Выйти из режима площади
        </button>

        <button v-if="measuredArea !== null && !isMeasuringArea" @click="clearAreaMeasurement">
          Очистить область
        </button>
      </div>

      <div class="edit-tool-panel">

        <button class="add-single-source-btn" @click="startCreateModeSingle">
          <span>Добавить котельную</span>
        </button>

      </div>

    </div>

    <div class="legend">
      <div class="legend-title">Уровни загрязнения</div>

      <div v-for="level in levels" :key="level.label" class="legend-item" @click="showInfo = true">
        <span class="legend-color" :style="{ backgroundColor: level.color }" />
        <span class="legend-label">{{ level.label }}</span>
      </div>

      <div v-if="showInfo" class="legend-popup" @click.self="showInfo = false">
        <div class="legend-popup-content">
          <img src="../assets/info.png" alt="Фото загрязнения" />
        </div>
      </div>
    </div>

    <!-- <div v-if="currentRecommendation?.recommendationLevel" class="recommendation-title">
      {{ currentRecommendation.recommendationLevel }}: {{ currentRecommendation.recommendationText }}
    </div>-->
  </div>

  <WeatherInfo v-if="weather" :temperature="weather.temperature" :icon-class="weather.iconClass"
    :wind-speed="weather.windSpeed" :wind-direction="weather.windDirection" />

  <SimulationPanel v-model:data="simulationData" :start-data="simulationStartData" v-if="showSimulationPanel"
    @buildSimulation="handleBuildSimulation" @close="closeSimulationPanel" />

  <Teleport to="body">
    <div v-if="showSingleSourceForm" class="single-source-overlay">
      <div class="single-source-modal">

        <div class="single-source-modal__header">
          <div>
            <h2>Новый источник выброса</h2>
            <p>Укажите параметры точечного источника</p>
          </div>
        </div>

        <div class="single-source-modal__body">

          <div class="single-source-modal__section">
            <h3>Параметры выброса</h3>

            <label class="single-source-field">
              <span>Температура выбрасываемых газов, К</span>
              <input type="number" v-model.number="newSingleSource.ejectedTemp" min="235" max="265" />
              <small :class="{ 'field-error': singleSourceValidation.ejectedTemp }">
                {{ singleSourceValidation.ejectedTemp || 'Допустимое значение: 235–265' }}
              </small>
            </label>

            <label class="single-source-field">
              <span>Средняя скорость выхода, м/с</span>
              <input type="number" v-model.number="newSingleSource.avgExitSpeed" min="15" max="30" />
              <small :class="{ 'field-error': singleSourceValidation.avgExitSpeed }">
                {{ singleSourceValidation.avgExitSpeed || 'Допустимое значение: 15-30' }}
              </small>
            </label>

            <label class="single-source-field">
              <span>Высота источника, м</span>
              <input type="number" v-model.number="newSingleSource.heightSource" min="13" max="150" />
              <small :class="{ 'field-error': singleSourceValidation.heightSource }">
                {{ singleSourceValidation.heightSource || 'Допустимое значение: 13-150' }}
              </small>
            </label>

            <label class="single-source-field">
              <span>Диаметр устья источника, м</span>
              <input type="number" v-model.number="newSingleSource.diameterSource" min="1" max="10" />
              <small :class="{ 'field-error': singleSourceValidation.diameterSource }">
                {{ singleSourceValidation.diameterSource || 'Допустимое значение: 1-10' }}
              </small>
            </label>
          </div>

          <div class="single-source-modal__section">
            <h3>Коэффициенты</h3>

            <label class="single-source-field">
              <span>Коэффициент региона</span>

              <select v-model.number="newSingleSource.tempStratificationRatio">
                <option :value="null" disabled>
                  Выберите регион
                </option>

                <option :value="140">
                  Южная часть — 140
                </option>

                <option :value="160">
                  Средняя зона — 160
                </option>

                <option :value="180">
                  Северная часть — 180
                </option>

                <option :value="200">
                  Центральные регионы — 200
                </option>

                <option :value="250">
                  Бурятия или Забайкальский край — 250
                </option>
              </select>

              <small v-if="singleSourceValidation.tempStratificationRatio" class="field-error">
                {{ singleSourceValidation.tempStratificationRatio }}
              </small>
            </label>

            <label class="single-source-field">
              <span>Коэффициент степени очистки</span>

              <select v-model.number="newSingleSource.sedimentationRateRatio">
                <option :value="null" disabled>
                  Выберите степень очистки
                </option>

                <option :value="1">
                  Высокая — свыше 90%
                </option>

                <option :value="2">
                  Средняя — от 75% до 90%
                </option>

                <option :value="3">
                  Низкая — менее 75% или отсутствует
                </option>
              </select>

              <small v-if="singleSourceValidation.sedimentationRateRatio" class="field-error">
                {{ singleSourceValidation.sedimentationRateRatio }}
              </small>
            </label>
          </div>

          <div class="single-source-location">
            <span class="single-source-location__icon">📍</span>

            <div>
              <strong>Местоположение выбрано</strong>
              <p>
                {{ newSingleSource.location?.coordinates?.[1]?.toFixed(5) }},
                {{ newSingleSource.location?.coordinates?.[0]?.toFixed(5) }}
              </p>
            </div>
          </div>

          <div v-if="singleSourceError" class="single-source-error">
            {{ singleSourceError }}
          </div>

        </div>

        <div class="single-source-modal__footer">
          <button class="single-source-button single-source-button--cancel" @click="showSingleSourceForm = false"
            :disabled="savingSingleSource">
            Отмена
          </button>

          <button class="single-source-button single-source-button--submit" @click="submitSingleSource"
            :disabled="savingSingleSource || !isSingleSourceFormValid">
            {{ savingSingleSource ? 'Сохранение...' : 'Добавить источник' }}
          </button>
        </div>

      </div>
    </div>
  </Teleport>

</template>

<script setup>
import { onMounted, reactive, ref, computed } from 'vue'
import 'ol/ol.css'
import { defaults as defaultControls } from 'ol/control'
import Map from 'ol/Map'
import { fromLonLat, toLonLat } from 'ol/proj'
import View from 'ol/View'
import TileLayer from 'ol/layer/Tile'
import OSM from 'ol/source/OSM'

import VectorLayer from 'ol/layer/Vector'
import VectorSource from 'ol/source/Vector'
import { LineString, Point, Polygon, MultiPolygon } from 'ol/geom'
import Feature from 'ol/Feature'
import { Circle as CircleStyle, Fill, Icon, Stroke, Style, Text } from 'ol/style'
import Modify from 'ol/interaction/Modify';
import Draw from 'ol/interaction/Draw';
import { getArea } from 'ol/sphere';

import {
  calculateSingleDangerZones, calculateSingleDangerZone, calculateVehicleFlowDangerZones,
  calculateTrafficLightQueueDangerZones
} from '../api/dangerZone.js';
import { calculateTileGrid, calculateTileArea } from '../api/grid.js';
import { getAllEnterpriseSanitaryAreas } from '../api/enterprise.js';
import { getRecommendations } from '../api/recommendation.js';
import { getCurrentWeather } from '../api/weather.js';
import {
  addTrafficLightQueueEmissionSource,
  addVehicleFlowEmissionSource,
  getSingleEmissionSourceById, addSingleEmissionSource, updateVehicleFlowEmissionSource, getVehicleFlowEmissionSourceById
} from '../api/emissionSource.js';
import boilerIcon from '../icons/boiler.png';
import WeatherInfo from "../components/WeatherInfo.vue";
import SimulationPanel from '../components/SimulationPanel.vue'
import { asArray } from "ol/color";
import { altKeyOnly, singleClick } from 'ol/events/condition';
import { getCityById } from "../api/city.js";
import Overlay from 'ol/Overlay'
import XYZ from 'ol/source/XYZ.js';
import { useAreaMeasurement } from '../composables/useAreaMeasurement.js'
import { useTileGrid } from '../composables/useTileGrid.js'
import { useMapLayers } from '../composables/useMapLayers.js'
import { useBaseMap } from '../composables/useBaseMap'
import '../styles/map.css';

const mapRoot = ref(null)
const map = ref(null)

const {
  baseLayers,
  selectedBaseMap,
  changeBaseMap
} = useBaseMap(map)

const createModeSingle = ref(false);
const showSingleSourceForm = ref(false);

const newSingleSource = ref({
  location: null,
  ejectedTemp: 250,
  avgExitSpeed: 20,
  heightSource: 30,
  diameterSource: 3,
  tempStratificationRatio: null,
  sedimentationRateRatio: null,
  cityId: null
});

const savingSingleSource = ref(false);
const singleSourceError = ref('');

const singleSourceLimits = {
  ejectedTemp: {
    min: 235,
    max: 265,
    label: 'Температура выбрасываемых газов'
  },
  avgExitSpeed: {
    min: 15,
    max: 30,
    label: 'Средняя скорость выхода'
  },
  heightSource: {
    min: 13,
    max: 150,
    label: 'Высота источника'
  },
  diameterSource: {
    min: 1,
    max: 10,
    label: 'Диаметр устья источника'
  }
};

const singleSourceValidation = computed(() => {
  const errors = {};

  Object.entries(singleSourceLimits).forEach(([key, limit]) => {
    const value = Number(newSingleSource.value[key]);

    if (
      newSingleSource.value[key] === null ||
      newSingleSource.value[key] === '' ||
      !Number.isFinite(value)
    ) {
      errors[key] = 'Заполните поле';
    } else if (value < limit.min || value > limit.max) {
      errors[key] = `Допустимое значение: ${limit.min}–${limit.max}`;
    }
  });

  if (newSingleSource.value.tempStratificationRatio == null) {
    errors.tempStratificationRatio = 'Выберите коэффициент региона';
  }

  if (newSingleSource.value.sedimentationRateRatio == null) {
    errors.sedimentationRateRatio = 'Выберите степень очистки';
  }

  return errors;
});

const isSingleSourceFormValid = computed(() => {
  return Object.keys(singleSourceValidation.value).length === 0
    && !!newSingleSource.value.location
    && !!newSingleSource.value.cityId;
});

function startCreateModeSingle() {
  createModeSingle.value = true;
  createModeQueue.value = false;
  createModeFlow.value = false;

  singleSourceError.value = '';
}

function cancelCreateModeSingle() {
  createModeSingle.value = false;
}

async function submitSingleSource() {
  singleSourceError.value = '';

  const payload = {
    location: newSingleSource.value.location,
    ejectedTemp: Number(newSingleSource.value.ejectedTemp),
    avgExitSpeed: Number(newSingleSource.value.avgExitSpeed),
    heightSource: Number(newSingleSource.value.heightSource),
    diameterSource: Number(newSingleSource.value.diameterSource),
    tempStratificationRatio:
      newSingleSource.value.tempStratificationRatio,
    sedimentationRateRatio:
      newSingleSource.value.sedimentationRateRatio,
    cityId: Number(newSingleSource.value.cityId)
  };

  if (!payload.location) {
    singleSourceError.value = 'Выберите точку на карте';
    return;
  }

  if (
    payload.tempStratificationRatio == null ||
    payload.sedimentationRateRatio == null
  ) {
    singleSourceError.value = 'Выберите оба коэффициента';
    return;
  }

  savingSingleSource.value = true;

  try {
    await addSingleEmissionSource(payload);

    showSingleSourceForm.value = false;

    latestSingleDangerZones = await updateSingleLayer();

    await preloadTileGrids(
      latestSingleDangerZones,
      latestVehicleFlowDangerZones,
      latestVehicleQueueDangerZones,
      true
    );

    await refreshTileGrid();

    newSingleSource.value = {
      location: null,
      ejectedTemp: 250,
      avgExitSpeed: 20,
      heightSource: 30,
      diameterSource: 3,
      tempStratificationRatio: null,
      sedimentationRateRatio: null,
      cityId: null
    };

  } catch (error) {
    console.error('Ошибка добавления источника:', error);
    singleSourceError.value = error.message || 'Не удалось добавить источник';
  } finally {
    savingSingleSource.value = false;
  }
}

const {
  isMeasuringArea,
  measuredArea,
  measureLayer,
  startAreaMeasurement,
  stopAreaMeasurement,
  clearAreaMeasurement
} = useAreaMeasurement(map)

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
const currentPopupData = ref(null)
const userPosition = ref(null);
const currentRecommendation = ref(null)

const selectedGridLayer = ref('tile')

const tileSize = ref(750);
const autoTileSizeByZoom = ref(true);

const olLayers = reactive({
  single: null,
  vehicleFlow: null,
  vehicleQueue: null,
  sanitaryArea: null,
  tileGrid: null,
  tile: null,
  area: null,
})

const layersState = reactive({
  single: {
    visible: false,
    zIndex: 2
  },
  vehicleFlow: {
    visible: false,
    zIndex: 1
  },
  vehicleQueue: {
    visible: false,
    zIndex: 3
  },
  sanitaryArea: {
    visible: false,
    zIndex: 5
  },
  tileGrid: {
    visible: false,
    zIndex: 4
  },
  tile: {
    visible: false
  },
  area: {
    visible: false
  }
});

const levels = [
  { max: 9.0, color: 'rgba(171, 209, 98, 1)', label: 'Очень низкий' },
  { max: 35.4, color: 'rgba(248, 212, 97, 1)', label: 'Низкий' },
  { max: 55.4, color: 'rgba(251, 153, 86, 1)', label: 'Средний' },
  { max: 125.4, color: 'rgba(246, 104, 106, 1)', label: 'Высокий' },
  { max: 225.4, color: 'rgba(164, 125, 184, 1)', label: 'Очень высокий' },
  { max: 9999, color: 'rgba(138, 79, 163, 1)', label: 'Экстремальный' }
]

const cities = ref([
  { id: 1, name: 'Кемерово' },
  { id: 2, name: 'Новокузнецк' },
  { id: 3, name: 'Прокопьевск' },
  { id: 4, name: 'Киселевск' },
])

const selectedCities = ref(JSON.parse(localStorage.getItem("selectedCities") || 'null') ?? [cities.value[0]])

const {
  tileGridCache,
  currentTileSize,
  preloadTileGrids,
  getTileSizeForZoom,
  applyTileSize,
  onAutoTileSizeChange,
  refreshTileGridByZoom,
  refreshTileGrid,
  updateTileGridLayer
} = useTileGrid({
  map,
  selectedCities,
  olLayers,
  tileSize,
  autoTileSizeByZoom,
  withCache,
  geoJsonToOlGeometry
})

const {
  createSingleLayer,
  createVehicleFlowLayer,
  createVehicleQueueLayer,
  createSanitaryAreaLayer
} = useMapLayers({
  fromLonLat,
  createEllipse,
  getColorWithAlpha,
  getHatchPattern,
  boilerIcon
})

const cityDropdownOpen = ref(false)
const editPanelOpen = ref(false)

const updateLayerZIndex = (layerName) => {
  let layer;

  if (layerName === 'tileGrid') {
    layer = olLayers[selectedGridLayer.value];
  } else {
    layer = olLayers[layerName];
  }

  if (!layer) {
    console.warn('Слой не найден:', layerName);
    return;
  }

  const zIndex = Number(layersState[layerName].zIndex);

  if (!Number.isFinite(zIndex)) return;

  layer.setZIndex(zIndex);

  saveLayersState();
};

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

function showPopup(coordinate, feature, sourceType) {
  currentPopupData.value = feature.get('dangerData')
  currentPopupData.value.sourceType = sourceType

  const popupElement = createPopupElement()

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

function hidePopup() {
  if (singlePopup.value) {
    map.value.removeOverlay(singlePopup.value)
    singlePopup.value = null
  }
  currentPopupData.value = null
}

function createPopupElement() {
  const popup = document.createElement('div')
  popup.className = 'popup-element'

  const coalConsumptionHtml =
    currentPopupData.value.coalConsumption != null
      ? `
        <div class="average-concentration" style="margin-top: 12px;">
          <strong><label>Предполагаемый расход угля: </label></strong>
          <span>${currentPopupData.value.coalConsumption.toFixed(2)} тонн/час</span>
        </div>
      `
      : ''

  const wateringHtml =
    currentPopupData.value.chemicalDustVolume != null && currentPopupData.value.waterVolume != null
      ? `
          <div style="margin-top: 12px;">
            <strong>
              <label style="color: #0D9488;">Требуется для полива:</label>
            </strong>

            <div class="average-concentration">
              <strong><label>Объем хим. пылеподавителя: </label></strong>
              <span>${currentPopupData.value.chemicalDustVolume.toFixed(2)} кг</span>
            </div>

            <div class="average-concentration">
              <strong><label>Объем воды: </label></strong>
              <span>${currentPopupData.value.waterVolume.toFixed(2)} л</span>
            </div>
          </div>
        `
      : ''

  popup.innerHTML = `
    <div class="popup-content">
      <div class="popup-text">
        <div class="source-type">
          <strong><label>Тип источника: </label></strong>
          <span>${currentPopupData.value.sourceType}</span>
        </div>

        <div class="average-concentration">
          <strong><label>Концентрация: </label></strong>
          <span>${currentPopupData.value.averageConcentration.toFixed(2)} мкг/м3</span>
        </div>

        <div class="legend-item">
          <strong><label>Уровень загрязнения -&nbsp;</label></strong>
          <span>${currentPopupData.value.pollutionLevel}&nbsp;</span>
          <span
            class="legend-color"
            style="background-color: ${currentPopupData.value.color}"
          ></span>
        </div>

        ${coalConsumptionHtml}

        ${wateringHtml}
      </div>
    </div>
  `

  return popup
}

async function patchSingleZoneForSource(data) {
  const source = olLayers.single?.getSource();
  if (!source) return;

  const feature = source.getFeatures().find(
    f =>
      f.get('emissionSourceId') === data.emissionSourceId &&
      f.getGeometry().getType() === 'Polygon'
  );

  if (!feature) return;

  const updatedZone = feature.get('dangerData');
  if (!updatedZone) return;

  const idx = latestSingleDangerZones.findIndex(
    z => z.emissionSourceId === data.emissionSourceId
  );

  if (idx >= 0) {
    latestSingleDangerZones[idx] = updatedZone;
  } else {
    latestSingleDangerZones.push(updatedZone);
  }
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

const previousSimulationData = ref(null);

async function handleBuildSimulation(data) {
  const previous = previousSimulationData.value;
  previousSimulationData.value = { ...data };

  if (!previous) {
    await buildSimulation(data);
    return;
  }

  const windChanged =
    Number(data.windSpeed) !== Number(previous.windSpeed) ||
    Number(data.windDirection) !== Number(previous.windDirection);

  const sourceChanged =
    Number(data.ejectedTemp) !== Number(previous.ejectedTemp) ||
    Number(data.airTemp) !== Number(previous.airTemp) ||
    Number(data.avgExitSpeed) !== Number(previous.avgExitSpeed) ||
    Number(data.heightSource) !== Number(previous.heightSource) ||
    Number(data.diameterSource) !== Number(previous.diameterSource) ||
    Number(data.tempStratificationRatio) !== Number(previous.tempStratificationRatio) ||
    Number(data.sedimentationRateRatio) !== Number(previous.sedimentationRateRatio);

  if (windChanged || sourceChanged) {
    if (windChanged) {
      weather.value = {
        ...weather.value,
        windSpeed: Number(data.windSpeed),
        windDirection: Number(data.windDirection),
      };

      latestSingleDangerZones = await updateSingleLayer();
      await preloadTileGrids(
        latestSingleDangerZones,
        latestVehicleFlowDangerZones,
        latestVehicleQueueDangerZones,
        true
      );

      await refreshTileGrid();
      await buildSimulation(data);
      return;
    }

    await buildSimulation(data);
    await patchSingleZoneForSource(data);

    await preloadTileGrids(
      latestSingleDangerZones,
      latestVehicleFlowDangerZones,
      latestVehicleQueueDangerZones,
      true
    );
    await refreshTileGrid();
    return;
  }

  await buildSimulation(data);
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

  const ellipse = await createEllipse(dangerZone);
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

  singleDangerZones.forEach(async dangerZone => {
    const ellipse = await createEllipse(dangerZone);
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

let latestSingleDangerZones = [];
let latestVehicleFlowDangerZones = [];
let latestVehicleQueueDangerZones = [];

async function updateLayers() {
  const singleDangerZones = await updateSingleLayer();
  const vehicleFlowDangerZones = await updateVehicleFlowLayer();
  const vehicleQueueDangerZones = await updateVehicleQueueLayer();

  latestSingleDangerZones = singleDangerZones;
  latestVehicleFlowDangerZones = vehicleFlowDangerZones;
  latestVehicleQueueDangerZones = vehicleQueueDangerZones;

  await preloadTileGrids(
    singleDangerZones,
    vehicleFlowDangerZones,
    vehicleQueueDangerZones
  );

  await refreshTileGridByZoom();
}

const weatherBeforeSimulation = ref(null);

async function closeSimulationPanel() {
  if (weatherBeforeSimulation.value) {
    weather.value = { ...weatherBeforeSimulation.value };
  }

  const singleDangerZones = await updateSingleLayer();

  latestSingleDangerZones = singleDangerZones;

  await preloadTileGrids(
    latestSingleDangerZones,
    latestVehicleFlowDangerZones,
    latestVehicleQueueDangerZones
  );
  await refreshTileGrid();

  showSimulationPanel.value = false;
  simulationStartData.value = null;
  simulationData.value = null;
  previousSimulationData.value = null;
  weatherBeforeSimulation.value = null;
}

function geoJsonToOlGeometry(geometry) {
  if (!geometry || !geometry.type) return null

  switch (geometry.type) {
    case 'Polygon': {
      const rings = geometry.coordinates.map(ring =>
        ring.map(([lon, lat]) => fromLonLat([lon, lat]))
      )
      return new Polygon(rings)
    }

    case 'MultiPolygon': {
      const polys = geometry.coordinates.map(polygon =>
        polygon.map(ring =>
          ring.map(([lon, lat]) => fromLonLat([lon, lat]))
        )
      )
      return new MultiPolygon(polys)
    }

    case 'GeometryCollection': {
      const parts = geometry.geometries
        .map(geoJsonToOlGeometry)
        .filter(Boolean)
      return parts.length ? new MultiPolygon(parts.flatMap(toPolygons)) : null
    }

    default:
      console.warn('Неизвестный тип геометрии тайла:', geometry.type)
      return null
  }
}

function toPolygons(geom) {
  if (geom instanceof Polygon) return [geom.getCoordinates()]
  if (geom instanceof MultiPolygon) return geom.getCoordinates()
  return []
}

async function createEllipse(dangerZone) {
  const polygonCoords = dangerZone.polygon.coordinates[0];
  const coords = polygonCoords.map(coord =>
    fromLonLat(coord)
  );

  const ellipseFeature = new Feature({
    geometry: new Polygon([coords])
  });

  const sourceData = await getSingleEmissionSourceById(dangerZone.emissionSourceId);

  const s = Math.PI * Math.pow(sourceData.diameterSource, 2) / 4;
  const v = sourceData.avgExitSpeed * s * 3600;

  const coalConsumption = (v / 11) * (273 / (273 + sourceData.ejectedTemp));

  dangerZone.coalConsumption = coalConsumption / 1000;

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

function createTileGridLayer(tileGridResult) {
  const tileGridSource = new VectorSource();

  const allTiles = tileGridResult.flatMap(city => city.tiles);

  allTiles.forEach(tileInfo => {
    const geometry = geoJsonToOlGeometry(tileInfo.tile)
    if (!geometry) return

    const feature = new Feature({
      geometry,
      color: tileInfo.color,
      cityId: tileInfo.cityId
    })

    feature.set('dangerData', tileInfo)
    tileGridSource.addFeature(feature)
  })

  return new VectorLayer({
    source: tileGridSource,
    visible: false,
    zIndex: 3,
    style: feature => {
      const color = getColorWithAlpha(feature.get('color'), 0.6)
      return new Style({
        fill: new Fill({ color }),
        stroke: new Stroke({
          color: 'black',
          width: 0.1
        })
      })
    }
  })
}

function createLayers(singleDangerZones, vehicleFlowDangerZones, vehicleQueueDangerZones, sanitaryAreas, tileGridResult, areaGridResult) {
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

  latestSingleDangerZones = singleDangerZones;
  latestVehicleFlowDangerZones = vehicleFlowDangerZones;
  latestVehicleQueueDangerZones = vehicleQueueDangerZones;

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

  //const userPos = await getUserPosition();

  /*const recommendationResult = await getRecommendations({
    tiles: tileGridResult.flatMap(result => result.tiles),
    userLocation: userPosition.value
  });

  currentRecommendation.value = {
    recommendationLevel: recommendationResult.recommendationLevel,
    recommendationText: recommendationResult.recommendationText
  };*/

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

    if (createModeSingle.value) {
      const [lon, lat] = toLonLat(evt.coordinate);

      newSingleSource.value.location = {
        type: 'Point',
        coordinates: [lon, lat]
      };

      newSingleSource.value.cityId =
        selectedCities.value.length > 0
          ? selectedCities.value[0].id
          : null;

      createModeSingle.value = false;

      showSingleSourceForm.value = true;

      return;
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

    if (isMeasuringArea.value) {
      return
    }

    if (showSimulationPanel.value && found == null) {
      await closeSimulationPanel();
    } else if (found != null) {
      if (simulationStartData.value != null) {
        await closeSimulationPanel();
      }

      showSimulationPanel.value = true;

      weatherBeforeSimulation.value = { ...weather.value };

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
    if (evt.dragging || isMeasuringArea.value) return

    const pixel = map.value.getEventPixel(evt.originalEvent)
    let popupFeature = null
    let sourceType = null

    map.value.forEachFeatureAtPixel(pixel, (feature, layer) => {
      if (layer === olLayers.single ||
        layer === olLayers.vehicleFlow ||
        layer === olLayers.vehicleQueue /*|| 
          layer === olLayers.tile ||
          layer === olLayers.area*/) {

        popupFeature = feature
        sourceType = layer === olLayers.single ? 'котельная' : layer === olLayers.vehicleFlow ? 'дорога' : 'перекресток'
        return false
      }
    })

    if (popupFeature) {
      const coordinate = evt.coordinate
      showPopup(coordinate, popupFeature, sourceType)

      const mapElement = map.value.getTargetElement()
      mapElement.style.cursor = 'pointer'
    } else {
      hidePopup()
    }

    const allHit = map.value.hasFeatureAtPixel(pixel)
    const mapElement = map.value.getTargetElement()
    mapElement.style.cursor = allHit ? 'pointer' : ''
  })

  await updateModifyFlow();
})

async function withCache(key, fn, ttl = 5 * 60 * 1000) {
  const cached = localStorage.getItem(key);

  if (cached) {
    try {
      const parsed = JSON.parse(cached);
      const isExpired = Date.now() - parsed.timestamp > ttl;

      if (!isExpired) {
        return parsed.data;
      }
    } catch (e) {
      console.error('Ошибка парсинга кеша', e);
    }
  }

  const data = await fn();

  localStorage.setItem(
    key,
    JSON.stringify({
      data,
      timestamp: Date.now()
    })
  );

  return data;
}

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
        layersState[key].visible =
          parsedState[key].visible ?? layersState[key].visible;

        if (layersState[key].zIndex !== undefined) {
          layersState[key].zIndex =
            parsedState[key].zIndex ?? layersState[key].zIndex;
        }
      }
    });
  }

  selectedGridLayer.value =
    localStorage.getItem('selectedGridLayer') || 'tile';

  Object.keys(olLayers).forEach(key => {
    if (olLayers[key]) {
      olLayers[key].setVisible(layersState[key].visible);
    }
  });

  updateLayerZIndex('single');
  updateLayerZIndex('vehicleFlow');
  updateLayerZIndex('vehicleQueue');
  updateLayerZIndex('sanitaryArea');
  updateLayerZIndex('tileGrid');
};

const toggleLayer = key => {
  if (key === 'tile' || key === 'area') {
    const oppositeKey = key === 'tile' ? 'area' : 'tile';
    layersState[oppositeKey].visible = false;
    olLayers[oppositeKey].setVisible(false);

    layersState[key].visible = true;
    olLayers[key].setVisible(true);
    olLayers[key].setZIndex(Number(layersState.tileGrid.zIndex));
  } else if (key === 'tileGrid') {
    const visible = layersState[key].visible;

    const oppositeKey = selectedGridLayer.value === 'tile' ? 'area' : 'tile';

    layersState[selectedGridLayer.value].visible = visible;
    olLayers[selectedGridLayer.value].setVisible(visible);

    layersState[oppositeKey].visible = false;
    olLayers[oppositeKey].setVisible(false);
  }
  else {
    const visible = layersState[key].visible;
    olLayers[key].setVisible(visible);
  }

  saveLayersState();

  if (key === 'vehicleFlow' && modifyFlow.value) {
    modifyFlow.value.setActive(visible);
  }
};

const saveLayersState = () => {
  const stateToSave = {};

  Object.keys(layersState).forEach(key => {
    stateToSave[key] = {
      visible: layersState[key].visible,
      zIndex: layersState[key].zIndex
    };
  });

  localStorage.setItem('layersState', JSON.stringify(stateToSave));

  localStorage.setItem('selectedGridLayer', selectedGridLayer.value);
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