<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов при сварочных работах</h1>
    </div>
    
    <div class="form-container">
      <form @submit.prevent="calculate">
        <div class="form-group">
          <label>Расход сварочных электродов в год, кг:</label>
          <input type="number" v-model="formData.electrodesPerYear" step="0.1">
        </div>

        <div class="form-group">
          <label>Время работы сварочного оборудования, ч/год:</label>
          <input type="number" v-model="formData.workDaysPerYear" step="0.1">
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
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import ResultsTable from '../components/ResultsTable.vue'
import {calculateDuringWeldingOperationsEmission} from "../api/emission.js";

const router = useRouter()
const result = ref(null)

const formData = ref({
  electrodesPerYear: 0,
  workDaysPerYear: 0,
})

const goBack = () => {
  router.back()
}

const calculate = async () => {
  try {
    result.value = await calculateDuringWeldingOperationsEmission(formData.value);
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
</style>