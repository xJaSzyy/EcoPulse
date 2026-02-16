<template>
  <div>
    <AdminPanel :table-config="cityConfig" />
  </div>
</template>

<script>
import AdminPanel from '../components/AdminPanel.vue'

export default {
  components: { AdminPanel },
  data() {
    return {
      cityConfig: {
        name: 'city',
        title: 'Города',
        fields: [
          { key: 'name', type: 'text', label: 'Название'},
          { key: 'location', type: 'geojson', label: 'Координаты' },
          { key: 'polygon', type: 'geojson', label: 'Границы' },
        ]
      }
    }
  }
}
</script>
