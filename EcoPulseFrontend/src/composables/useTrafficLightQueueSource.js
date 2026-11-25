import { ref, computed } from 'vue'
import { addTrafficLightQueueEmissionSource } from '../api/emissionSource.js'

export function useTrafficLightQueueSource({ afterSave }) {
  const showTrafficLightQueueForm = ref(false)
  const savingTrafficLightQueue = ref(false)
  const trafficLightQueueError = ref('')

  const newTrafficLightQueue = ref({
    location: null,
    vehicleGroups: [
      {
        vehicleType: 1,
        vehiclesCount: 5
      }
    ],
    trafficLightCycles: 12,
    trafficLightStopTime: 60,
    cityId: null
  })

  const trafficLightQueueValidation = computed(() => {
    const errors = {}

    const groups = newTrafficLightQueue.value.vehicleGroups || []

    const groupErrors = groups.map((group) => {
      const groupError = {}

      if (!group.vehicleType) {
        groupError.vehicleType = 'Выберите тип транспортного средства'
      }

      const vehiclesCount = Number(group.vehiclesCount)
      if (!Number.isFinite(vehiclesCount) || vehiclesCount <= 0) {
        groupError.vehiclesCount = 'Укажите количество автомобилей'
      }

      return groupError
    })

    if (
      groups.length === 0 ||
      groupErrors.some((groupError) => Object.keys(groupError).length)
    ) {
      errors.groups = groupErrors
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
      vehicleGroups: [
        {
          vehicleType: 1,
          vehiclesCount: 5
        }
      ],
      trafficLightCycles: 12,
      trafficLightStopTime: 60,
      cityId
    }

    showTrafficLightQueueForm.value = true
  }

  function addVehicleGroup() {
    newTrafficLightQueue.value.vehicleGroups.push({
      vehicleType: 1,
      vehiclesCount: 5
    })
  }

  function removeVehicleGroup(index) {
    if (newTrafficLightQueue.value.vehicleGroups.length <= 1) return

    newTrafficLightQueue.value.vehicleGroups.splice(index, 1)
  }

  function resetTrafficLightQueueForm() {
    newTrafficLightQueue.value = {
      location: null,
      vehicleGroups: [
        {
          vehicleType: 1,
          vehiclesCount: 5
        }
      ],
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
        vehicleGroups: newTrafficLightQueue.value.vehicleGroups.map((group) => ({
          vehicleType: Number(group.vehicleType),
          vehiclesCount: Number(group.vehiclesCount)
        })),
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
    addVehicleGroup,
    removeVehicleGroup,
    submitTrafficLightQueue,
    closeTrafficLightQueueForm
  }
}
