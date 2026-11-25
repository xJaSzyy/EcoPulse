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

    <div v-if="createModeQueue" class="map-pick-notification">
      <div class="map-pick-notification__icon">🚦</div>

      <div class="map-pick-notification__content">
        <strong>Добавление перекрестка</strong>

        <span>Выберите точку на карте, откроется форма для заполнения параметров.</span>
      </div>

      <button class="map-pick-notification__cancel" @click="cancelCreateModeQueue">
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

        <button class="add-source-btn" @click="startCreateModeQueue">
          <span>Добавить перекресток</span>
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

    <div v-if="showVehicleFlowForm" class="single-source-overlay">
      <div class="single-source-modal">

        <div class="single-source-modal__header">
          <div>
            <h2>Новая дорога</h2>
            <p>Укажите параметры транспортного потока</p>
          </div>
        </div>

        <div class="single-source-modal__body">

          <div class="single-source-field">
            <span>Город</span>

            <div v-if="loadingCities" class="form-hint">
              Загрузка городов...
            </div>

            <div v-else-if="citiesError" class="field-error">
              {{ citiesError }}

              <button type="button" @click="loadCities">
                Повторить
              </button>
            </div>

            <select v-else v-model.number="newVehicleFlow.cityId">
              <option :value="null" disabled>
                Выберите город
              </option>

              <option v-for="city in vehicleFlowCities" :key="city.id" :value="city.id">
                {{ city.name }}
              </option>
            </select>

            <small v-if="vehicleFlowValidation.cityId" class="field-error">
              {{ vehicleFlowValidation.cityId }}
            </small>
          </div>

          <label class="single-source-field">
            <span>Тип транспортного средства</span>

            <select v-model.number="newVehicleFlow.vehicleType">
              <option v-for="type in vehicleTypes" :key="type.value" :value="type.value">
                {{ type.label }}
              </option>
            </select>

            <small v-if="vehicleFlowValidation.vehicleType" class="field-error">
              {{ vehicleFlowValidation.vehicleType }}
            </small>
          </label>

          <label class="single-source-field">
            <span>Название улицы</span>

            <input type="text" v-model="newVehicleFlow.streetName" placeholder="Например, проспект Ленина"
              maxlength="150" />

            <small v-if="vehicleFlowValidation.streetName" class="field-error">
              {{ vehicleFlowValidation.streetName }}
            </small>
          </label>

          <div class="single-source-location">
            <span class="single-source-location__icon">🛣️</span>

            <div>
              <strong>Маршрут выбран</strong>
              <p>
                Количество точек:
                {{ newVehicleFlow.points?.coordinates?.length || 0 }}
              </p>
            </div>
          </div>

          <div v-if="vehicleFlowError" class="single-source-error">
            {{ vehicleFlowError }}
          </div>

        </div>

        <div class="single-source-modal__footer">

          <button class="single-source-button single-source-button--cancel" @click="closeVehicleFlowForm"
            :disabled="savingVehicleFlow">
            Отмена
          </button>

          <button class="single-source-button single-source-button--submit" @click="submitVehicleFlow"
            :disabled="savingVehicleFlow || !isVehicleFlowFormValid">
            {{ savingVehicleFlow ? 'Сохранение...' : 'Добавить дорогу' }}
          </button>

        </div>

      </div>
    </div>

    <div v-if="showTrafficLightQueueForm" class="single-source-overlay">
      <div class="single-source-modal">

        <div class="single-source-modal__header">
          <div>
            <h2>Новый перекресток</h2>
            <p>Укажите параметры регулируемого перекрестка</p>
          </div>
        </div>

        <div class="single-source-modal__body">

          <div class="single-source-field">
            <span>Город</span>

            <select v-model.number="newTrafficLightQueue.cityId">
              <option :value="null" disabled>
                Выберите город
              </option>
              <option v-for="city in cities" :key="city.id" :value="city.id">
                {{ city.name }}
              </option>
            </select>

            <small v-if="trafficLightQueueValidation.cityId" class="field-error">
              {{ trafficLightQueueValidation.cityId }}
            </small>
          </div>

          <label class="single-source-field">
            <span>Тип транспортного средства</span>

            <select v-model.number="newTrafficLightQueue.vehicleType">
              <option v-for="type in vehicleTypes" :key="type.value" :value="type.value">
                {{ type.label }}
              </option>
            </select>

            <small v-if="trafficLightQueueValidation.vehicleType" class="field-error">
              {{ trafficLightQueueValidation.vehicleType }}
            </small>
          </label>

          <label class="single-source-field">
            <span>Количество машин в очереди</span>

            <input type="number" v-model.number="newTrafficLightQueue.vehiclesCount" min="1" />

            <small :class="{ 'field-error': trafficLightQueueValidation.vehiclesCount }">
              {{ trafficLightQueueValidation.vehiclesCount || 'Допустимое значение: 1 и более' }}
            </small>
          </label>

          <label class="single-source-field">
            <span>Количество циклов за 20 минут</span>

            <input type="number" v-model.number="newTrafficLightQueue.trafficLightCycles" min="1" />

            <small :class="{ 'field-error': trafficLightQueueValidation.trafficLightCycles }">
              {{ trafficLightQueueValidation.trafficLightCycles || 'Допустимое значение: 1 и более' }}
            </small>
          </label>

          <label class="single-source-field">
            <span>Длительность запрещающего сигнала, с</span>

            <input type="number" v-model.number="newTrafficLightQueue.trafficLightStopTime" min="1" />

            <small :class="{ 'field-error': trafficLightQueueValidation.trafficLightStopTime }">
              {{ trafficLightQueueValidation.trafficLightStopTime || 'Допустимое значение: 1 и более' }}
            </small>
          </label>

          <div class="single-source-location">
            <span class="single-source-location__icon">🚦</span>

            <div>
              <strong>Местоположение выбрано</strong>
              <p>
                {{ newTrafficLightQueue.location?.coordinates?.[1]?.toFixed(5) }},
                {{ newTrafficLightQueue.location?.coordinates?.[0]?.toFixed(5) }}
              </p>
            </div>
          </div>

          <div v-if="trafficLightQueueError" class="single-source-error">
            {{ trafficLightQueueError }}
          </div>

        </div>

        <div class="single-source-modal__footer">
          <button class="single-source-button single-source-button--cancel"
            @click="closeTrafficLightQueueForm" :disabled="savingTrafficLightQueue">
            Отмена
          </button>

          <button class="single-source-button single-source-button--submit" @click="submitTrafficLightQueue"
            :disabled="savingTrafficLightQueue || !isTrafficLightQueueFormValid">
            {{ savingTrafficLightQueue ? 'Сохранение...' : 'Добавить перекресток' }}
          </button>
        </div>

      </div>
    </div>
  </Teleport>

</template>

<script setup>
import { reactive, ref } from 'vue'
import 'ol/ol.css'
import { fromLonLat } from 'ol/proj'

import boilerIcon from '../icons/boiler.png';
import WeatherInfo from "../components/WeatherInfo.vue";
import SimulationPanel from '../components/SimulationPanel.vue'

import { useAreaMeasurement } from '../composables/useAreaMeasurement.js'
import { useBaseMap } from '../composables/useBaseMap.js'
import { useDangerLayers } from '../composables/useDangerLayers.js'
import { useLayerSettings } from '../composables/useLayerSettings.js'
import { useMapCities } from '../composables/useMapCities.js'
import { useMapControls } from '../composables/useMapControls.js'
import { useMapCreateModes } from '../composables/useMapCreateModes.js'
import { useMapDelete } from '../composables/useMapDelete.js'
import { useMapHelpers } from '../composables/useMapHelpers.js'
import { useMapLayers } from '../composables/useMapLayers.js'
import { useMapPopup } from '../composables/useMapPopup.js'
import { useMapSetup } from '../composables/useMapSetup.js'
import { useSimulation } from '../composables/useSimulation.js'
import { useSingleEmissionSource } from '../composables/useSingleEmissionSource.js'
import { useTileGrid } from '../composables/useTileGrid.js'
import { useTrafficLightQueueSource } from '../composables/useTrafficLightQueueSource.js'
import { useVehicleFlowSource } from '../composables/useVehicleFlowSource.js'
import '../styles/map.css';

const mapRoot = ref(null)
const map = ref(null)

const olLayers = reactive({
  single: null,
  vehicleFlow: null,
  vehicleQueue: null,
  sanitaryArea: null,
  tileGrid: null,
  tile: null,
  area: null,
})

const weather = ref(null)

const tileSize = ref(750);
const autoTileSizeByZoom = ref(true);
const currentRecommendation = ref(null)
const modifyFlow = ref(null)
const showInfo = ref(false);
const levels = [
  { max: 9.0, color: 'rgba(171, 209, 98, 1)', label: 'Очень низкий' },
  { max: 35.4, color: 'rgba(248, 212, 97, 1)', label: 'Низкий' },
  { max: 55.4, color: 'rgba(251, 153, 86, 1)', label: 'Средний' },
  { max: 125.4, color: 'rgba(246, 104, 106, 1)', label: 'Высокий' },
  { max: 225.4, color: 'rgba(164, 125, 184, 1)', label: 'Очень высокий' },
  { max: 9999, color: 'rgba(138, 79, 163, 1)', label: 'Экстремальный' }
]

const { baseLayers, selectedBaseMap, changeBaseMap } = useBaseMap(map)

const {
  measureLayer,
  isMeasuringArea,
  startAreaMeasurement,
  stopAreaMeasurement,
  clearAreaMeasurement
} = useAreaMeasurement(map)

const { cities, selectedCities, loadMapCities } = useMapCities()

const {
  createEllipse,
  getColorWithAlpha,
  getHatchPattern,
  geoJsonToOlGeometry,
  withCache,
  createTileGridLayer
} = useMapHelpers()

const {
  layersState,
  selectedGridLayer,
  initLayersState,
  toggleLayer,
  updateLayerZIndex
} = useLayerSettings({ olLayers, modifyFlow })

const {
  tileGridCache,
  currentTileSize,
  preloadTileGrids,
  applyTileSize,
  onAutoTileSizeChange,
  refreshTileGridByZoom,
  refreshTileGrid
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

const {
  showPopup,
  hidePopup
} = useMapPopup(map)

let createModes = null

const {
  isDeleteMode,
  toggleDeleteMode,
  stopDeleteMode
} = useMapDelete({
  map,
  olLayers,
  selectedCities,
  updateLayers,
  preloadTileGrids,
  refreshTileGrid,
  cancelCreateModes: () => createModes
})

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
  showVehicleFlowForm,
  savingVehicleFlow,
  vehicleFlowError,
  newVehicleFlow,
  vehicleTypes,
  vehicleFlowValidation,
  isVehicleFlowFormValid,
  setVehicleFlowData,
  submitVehicleFlow,
  closeVehicleFlowForm,
  cities: vehicleFlowCities,
  loadingCities,
  citiesError
} = useVehicleFlowSource({
  afterSave: async () => {
    const vehicleFlowDangerZones = await updateVehicleFlowLayer()

    await preloadTileGrids(
      latestDangerZones.single,
      vehicleFlowDangerZones,
      latestDangerZones.vehicleQueue,
      true
    )

    await refreshTileGrid()
  }
})

const {
  showTrafficLightQueueForm,
  savingTrafficLightQueue,
  trafficLightQueueError,
  newTrafficLightQueue,
  trafficLightQueueValidation,
  isTrafficLightQueueFormValid,
  setTrafficLightQueueLocation,
  submitTrafficLightQueue,
  closeTrafficLightQueueForm
} = useTrafficLightQueueSource({
  afterSave: async () => {
    const vehicleQueueDangerZones = await updateVehicleQueueLayer()

    await preloadTileGrids(
      latestDangerZones.single,
      latestDangerZones.vehicleFlow,
      vehicleQueueDangerZones,
      true
    )

    await refreshTileGrid()
  }
})

createModes = useMapCreateModes({
  map,
  selectedCities,
  createModeSingle,
  setSingleSourceLocation,
  setVehicleFlowData,
  setTrafficLightQueueLocation,
  stopDeleteMode
})

const {
  createModeFlow,
  createModeQueue,
  cancelCreateModeSingle,
  cancelCreateModeFlow,
  cancelCreateModeQueue,
  startCreateModeSingle,
  startCreateModeFlow,
  startCreateModeQueue
} = createModes

const { cityDropdownOpen, toggleCityDropdown } = useMapControls({
  map,
  selectedCities,
  updateLayers,
  preloadTileGrids,
  refreshTileGrid
})

useMapSetup({
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
})
</script>
