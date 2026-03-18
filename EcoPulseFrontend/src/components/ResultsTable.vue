<template>
  <div class="results-container">
    <div class="panel-header">
      <h3>Результаты расчета выбросов</h3>
    </div>
    
    <div class="results-content">
      <div class="emissions-summary">
        <div class="summary-card" v-if="hasGrossEmission">
          <div class="summary-icon">📊</div>
          <div class="summary-value">{{ formatNumber(totalGrossEmission) }}</div>
          <div class="summary-label">Суммарный валовый выброс (г)</div>
        </div>

        <div class="summary-card">
          <div class="summary-icon">⚡</div>
          <div class="summary-value">{{ formatNumber(totalMaximumEmission) }}</div>
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
              <td v-if="hasGrossEmission" class="emission-value">
                {{ formatNumber(item.grossEmission) || '—' }}
              </td>
              <td class="emission-value">{{ formatNumber(item.maximumEmission) || '—' }}</td>
              <td class="concentration-value" :class="getConcentrationClass(item)">
                <div v-if="item.pollutantInfo.maxPermissibleConcentration">
                  {{ item.pollutantInfo.maxPermissibleConcentration }}
                </div>
                <div v-if="item.pollutantInfo.dailyAverageConcentration" class="concentration-daily">
                  (ср.суточная: {{ item.pollutantInfo.dailyAverageConcentration }})
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
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

const hasGrossEmission = computed(() => {
  return props.data.some(item => item.grossEmission != null && item.grossEmission > 0)
})

const totalGrossEmission = computed(() => {
  return props.data.reduce((sum, item) => sum + (item.grossEmission || 0), 0)
})

const totalMaximumEmission = computed(() => {
  return props.data.reduce((sum, item) => sum + (item.maximumEmission || 0), 0)
})

const formatNumber = (num) => {
  return num != null && num !== 0 ? num.toFixed(6) : null
}

const getConcentrationClass = (item) => {
  if (!item.maximumEmission || !item.pollutantInfo.maxPermissibleConcentration) return ''
  return item.maximumEmission > item.pollutantInfo.maxPermissibleConcentration ? 'exceeded' : 'normal'
}
</script>

<style scoped>
.results-container {
  flex: 1;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.panel-header {
  background: linear-gradient(135deg, #27ae60, #229954);
  color: white;
  padding: 20px;
  text-align: center;
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
}

.results-content {
  flex: 1;
  padding: 25px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.emissions-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 25px;
}

.summary-card {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  text-align: center;
  border: 1px solid #e9ecef;
}

.summary-icon {
  font-size: 28px;
  margin-bottom: 10px;
}

.summary-value {
  font-size: 22px;
  font-weight: bold;
  color: #e74c3c;
  margin-bottom: 8px;
}

.summary-label {
  font-size: 13px;
  color: #6c757d;
}

.pollutants-table {
  flex: 1;
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  min-width: 600px;
}

th {
  background: #34495e;
  color: white;
  padding: 12px 10px;
  font-weight: 600;
  font-size: 13px;
  white-space: nowrap;
}

td {
  padding: 12px 10px;
  border-bottom: 1px solid #ecf0f1;
  font-size: 13px;
}

.pollutant-name {
  min-width: 180px;
  text-align: left;
}

.name-main {
  font-weight: 500;
  color: #2c3e50;
}

.name-short {
  font-size: 11px;
  color: #7f8c8d;
  margin-top: 3px;
}

.code-cell, .emission-value {
  font-family: 'Courier New', monospace;
  font-weight: 500;
}

.emission-value {
  text-align: right;
}

.concentration-value {
  text-align: center;
}

.concentration-daily {
  font-size: 10px;
  color: #7f8c8d;
}

.exceeded {
  color: #e74c3c;
  font-weight: bold;
}

.normal {
  color: #27ae60;
  font-weight: bold;
}

tbody tr:hover {
  background: #f8f9fa;
}

/* Мобильная адаптивность */
@media (max-width: 768px) {
  .emissions-summary {
    grid-template-columns: 1fr;
  }
  
  .results-content {
    padding: 15px;
  }
}
</style>