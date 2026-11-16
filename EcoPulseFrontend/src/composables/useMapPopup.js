import { ref } from 'vue'
import Overlay from 'ol/Overlay'

export function useMapPopup(map) {
    const singlePopup = ref(null)
    const currentPopupData = ref(null)

    function showPopup(coordinate, feature, sourceType) {
        currentPopupData.value = {
            ...feature.get('dangerData'),
            sourceType
        }

        if (!currentPopupData.value) return

        const popupElement = createPopupElement()

        if (!singlePopup.value) {
            singlePopup.value = new Overlay({
                element: popupElement,
                positioning: 'bottom-center',
                stopEvent: false,
                insertFirst: false
            })

            map.value.addOverlay(singlePopup.value)
        } else {
            singlePopup.value.setElement(popupElement)
        }

        singlePopup.value.setPosition(coordinate)
    }

    function hidePopup() {
        if (singlePopup.value) {
            map.value.removeOverlay(singlePopup.value)
            singlePopup.value = null
        }

        currentPopupData.value = null
    }

    function createPopupElement() {
        const data = currentPopupData.value

        const popup = document.createElement('div')
        popup.className = 'popup-element'

        const coalConsumptionHtml =
            data.coalConsumption != null
                ? `
          <div class="average-concentration" style="margin-top: 12px;">
            <strong><label>Предполагаемый расход угля: </label></strong>
            <span>${data.coalConsumption.toFixed(2)} тонн/час</span>
          </div>
        `
                : ''

        const wateringHtml =
            data.chemicalDustVolume != null && data.waterVolume != null
                ? `
          <div style="margin-top: 12px;">
            <strong>
              <label style="color: #0D9488;">Требуется для полива:</label>
            </strong>

            <div class="average-concentration">
              <strong><label>Объем хим. пылеподавителя: </label></strong>
              <span>${data.chemicalDustVolume.toFixed(2)} кг</span>
            </div>

            <div class="average-concentration">
              <strong><label>Объем воды: </label></strong>
              <span>${data.waterVolume.toFixed(2)} л</span>
            </div>
          </div>
        `
                : ''

        popup.innerHTML = `
      <div class="popup-content">
        <div class="popup-text">
          <div class="source-type">
            <strong><label>Тип источника: </label></strong>
            <span>${data.sourceType}</span>
          </div>

          <div class="average-concentration">
            <strong><label>Концентрация: </label></strong>
            <span>${data.averageConcentration.toFixed(2)} мкг/м3</span>
          </div>

          <div class="legend-item">
            <strong><label>Уровень загрязнения -&nbsp;</label></strong>
            <span>${data.pollutionLevel}&nbsp;</span>
            <span
              class="legend-color"
              style="background-color: ${data.color}"
            ></span>
          </div>

          ${coalConsumptionHtml}
          ${wateringHtml}
        </div>
      </div>
    `

        return popup
    }

    return {
        singlePopup,
        currentPopupData,
        showPopup,
        hidePopup
    }
}