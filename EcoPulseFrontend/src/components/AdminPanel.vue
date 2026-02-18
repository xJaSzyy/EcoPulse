<template>
  <div class="admin-panel">
    <div class="table-section">
      <div class="table-header">
        <h2>{{ tableConfig.title }}</h2>
        <button @click="openForm()" class="btn btn-primary">Добавить</button>
      </div>
      
      <DataTable
        :columns="columns"
        :data="tableData"
        :loading="loading"
        :pagination="pagination"
        @edit="editRow"
        @delete="deleteRow"
        @page-change="handlePageChange"
      />
    </div>

    <Modal v-model="showForm" title="Редактировать запись">
      <form @submit.prevent="saveRow" class="admin-form">
        <div class="form-grid">
          <FormField
            v-for="field in tableConfig.fields"
            :key="field.key"
            :field="field"
            v-model="formData[field.key]"
            @input="updateFormData"
          />
        </div>
        <div class="form-actions">
          <button type="submit" :disabled="saving" class="btn btn-primary">
            {{ editingId ? 'Сохранить' : 'Создать' }}
          </button>
          <button @click="closeForm" class="btn btn-secondary">Отмена</button>
        </div>
      </form>
    </Modal>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import DataTable from './AdminDataTable.vue'
import FormField from './AdminFormField.vue'
import Modal from './Modal.vue'

export default {
  name: 'AdminPanel',
  components: { DataTable, FormField, Modal },
  props: {
    tableConfig: {
      type: Object,
      required: true,
      default: () => ({
        name: 'default',
        title: 'Таблица',
        fields: [],
        columns: []
      })
    }
  },
  setup(props) {
    const tableData = ref([])
    const formData = ref({})
    const loading = ref(false)
    const saving = ref(false)
    const showForm = ref(false)
    const editingId = ref(null)
    const pagination = ref({
      page: 1,
      limit: 5,
      total: 0
    })

    const columns = computed(() => props.tableConfig.columns || 
      props.tableConfig.fields.map(f => ({ key: f.key, title: f.label || f.key }))
    )

    const apiBase = `/api/admin/${props.tableConfig.name}`
    
    const fetchData = async () => {
      loading.value = true
      try {
        const res = await fetch(`${apiBase}?page=${pagination.value.page}&limit=${pagination.value.limit}`)
        const data = await res.json()
        tableData.value = data.data
        pagination.value.total = data.total
      } finally {
        loading.value = false
      }
    }

    const saveRow = async () => {
        console.log(formData.value)

      saving.value = true
      try {
        const method = editingId.value ? 'PUT' : 'POST'
        const url = apiBase
        await fetch(url, {
          method,
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(formData.value)
        })
        closeForm()
        fetchData()
      } finally {
        saving.value = false
      }
    }

    const deleteRow = async (id) => {
      if (!confirm('Удалить запись?')) return
      await fetch(`${apiBase}/${id}`, { method: 'DELETE' })
      fetchData()
    }

    const openForm = () => {
      formData.value = {}
      editingId.value = null
      showForm.value = true
    }

    const editRow = (row) => {
      formData.value = { ...row }
      editingId.value = row.id
      showForm.value = true
    }

    const closeForm = () => {
      showForm.value = false
      formData.value = {}
      editingId.value = null
    }

    const updateFormData = (key, value) => {
      formData.value = { ...formData.value, [key]: value }
    }

    const handlePageChange = (page) => {
      pagination.value.page = page
      fetchData()
    }

    onMounted(fetchData)

    return {
      tableData,
      formData,
      loading,
      saving,
      showForm,
      editingId,
      pagination,
      columns,
      openForm,
      editRow,
      deleteRow,
      closeForm,
      updateFormData,
      saveRow,
      handlePageChange
    }
  }
}
</script>

<style scoped>
.admin-panel {
  padding: 20px;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.form-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary {
  background: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0056b3;
  transform: translateY(-1px);
}

.btn-primary:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background: #545b62;
}

.form-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid #e9ecef;
  margin-top: 20px;
}

/* Адаптив */
@media (max-width: 768px) {
  .table-header {
    flex-direction: column;
    gap: 15px;
    align-items: stretch;
  }
  
  .btn {
    width: 100%;
  }
}
</style>
