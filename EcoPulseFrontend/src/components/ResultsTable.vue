<template>
  <div class="result-section">
    <h3>Результаты расчета выбросов</h3>

    <div class="emissions-summary">
      <div class="summary-card" v-if="hasGrossEmission">
        <div class="summary-icon">📊</div>
        <div class="summary-value">{{ totalGrossEmission.toFixed(6) }}</div>
        <div class="summary-label">Суммарный валовый выброс (г)</div>
      </div>

      <div class="summary-card">
        <div class="summary-icon">⚡</div>
        <div class="summary-value">{{ totalMaximumEmission.toFixed(6) }}</div>
        <div class="summary-label">Суммарный макс. выброс (г/час)</div>
      </div>
    </div>

    <div class="pollutants-table">
      <table>
        <thead>
        <tr>
          <th>Загрязняющее вещество</th>
          <th>Код</th>
          <th v-if="hasGrossEmission">Валовый выброс (г)</th>
          <th>Макс. выброс (г/час)</th>
          <th>ПДК (мг/м³)</th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="item in data" :key="item.pollutantInfo.code">
          <td class="pollutant-name">
            <div class="name-main">{{ item.pollutantInfo.name }}</div>
            <div v-if="item.pollutantInfo.shortName" class="name-short">
              {{ item.pollutantInfo.shortName }}
            </div>
          </td>
          <td class="code-cell">{{ item.pollutantInfo.code }}</td>
          <td v-if="hasGrossEmission" class="emission-value">{{ item.grossEmission?.toFixed(6) || '—' }}</td>
          <td class="emission-value">{{ item.maximumEmission?.toFixed(6) || '—' }}</td>
          <td v-if="item.pollutantInfo.maxPermissibleConcentration" class="concentration-value" :class="getConcentrationClass(item)">
            <div v-if="item.pollutantInfo.maxPermissibleConcentration">{{ item.pollutantInfo.maxPermissibleConcentration }}</div>
            <div v-if="item.pollutantInfo.dailyAverageConcentration" class="concentration-daily">
              (среднесуточная: {{ item.pollutantInfo.dailyAverageConcentration }})
            </div>
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
    type: Array,
    required: true,
    default: () => []
  }
})

// Проверяем, есть ли вообще данные по валовым выбросам
const hasGrossEmission = computed(() => {
  if (!props.data || props.data.length === 0) return false
  return props.data.some(item => item.grossEmission != null && item.grossEmission !== 0.0)
})

// Вычисляемые свойства для суммарных показателей
const totalGrossEmission = computed(() => {
  if (!props.data || props.data.length === 0) return 0
  return props.data.reduce((sum, item) => sum + (item.grossEmission || 0), 0)
})

const totalMaximumEmission = computed(() => {
  if (!props.data || props.data.length === 0) return 0
  return props.data.reduce((sum, item) => sum + (item.maximumEmission || 0), 0)
})

const getConcentrationClass = (item) => {
  if (item.maximumEmission * 1000 > item.pollutantInfo.maxPermissibleConcentration) {
    return 'exceeded';
  }

  return 'normal';
}
</script>

<style scoped>
/* Перенесите сюда все стили таблицы из основного компонента */
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

.pollutant-name {
  min-width: 200px;
}

.name-main {
  font-weight: 500;
  color: #2c3e50;
}

.name-short {
  font-size: 12px;
  color: #7f8c8d;
  margin-top: 4px;
}

.code-cell {
  font-family: 'Courier New', monospace;
  color: #7f8c8d;
}

.emission-value {
  font-family: 'Courier New', monospace;
  font-weight: 500;
}

.concentration-daily {
  font-size: 11px;
  color: #7f8c8d;
  margin-top: 2px;
}

.exceeded {
  color: #e74c3c;
  font-weight: bold;
}

.normal {
  color: #27ae60;
  font-weight: bold;
}

@media (max-width: 768px) {
  .emissions-summary {
    grid-template-columns: 1fr;
  }

  th, td {
    padding: 8px 4px;
    font-size: 12px;
  }

  .pollutant-name {
    min-width: 150px;
  }
}
</style>