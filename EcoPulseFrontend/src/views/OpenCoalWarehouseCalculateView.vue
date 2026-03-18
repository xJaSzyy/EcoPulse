<template>
  <div class="method-page">
    <div class="page-header">
      <button class="back-button" @click="goBack">← Назад</button>
      <h1>Расчет выбросов от открытых складов угля</h1>
    </div>

    <div class="main-content">
      <!-- Левая панель - форма ввода -->
      <div class="form-panel">
        <div class="panel-header">
          <h2>Параметры разгрузки угля</h2>
        </div>

        <form @submit.prevent="calculate" class="input-form">
          <div class="form-group">
            <label>Удельное выделение твердых частиц при разгрузке, г/т:</label>
            <input
              type="number"
              v-model.number="formData.specificEmission"
              step="any"
              placeholder="0.32"
            >
          </div>

          <div class="form-group">
            <label>Количество разгружаемого материала, т/г:</label>
            <input
              type="number"
              v-model.number="formData.unloadMaterialCountPerYear"
              step="any"
              placeholder="2 700 000"
            >
          </div>

          <div class="form-group">
            <label>Количество разгружаемого материала, т/ч:</label>
            <input
              type="number"
              v-model.number="formData.unloadMaterialCountPerHour"
              step="any"
              placeholder="285.388"
            >
          </div>

          <div class="form-group">
            <label>Эффективность пылеподавления, дол. ед.:</label>
            <input
              type="number"
              v-model.number="formData.dustSuppressionEfficiency"
              step="any"
              min="0"
              max="1"
              placeholder="0"
            >
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
          <div class="empty-icon">🏆</div>
          <h3>Введите параметры разгрузки угля</h3>
          <p>Укажите удельное выделение, объемы материала и эффективность пылеподавления</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import ResultsTable from '../components/ResultsTable.vue'
import { calculateOpenCoalWarehouseEmission } from '../api/emission.js'

const router = useRouter()
const result = ref(null)

const formData = ref({
  specificEmission: 0.32,
  unloadMaterialCountPerYear: 2700000,
  unloadMaterialCountPerHour: 285.388,
  dustSuppressionEfficiency: 0,
})

// Суммарные валовые/максимумы (для отображения вне таблицы, если нужно)
const totalGrossEmission = computed(() => {
  if (!result.value) return 0
  return result.value.reduce((sum, item) => sum + (item.grossEmission || 0), 0)
})

const totalMaximumEmission = computed(() => {
  if (!result.value) return 0
  return result.value.reduce((sum, item) => sum + (item.maximumEmission || 0), 0)
})

const goBack = () => {
  router.back()
}

const calculate = async () => {
  try {
    result.value = await calculateOpenCoalWarehouseEmission(formData.value)
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

/* Основная сетка: форма слева, результаты справа */
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
  background: linear-gradient(135deg, #8b4513, #654321);
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

input {
  width: 100%;
  padding: 12px;
  border: 2px solid #e1e8ed;
  border-radius: 8px;
  font-size: 16px;
  background: white;
  transition: border-color 0.2s;
}

input:focus {
  outline: none;
  border-color: #8b4513;
}

input::placeholder {
  color: #adb5bd;
}

.calculate-button {
  width: 100%;
  background: linear-gradient(135deg, #8b4513, #654321);
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

/* Адаптивность */
@media (max-width: 1024px) {
  .main-content {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .results-panel {
    order: -1; /* на мобильных: форма сверху, таблица ниже */
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