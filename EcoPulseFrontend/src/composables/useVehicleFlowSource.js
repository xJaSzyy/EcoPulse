import { ref, computed } from 'vue'
import { addVehicleFlowEmissionSource } from '../api/emissionSource.js'
import { getAllCities } from '../api/city.js'

export function useVehicleFlowSource({ afterSave }) {
  const showVehicleFlowForm = ref(false)
  const savingVehicleFlow = ref(false)
  const vehicleFlowError = ref('')
  const cities = ref([])
  const loadingCities = ref(false)
  const citiesError = ref('')

  async function loadCities() {
    loadingCities.value = true
    citiesError.value = ''

    try {
      cities.value = await getAllCities()
    } catch (error) {
      console.error('Ошибка загрузки городов:', error)
      citiesError.value = 'Не удалось загрузить список городов'
    } finally {
      loadingCities.value = false
    }
  }

  const newVehicleFlow = ref({
    points: null,
    vehicleType: 1,
    streetName: '',
    cityId: null
  })

  const vehicleTypes = [
    { value: 1, label: 'Легковые автомобили' },
    { value: 2, label: 'Легковые дизельные' },
    { value: 3, label: 'Грузовые карбюраторные до 3 т и микроавтобусы' },
    { value: 4, label: 'Грузовые карбюраторные более 3 т' },
    { value: 5, label: 'Автобусы карбюраторные' },
    { value: 6, label: 'Грузовые дизельные' },
    { value: 7, label: 'Автобусы дизельные' },
    { value: 8, label: 'Грузовые газобаллонные' }
  ]

  const vehicleFlowValidation = computed(() => {
    const errors = {}

    if (!newVehicleFlow.value.vehicleType) {
      errors.vehicleType = 'Выберите тип транспортного средства'
    }

    if (!newVehicleFlow.value.streetName.trim()) {
      errors.streetName = 'Укажите название улицы'
    }

    if (!newVehicleFlow.value.points) {
      errors.points = 'Необходимо указать маршрут'
    }

    if (!newVehicleFlow.value.cityId) {
      errors.cityId = 'Не выбран город'
    }

    return errors
  })

  const isVehicleFlowFormValid = computed(() =>
    Object.keys(vehicleFlowValidation.value).length === 0
  )

  async function setVehicleFlowData(coordinates, cityId) {
    newVehicleFlow.value = {
      points: {
        type: 'LineString',
        coordinates
      },
      vehicleType: 1,
      streetName: '',
      cityId
    }

    showVehicleFlowForm.value = true

    await loadCities()
  }

  function resetVehicleFlowForm() {
    newVehicleFlow.value = {
      points: null,
      vehicleType: 1,
      streetName: '',
      cityId: null
    }
  }

  function closeVehicleFlowForm() {
    if (savingVehicleFlow.value) return

    showVehicleFlowForm.value = false
    vehicleFlowError.value = ''
    resetVehicleFlowForm()
  }

  async function submitVehicleFlow() {
    vehicleFlowError.value = ''

    if (!isVehicleFlowFormValid.value) {
      vehicleFlowError.value = 'Проверьте правильность заполнения полей'
      return
    }

    savingVehicleFlow.value = true

    try {
      await addVehicleFlowEmissionSource({
        cityId: Number(newVehicleFlow.value.cityId),
        points: newVehicleFlow.value.points,
        vehicleType: Number(newVehicleFlow.value.vehicleType),
        maxTrafficIntensity: 35 / 2,
        averageSpeed: 40 / 2,
        streetName: newVehicleFlow.value.streetName.trim()
      })

      if (afterSave) {
        await afterSave()
      }

      showVehicleFlowForm.value = false
      resetVehicleFlowForm()

    } catch (error) {
      console.error('Ошибка добавления дороги:', error)

      vehicleFlowError.value =
        error.message || 'Не удалось добавить дорогу'
    } finally {
      savingVehicleFlow.value = false
    }
  }

  return {
    showVehicleFlowForm,
    savingVehicleFlow,
    vehicleFlowError,
    newVehicleFlow,
    vehicleTypes,
    vehicleFlowValidation,
    isVehicleFlowFormValid,
    setVehicleFlowData,
    submitVehicleFlow,
    closeVehicleFlowForm,
    cities,
    loadingCities,
    citiesError,
    loadCities
  }
}