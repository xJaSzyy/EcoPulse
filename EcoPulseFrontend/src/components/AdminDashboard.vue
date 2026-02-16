<template>
  <div class="admin-dashboard">
    <header>
      <h1>Админ-панель EcoPulse</h1>
      <nav>
        <router-link to="/admin/weather">Погода</router-link>
        <router-link to="/admin/city">Города</router-link>
      </nav>
    </header>
    <router-view />  
  </div>
</template>

<style scoped>
.admin-dashboard {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

header {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
  padding: 2rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}

header h1 {
  margin: 0 0 1.5rem 0;
  font-size: 2.5rem;
  font-weight: 700;
  background: linear-gradient(45deg, #fff, #f0f0f0);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

nav {
  display: flex;
  gap: 0;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 12px;
  padding: 4px;
  backdrop-filter: blur(10px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

nav a {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 24px;
  color: rgba(255, 255, 255, 0.9);
  text-decoration: none;
  font-weight: 500;
  border-radius: 8px;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

nav a:hover {
  background: rgba(255, 255, 255, 0.25);
  transform: translateY(-2px);
  color: white;
}

nav a.router-link-active {
  background: rgba(255, 255, 255, 0.3);
  color: white;
  box-shadow: 0 4px 15px rgba(255, 255, 255, 0.2);
}

/* Контентная область */
:deep(.admin-panel) {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  margin: 2rem;
  padding: 2.5rem;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
  color: #333;
}

:deep(.table-section) {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
}

/* Адаптив */
@media (max-width: 768px) {
  header {
    padding: 1.5rem;
    text-align: center;
  }
  
  header h1 {
    font-size: 2rem;
  }
  
  nav {
    flex-direction: column;
  }
  
  nav a {
    justify-content: center;
  }
  
  :deep(.admin-panel) {
    margin: 1rem;
    padding: 1.5rem;
  }
}
</style>