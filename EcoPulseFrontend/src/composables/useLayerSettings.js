import { reactive, ref } from 'vue'

export function useLayerSettings({ olLayers, modifyFlow }) {

    const layersState = reactive({
        single: {
            visible: false,
            zIndex: 2
        },
        vehicleFlow: {
            visible: false,
            zIndex: 1
        },
        vehicleQueue: {
            visible: false,
            zIndex: 3
        },
        sanitaryArea: {
            visible: false,
            zIndex: 5
        },
        tileGrid: {
            visible: false,
            zIndex: 4
        },
        tile: {
            visible: false
        },
        area: {
            visible: false
        }
    })

    const selectedGridLayer = ref('tile')

    const saveLayersState = () => {
        const stateToSave = {}

        Object.keys(layersState).forEach(key => {
            stateToSave[key] = {
                visible: layersState[key].visible,
                zIndex: layersState[key].zIndex
            }
        })

        localStorage.setItem(
            'layersState',
            JSON.stringify(stateToSave)
        )

        localStorage.setItem(
            'selectedGridLayer',
            selectedGridLayer.value
        )
    }

    const updateLayerZIndex = (layerName) => {
        let layer

        if (layerName === 'tileGrid') {
            layer = olLayers[selectedGridLayer.value]
        } else {
            layer = olLayers[layerName]
        }

        if (!layer) {
            console.warn('Слой не найден:', layerName)
            return
        }

        const zIndex = Number(layersState[layerName].zIndex)

        if (!Number.isFinite(zIndex)) return

        layer.setZIndex(zIndex)

        saveLayersState()
    }

    const initLayersState = () => {
        const savedState = localStorage.getItem('layersState')

        if (savedState) {
            try {
                const parsedState = JSON.parse(savedState)

                Object.keys(layersState).forEach(key => {
                    if (parsedState[key]) {
                        layersState[key].visible =
                            parsedState[key].visible ?? layersState[key].visible

                        if (layersState[key].zIndex !== undefined) {
                            layersState[key].zIndex =
                                parsedState[key].zIndex ?? layersState[key].zIndex
                        }
                    }
                })
            } catch (error) {
                console.error('Ошибка загрузки настроек слоёв:', error)
            }
        }

        selectedGridLayer.value =
            localStorage.getItem('selectedGridLayer') || 'tile'

        Object.keys(olLayers).forEach(key => {
            if (olLayers[key]) {
                olLayers[key].setVisible(
                    layersState[key].visible
                )
            }
        })

        updateLayerZIndex('single')
        updateLayerZIndex('vehicleFlow')
        updateLayerZIndex('vehicleQueue')
        updateLayerZIndex('sanitaryArea')
        updateLayerZIndex('tileGrid')
    }

    const toggleLayer = (key) => {

        if (key === 'tile' || key === 'area') {
            const oppositeKey = key === 'tile' ? 'area' : 'tile'

            layersState[oppositeKey].visible = false
            olLayers[oppositeKey]?.setVisible(false)

            layersState[key].visible = true
            olLayers[key]?.setVisible(true)

            olLayers[key]?.setZIndex(
                Number(layersState.tileGrid.zIndex)
            )

        } else if (key === 'tileGrid') {

            const visible = layersState[key].visible

            const oppositeKey =
                selectedGridLayer.value === 'tile' ? 'area' : 'tile'

            layersState[selectedGridLayer.value].visible = visible
            olLayers[selectedGridLayer.value]?.setVisible(visible)

            layersState[oppositeKey].visible = false
            olLayers[oppositeKey]?.setVisible(false)

        } else {

            const visible = layersState[key].visible

            olLayers[key]?.setVisible(visible)
        }

        if (key === 'vehicleFlow' && modifyFlow?.value) {
            modifyFlow.value.setActive(layersState.vehicleFlow.visible)
        }

        saveLayersState()
    }

    return {
        layersState,
        selectedGridLayer,
        initLayersState,
        toggleLayer,
        updateLayerZIndex,
        saveLayersState
    }
}