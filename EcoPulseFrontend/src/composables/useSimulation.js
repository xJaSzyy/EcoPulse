import { ref } from 'vue'
import Feature from 'ol/Feature'
import { Point } from 'ol/geom'
import { fromLonLat } from 'ol/proj'

import { calculateSingleDangerZone } from '../api/dangerZone.js'
import { getSingleEmissionSourceById } from '../api/emissionSource.js'

export function useSimulation({
    map,
    olLayers,
    weather,
    updateSingleLayer,
    preloadTileGrids,
    refreshTileGrid,
    latestDangerZones,
    createEllipse
}) {

    const showSimulationPanel = ref(false)
    const simulationStartData = ref(null)
    const simulationData = ref(null)

    const previousSimulationData = ref(null)
    const weatherBeforeSimulation = ref(null)

    async function handleBuildSimulation(data) {
        const previous = previousSimulationData.value

        previousSimulationData.value = { ...data }

        if (!previous) {
            await buildSimulation(data)
            return
        }

        const windChanged =
            Number(data.windSpeed) !== Number(previous.windSpeed) ||
            Number(data.windDirection) !== Number(previous.windDirection)

        const sourceChanged =
            Number(data.ejectedTemp) !== Number(previous.ejectedTemp) ||
            Number(data.airTemp) !== Number(previous.airTemp) ||
            Number(data.avgExitSpeed) !== Number(previous.avgExitSpeed) ||
            Number(data.heightSource) !== Number(previous.heightSource) ||
            Number(data.diameterSource) !== Number(previous.diameterSource) ||
            Number(data.tempStratificationRatio) !== Number(previous.tempStratificationRatio) ||
            Number(data.sedimentationRateRatio) !== Number(previous.sedimentationRateRatio)

        if (windChanged || sourceChanged) {

            if (windChanged) {
                weather.value = {
                    ...weather.value,
                    windSpeed: Number(data.windSpeed),
                    windDirection: Number(data.windDirection)
                }

                const singleDangerZones = await updateSingleLayer()

                /*await preloadTileGrids(
                    singleDangerZones,
                    latestDangerZones.vehicleFlow,
                    latestDangerZones.vehicleQueue,
                    true
                )

                await refreshTileGrid()*/
                await buildSimulation(data)

                return
            }

            await buildSimulation(data)
            await patchSingleZoneForSource(data)

            /*await preloadTileGrids(
                latestDangerZones.single,
                latestDangerZones.vehicleFlow,
                latestDangerZones.vehicleQueue,
                true
            )

            await refreshTileGrid()*/

            return
        }

        await buildSimulation(data)
    }

    async function buildSimulation(data) {
        const dangerZone = await calculateSingleDangerZone({
            pollutant: 2,
            ejectedTemp: data.ejectedTemp,
            airTemp: data.airTemp,
            avgExitSpeed: data.avgExitSpeed,
            heightSource: data.heightSource,
            diameterSource: data.diameterSource,
            tempStratificationRatio: data.tempStratificationRatio,
            sedimentationRateRatio: data.sedimentationRateRatio,
            windSpeed: data.windSpeed,
            windDirection: data.windDirection,
            distance: 10000,
            sourceLocation: data.location
        })

        dangerZone.emissionSourceId = data.emissionSourceId
        dangerZone.location = data.location

        const singleLayer = olLayers.single
        const source = singleLayer.getSource()

        const featuresToRemove = source.getFeatures().filter(f => {
            return f.get('emissionSourceId') === dangerZone.emissionSourceId
        })

        source.removeFeatures(featuresToRemove)

        const ellipse = await createEllipse(dangerZone)

        ellipse.set('emissionSourceId', dangerZone.emissionSourceId)

        source.addFeature(ellipse)

        const pointFeature = new Feature({
            geometry: new Point(
                fromLonLat(dangerZone.location.coordinates)
            ),
            type: 'boiler'
        })

        pointFeature.set('emissionSourceId', dangerZone.emissionSourceId)

        source.addFeature(pointFeature)

        simulationData.value = {
            averageConcentration: dangerZone.averageConcentration,
            pollutionLevel: dangerZone.pollutionLevel,
            color: dangerZone.color
        }
    }

    async function patchSingleZoneForSource(data) {
        const source = olLayers.single?.getSource()

        if (!source) return

        const feature = source.getFeatures().find(
            f =>
                f.get('emissionSourceId') === data.emissionSourceId &&
                f.getGeometry().getType() === 'Polygon'
        )

        if (!feature) return

        const updatedZone = feature.get('dangerData')

        if (!updatedZone) return

        const idx = latestDangerZones.single.findIndex(
            z => z.emissionSourceId === data.emissionSourceId
        )

        if (idx >= 0) {
            latestDangerZones.single[idx] = updatedZone
        } else {
            latestDangerZones.single.push(updatedZone)
        }
    }

    async function closeSimulationPanel() {
        if (weatherBeforeSimulation.value) {
            weather.value = { ...weatherBeforeSimulation.value }
        }

        const singleDangerZones = await updateSingleLayer()

        await preloadTileGrids(
            singleDangerZones,
            latestDangerZones.vehicleFlow,
            latestDangerZones.vehicleQueue
        )

        await refreshTileGrid()

        showSimulationPanel.value = false
        simulationStartData.value = null
        simulationData.value = null
        previousSimulationData.value = null
        weatherBeforeSimulation.value = null
    }

    function openSimulationPanel(emissionSource, dangerData) {
        weatherBeforeSimulation.value = { ...weather.value }

        simulationStartData.value = {
            emissionSourceId: emissionSource.id,
            location: emissionSource.location,
            ejectedTemp: emissionSource.ejectedTemp,
            airTemp: weather.value.temperature,
            avgExitSpeed: emissionSource.avgExitSpeed,
            heightSource: emissionSource.heightSource,
            diameterSource: emissionSource.diameterSource,
            windSpeed: weather.value.windSpeed,
            windDirection: weather.value.windDirection,
            tempStratificationRatio: emissionSource.tempStratificationRatio,
            sedimentationRateRatio: emissionSource.sedimentationRateRatio
        }

        simulationData.value = {
            averageConcentration: dangerData.averageConcentration,
            pollutionLevel: dangerData.pollutionLevel,
            color: dangerData.color
        }

        showSimulationPanel.value = true
    }

    return {
        showSimulationPanel,
        simulationStartData,
        simulationData,
        previousSimulationData,
        weatherBeforeSimulation,
        handleBuildSimulation,
        buildSimulation,
        closeSimulationPanel,
        openSimulationPanel
    }
}