import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'home',
    component: () => import('../views/HomeView.vue')
  },
  {
    path: '/gasoline-generator',
    name: 'gasoline-generator',
    component: () => import('../views/GasolineGeneratorCalculateView.vue')
  },
  {
    path: '/reservoirs',
    name: 'reservoirs',
    component: () => import('../views/ReservoirsCalculateView.vue')
  },
  {
    path: '/during-metal-machining',
    name: 'during-metal-machining',
    component: () => import('../views/DuringMetalMachiningCalculateView.vue')
  },
  {
    path: '/during-welding-operations',
    name: 'during-welding-operations',
    component: () => import('../views/DuringWeldingOperationsCalculateView.vue')
  },
  {
    path: '/maximum-single',
    name: 'maximum-single',
    component: () => import('../views/MaximumSingleCalculateView.vue')
  },
  {
    path: '/vehicle-flow',
    name: 'vehicle-flow',
    component: () => import('../views/VehicleFlowCalculateView.vue')
  },
  {
    path: '/traffic-light-queue',
    name: 'traffic-light-queue',
    component: () => import('../views/TrafficLightQueueCalculateView.vue')
  },
  {
    path: '/open-coal-warehouse',
    name: 'open-coal-warehouse',
    component: () => import('../views/OpenCoalWarehouseCalculateView.vue')
  },
  {
    path: '/map',
    name: 'map',
    component: () => import('../components/Map.vue')
  },
  {
    path: '/admin',
    name: 'admin',
    component: () => import('../components/AdminDashboard.vue')
  },
  {
    path: '/admin/weather',
    name: 'admin-weather',
    component: () => import('../views/AdminWeatherView.vue')
  },
  {
    path: '/admin/city',
    name: 'admin-city',
    component: () => import('../views/AdminCityView.vue')
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router