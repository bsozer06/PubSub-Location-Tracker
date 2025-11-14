import { Geometry } from "./geometry.interface";

export interface PointFeature {
  id: number;
  type: string;
  geometry: Geometry;
  properties: { [key: string]: string };
}