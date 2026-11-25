import { ref, computed } from 'vue'
import { addTrafficLightQueueEmissionSource } from '../api/emissionSource.js'

export function useTrafficLightQueueSource({ afterSave }) {
  const showTrafficLightQueueForm = ref(false)
  const savingTrafficLightQueue = ref(false)
  const trafficLightQueueError = ref('')

  const newTrafficLightQueue = ref({
    location: null,
    vehicleType: 1,
    vehiclesCount: 5,
    trafficLightCycles: 12,
    trafficLightStopTime: 60,
    cityId: null
  })

  const trafficLightQueueValidation = computed(() => {
    const errors = {}

    if (!newTrafficLightQueue.value.vehicleType) {
      errors.vehicleType = 'Выберите тип транспортного средства'
    }

    const vehiclesCount = Number(newTrafficLightQueue.value.vehiclesCount)
    if (!Number.isFinite(vehiclesCount) || vehiclesCount <= 0) {
      errors.vehiclesCount = 'Укажите количество автомобилей в очереди'
    }

    const trafficLightCycles = Number(newTrafficLightQueue.value.trafficLightCycles)
    if (!Number.isFinite(trafficLightCycles) || trafficLightCycles <= 0) {
      errors.trafficLightCycles = 'Укажите количество циклов'
    }

    const trafficLightStopTime = Number(newTrafficLightQueue.value.trafficLightStopTime)
    if (!Number.isFinite(trafficLightStopTime) || trafficLightStopTime <= 0) {
      errors.trafficLightStopTime = 'Укажите длительность запрещающего сигнала'
    }

    if (!newTrafficLightQueue.value.cityId) {
      errors.cityId = 'Не выбран город'
    }

    return errors
  })

  const isTrafficLightQueueFormValid = computed(
    () =>
      Object.keys(trafficLightQueueValidation.value).length === 0 &&
      !!newTrafficLightQueue.value.location
  )

  function setTrafficLightQueueLocation(coordinates, cityId) {
    newTrafficLightQueue.value = {
      location: {
        type: 'Point',
        coordinates
      },
      vehicleType: 1,
      vehiclesCount: 5,
      trafficLightCycles: 12,
      trafficLightStopTime: 60,
      cityId
    }

    showTrafficLightQueueForm.value = true
  }

  function resetTrafficLightQueueForm() {
    newTrafficLightQueue.value = {
      location: null,
      vehicleType: 1,
      vehiclesCount: 5,
      trafficLightCycles: 12,
      trafficLightStopTime: 60,
      cityId: null
    }
  }

  function closeTrafficLightQueueForm() {
    if (savingTrafficLightQueue.value) return

    showTrafficLightQueueForm.value = false
    trafficLightQueueError.value = ''
    resetTrafficLightQueueForm()
  }

  async function submitTrafficLightQueue() {
    trafficLightQueueError.value = ''

    if (!isTrafficLightQueueFormValid.value) {
      trafficLightQueueError.value = 'Проверьте правильность заполнения полей'
      return
    }

    savingTrafficLightQueue.value = true

    try {
      await addTrafficLightQueueEmissionSource({
        location: newTrafficLightQueue.value.location,
        vehicleGroups: [
          {
            vehicleType: Number(newTrafficLightQueue.value.vehicleType),
            vehiclesCount: Number(newTrafficLightQueue.value.vehiclesCount)
          }
        ],
        trafficLightCycles: Number(newTrafficLightQueue.value.trafficLightCycles),
        trafficLightStopTime: Number(newTrafficLightQueue.value.trafficLightStopTime),
        cityId: Number(newTrafficLightQueue.value.cityId)
      })

      if (afterSave) {
        await afterSave()
      }

      showTrafficLightQueueForm.value = false
      resetTrafficLightQueueForm()
    } catch (error) {
      console.error('Ошибка добавления перекрестка:', error)
      trafficLightQueueError.value =
        error.message || 'Не удалось добавить перекресток'
    } finally {
      savingTrafficLightQueue.value = false
    }
  }

  return {
    showTrafficLightQueueForm,
    savingTrafficLightQueue,
    trafficLightQueueError,
    newTrafficLightQueue,
    trafficLightQueueValidation,
    isTrafficLightQueueFormValid,
    setTrafficLightQueueLocation,
    submitTrafficLightQueue,
    closeTrafficLightQueueForm
  }
}
