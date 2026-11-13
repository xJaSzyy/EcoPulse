import VectorLayer from 'ol/layer/Vector'
import VectorSource from 'ol/source/Vector'
import Feature from 'ol/Feature'
import { LineString, Point, Polygon } from 'ol/geom'
import {
  Circle as CircleStyle,
  Fill,
  Icon,
  Stroke,
  Style,
  Text
} from 'ol/style'

export function useMapLayers({
  fromLonLat,
  createEllipse,
  getColorWithAlpha,
  getHatchPattern,
  boilerIcon
}) {
  function createSingleLayer(dangerZones) {
    const singleSource = new VectorSource()

    dangerZones.forEach(async dangerZone => {
      const ellipse = await createEllipse(dangerZone)
      ellipse.set('emissionSourceId', dangerZone.emissionSourceId)
      singleSource.addFeature(ellipse)

      const pointFeature = new Feature({
        geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
        type: 'boiler'
      })

      pointFeature.set('dangerColor', dangerZone.color)
      pointFeature.set('emissionSourceId', dangerZone.emissionSourceId)
      pointFeature.set('averageConcentration', dangerZone.averageConcentration)

      singleSource.addFeature(pointFeature)
    })

    const pointStyle = new Style({
      image: new Icon({
        src: boilerIcon,
        scale: 0.055,
        anchor: [0.5, 1],
        anchorXUnits: 'fraction',
        anchorYUnits: 'fraction'
      })
    })

    const ellipseFillStyle = new Style({
      fill: new Fill({ color: 'black' }),
      stroke: new Stroke({
        color: 'rgba(0, 0, 0, 0.5)'
      })
    })

    const textStyle = new Style({
      text: new Text({
        font: 'bold 18px roboto',
        fill: new Fill({ color: 'black' }),
        textAlign: 'center',
        textBaseline: 'middle'
      })
    })

    return new VectorLayer({
      source: singleSource,
      visible: true,
      zIndex: 3,
      style: feature => {
        const geom = feature.getGeometry()
        const color = getColorWithAlpha(feature.get('dangerColor'), 0.6)

        if (geom.getType() === 'Polygon') {
          ellipseFillStyle.getFill().setColor(color)
          return ellipseFillStyle
        }

        if (geom.getType() === 'Point') {
          return pointStyle
        }

        return null
      }
    })
  }

  function haversineDistanceMeters([lon1, lat1], [lon2, lat2]) {
    const R = 6371000
    const toRad = deg => (deg * Math.PI) / 180

    const dLat = toRad(lat2 - lat1)
    const dLon = toRad(lon2 - lon1)

    const a =
      Math.sin(dLat / 2) ** 2 +
      Math.cos(toRad(lat1)) *
      Math.cos(toRad(lat2)) *
      Math.sin(dLon / 2) ** 2

    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))

    return R * c
  }

  function getLineStringLengthMeters(lineString) {
    const { coordinates } = lineString
    let totalDistance = 0

    for (let i = 0; i < coordinates.length - 1; i++) {
      totalDistance += haversineDistanceMeters(
        coordinates[i],
        coordinates[i + 1]
      )
    }

    return totalDistance
  }

  function createVehicleFlowLayer(dangerZones) {
    const vehicleFlowSource = new VectorSource()

    dangerZones.forEach(dz => {
      const coords = dz.points.coordinates.map(
        ([lon, lat]) => fromLonLat([lon, lat])
      )

      const lineFeature = new Feature({
        geometry: new LineString(coords),
        type: 'flow'
      })

      const v = getLineStringLengthMeters(dz.points) * 6 * 2
      const m = (dz.averageConcentration * v) / 1e6

      const chemicalDustVolume = m * 0.2

      dz.chemicalDustVolume = chemicalDustVolume
      dz.waterVolume = chemicalDustVolume * 2.33

      lineFeature.set('dangerColor', dz.color)
      lineFeature.set('emissionSourceId', dz.emissionSourceId)
      lineFeature.set('dangerData', dz)

      vehicleFlowSource.addFeature(lineFeature)
    })

    const lineStyle = new Style({
      stroke: new Stroke({
        color: 'black',
        width: 5
      })
    })

    const pointStyle = new Style({
      image: new CircleStyle({
        radius: 3,
        fill: new Fill({ color: 'black' }),
        stroke: new Stroke({
          color: 'rgba(0, 0, 0, 0.5)'
        })
      })
    })

    return new VectorLayer({
      source: vehicleFlowSource,
      visible: false,
      zIndex: 1,
      style: feature => {
        const geomType = feature.getGeometry().getType()
        const color = getColorWithAlpha(feature.get('dangerColor'), 0.75)

        if (geomType === 'LineString') {
          lineStyle.getStroke().setColor(color)
          return lineStyle
        }

        if (geomType === 'Point') {
          pointStyle.getImage().getFill().setColor(color)
          return pointStyle
        }

        return null
      }
    })
  }

  function createVehicleQueueLayer(dangerZones) {
    const vehicleQueueSource = new VectorSource()

    dangerZones.forEach(dangerZone => {
      const pointFeature = new Feature({
        geometry: new Point(fromLonLat(dangerZone.location.coordinates)),
        type: 'queue'
      })

      pointFeature.set('dangerColor', dangerZone.color)
      pointFeature.set('emissionSourceId', dangerZone.emissionSourceId)
      pointFeature.set('dangerData', dangerZone)

      vehicleQueueSource.addFeature(pointFeature)
    })

    return new VectorLayer({
      source: vehicleQueueSource,
      visible: false,
      zIndex: 2,
      style: feature => {
        const geomType = feature.getGeometry().getType()
        const color = getColorWithAlpha(feature.get('dangerColor'), 0.75)

        if (geomType === 'Point') {
          return new Style({
            image: new CircleStyle({
              radius: 5,
              fill: new Fill({ color }),
              stroke: new Stroke({
                color: 'rgba(0, 0, 0, 0.5)'
              })
            })
          })
        }

        return null
      }
    })
  }

  function createSanitaryAreaLayer(sanitaryAreas) {
    const sanitaryAreaSource = new VectorSource()

    sanitaryAreas.forEach(area => {
      const polygonCoords = area.coordinates[0]

      const polygonFeature = new Feature({
        geometry: new Polygon([
          polygonCoords.map(coord => fromLonLat(coord))
        ])
      })

      sanitaryAreaSource.addFeature(polygonFeature)
    })

    const hatchPattern = getHatchPattern(24)

    return new VectorLayer({
      source: sanitaryAreaSource,
      visible: false,
      zIndex: 3,
      style: () => new Style({
        fill: new Fill({ color: hatchPattern }),
        stroke: new Stroke({
          color: 'black',
          width: 1
        })
      })
    })
  }

  return {
    createSingleLayer,
    createVehicleFlowLayer,
    createVehicleQueueLayer,
    createSanitaryAreaLayer
  }
}