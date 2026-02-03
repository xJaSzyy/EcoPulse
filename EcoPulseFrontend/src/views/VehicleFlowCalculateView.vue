<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов от движущегося транспорта</h1>
    </div>

    <div class="form-container">
      <form @submit.prevent="calculate">
        <div class="form-group">
          <label>Протяженность автомагистрали (или ее участка):</label>
          <input type="number" v-model="formData.length" step="0.1" />
        </div>

        <div class="vehicle-groups">
          <h3>Группы транспортных средств</h3>

          <div
            v-for="(group, index) in formData.vehicleGroups"
            :key="index"
            class="vehicle-group"
          >
            <h4>Группа {{ index + 1 }}</h4>

            <div class="form-group">
              <label>Тип транспортного средства:</label>
              <select v-model.number="group.vehicleType">
                <option value=1>Легковые автомобили</option>
                <option value=2>Легковые дизельные</option>
                <option value=3>Грузовые карбюраторные с грузоподъемностью до 3 т (в том числе работающие на сжиженном нефтяном газе) и микроавтобусы</option>
                <option value=4>Грузовые карбюраторные с грузоподъемностью более 3 т (в том числе работающие на сжиженном нефтяном газе)</option>
                <option value=5>Автобусы карбюраторные</option>
                <option value=6>Грузовые дизельные</option>
                <option value=7>Автобусы дизельные</option>
                <option value=8>Грузовые газобаллонные, работающие на сжатом природном</option>
              </select>
            </div>

            <div class="form-group">
              <label
                >Фактическая наибольшая интенсивность движения (авт/ч):</label
              >
              <input
                type="number"
                v-model="group.maxTrafficIntensity"
                step="0.1"
                min="0"
              />
            </div>

            <div class="form-group">
              <label>Средняя скорость движения (км/ч):</label>
              <input
                type="number"
                v-model="group.averageSpeed"
                step="0.1"
                min="0"
              />
            </div>

            <button
              type="button"
              @click="removeVehicleGroup(index)"
              class="btn-remove"
              :disabled="formData.vehicleGroups.length === 1"
            >
              Удалить группу
            </button>
          </div>

          <button type="button" @click="addVehicleGroup" class="btn-add">
            Добавить группу транспортных средств
          </button>
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
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import ResultsTable from '../components/ResultsTable.vue'
import {calculateVehicleFlowEmission} from "../api/emission.js";

const router = useRouter();
const result = ref(null);

const formData = ref({
  length: 0,
  vehicleGroups: []
});

onMounted(() => {
  addVehicleGroup();
});

const goBack = () => {
  router.back();
};

const calculate = async () => {
  try {
    result.value = await calculateVehicleFlowEmission(formData.value);
  } catch (error) {
    console.error('Ошибка расчета:', error);
  }
}

const addVehicleGroup = () => {
  formData.value.vehicleGroups.push({
    vehicleType: 1,
    maxTrafficIntensity: 0,
    averageSpeed: 0
  });
};

const removeVehicleGroup = (index) => {
  if (formData.value.vehicleGroups.length > 1) {
    formData.value.vehicleGroups.splice(index, 1);
  }
};
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
  box-sizing: border-box;
}

.calculate-button {
  background: #3498db;
  color: white;
  padding: 12px 30px;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  cursor: pointer;
  margin-top: 20px;
  width: 100%;
}

.calculate-button:hover {
  background: #2980b9;
}

.result {
  margin-top: 20px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 6px;
  text-align: center;
}

/* Стили для групп транспортных средств */
.vehicle-groups {
  margin-top: 30px;
  border-top: 1px solid #eee;
  padding-top: 20px;
}

.vehicle-groups h3 {
  margin-bottom: 20px;
  color: #2c3e50;
}

.vehicle-group {
  border: 1px solid #e0e0e0;
  padding: 20px;
  margin-bottom: 20px;
  border-radius: 8px;
  background: #fafafa;
  position: relative;
}

.vehicle-group h4 {
  margin-top: 0;
  margin-bottom: 15px;
  color: #34495e;
  border-bottom: 1px solid #e0e0e0;
  padding-bottom: 10px;
}

.btn-add {
  background: #27ae60;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  margin-bottom: 20px;
}

.btn-add:hover {
  background: #219a52;
}

.btn-remove {
  background: #e74c3c;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
}

.btn-remove:hover:not(:disabled) {
  background: #c0392b;
}

.btn-remove:disabled {
  background: #95a5a6;
  cursor: not-allowed;
  opacity: 0.6;
}
</style>