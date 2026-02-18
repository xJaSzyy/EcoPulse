<template>
  <div>
    <AdminPanel :table-config="weatherConfig" />
  </div>
</template>

<script>
import AdminPanel from '../components/AdminPanel.vue'

export default {
  components: { AdminPanel },
  data() {
    return {
      weatherConfig: {
        name: 'weather',
        title: 'Погода',
        fields: [
          { key: 'date', type: 'date', label: 'Дата'},
          { key: 'temperature', type: 'number', label: 'Температура' },
          { key: 'windDirection', type: 'number', label: 'Направление ветра' },
          { key: 'windSpeed', type: 'number', label: 'Скорость ветра' },
          { key: 'iconClass', type: 'text', label: 'Класс иконки' },
        ]
      }
    }
  }
}
</script>
