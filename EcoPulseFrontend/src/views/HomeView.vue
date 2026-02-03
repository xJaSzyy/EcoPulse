<template>
  <div class="home">
    <h1>Выбор методики расчета выбросов</h1>
    <div class="methods-grid">
      <!-- Первые 4 карточки -->
      <MethodCard
          v-for="method in leftMethods"
          :key="method.id"
          :title="method.title"
          :description="method.description"
          :route="method.route"
          :icon="method.icon"
      />

      <div class="method-card earth-card" @click="$router.push('/map')">
        <div class="method-icon">
          <div class="earth-wrapper">
            <img src="../icons/earth.png" alt="Земля" class="earth" />
            <div class="earth-glow"></div>
          </div>
        </div>
        <div class="earth-arrow">→</div>
      </div>

      <!-- Последние 4 карточки -->
      <MethodCard
          v-for="method in rightMethods"
          :key="method.id"
          :title="method.title"
          :description="method.description"
          :route="method.route"
          :icon="method.icon"
      />
    </div>
  </div>
</template>

<script setup>
import MethodCard from '../components/MethodCard.vue'
import { useRouter } from 'vue-router'
import EarthIcon from '../icons/earth.png'
import { ref } from 'vue'

const router = useRouter()

const allMethods = ref([
  {
    id: 1,
    title: 'Бензогенератор',
    description: 'Расчет выбросов от бензогенератора',
    route: '/gasoline-generator',
    icon: '⚡'
  },
  {
    id: 2,
    title: 'Резервуары',
    description: 'Расчет выбросов от резервуаров',
    route: '/reservoirs',
    icon: '🛢️'
  },
  {
    id: 3,
    title: 'Обработка металлов',
    description: 'Расчет выбросов при механической обработке металлов',
    route: '/during-metal-machining',
    icon: '⚒️'
  },
  {
    id: 4,
    title: 'Сварочные работы',
    description: 'Расчет выбросов при сварочных работах',
    route: '/during-welding-operations',
    icon: '🔧'
  },
  {
    id: 6,
    title: 'Точечный источник',
    description: 'Расчет выбросов от одиночного точечного источника',
    route: '/maximum-single',
    icon: '📍'
  },
  {
    id: 7,
    title: 'Движущийся транспорт',
    description: 'Расчет выбросов от движущегося автотранспорта',
    route: '/vehicle-flow',
    icon: '🚗'
  },
  {
    id: 8,
    title: 'Стоящий транспорт',
    description: 'Расчет выбросов от автотранспорта в районе регулируемого перекрестка',
    route: '/traffic-light-queue',
    icon: '🚦'
  },
  {
    id: 9,
    title: 'Открытый склад угля',
    description: 'Расчет выбросов от открытых складов угля',
    route: '/open-coal-warehouse',
    icon: '🏭'
  }
])

const leftMethods = ref(allMethods.value.slice(0, 4))
const rightMethods = ref(allMethods.value.slice(4, 8))
</script>

<style scoped>
.home {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

h1 {
  text-align: center;
  margin-bottom: 40px;
  color: #2c3e50;
}

.methods-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  padding: 20px;
}

.earth-card {
  background: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  cursor: pointer;
}

.earth-wrapper {
  position: relative;
  width: 10em;
  height: 10em;
}

.earth {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  animation: rotate 16s linear infinite;
  box-shadow:
      0 0 16px rgba(47, 84, 213, 0.5),
      inset -10px 10px 10px 10px rgba(47, 84, 213, 0.3);
  display: block;
}

.earth-glow {
  position: absolute;
  top: -4px;
  left: -4px;
  right: -4px;
  bottom: -4px;
  border-radius: 50%;
  animation: glow 3s ease-in-out infinite alternate;
  z-index: -1;
}

@keyframes rotate {
  from { transform: rotateY(0deg); }
  to { transform: rotateY(360deg); }
}

@keyframes glow {
  from { opacity: 0.6; }
  to { opacity: 1; }
}

.earth-card {
  color: white !important;
}

.earth-card {
  color: rgba(255, 255, 255, 0.9) !important;
}

.earth-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  border-color: #3498db;
}

.earth-arrow {
  position: fixed;
  top: 24px;
  right: 24px;
  font-size: 1.5rem;
  color: #3498db;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.earth-card:hover .earth-arrow {
  opacity: 1;
}
</style>
