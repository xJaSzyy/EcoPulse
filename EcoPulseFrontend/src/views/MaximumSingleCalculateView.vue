<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов от одиночного точечного источника</h1>
    </div>

    <div class="form-container">
      <form @submit.prevent="calculate">
        <div class="form-group">
          <label>Загрязняющее вещество:</label>
          <select v-model.number="formData.pollutant">
            <option value=2>Твердые частицы (PM2.5)</option>
            <option value=380>Углерод диоксид (CO2)</option>
            <option value=301>Азота диоксид (NO2)</option>
            <option value=304>Азота оксид (NO)</option>
            <option value=330>Серы диоксид (SO2)</option>
          </select>
        </div>

        <div class="form-group">
          <label>Температура выбрасываемой ГВС:</label>
          <input type="number" v-model="formData.ejectedTemp" step="0.1" />
        </div>

        <div class="form-group">
          <label>Температура атмосферного воздуха:</label>
          <input type="number" v-model="formData.airTemp" step="0.1" />
        </div>

        <div class="form-group">
          <label>Средняя скорость выхода ГВС из устья источника выброса, м/с:</label>
          <input type="number" v-model="formData.avgExitSpeed" step="0.1" />
        </div>

        <div class="form-group">
          <label>Высота источника выброса, м.:</label>
          <input type="number" v-model="formData.heightSource" step="0.1" />
        </div>

        <div class="form-group">
          <label>Диаметр устья источника, м.:</label>
          <input type="number" v-model="formData.diameterSource" step="0.1" />
        </div>

        <div class="form-group">
          <label>Коэффициент региона:</label>
          <select v-model.number="formData.tempStratificationRatio">
            <option value=140>Владимирская, Ивановская, Калужская, Московская, Рязанская и Тульская области</option>
            <option value=160>Европейская территория РФ и Урала севернее 52° с.ш. (за исключением центра европейской территории РФ)</option>
            <option value=180>Европейская территория РФ и Урала от 50° с.ш. до 52° с.ш. включительно</option>
            <option value=200>Районы европейской территории РФ южнее 50° с.ш., остальные районы Нижнего Поволжья, азиатская территория РФ</option>
            <option value=250>Республика Бурятия и Забайкальский край</option>
          </select>
        </div>

        <div class="form-group">
          <label>Коэффициент степени очистки:</label>
          <select v-model.number="formData.sedimentationRateRatio">
            <option value=1>При среднем эксплуатационном коэффициенте очистки выбросов свыше 90%</option>
            <option value=2>При среднем эксплуатационном коэффициенте очистки выбросов от 75% до 90% включительно</option>
            <option value=3>При среднем эксплуатационном коэффициенте очистки выбросов менее 75% или отсутствии очистки выбросов</option>
          </select>
        </div>

        <div class="form-group">
          <label>Расстояние от источника выброса:</label>
          <input type="number" v-model="formData.distance" step="0.1" />
        </div>

        <div class="form-group">
          <label>Количество максимальных точек:</label>
          <input type="number" v-model="formData.maxCount" step="1" />
        </div>

        <button type="submit" class="calculate-button">Рассчитать</button>
      </form>

      <ResultsTable v-if="result && result.emissions.length > 0" :data="result" />

      <div v-else-if="result" class="no-data">
        Нет данных для отображения
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import ResultsTable from '../components/DistanceResultsTable.vue'
import {calculateMaximumSingleEmission} from "../api/emission.js";

const router = useRouter();
const result = ref(null);

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
  maxCount: 5
});

const goBack = () => {
  router.back();
};

const calculate = async () => {
  try {
    result.value = await calculateMaximumSingleEmission(formData.value);
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

input,
select {
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
