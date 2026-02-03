<template>
  <div id="weather-info" class="weather-panel">
    <div class="weather-row">
      <span>{{ temperature }}°C</span>
      <i :class="['wi', iconClass]" class="weather-icon"></i>
    </div>
    <div class="weather-row">
      <span>{{ windSpeed }} м/с</span>
      <img
          :src="arrowIcon"
          style="width: 24px; height: 24px;"
          :style="{ transform: `rotate(${windDirection}deg)` }"
      >
    </div>
  </div>
</template>

<script>
import arrowIcon from "../icons/arrow.png";

export default {
  name: "WeatherInfo",
  props: {
    temperature: {type: Number, required: true},
    iconClass: {type: String, required: true},
    windSpeed: {type: Number, required: true},
    windDirection: {type: Number, required: true},
  },
  data() {
    return {
      arrowIcon,
    };
  },
};
</script>

<style>
#weather-info {
  position: absolute;
  top: 24px;
  right: 24px;
  background-color: #ffffff;
  padding: 12px;
  border-radius: 12px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
  width: 120px;
  font-family: Arial, sans-serif;
  display: flex;
  flex-direction: column;
  gap: 6px;
  z-index: 1;
  text-align: left;
}

.weather-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 4px;
}

.weather-icon {
  font-size: 24px;
  line-height: 1;
}
</style>
