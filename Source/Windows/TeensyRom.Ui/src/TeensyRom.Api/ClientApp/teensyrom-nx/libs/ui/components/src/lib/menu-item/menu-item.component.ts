import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MenuItem } from './menu-item.model';

@Component({
  selector: 'lib-menu-item',
  standalone: true,
  imports: [MatIconModule, MatListModule],
  templateUrl: './menu-item.component.html',
  styleUrls: ['./menu-item.component.scss'],
})
export class MenuItemComponent<T = unknown> {
  @Input() item!: MenuItem<T>;
  @Output() menuClick = new EventEmitter<MenuItem<T>>();

  onClick() {
    this.menuClick.emit(this.item);
  }
}
