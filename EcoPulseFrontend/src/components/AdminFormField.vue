<template>
  <div class="form-field">
    <label :for="field.key" class="field-label">
      {{ field.label }}
      <span v-if="field.required" class="required">*</span>
    </label>
    
    <!-- Динамический рендерер полей -->
    <component
      :is="inputComponent"
      :id="field.key"
      :type="field.type"
      :value="modelValue"
      :options="field.options"
      :step="field.step"
      :min="field.min"
      :max="field.max"
      :rows="field.rows"
      @input="$emit('update:modelValue', $event.target?.value || $event)"
      class="field-input"
      :class="{ 'error': hasError }"
    />
    
    <div v-if="hasError" class="error-message">
      Поле обязательно для заполнения
    </div>
  </div>
</template>

<script>
import { computed } from 'vue'

export default {
  name: 'AdminFormField',
  props: {
    field: {
      type: Object,
      required: true
    },
    modelValue: [String, Number, Object]
  },
  emits: ['update:modelValue'],
  setup(props, { emit }) {
    const inputComponent = computed(() => {
      const type = props.field.type
      const components = {
        text: 'input',
        email: 'input',
        number: 'input',
        textarea: 'textarea',
        select: 'select',
        date: 'input'
      }
      return components[type] || 'input'
    })

    const hasError = computed(() => {
      return props.field.required && !props.modelValue
    })

    return {
      inputComponent,
      hasError
    }
  }
}
</script>

<style scoped>
.form-field {
  margin-bottom: 20px;
}

.field-label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
  color: #333;
}

.required {
  color: #dc3545;
}

.field-input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.field-input:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0,123,255,0.1);
}

select.field-input {
  background: white;
  cursor: pointer;
}

textarea.field-input {
  resize: vertical;
  min-height: 80px;
}

.field-input.error {
  border-color: #dc3545;
}

.error-message {
  color: #dc3545;
  font-size: 12px;
  margin-top: 4px;
}
</style>
