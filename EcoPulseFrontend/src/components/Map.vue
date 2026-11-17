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

    <div v-if="createModeFlow" class="map-pick-notification">
      <div class="map-pick-notification__icon">🛣️</div>

      <div class="map-pick-notification__content">
        <strong>Добавление дороги</strong>

        <span>
          Нажимайте на карту, чтобы добавлять точки маршрута.
          Дважды щёлкните, чтобы завершить рисование.
        </span>
      </div>

      <button class="map-pick-notification__cancel" @click="cancelCreateModeFlow">
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

      <div v-if="!layersState.tileGrid.visible" class="area-measure-panel">
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

      <div v-if="!layersState.tileGrid.visible" class="edit-tool-panel">

        <button class="add-source-btn" @click="startCreateModeSingle">
          <span>Добавить котельную</span>
        </button>

        <button class="add-source-btn" @click="startCreateModeFlow">
          <span>Добавить дорогу</span>
        </button>

        <button class="delete-source-btn" @click="toggleDeleteMode">
          <span>{{ isDeleteMode ? 'Отменить удаление' : 'Удалить объект' }}</span>
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
import { onMounted, reactive, ref, computed, toRaw } from 'vue'
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
import Select from 'ol/interaction/Select'
import { click } from 'ol/events/condition'

import {
  calculateSingleDangerZones, calculateSingleDangerZone, calculateVehicleFlowDangerZones,
  calculateTrafficLightQueueDangerZones
} from '../api/dangerZone.js';
import { calculateTileGrid, calculateTileArea } from '../api/grid.js';
import { getAllEnterpriseSanitaryAreas } from '../api/enterprise.js';
import { getRecommendations } from '../api/recommendation.js';
import { getCurrentWeather } from '../api/weather.js';
import {
  addTrafficLightQueueEmissionSource, addVehicleFlowEmissionSource, getSingleEmissionSourceById,
  addSingleEmissionSource, updateVehicleFlowEmissionSource, getVehicleFlowEmissionSourceById,
  deleteSingleEmissionSource, deleteVehicleFlowEmissionSource, deleteTrafficLightQueueEmissionSource
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
import { useSingleEmissionSource } from '../composables/useSingleEmissionSource.js'
import { useDangerLayers } from '../composables/useDangerLayers.js'
import { useMapPopup } from '../composables/useMapPopup.js'
import { useLayerSettings } from '../composables/useLayerSettings.js'
import { useSimulation } from '../composables/useSimulation.js'
import '../styles/map.css';

const mapRoot = ref(null)
const map = ref(null)

const isDeleteMode = ref(false)
const selectInteraction = ref(null)
const selectedFeature = ref(null)
const deleting = ref(false)

function toggleDeleteMode() {
  if (isDeleteMode.value) {
    stopDeleteMode()
  } else {
    startDeleteMode()
  }
}

let deleteClickHandler = null

function startDeleteMode() {
  if (!map.value) {
    console.error('Карта ещё не инициализирована')
    return
  }

  cancelCreateModeSingle()
  cancelCreateModeFlow()
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

const {
  singlePopup,
  currentPopupData,
  showPopup,
  hidePopup
} = useMapPopup(map)

const {
  baseLayers,
  selectedBaseMap,
  changeBaseMap
} = useBaseMap(map)

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

const {
  createModeSingle,
  showSingleSourceForm,
  savingSingleSource,
  singleSourceError,
  newSingleSource,
  singleSourceValidation,
  isSingleSourceFormValid,
  setSingleSourceLocation,
  submitSingleSource
} = useSingleEmissionSource({
  afterSave: async () => {
    const singleDangerZones = await updateSingleLayer()

    await preloadTileGrids(
      singleDangerZones,
      latestDangerZones.vehicleFlow,
      latestDangerZones.vehicleQueue,
      true
    )

    await refreshTileGrid()
  }
})

const {
  isMeasuringArea,
  measuredArea,
  measureLayer,
  startAreaMeasurement,
  stopAreaMeasurement,
  clearAreaMeasurement
} = useAreaMeasurement(map)

const weather = ref(null)

const createModeFlow = ref(false)
const createModeQueue = ref(false)
const createPoints = ref([])
const modifyFlow = ref(null);
const showInfo = ref(false);
const streetName = ref(null);

const userPosition = ref(null);
const currentRecommendation = ref(null)

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

const {
  layersState,
  selectedGridLayer,
  initLayersState,
  toggleLayer,
  updateLayerZIndex,
  saveLayersState
} = useLayerSettings({
  olLayers,
  modifyFlow
})

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

const {
  updateSingleLayer,
  updateVehicleFlowLayer,
  updateVehicleQueueLayer,
  updateLayers,
  latestDangerZones,
  setLatestDangerZones
} = useDangerLayers({
  olLayers,
  selectedCities,
  weather,
  createEllipse
})

const {
  showSimulationPanel,
  simulationStartData,
  simulationData,
  previousSimulationData,
  weatherBeforeSimulation,
  handleBuildSimulation,
  buildSimulation,
  closeSimulationPanel,
  openSimulationPanel
} = useSimulation({
  map,
  olLayers,
  weather,
  updateSingleLayer,
  preloadTileGrids,
  refreshTileGrid,
  latestDangerZones,
  createEllipse
})

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

function startCreateModeFlow() {
  if (!map.value) return

  cancelCreateModeSingle()
  cancelCreateModeFlow()

  createModeQueue.value = false
  createModeFlow.value = true

  const drawSource = new VectorSource()

  const drawInteraction = new Draw({
    source: drawSource,
    type: 'LineString'
  })

  map.value.addInteraction(drawInteraction)

  drawInteraction.on('drawend', async (event) => {
    const coordinates = event.feature
      .getGeometry()
      .getCoordinates()
      .map(coord => toLonLat(coord))

    createModeFlow.value = false

    removeAllDrawInteractions()

    try {
      await saveVehicleFlow(coordinates)
    } catch (error) {
      console.error('Ошибка добавления дороги:', error)
    }
  })
}

function cancelCreateModeFlow() {
  removeAllDrawInteractions()

  createModeFlow.value = false
  createPoints.value = []
}

function startCreateModeQueue() {
  createModeQueue.value = true
}

async function saveVehicleFlow(coordinates) {
  const selectedCityId = selectedCities.value.length > 0
    ? selectedCities.value[0].id
    : null;

  await addVehicleFlowEmissionSource({
    cityId: selectedCityId,
    points: {
      type: "LineString",
      coordinates
    },
    vehicleType: 1,
    maxTrafficIntensity: 35 / 2,
    averageSpeed: 40 / 2,
    streetName: "-"
  });

  const cityIds = selectedCities.value.map(c => c.id);
  const cacheKey = `vehicleFlow_${cityIds.join('_')}`;

  localStorage.removeItem(cacheKey);

  await updateVehicleFlowLayer();
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

function randomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

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

</script>