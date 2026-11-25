import { asArray } from 'ol/color';
import { fromLonLat } from 'ol/proj';
import VectorLayer from 'ol/layer/Vector';
import VectorSource from 'ol/source/Vector';
import Feature from 'ol/Feature';
import { Polygon, MultiPolygon } from 'ol/geom';
import { Fill, Stroke, Style } from 'ol/style';
import { getSingleEmissionSourceById } from '../api/emissionSource.js';

export function useMapHelpers() {
  function geoJsonToOlGeometry(geometry) {
    if (!geometry || !geometry.type) return null

    switch (geometry.type) {
      case 'Polygon': {
        const rings = geometry.coordinates.map(ring =>
          ring.map(([lon, lat]) => fromLonLat([lon, lat]))
        )
        return new Polygon(rings)
      }

      case 'MultiPolygon': {
        const polys = geometry.coordinates.map(polygon =>
          polygon.map(ring =>
            ring.map(([lon, lat]) => fromLonLat([lon, lat]))
          )
        )
        return new MultiPolygon(polys)
      }

      case 'GeometryCollection': {
        const parts = geometry.geometries
          .map(geoJsonToOlGeometry)
          .filter(Boolean)
        return parts.length ? new MultiPolygon(parts.flatMap(toPolygons)) : null
      }

      default:
        console.warn('Неизвестный тип геометрии тайла:', geometry.type)
        return null
    }
  }

  function toPolygons(geom) {
    if (geom instanceof Polygon) return [geom.getCoordinates()]
    if (geom instanceof MultiPolygon) return geom.getCoordinates()
    return []
  }

  async function createEllipse(dangerZone) {
    const polygonCoords = dangerZone.polygon.coordinates[0];
    const coords = polygonCoords.map(coord => fromLonLat(coord));

    const ellipseFeature = new Feature({
      geometry: new Polygon([coords])
    });

    const sourceData = await getSingleEmissionSourceById(dangerZone.emissionSourceId);

    const s = Math.PI * Math.pow(sourceData.diameterSource, 2) / 4;
    const v = sourceData.avgExitSpeed * s * 3600;

    const coalConsumption = (v / 11) * (273 / (273 + sourceData.ejectedTemp));

    dangerZone.coalConsumption = coalConsumption / 1000;

    ellipseFeature.set('dangerData', dangerZone);
    ellipseFeature.set('emissionSourceId', dangerZone.emissionSourceId);
    ellipseFeature.set('dangerColor', dangerZone.color);
    ellipseFeature.set('averageConcentration', dangerZone.averageConcentration || 'N/A');

    return ellipseFeature;
  }

  function getColorWithAlpha(baseColor, alpha = 0.75) {
    if (!baseColor) {
      return [0, 0, 0, alpha];
    }

    return Array.isArray(baseColor)
      ? [baseColor[0], baseColor[1], baseColor[2], alpha]
      : asArray(baseColor).slice(0, 3).concat(alpha);
  }

  function createTileGridLayer(tileGridResult) {
    const tileGridSource = new VectorSource();

    const allTiles = tileGridResult.flatMap(city => city.tiles);

    allTiles.forEach(tileInfo => {
      const geometry = geoJsonToOlGeometry(tileInfo.tile)
      if (!geometry) return

      const feature = new Feature({
        geometry,
        color: tileInfo.color,
        cityId: tileInfo.cityId
      })

      feature.set('dangerData', tileInfo)
      tileGridSource.addFeature(feature)
    })

    return new VectorLayer({
      source: tileGridSource,
      visible: false,
      zIndex: 3,
      style: feature => {
        const color = getColorWithAlpha(feature.get('color'), 0.6)
        return new Style({
          fill: new Fill({ color }),
          stroke: new Stroke({
            color: 'black',
            width: 0.1
          })
        })
      }
    })
  }

  async function withCache(key, fn, ttl = 5 * 60 * 1000) {
    const cached = localStorage.getItem(key);

    if (cached) {
      try {
        const parsed = JSON.parse(cached);
        const isExpired = Date.now() - parsed.timestamp > ttl;

        if (!isExpired) {
          return parsed.data;
        }
      } catch (e) {
        console.error('Ошибка парсинга кеша', e);
      }
    }

    const data = await fn();

    localStorage.setItem(
      key,
      JSON.stringify({
        data,
        timestamp: Date.now()
      })
    );

    return data;
  }

  function randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }

  function getHatchPattern(size) {
    const canvas = document.createElement('canvas');
    const context = canvas.getContext('2d');

    canvas.width = size;
    canvas.height = size;

    context.strokeStyle = 'black';
    context.lineWidth = 1.5;

    context.beginPath();
    context.moveTo(0, size);
    context.lineTo(size, 0);
    context.stroke();

    return context.createPattern(canvas, 'repeat');
  }

  return {
    geoJsonToOlGeometry,
    toPolygons,
    createEllipse,
    getColorWithAlpha,
    createTileGridLayer,
    withCache,
    randomInt,
    getHatchPattern
  }
}
