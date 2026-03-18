<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов от стоящего транспорта</h1>
    </div>

    <div class="main-content">
      <!-- Левая панель - форма ввода -->
      <div class="form-panel">
        <div class="panel-header">
          <h2>Параметры светофора и транспорта</h2>
        </div>

        <form @submit.prevent="calculate" class="input-form">
          <div class="form-group">
            <label>Циклы запрещающего сигнала за 20 мин:</label>
            <input
              type="number"
              v-model.number="formData.trafficLightCycles"
              step="0.1"
              placeholder="0"
            >
          </div>

          <div class="form-group">
            <label>Длительность запрещающего сигнала, с:</label>
            <input
              type="number"
              v-model.number="formData.trafficLightStopTime"
              step="0.1"
              placeholder="0"
            >
          </div>

          <!-- Группы транспортных средств -->
          <div class="vehicle-groups">
            <h3>Группы транспортных средств</h3>

            <div
              v-for="(group, index) in formData.vehicleGroups"
              :key="index"
              class="vehicle-group"
            >
              <h4>Группа {{ index + 1 }}</h4>

              <div class="form-group">
                <label>Тип ТС:</label>
                <select v-model.number="group.vehicleType" class="form-select">
                  <option value="1">Легковые бензиновые</option>
                  <option value="2">Легковые дизельные</option>
                  <option value="3">Грузовые карбюраторные ≤ 3 т, микроавтобусы</option>
                  <option value="4">Грузовые карбюраторные > 3 т</option>
                  <option value="5">Автобусы карбюраторные</option>
                  <option value="6">Грузовые дизельные</option>
                  <option value="7">Автобусы дизельные</option>
                  <option value="8">Грузовые газобаллонные, на сжатом природном газе</option>
                </select>
              </div>

              <div class="form-group">
                <label>Кол‑во авто в «очереди» в конце цикла:</label>
                <input
                  type="number"
                  v-model.number="group.vehiclesCount"
                  step="0.1"
                  min="0"
                  placeholder="3"
                >
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

          <button type="submit" class="calculate-button">
            Рассчитать выбросы
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
          <div class="empty-icon">🚗</div>
          <h3>Настройте группы транспорта</h3>
          <p>Укажите параметры светофора и состав очереди авто у перекрёстка</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import ResultsTable from '../components/ResultsTable.vue'
import { calculateTrafficLightQueueEmission } from '../api/emission.js'

const router = useRouter()
const result = ref(null)

const formData = ref({
  trafficLightCycles: 0,
  trafficLightStopTime: 0,
  vehicleGroups: [],
})

onMounted(() => {
  addVehicleGroup()
})

const goBack = () => {
  router.back()
}

const calculate = async () => {
  try {
    result.value = await calculateTrafficLightQueueEmission(formData.value)
  } catch (error) {
    console.error('Ошибка расчета:', error)
  }
}

const addVehicleGroup = () => {
  formData.value.vehicleGroups.push({
    vehicleType: 1,
    vehiclesCount: 3,
  })
}

const removeVehicleGroup = (index) => {
  if (formData.value.vehicleGroups.length > 1) {
    formData.value.vehicleGroups.splice(index, 1)
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

/* Основная сетка: форма слева, таблица справа */
.main-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 30px;
  min-height: 500px;
}

/* Панель формы ввода */
.form-panel {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.panel-header {
  background: linear-gradient(135deg, #34495e, #2c3e50);
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
  box-sizing: border-box;
}

input:focus,
.form-select:focus {
  outline: none;
  border-color: #34495e;
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
  background: linear-gradient(135deg, #34495e, #2c3e50);
  color: white;
  padding: 14px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  margin-top: 30px;
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

/* Адаптивность */
@media (max-width: 1024px) {
  .main-content {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .results-panel {
    order: -1; /* форма сверху на мобильных */
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