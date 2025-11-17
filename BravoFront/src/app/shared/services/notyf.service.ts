import { Injectable } from '@angular/core';
import { Notyf } from 'notyf';
import 'notyf/notyf.min.css';

@Injectable({
  providedIn: 'root'
})
export class NotyfService {
  private notyf = new Notyf({ duration: 3000, position: { x: 'right', y: 'top' } });

  success(message: string) {
    this.notyf.success(message);
  }

  error(message: string) {
    this.notyf.error(message);
  }

  info(message: string) {
    this.notyf.open({ type: 'info', message });
  }

  warning(message: string) {
    this.notyf.open({ type: 'warning', message });
  }
}
