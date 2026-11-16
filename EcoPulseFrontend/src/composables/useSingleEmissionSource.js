import { ref, computed } from 'vue'
import { addSingleEmissionSource } from '../api/emissionSource.js'

export function useSingleEmissionSource({ afterSave }) {
  const createModeSingle = ref(false)
  const showSingleSourceForm = ref(false)
  const savingSingleSource = ref(false)
  const singleSourceError = ref('')

  const newSingleSource = ref({
    location: null,
    ejectedTemp: 250,
    avgExitSpeed: 20,
    heightSource: 30,
    diameterSource: 3,
    tempStratificationRatio: null,
    sedimentationRateRatio: null,
    cityId: null
  })

  const singleSourceLimits = {
    ejectedTemp: {
      min: 235,
      max: 265
    },
    avgExitSpeed: {
      min: 15,
      max: 30
    },
    heightSource: {
      min: 13,
      max: 150
    },
    diameterSource: {
      min: 1,
      max: 10
    }
  }

  const singleSourceValidation = computed(() => {
    const errors = {}

    Object.entries(singleSourceLimits).forEach(([key, limit]) => {
      const value = Number(newSingleSource.value[key])

      if (
        newSingleSource.value[key] === null ||
        newSingleSource.value[key] === '' ||
        !Number.isFinite(value)
      ) {
        errors[key] = 'Заполните поле'
      } else if (value < limit.min || value > limit.max) {
        errors[key] = `Допустимое значение: ${limit.min}–${limit.max}`
      }
    })

    if (newSingleSource.value.tempStratificationRatio == null) {
      errors.tempStratificationRatio = 'Выберите коэффициент региона'
    }

    if (newSingleSource.value.sedimentationRateRatio == null) {
      errors.sedimentationRateRatio = 'Выберите степень очистки'
    }

    return errors
  })

  const isSingleSourceFormValid = computed(() => {
    return Object.keys(singleSourceValidation.value).length === 0
      && !!newSingleSource.value.location
      && !!newSingleSource.value.cityId
  })

  function resetSingleSourceForm() {
    newSingleSource.value = {
      location: null,
      ejectedTemp: 250,
      avgExitSpeed: 20,
      heightSource: 30,
      diameterSource: 3,
      tempStratificationRatio: null,
      sedimentationRateRatio: null,
      cityId: null
    }
  }

  function startCreateModeSingle() {
    createModeSingle.value = true
    singleSourceError.value = ''
  }

  function cancelCreateModeSingle() {
    createModeSingle.value = false
  }

  function setSingleSourceLocation(coordinates, cityId) {
    newSingleSource.value.location = {
      type: 'Point',
      coordinates
    }

    newSingleSource.value.cityId = cityId
    createModeSingle.value = false
    showSingleSourceForm.value = true
  }

  async function submitSingleSource() {
    singleSourceError.value = ''

    if (!isSingleSourceFormValid.value) {
      singleSourceError.value = 'Проверьте правильность заполнения полей'
      return
    }

    const payload = {
      location: newSingleSource.value.location,
      ejectedTemp: Number(newSingleSource.value.ejectedTemp),
      avgExitSpeed: Number(newSingleSource.value.avgExitSpeed),
      heightSource: Number(newSingleSource.value.heightSource),
      diameterSource: Number(newSingleSource.value.diameterSource),
      tempStratificationRatio:
        newSingleSource.value.tempStratificationRatio,
      sedimentationRateRatio:
        newSingleSource.value.sedimentationRateRatio,
      cityId: Number(newSingleSource.value.cityId)
    }

    savingSingleSource.value = true

    try {
      await addSingleEmissionSource(payload)

      if (afterSave) {
        await afterSave()
      }

      showSingleSourceForm.value = false
      resetSingleSourceForm()
    } catch (error) {
      console.error('Ошибка добавления источника:', error)
      singleSourceError.value =
        error.message || 'Не удалось добавить источник'
    } finally {
      savingSingleSource.value = false
    }
  }

  return {
    createModeSingle,
    showSingleSourceForm,
    savingSingleSource,
    singleSourceError,
    newSingleSource,
    singleSourceValidation,
    isSingleSourceFormValid,
    startCreateModeSingle,
    cancelCreateModeSingle,
    setSingleSourceLocation,
    submitSingleSource
  }
}