import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { PointFeature } from '../models/point.interface';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FeatureSignalRService {
  private hubConnection!: signalR.HubConnection
   public hubUrl = `${environment.apiUrl}/pointHub`;
  
  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .withAutomaticReconnect()
      .build();

     this.hubConnection
      .start()
      .then(() => console.log('✅ SignalR connected'))
      .catch(err => console.error('❌ SignalR error: ', err));
  }

  onFeatureUpdate(callback: (features: PointFeature[]) => void): void {
    console.log('🔗 Subscribing to ReceivePointUpdates');
    this.hubConnection.on('ReceivePointUpdate', (features: PointFeature[]) => {
      console.log('📡 Updates received:', features);
      callback(features)
    });
  }
}
