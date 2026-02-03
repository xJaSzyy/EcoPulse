<template>
  <div id="app">
    <header class="app-header" v-if="!['home', 'map'].includes($route.name)">
      <div class="header-content">
        <h1 class="app-title">EcoPulse</h1>
        <p class="app-subtitle">Система расчета выбросов загрязняющих веществ</p>
      </div>
    </header>
    
    <main class="app-main">
      <router-view />
    </main>
    
    <footer class="app-footer" v-if="$route.name === 'home'">
      <p>&copy; 2025 EcoPulse. Все права защищены.</p>
    </footer>
  </div>
</template>

<script setup>
import { useRoute } from 'vue-router'

const $route = useRoute()
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}

.app-header {
  background: linear-gradient(135deg, #2c3e50 0%, #3498db 100%);
  color: white;
  padding: 1rem 0;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.app-title {
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 0.25rem;
}

.app-subtitle {
  font-size: 1rem;
  opacity: 0.9;
}

.app-main {
  flex: 1;
  padding: 0;
}

.app-footer {
  background: #2c3e50;
  color: white;
  text-align: center;
  padding: 1rem;
  margin-top: auto;
}

/* Адаптивность */
@media (max-width: 768px) {
  .app-title {
    font-size: 1.5rem;
  }
  
  .app-subtitle {
    font-size: 0.9rem;
  }
  
  .header-content {
    padding: 0 15px;
  }
}
</style>