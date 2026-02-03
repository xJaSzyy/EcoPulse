<template>
  <div class="method-card" @click="navigateToMethod">
    <div class="method-icon">
      {{ icon }}
    </div>
    <h3 class="method-title">{{ title }}</h3>
    <p class="method-description">{{ description }}</p>
    <div class="method-arrow">→</div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'

const props = defineProps({
  title: String,
  description: String,
  route: String,
  icon: String
})

const router = useRouter()

const navigateToMethod = () => {
  router.push(props.route)
}
</script>

<style scoped>
.method-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  border: 1px solid #e1e5e9;
}

.method-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  border-color: #3498db;
}

.method-icon {
  font-size: 2rem;
  margin-bottom: 12px;
}

.method-title {
  font-size: 1.25rem;
  font-weight: 600;
  margin-bottom: 8px;
  color: #2c3e50;
}

.method-description {
  color: #666;
  line-height: 1.4;
  margin-bottom: 16px;
}

.method-arrow {
  position: absolute;
  top: 24px;
  right: 24px;
  font-size: 1.5rem;
  color: #3498db;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.method-card:hover .method-arrow {
  opacity: 1;
}
</style>