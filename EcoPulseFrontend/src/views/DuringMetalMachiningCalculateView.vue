<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов при обработке металлов</h1>
    </div>
    
    <div class="main-content">
      <!-- Левая панель - форма ввода -->
      <div class="form-panel">
        <div class="panel-header">
          <h2>Параметры расчета</h2>
        </div>
        
        <form @submit.prevent="calculate" class="input-form">
          <div class="form-group">
            <label>Тип станка для обработки металла:</label>
            <select v-model.number="formData.metalMachiningMachineType" class="form-select">
              <option value="1">Сверлильный</option>
              <option value="2">Крацевальный</option>
              <option value="3">Отрезной</option>
            </select>
          </div>

          <div class="form-group">
            <label>Годовой фонд времени работы оборудования, ч:</label>
            <input type="number" v-model="formData.workDaysPerYear" step="0.1">
          </div>
          
          <button type="submit" class="calculate-button">
            Рассчитать
          </button>
        </form>
      </div>

      <!-- Правая панель - результаты -->
      <div class="results-panel">
        <ResultsTable v-if="result && result.length > 0" :data="result" />
        <div v-else-if="result" class="no-data">
          Нет данных для отображения
        </div>
        <div v-else class="empty-state">
          <div class="empty-icon">⚙️</div>
          <h3>Введите параметры обработки</h3>
          <p>Выберите тип станка и укажите время работы для расчета выбросов</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import ResultsTable from '../components/ResultsTable.vue'
import {calculateDuringMetalMachiningEmission} from "../api/emission.js";

const router = useRouter()
const result = ref(null)

const formData = ref({
  metalMachiningMachineType: 1,
  workDaysPerYear: 0,
})

const goBack = () => {
  router.back()
}

const calculate = async () => {
  try {
    result.value = await calculateDuringMetalMachiningEmission(formData.value);
  } catch (error) {
    console.error('Ошибка расчета:', error);
  }
}
</script>

<style scoped>
.method-page {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  margin-bottom: 30px;
}

.page-header h1 {
  font-size: 24px;
  margin: 0;
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

/* Основная сетка */
.main-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 30px;
  min-height: 500px;
}

/* Панель формы */
.form-panel {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.panel-header {
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
  padding: 20px;
  text-align: center;
}

.panel-header h2 {
  margin: 0;
  font-size: 18px;
}

.input-form {
  padding: 30px;
}

.form-group {
  margin-bottom: 20px;
}

label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
  color: #2c3e50;
}

input, .form-select {
  width: 100%;
  padding: 12px;
  border: 2px solid #e1e8ed;
  border-radius: 8px;
  font-size: 16px;
  transition: border-color 0.2s;
  background: white;
}

input:focus, .form-select:focus {
  outline: none;
  border-color: #f39c12;
}

.form-select {
  cursor: pointer;
  appearance: none;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-position: right 12px center;
  background-repeat: no-repeat;
  background-size: 16px;
  padding-right: 40px;
}

.calculate-button {
  width: 100%;
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
  padding: 14px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s;
}

.calculate-button:hover {
  transform: translateY(-1px);
}

/* Панель результатов */
.results-panel {
  display: flex;
  flex-direction: column;
}

.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 40px;
  text-align: center;
  color: #7f8c8d;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 20px;
}

.empty-state h3 {
  margin: 0 0 10px 0;
  color: #2c3e50;
}

.no-data {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 40px;
  color: #7f8c8d;
  font-style: italic;
}

/* Адаптивность */
@media (max-width: 1024px) {
  .main-content {
    grid-template-columns: 1fr;
    gap: 20px;
  }
  
  .results-panel {
    order: -1; /* Форма идет первой на мобильных */
  }
}

@media (max-width: 768px) {
  .method-page {
    padding: 15px;
  }
  
  .input-form {
    padding: 20px;
  }
}
</style>