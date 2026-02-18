<template>
  <div class="data-table-wrapper">
    <table class="data-table">
      <thead>
        <tr>
          <th v-for="col in columns" :key="col.key" class="table-header">
            {{ col.title }}
          </th>
          <th class="actions-header">Действия</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="row in data" :key="row.id" class="table-row">
          <td v-for="col in columns" :key="col.key" class="table-cell">
            {{ getCellValue(row, col.key) }}
          </td>
          <td class="actions-cell">
            <button @click="$emit('edit', row)" class="btn btn-sm btn-edit">✏️</button>
            <button @click="$emit('delete', row.id)" class="btn btn-sm btn-delete">🗑️</button>
          </td>
        </tr>
        <tr v-if="data.length === 0">
          <td :colspan="columns.length + 1" class="no-data">Нет данных</td>
        </tr>
      </tbody>
    </table>

    <!-- Пагинация -->
    <div v-if="pagination.total > pagination.limit" class="pagination">
      <button 
        @click="$emit('page-change', pagination.page - 1)"
        :disabled="pagination.page === 1"
        class="btn btn-sm"
      >Назад</button>
      <span>Страница {{ pagination.page }} из {{ Math.ceil(pagination.total / pagination.limit) }}</span>
      <button 
        @click="$emit('page-change', pagination.page + 1)"
        :disabled="pagination.page * pagination.limit >= pagination.total"
        class="btn btn-sm"
      >Вперед</button>
    </div>

    <div v-if="loading" class="loading">Загрузка...</div>
  </div>
</template>

<script>
export default {
  name: 'AdminDataTable',
  props: {
    columns: Array,
    data: Array,
    loading: Boolean,
    pagination: Object
  },
  methods: {
    getCellValue(row, key) {
      return key.split('.').reduce((obj, k) => obj?.[k], row) || ''
    }
  }
}
</script>

<style scoped>
.data-table-wrapper {
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

.table-header {
  background: #f8f9fa;
  padding: 12px 16px;
  font-weight: 600;
  border-bottom: 2px solid #e9ecef;
  text-align: left;
}

.table-row:hover {
  background: #f8f9fa;
}

.table-cell {
  padding: 12px 16px;
  border-bottom: 1px solid #e9ecef;
}

.actions-header, .actions-cell {
  width: 120px;
  text-align: center;
}

.btn-sm {
  padding: 4px 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin: 0 2px;
}

.btn-edit { background: #28a745; color: white; }
.btn-delete { background: #dc3545; color: white; }

.pagination {
  padding: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8f9fa;
  border-top: 1px solid #e9ecef;
}

.no-data {
  text-align: center;
  padding: 40px;
  color: #6c757d;
  font-style: italic;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #6c757d;
}
</style>
