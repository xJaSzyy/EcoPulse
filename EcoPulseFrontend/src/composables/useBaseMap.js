import { ref } from 'vue'
import TileLayer from 'ol/layer/Tile'
import OSM from 'ol/source/OSM'
import XYZ from 'ol/source/XYZ.js';

export function useBaseMap(map) {
    const selectedBaseMap = ref(
        localStorage.getItem('selectedBaseMap') || 'osm'
    )

    const CARTO_KEY = import.meta.env.VITE_CARTO_KEY

    const baseLayers = {
        osm: new TileLayer({
            source: new OSM(),
            visible: true
        }),

        esri: new TileLayer({
            source: new XYZ({
                url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
                attributions: 'Tiles © Esri — Source: Esri, Maxar, Earthstar Geographics'
            }),
            visible: false
        }),

        cartoVoyager: new TileLayer({
            source: new XYZ({
                url: `https://basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}.png?key=${CARTO_KEY}`,
                attributions: '© CARTO © OpenStreetMap contributors'
            }),
            visible: false
        }),

        cartoLight: new TileLayer({
            source: new XYZ({
                url: `https://basemaps.cartocdn.com/rastertiles/light_all/{z}/{x}/{y}.png?key=${CARTO_KEY}`,
                attributions: '© CARTO © OpenStreetMap contributors'
            }),
            visible: false
        }),

        cartoDark: new TileLayer({
            source: new XYZ({
                url: `https://basemaps.cartocdn.com/rastertiles/dark_all/{z}/{x}/{y}.png?key=${CARTO_KEY}`,
                attributions: '© CARTO © OpenStreetMap contributors'
            }),
            visible: false
        }),
    }

    function changeBaseMap() {
        Object.entries(baseLayers).forEach(([key, layer]) => {
            layer.setVisible(key === selectedBaseMap.value)
        })

        localStorage.setItem('selectedBaseMap', selectedBaseMap.value)
    }

    return {
        baseLayers,
        selectedBaseMap,
        changeBaseMap
    }
}