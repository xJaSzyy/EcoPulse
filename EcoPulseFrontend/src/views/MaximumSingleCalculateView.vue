<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов от одиночного точечного источника</h1>
    </div>

    <div class="main-content">
      <div class="form-panel">
        <div class="panel-header">
          <h2>Параметры расчета</h2>
        </div>

        <form @submit.prevent="calculate" class="input-form">
          <div class="form-group">
            <label>Загрязняющее вещество:</label>
            <select v-model.number="formData.pollutant" class="form-select">
              <option value="2">Твердые частицы (PM2.5)</option>
              <option value="380">Углерод диоксид (CO₂)</option>
              <option value="301">Азота диоксид (NO₂)</option>
              <option value="304">Азота оксид (NO)</option>
              <option value="330">Серы диоксид (SO₂)</option>
            </select>
          </div>

          <div class="form-group">
            <label>Температура выбрасываемой ГВС, °C:</label>
            <input
              type="number"
              v-model.number="formData.ejectedTemp"
              step="0.1"
              placeholder="265"
            >
          </div>

          <div class="form-group">
            <label>Температура атмосферного воздуха, °C:</label>
            <input
              type="number"
              v-model.number="formData.airTemp"
              step="0.1"
              placeholder="40"
            >
          </div>

          <div class="form-group">
            <label>Средняя скорость выхода ГВС, м/с:</label>
            <input
              type="number"
              v-model.number="formData.avgExitSpeed"
              step="0.1"
              placeholder="25"
            >
          </div>

          <div class="form-group">
            <label>Высота источника, м:</label>
            <input
              type="number"
              v-model.number="formData.heightSource"
              step="0.1"
              placeholder="65"
            >
          </div>

          <div class="form-group">
            <label>Диаметр устья источника, м:</label>
            <input
              type="number"
              v-model.number="formData.diameterSource"
              step="0.1"
              placeholder="7"
            >
          </div>

          <div class="form-group">
            <label>Коэффициент региона:</label>
            <select v-model.number="formData.tempStratificationRatio" class="form-select">
              <option value="140">Владимирская, Ивановская, Калужская, Московская, Рязанская и Тульская области</option>
              <option value="160">Европейская территория РФ и Урала севернее 52° с.ш.</option>
              <option value="180">Европейская территория РФ и Урала от 50° с.ш. до 52° с.ш.</option>
              <option value="200">Районы европейской территории РФ южнее 50° с.ш., азиатская территория РФ</option>
              <option value="250">Республика Бурятия и Забайкальский край</option>
            </select>
          </div>

          <div class="form-group">
            <label>Коэффициент степени очистки:</label>
            <select v-model.number="formData.sedimentationRateRatio" class="form-select">
              <option value="1">Коэффициент очистки > 90%</option>
              <option value="2">75% ≤ очистка ≤ 90%</option>
              <option value="3">Очистка < 75% или отсутствует</option>
            </select>
          </div>

          <div class="form-group">
            <label>Расстояние от источника, м:</label>
            <input
              type="number"
              v-model.number="formData.distance"
              step="0.1"
              placeholder="10000"
            >
          </div>

          <div class="form-group">
            <label>Количество макс точек:</label>
            <input
              type="number"
              v-model.number="formData.maxCount"
              step="1"
              placeholder="5"
            >
          </div>

          <button type="submit" class="calculate-button">
            Рассчитать
          </button>
        </form>
      </div>

      <div class="results-panel">
        <DistanceResultsTable v-if="result" :data="result" />

        <div v-else-if="result" class="no-data">
          Нет данных для отображения
        </div>

        <div v-else class="empty-state">
          <div class="empty-icon loading-spinner">📍</div>
          <p>ожидание расчета</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import DistanceResultsTable from '../components/DistanceResultsTable.vue'
import { calculateMaximumSingleEmission } from '../api/emission.js'

const router = useRouter()
const result = ref(null)

const formData = ref({
  pollutant: 2,
  ejectedTemp: 265,
  airTemp: 40,
  avgExitSpeed: 25,
  heightSource: 65,
  diameterSource: 7,
  tempStratificationRatio: 140,
  sedimentationRateRatio: 1,
  distance: 10000,
  maxCount: 5,
})

const goBack = () => {
  router.back()
}

const calculate = async () => {
  try {
    result.value = await calculateMaximumSingleEmission(formData.value)
  } catch (error) {
    console.error('Ошибка расчета:', error)
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

.main-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 30px;
  min-height: 500px;
}

.form-panel {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.panel-header {
  background: linear-gradient(135deg, #2c3e50, #1a2530);
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

input,
.form-select {
  width: 100%;
  padding: 12px;
  border: 2px solid #e1e8ed;
  border-radius: 8px;
  font-size: 16px;
  background: white;
  transition: border-color 0.2s;
}

input:focus,
.form-select:focus {
  outline: none;
  border-color: #2c3e50;
}

.form-select {
  cursor: pointer;
  appearance: none;
  padding-right: 40px;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-position: right 12px center;
  background-repeat: no-repeat;
  background-size: 16px;
}

input::placeholder {
  color: #adb5bd;
}

.calculate-button {
  width: 100%;
  background: linear-gradient(135deg, #2c3e50, #1a2530);
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

.empty-state p {
  margin: 0;
  font-size: 14px;
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

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.loading-spinner {
  animation: spin 2s linear infinite;
}

@media (max-width: 1024px) {
  .main-content {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .results-panel {
    order: -1; 
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