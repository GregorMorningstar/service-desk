import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { faClipboardCheck, faClockRotateLeft, faGauge, faLaptop } from '@fortawesome/free-solid-svg-icons';
import { SubNav, SubNavItem } from '../../../components/sub-nav/sub-nav';

/** Layout of the moderator pages: submenu on top, the child route below. */
@Component({
  selector: 'app-moderator-layout',
  imports: [RouterOutlet, SubNav],
  styleUrl: './moderator-layout.css',
  templateUrl: './moderator-layout.html',
})
export class ModeratorLayout {
  protected readonly items: SubNavItem[] = [
    { link: '/moderator', label: 'Panel', icon: faGauge, exact: true },
    { link: '/moderator/akceptacja', label: 'Do akceptacji', icon: faClipboardCheck },
    { link: '/moderator/zgloszenia', label: 'Zgłoszenia', icon: faClockRotateLeft },
    { link: '/moderator/sprzet', label: 'Sprzęt', icon: faLaptop },
  ];
}
