<template>
  <div class="admin-dashboard">
    <header>
      <h1>Админ-панель EcoPulse</h1>
      <nav>
        <router-link to="/admin/weather">Погода</router-link>
      </nav>
    </header>
    <router-view />  
  </div>
</template>
