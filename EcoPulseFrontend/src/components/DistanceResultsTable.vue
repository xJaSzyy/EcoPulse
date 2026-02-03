<template>
  <div class="result-section">
    <h3>Результаты расчета выбросов</h3>

    <!-- Информация о загрязняющем веществе -->
    <div class="pollutant-info" v-if="data.pollutantInfo">
      <h4>{{ data.pollutantInfo.name }}</h4>
      <div class="pollutant-details">
        <span v-if="data.pollutantInfo.mass">Масса: {{ data.pollutantInfo.mass }}</span>
      </div>
    </div>

    <div class="emissions-summary">
      <div class="summary-card">
        <div class="summary-icon">⚡</div>
        <div class="summary-value">{{ totalMaximumEmission.toFixed(6) }}</div>
        <div class="summary-label">Суммарный макс. выброс (г/час)</div>
      </div>

      <!-- Карточка с ПДК -->
      <div class="summary-card">
        <div class="summary-icon">📏</div>
        <div class="summary-value">{{ data.pollutantInfo?.maxPermissibleConcentration || '—' }}</div>
        <div class="summary-label">ПДК (мг/м³)</div>
      </div>
    </div>

    <!-- Таблица выбросов по расстояниям -->
    <div class="pollutants-table">
      <table>
        <thead>
        <tr>
          <th>Расстояние (м)</th>
          <th>Макс. выброс (г/час)</th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="(item, index) in data.emissions" :key="index">
          <td class="distance-cell">{{ item.distance }} м</td>
          <td class="emission-value" :class="getEmissionClass(item)">
            {{ item.maximumEmission?.toFixed(6) || '—' }}
          </td>
        </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  data: {
    type: Object,
    required: true,
    default: () => ({
      pollutantInfo: null,
      emissions: []
    })
  }
})

// Вычисляемые свойства для суммарных показателей
const totalMaximumEmission = computed(() => {
  if (!props.data.emissions || props.data.emissions.length === 0) return 0
  return props.data.emissions.reduce((sum, item) => sum + (item.maximumEmission || 0), 0)
})

// Проверка превышения ПДК
const isPdKExceeded = (item) => {
  if (!props.data.pollutantInfo?.maxPermissibleConcentration) return false
  // Конвертируем г/час в мг/м³ (предполагаем стандартные условия)
  // Если нужна точная конвертация, уточните формулу
  const concentration = item.maximumEmission * 1000 // упрощенная конвертация
  return concentration > props.data.pollutantInfo.maxPermissibleConcentration
}

// Класс для ячейки с выбросом
const getEmissionClass = (item) => {
  return isPdKExceeded(item) ? 'exceeded-emission' : 'normal-emission'
}

// Класс для статуса
const getStatusClass = (item) => {
  return isPdKExceeded(item) ? 'status-exceeded' : 'status-normal'
}

// Текст статуса
const getStatusText = (item) => {
  return isPdKExceeded(item) ? 'Превышение' : 'Норма'
}
</script>

<style scoped>
/* Стили для информации о загрязняющем веществе */
.pollutant-info {
  background: white;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.pollutant-info h4 {
  margin: 0 0 10px 0;
  color: #2c3e50;
  font-size: 18px;
}

.pollutant-details {
  display: flex;
  gap: 20px;
  font-size: 14px;
  color: #7f8c8d;
  flex-wrap: wrap;
}

/* Стили для выбросов */
.exceeded-emission {
  color: #e74c3c;
  font-weight: bold;
}

.normal-emission {
  color: #27ae60;
  font-weight: bold;
}

/* Стили для статуса */
.status-cell {
  text-align: center;
}

.status-indicator {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.status-exceeded {
  background-color: #e74c3c;
  color: white;
}

.status-normal {
  background-color: #27ae60;
  color: white;
}

/* Остальные стили остаются прежними */
.distance-cell {
  font-weight: 600;
  color: #2c3e50;
  text-align: center;
}

.emission-value {
  text-align: right;
  font-family: 'Courier New', monospace;
  font-weight: 500;
}

.result-section {
  margin-top: 30px;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
}

.result-section h3 {
  margin-bottom: 20px;
  color: #2c3e50;
  text-align: center;
}

.emissions-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.summary-icon {
  font-size: 32px;
  margin-bottom: 8px;
}

.summary-card {
  background: white;
  padding: 20px;
  border-radius: 8px;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.summary-value {
  font-size: 24px;
  font-weight: bold;
  color: #f39c12;
  margin-bottom: 8px;
}

.summary-label {
  font-size: 14px;
  color: #7f8c8d;
}

.pollutants-table {
  overflow-x: auto;
}

.pollutants-table th,
.pollutants-table td {
  text-align: center;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

th {
  background: #34495e;
  color: white;
  padding: 12px 8px;
  font-weight: 600;
  font-size: 14px;
}

td {
  padding: 12px 8px;
  border-bottom: 1px solid #ecf0f1;
  font-size: 14px;
}

tbody tr:hover {
  background: #f8f9fa;
}

@media (max-width: 768px) {
  .emissions-summary {
    grid-template-columns: 1fr;
  }

  .pollutant-details {
    flex-direction: column;
    gap: 5px;
  }

  th, td {
    padding: 8px 4px;
    font-size: 12px;
  }

  .status-indicator {
    font-size: 10px;
    padding: 2px 6px;
  }
}
</style>