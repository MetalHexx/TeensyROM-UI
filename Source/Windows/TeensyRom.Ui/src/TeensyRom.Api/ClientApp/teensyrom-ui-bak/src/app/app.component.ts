import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './features/header/header/header.component';  
import { TerminalViewComponent } from './features/terminal-view/terminal-view.component';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, TerminalViewComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'teensyrom-ui';
}
