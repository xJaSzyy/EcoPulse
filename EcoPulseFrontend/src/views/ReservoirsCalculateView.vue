<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов от резервуаров</h1>
    </div>
    
    <div class="form-container">
      <form @submit.prevent="calculate">
        <div class="form-group">
          <label>Конструкция резервуара:</label>
          <select v-model.number="formData.reservoirType">
            <option value=1>Наземный</option>
            <option value=2>Заглубленный</option>
          </select>
        </div>

        <div class="form-group">
          <label>Нефтепродукт:</label>
          <select v-model.number="formData.oilProduct">
            <option value=1>Бензин автомобильный</option>
            <option value=2>Дизельное топливо</option>
            <option value=3>Масла</option>
          </select>
        </div>

        <div class="form-group">
          <label>Климатическая зона:</label>
          <select v-model.number="formData.climateZone">
            <option value=1>1-я климатическая зона</option>
            <option value=2>2-я климатическая зона</option>
            <option value=3>3-я климатическая зона</option>
          </select>
        </div>

        <div class="form-group">
          <label>Кол-во закачиваемого в резервуар нефтепродукта в осенне-зимний период, м3:</label>
          <input type="number" v-model="formData.autumnWinterOilAmount" step="0.1">
        </div>

        <div class="form-group">
          <label>Кол-во закачиваемого в резервуар нефтепродукта в весенне-летний период, м3:</label>
          <input type="number" v-model="formData.springSummerOilAmount" step="0.1">
        </div>

        <div class="form-group">
          <label>Объем слитого нефтепродукта в резервуар, м3:</label>
          <input type="number" v-model="formData.drainedVolume" step="1">
        </div>

        <div class="form-group">
          <label>Среднее время слива, с:</label>
          <input type="number" v-model="formData.averageDrainTime" step="1">
        </div>
        
        <button type="submit" class="calculate-button">Рассчитать</button>
      </form>

      <ResultsTable v-if="result && result.length > 0" :data="result" />

      <div v-else-if="result" class="no-data">
        Нет данных для отображения
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import ResultsTable from '../components/ResultsTable.vue'
import {calculateReservoirsEmission} from "../api/emission.js";

const router = useRouter()
const result = ref(null)

const formData = ref({
  reservoirType: 1,
  oilProduct: 1,
  climateZone: 1,
  autumnWinterOilAmount: 0,
  springSummerOilAmount: 0,
  drainedVolume: 0,
  averageDrainTime: 0
})

const goBack = () => {
  router.back()
}

const calculate = async () => {
  try {
    result.value = await calculateReservoirsEmission(formData.value);
  } catch (error) {
    console.error('Ошибка расчета:', error);
  }
}
</script>

<style scoped>
.method-page {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  margin-bottom: 30px;
}

.page-header h1 {
  font-size: 24px;
}

.back-button {
  background: none;
  border: 1px solid #ddd;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  margin-right: 20px;
}

.back-button:hover {
  background: #f5f5f5;
}

.form-container {
  background: white;
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.form-group {
  margin-bottom: 20px;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
}

input, select {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 16px;
}

.calculate-button {
  background: #3498db;
  color: white;
  padding: 12px 30px;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  cursor: pointer;
}

.calculate-button:hover {
  background: #2980b9;
}

.result {
  margin-top: 20px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 6px;
}
/* Стили для секции результатов */
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

/* Суммарные показатели */
.emissions-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
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
  color: #e74c3c;
  margin-bottom: 8px;
}

.summary-label {
  font-size: 14px;
  color: #7f8c8d;
}

/* Таблица загрязняющих веществ */
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
  text-align: left;
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
  text-align: center;
  font-family: 'Courier New', monospace;
  color: #7f8c8d;
}

.emission-value {
  text-align: right;
  font-family: 'Courier New', monospace;
  font-weight: 500;
}

.concentration-value {
  text-align: center;
}

.concentration-daily {
  font-size: 11px;
  color: #7f8c8d;
  margin-top: 2px;
}

.no-data {
  text-align: center;
  padding: 40px;
  color: #7f8c8d;
  font-style: italic;
}

/* Адаптивность */
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