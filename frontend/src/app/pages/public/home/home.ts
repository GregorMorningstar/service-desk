import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

interface Category {
  title: string;
  description: string;
  icon: string; // SVG path, viewBox 0 0 24 24
}

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
})
export class Home {
  protected readonly auth = inject(AuthService);

  /** Target of the main "Zgłoś problem" button */
  protected readonly ctaLink = computed(() =>
    this.auth.isLoggedIn() ? this.auth.homeUrl() : '/register',
  );

  protected readonly categories: Category[] = [
    {
      title: 'Sprzęt komputerowy',
      description: 'Awarie laptopów, monitorów, stacji dokujących i akcesoriów.',
      icon: 'M4 5h16a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1h-6v2h3v2H7v-2h3v-2H4a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1Zm1 2v8h14V7H5Z',
    },
    {
      title: 'Oprogramowanie',
      description: 'Instalacja, aktualizacje i błędy aplikacji biznesowych.',
      icon: 'M4 4h16a1 1 0 0 1 1 1v14a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1Zm1 4v10h14V8H5Zm2.5 2 2.5 2.5L7.5 15 6 13.5l1-1-1-1L7.5 10Zm3.5 4h5v1.5h-5V14Z',
    },
    {
      title: 'Konta i dostępy',
      description: 'Reset hasła, odblokowanie konta, nowe uprawnienia.',
      icon: 'M12 2a5 5 0 0 1 5 5v3h1a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V11a1 1 0 0 1 1-1h1V7a5 5 0 0 1 5-5Zm0 2a3 3 0 0 0-3 3v3h6V7a3 3 0 0 0-3-3Zm0 9a2 2 0 0 0-1 3.7V19h2v-2.3A2 2 0 0 0 12 13Z',
    },
    {
      title: 'Sieć i VPN',
      description: 'Brak internetu, problemy z Wi-Fi i połączeniem VPN.',
      icon: 'M12 18a2 2 0 1 1 0 4 2 2 0 0 1 0-4Zm0-5a7 7 0 0 1 5 2.1l-1.4 1.4a5 5 0 0 0-7.2 0L7 15.1A7 7 0 0 1 12 13Zm0-5c3.3 0 6.3 1.3 8.5 3.5l-1.4 1.4A10 10 0 0 0 12 10a10 10 0 0 0-7.1 2.9l-1.4-1.4A12 12 0 0 1 12 8Zm0-5c4.7 0 9 1.9 12 5l-1.4 1.4A15 15 0 0 0 12 5 15 15 0 0 0 1.4 9.4L0 8c3-3.1 7.3-5 12-5Z',
    },
    {
      title: 'Poczta i kalendarz',
      description: 'Konfiguracja skrzynki, udostępnianie kalendarzy, spam.',
      icon: 'M3 5h18a1 1 0 0 1 1 1v12a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1Zm1 3.2V17h16V8.2l-8 5.3-8-5.3ZM5.4 7l6.6 4.4L18.6 7H5.4Z',
    },
    {
      title: 'Drukarki i skanery',
      description: 'Zacięcia papieru, sterowniki, dodawanie drukarek sieciowych.',
      icon: 'M7 3h10v4h3a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1h-3v4H7v-4H4a1 1 0 0 1-1-1V8a1 1 0 0 1 1-1h3V3Zm2 2v2h6V5H9Zm0 10v4h6v-4H9Zm-4-6v6h2v-2h10v2h2V9H5Z',
    },
  ];

  protected readonly steps = [
    {
      title: 'Zgłoś problem',
      description: 'Załóż konto i opisz, co się stało. Wybierz kategorię i priorytet.',
    },
    {
      title: 'Śledź postęp',
      description: 'Na bieżąco widzisz status zgłoszenia i odpowiedzi technika.',
    },
    {
      title: 'Problem rozwiązany',
      description: 'Potwierdź rozwiązanie i oceń jakość obsługi.',
    },
  ];

  protected readonly faq = [
    {
      q: 'Jak założyć zgłoszenie?',
      a: 'Zarejestruj się, zaloguj i przejdź do swojego panelu. Każde nowe konto otrzymuje rolę Użytkownik, która pozwala tworzyć zgłoszenia.',
    },
    {
      q: 'Ile trwa obsługa zgłoszenia?',
      a: 'Zgłoszenia krytyczne obsługujemy natychmiast, pozostałe zgodnie z priorytetem — zwykle w ciągu jednego dnia roboczego.',
    },
    {
      q: 'Zapomniałem hasła — co zrobić?',
      a: 'Skontaktuj się z działem IT telefonicznie lub poproś administratora o reset hasła.',
    },
    {
      q: 'Kto widzi moje zgłoszenia?',
      a: 'Ty, moderatorzy obsługujący zgłoszenia oraz administratorzy systemu.',
    },
  ];
}
