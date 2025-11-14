import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import Map from '@arcgis/core/Map';
import MapView from '@arcgis/core/views/MapView';
import Graphic from '@arcgis/core/Graphic';
import GraphicsLayer from '@arcgis/core/layers/GraphicsLayer';
import { HttpClient } from '@angular/common/http';
import { FeatureSignalRService } from '../../services/feature-signal-r.service';
import { PointFeature } from '../../models/point.interface';
import { environment } from '../../../environments/environment';
import Point from "@arcgis/core/geometry/Point";

@Component({
  selector: 'app-map',
  imports: [],
  templateUrl: './map.component.html',
  styleUrl: './map.component.css'
})
export class MapComponent implements OnInit, OnDestroy {

  public view!: MapView
  public graphicsLayer = new GraphicsLayer();
  public url = `${environment.apiUrl}/api/Feature`;
  private http = inject(HttpClient);
  private signalR = inject(FeatureSignalRService)
  private graphicIndex: Record<number, Graphic> = {};
  public connectionLayer = new GraphicsLayer()
  
  constructor() {}

  ngOnInit(): void {
    this._initializeMap();
    this.view.when(() => {
      this._loadInitialGraphics();
      this.signalR.startConnection();
      this.signalR.onFeatureUpdate(features => {
        console.log('🔄 Incoming updates:', features);
        this.updateFeatures(features)
      })
    })
  }
  ngOnDestroy(): void {
    this.view.destroy();
  }

  public updateFeatures(features: PointFeature[]) {
    const updatedIds = new Set<number>(features.map(f => f.id));

    this.connectionLayer.removeAll();

    for (const f of features) {
      const id = Number.parseInt(f.properties["Id"]);      
      
      const existing = this.graphicIndex[id]; // **** O(1) access ****

      if (existing && existing.geometry instanceof Point) {
         const prevCoords = existing.attributes.prevCoords || [
          existing.geometry['x'] || existing.geometry['longitude'],
          existing.geometry['y'] || existing.geometry['latitude']
        ];

        const newCoords = [
          f.geometry.coordinates[0],
          f.geometry.coordinates[1]
        ];

        const lineGraphic = new Graphic({
          geometry: {
            type: "polyline",
            paths: [prevCoords, newCoords]
          },
          symbol: {
            type: "simple-line",
            color: [0, 255, 0],
            width: 2,
            style: "dash"
          }
        });

        this.connectionLayer.add(lineGraphic);

        this._updateGraphic(existing, f);

        existing.attributes.prevCoords = newCoords;

      } else {
        this._createGraphic(f);
      }
    }

    for (const id in this.graphicIndex) {
      if (!updatedIds.has(Number(id))) {
        const g = this.graphicIndex[id];
        g.symbol = this._createSymbol(g.attributes.defaultColor || "blue");
      }
    }
  }

   private _updateGraphic(g: Graphic, f: PointFeature) {
    g.geometry = {
      type: "point",
      longitude: f.geometry.coordinates[0],
      latitude: f.geometry.coordinates[1]
    };

    g.attributes = { ...g.attributes, ...f.properties };
    g.symbol = this._createSymbol("red");
  }

  private _createSymbol(color: string) {
    return {
      type: 'simple-marker',
      color,
      size: '8px'
    } as any;
  }

   private _createGraphic(f: PointFeature) {
    const id = f.properties["Id"];

    const gr = new Graphic({
      geometry: {
        type: "point",
        longitude: f.geometry.coordinates[0],
        latitude: f.geometry.coordinates[1]
      },
      attributes: { ...f.properties, defaultColor: "blue" },
      symbol: this._createSymbol("red")
    });

    this.graphicsLayer.add(gr);

    // **** important !!
    this.graphicIndex[id as any] = gr;
  }

  private _initializeMap() {
    const map = new Map({ basemap: 'streets' });

    this.view = new MapView({
      container: 'viewDiv',
      map,
      center: [32.85, 39.93], // Example: Ankara
      zoom: 6
    });

    map.add(this.graphicsLayer);
    map.add(this.connectionLayer);
  }

  private _loadInitialGraphics() {
    this.http.get<PointFeature[]>(this.url).subscribe(features => {
      const graphics = features.map((feature: PointFeature) => {
        const id = feature.properties["Id"];

        const gr = new Graphic({
          geometry: {
            type: 'point',
            longitude: feature.geometry.coordinates[0],
            latitude: feature.geometry.coordinates[1]
          },
          attributes: { ...feature.properties, defaultColor: "blue" },
          symbol: this._createSymbol("blue")
        });

        // O(1) accessing 
        this.graphicIndex[id as any] = gr;

        return gr;
      });

      this.graphicsLayer.addMany(graphics);
    });
  }

}
