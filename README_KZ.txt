«ТИЫН ЖИНАУШЫ»: 3, 4, 5-модуль толық ойыны
==========================================

ҚАЛАЙ АШУ КЕРЕК
1. Zip-ті ашыңыз. Unity Hub → Add → Add project from disk → CoinCollector папкасын таңдаңыз
   (Unity 6, 6000.6.0f1 үшін дайындалған).
2. Бірінші ашылу бірнеше минут алады. Скрипттер компиляцияланған соң ойын ӨЗДІГІНЕН құрылады:
   TMP Essentials («Import Anyway» шықса, басыңыз), 4 сахна, Coin префабы, CoinPulse анимациясы,
   қазақ шрифті, тегтер, Build Settings. Соңында «Ойын дайын!» хабары шығады.
   Шықпаса: жоғарғы мәзір → Coin Collector → Build game.
3. Assets/Scenes папкасынан сахнаны екі рет басып ашыңыз (Game, Level2, Level3, MainMenu).
   Әр деңгейде жер, платформалар, шиптер, тиындар, кейіпкер Scene терезесінде көрінеді.
   Мәзір мен ойын мәтіндері Canvas-та: Game терезесінде көресіз.
4. Ойнау үшін MainMenu сахнасын ашып, Play басыңыз.

БАСҚАРУ: A / D немесе ← → жүру, Пробел секіру, Esc пауза.

ЕГЕР ӨЗ ЖОБАҢЫЗҒА КӨШІРГІҢІЗ КЕЛСЕ
Unity Hub → New project → 2D шаблоны → жобаны жасаңыз → Unity-ді жабыңыз → осы архивтегі
Assets папкасын жобаның Assets папкасына көшіріңіз → Unity-ді ашыңыз.

ДЕҢГЕЙЛЕР
  Game (1-деңгей)  5 тиын, 3 шип, тегіс жер. Шиптердің үстінде тиындар: секіру керек.
  Level2           8 тиын, 5 шип, 5 ауадағы платформа.
  Level3 (соңғы)   10 тиын, 6 шип, ең биік платформалар. «Келесі деңгей» батырмасы жоқ.

ТАПСЫРМАЛАР ҚАЙДА
  3-модуль         Player (көк шаршы, Collider 2D + Rigidbody 2D, Freeze Rotation Z), Ground, Coin, Spike,
                   теглер Ground / Coin / Enemy, Player скрипті, Spawner (Game сахнасында өшірулі тұр).
  4-модуль         Ұпай мен өмір мәтіні, басты мәзір (Ойнау / Шығу), Game Over панелі (Қайта бастау / Мәзір),
                   музыка (Main Camera) және тиын дыбысы, жыпылықтау (корутин Blink), қазақ шрифті.
  4 үй жұмысы      HealthSlider (өмір жолағы), WinPanel (жеңіс), CoinPulse анимациясы, пауза (Esc, PauseManager).
  5-модуль         Жеңіс панелі + «Келесі деңгей», Level2, рекорд (PlayerPrefs, уақыт пен рекорд жеңіс панелінде),
                   Build Settings: MainMenu(0), Game(1), Level2(2), Level3(3). Product Name = CoinCollector,
                   Windowed 1280x720.
  5 үй жұмысы      Level3, мәзірде рекорд («Рекорд: 14.2 с» не «Рекорд жоқ»), .gitignore файлы дайын.

ӨЗІҢІЗ ІСТЕЙТІН ЖЕРЛЕР
  .exe жасау       File → Build Profiles (Build Settings) → Windows → Build → бос «Build» папкасын таңдаңыз.
  GitHub           GitHub Desktop → Add local repository → CoinCollector папкасы → Git Ignore: Unity → Commit → Publish.
  Скриншоттар      Мәзір (рекордпен), ойын көрінісі, жеңіс панелі.
  Рекордты тазалау Edit → Clear All PlayerPrefs.
