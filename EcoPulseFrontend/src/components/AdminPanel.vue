<template>
  <div class="admin-panel">
    <h2>Админ панель</h2>
    
    <select v-model="selectedEntity" @change="loadData">
      <option value="city">Города</option>
      <option value="enterprise">Предприятия</option>
    </select>

    <!-- Форма добавления -->
    <div class="add-form">
      <h3>Добавить {{ selectedEntityLabel }}</h3>
      <div class="form-grid" v-if="selectedEntity === 'city'">
        <input v-model="newCity.name" placeholder="Название" />
        <input v-model="newCity.location" placeholder="Координаты" />
        <input v-model="newCity.polygon" placeholder="Границы" />
        <button @click="addNew('city')">Добавить город</button>
      </div>
      <div class="form-grid" v-else>
        <input v-model="newEnterprise.name" placeholder="Название" />
        <input v-model="newEnterprise.sanitaryArea" placeholder="Санитарная зона" />
        <input v-model="newEnterprise.cityId" placeholder="Id города" />
        <button @click="addNew('enterprise')">Добавить предприятие</button>
      </div>
    </div>

    <div v-if="loading">Загрузка...</div>
    
    <div v-else-if="items.length">
      <table>
        <thead>
          <tr>
            <th v-for="col in columns" :key="col">{{ col }}</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id">
            <td v-for="col in columns" :key="col">
              {{ item[col] }}
            </td>
            <td>
              <button @click="edit(item)">Edit</button>
              <button @click="remove(item)">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    
    <div v-else>Нет данных</div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import cityApi from '../api/city.js'
import enterpriseApi from '../api/enterprise.js'

const selectedEntity = ref('city')
const items = ref([])
const columns = ref([])
const loading = ref(false)

const newCity = ref({ name: '', location: '', polygon: '' })
const newEnterprise = ref({ name: '', sanitaryArea: '', cityId: 0 })

const apis = {
  city: cityApi,
  enterprise: enterpriseApi,
}

const selectedEntityLabel = computed(() => 
  selectedEntity.value === 'city' ? 'город' : 'предприятие'
)

const loadData = async () => {
  loading.value = true
  try {
    const data = await apis[selectedEntity.value].getAll()
    items.value = data
    columns.value = Object.keys(data[0] || {}).slice(0, 4)
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const addNew = async (entity) => {
  try {
    if (entity === 'city') {
      await apis.city.create({
        name: newCity.value.name,
        location: JSON.parse(newCity.value.location),
        polygon: JSON.parse(newCity.value.polygon)
      })
    } else {
      await apis.enterprise.create({
        name: newEnterprise.value.name,
        cityId: parseInt(newEnterprise.value.cityId), 
        sanitaryArea: JSON.parse(newEnterprise.value.sanitaryArea)
    })
    }
    loadData()
  } catch (e) {
    console.error('Ошибка добавления:', e)
  }
}

const edit = (item) => {
  console.log('Edit', item)
}

const remove = async (item) => {
  if (confirm('Удалить?')) {
    try {
      await apis[selectedEntity.value].delete(item.id)
      loadData()
    } catch (e) {
      console.error('Ошибка удаления:', e)
    }
  }
}

onMounted(loadData)
</script>

<style scoped>
.admin-panel {
  padding: 20px;
}

select {
  padding: 8px;
  margin-bottom: 20px;
}

.add-form {
  background: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
  border: 1px solid #eee;
}

.add-form h3 {
  margin-top: 0;
  margin-bottom: 15px;
  color: #333;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr auto;
  gap: 10px;
  align-items: end;
}

.form-grid input {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-grid button {
  padding: 8px 16px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.form-grid button:hover {
  background: #0056b3;
}

table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
}

th, td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 200px;
}

th {
  background: #f5f5f5;
}

button {
  margin-right: 5px;
  padding: 4px 8px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
}

button:nth-child(1) {
  background: #28a745;
  color: white;
}

button:nth-child(2) {
  background: #dc3545;
  color: white;
}
</style>
