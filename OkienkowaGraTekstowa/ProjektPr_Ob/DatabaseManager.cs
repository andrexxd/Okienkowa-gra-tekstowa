using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ProjektPr_Ob
{
    public class DatabaseManager
    {
        private static DatabaseManager? instance;
        private SqliteConnection connection;
        private string query;

        public static DatabaseManager Instance => instance ??= new DatabaseManager();

        private DatabaseManager()
        {

        }

        public void OpenConnectionForCreateDefaultStory()
        {
            if (!File.Exists(Form1.defaultDatabaseStoryPath))
            {
                Directory.CreateDirectory(Form1.defaultStoryPath);
                connection = new SqliteConnection($"Data Source={Form1.defaultDatabaseStoryPath}");
                connection.Open();
                CreateTables();
                InsertInitialData();
                connection.Close();
            }
        }
        public void OpenConnection(string historyDatabasePath, string historyDatabaseName)
        {
            string databasePath = Path.Combine(historyDatabasePath, historyDatabaseName);
            connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();
            //MessageBox.Show($"Wybrano historię:{historyDatabaseName}");
        }

        public void CloseConnection()
        {
            connection?.Close();
        }

        //public SqliteConnection GetConnection()
        //{
        //    return connection;
        //}

        public void CreateTables()
        {
            #region CreateTables
            string createChaptersTable = @"
                CREATE TABLE IF NOT EXISTS Chapters (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL
                );";

            string createStoryTable = @"
                CREATE TABLE IF NOT EXISTS Story (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ChapterId INTEGER NOT NULL,
                    Text TEXT NOT NULL,
                    BreakText INTEGER DEFAULT 0,
                    
                    FOREIGN KEY (ChapterId) REFERENCES Chapters (Id)

                );";

            string createLocationsTable = @"
                  CREATE TABLE IF NOT EXISTS Locations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL
                );";

            string createLocationConnectionsTable = @"
            CREATE TABLE IF NOT EXISTS LocationConnections (
            FromLocationId INTEGER NOT NULL,
            ToLocationId INTEGER NOT NULL,
            FOREIGN KEY (FromLocationId) REFERENCES Locations(Id),
            FOREIGN KEY (ToLocationId) REFERENCES Locations(Id)
            );";

            string createCharactersTable = @"
            CREATE TABLE IF NOT EXISTS Characters (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            CurrentLocationId INTEGER NOT NULL,
            DialogueId INTEGER NOT NULL,
            FOREIGN KEY (CurrentLocationId) REFERENCES Locations(Id)
            );";

            string createItemsTable = @"
            CREATE TABLE IF NOT EXISTS Items (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Description TEXT NOT NULL,
            DescriptionInGame TEXT NULL,
            LocationId INTEGER NOT NULL, -- Lokacja, w której znajduje się przedmiot
            IsCollectible INTEGER NOT NULL, -- Czy przedmiot można podnieść (1 = Tak, 0 = Nie)
            UsedOnce BOOL NOT NULL,
            FOREIGN KEY (LocationId) REFERENCES Locations(Id)
            );";

            string createInteractionsTable = @"
            CREATE TABLE IF NOT EXISTS Interactions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, -- Unikalne ID interakcji
            Description TEXT NOT NULL,           -- Opis interakcji np. Podejdź do szafy
            LocationId INTEGER NOT NULL,         -- ID lokacji, do której należy interakcja
            ItemId INTEGER,                      -- ID przedmiotu, jeśli dotyczy przedmiotu
            IsActive BOOL,
            FOREIGN KEY (LocationId) REFERENCES Locations(Id), -- Powiązanie z tabelą Locations
            FOREIGN KEY (ItemId) REFERENCES Items(Id)          -- Powiązanie z tabelą Items (jeśli dotyczy przedmiotu)
            );";


        string createInteractionActions = @"
            CREATE TABLE IF NOT EXISTS InteractionActions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ActionDescription TEXT NOT NULL,     -- Opis akcji np. Przeszukaj szuflady
            ActionResultDescription TEXT NOT NULL, -- Wynik akcji np. Znaleziono klucz do pokoju
            ActionResultFailed TEXT NOT NULL,
            RewardItemId INTEGER NULL,
            RequiredItemId INTEGER NULL,
            ConnectedLocationId INTEGER NULL,
            UnconnectedLocationId INTEGER NULL,
            IsActive BOOL,
            RequiredOneInteraction BOOL,
            HasOptionalInteraction BOOL,
            TwoSidedConnection BOOL,
            IsExcecuted BOOL,
            IsMainAction BOOL,
            InteractionId INTEGER NOT NULL);
        
            ";

            string createInteractParargraphsTable = @"
            CREATE TABLE IF NOT EXISTS InteractionsParagraphs (
            Id INTEGER PRIMARY KEY,
            DESCRIPTION TEXT NOT NULL,
            LocationId INTEGER NOT NULL,
            RewardItemId INTEGER NOT NULL,
            RequiredItemId INTEGER NOT NULL,
            FirstConnectedLocationId INTEGER NOT NULL,
            SecondConnectedLocationId INTEGER NOT NULL,
            FirstUnconnectedLocationId INTEGER NOT NULL,
            SecondUnconnectedLocationId INTEGER NOT NULL,
            ToSidedConnectionLocation BOOL,
            ItemId INTEGER NOT NULL,
            QuestId INTEGET NOT NULL,
            ParagraphId INTEGER NOT NULL,
            InteractionId INTEGER NOT NULL,
            InteractionNPCId INTEGER NOT NULL,
            MapImageId INTEGER NOT NULL,
            MapImageActive BOOL NOT NULL,
            DialogueId INTEGER NOT NULL,
            DIalogueRowId INTEGER NOT NULL);
            
            ";

            string createInteractionsForNPC = @"
            CREATE TABLE IF NOT EXISTS InteractionsForNPC (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Description TEXT NOT NULL,
            DescriptionInGame TEXT NOT NULL,
            CharacterId INTEGER NOT NULL,
            IsActive BOOL NOT NULL,
            FOREIGN KEY (CharacterId) REFERENCES Characters(Id)
            );";

            string createInteractionActionsForNPC = @"
            CREATE TABLE IF NOT EXISTS InteractionActionsForNPC (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            RowDialogue TEXT NOT NULL,
            RowDialogueId INTEGER NOT NULL,
            NextDialogueRowId TEXT NOT NULL,
            DialogueId INTEGER NOT NULL,
            NextDialogueId INTEGER NOT NULL,
            IsTree BOOL NOT NULL,
            LocationId INTEGER NOT NULL,
            RewardItemId INTEGER NOT NULL,
            RequiredItemId INTEGER NOT NULL,
            FirstConnectedLocationId INTEGER NOT NULL,
            SecondConnectedLocationId INTEGER NOT NULL,
            FirstUnconnectedLocationId INTEGER NOT NULL,
            SecondUnconnectedLocationId INTEGER NOT NULL,
            ToSidedConnectionLocation BOOL,
            ItemId INTEGER NOT NULL,
            QuestId INTEGET NOT NULL,
            ParagraphId INTEGER NOT NULL,
            InteractionId INTEGER NOT NULL,
            InteractionNPCId INTEGER NOT NULL,
            MapImageId INTEGER NOT NULL,
            MapImageActive BOOL NOT NULL,
            IsActive BOOL NOT NULL,
            IsEndingDialogue BOOL NOT NULL
            );";

            string createDialoguesTable = @"
            CREATE TABLE IF NOT EXISTS DialogueTable (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CharacterId INTEGER,
            DialogueRowOrderId INTEGER,
            NextDialogueId INTEGER
            );";

            string createDialogueRowsTable = @"
            CREATE TABLE IF NOT EXISTS DialogueRowsTable (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Dialogue TEXT NOT NULL,
            DialogueId INT NOT NULL,
            DialogueRowOrderId INTEGER
            );";

            string createPlayerInventoryTable = @"
            CREATE TABLE IF NOT EXISTS PlayerInventory (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, -- Unikalne ID w ekwipunku
            ItemId INTEGER NOT NULL,              -- ID przedmiotu z tabeli Items
            FOREIGN KEY (ItemId) REFERENCES Items(Id) ON DELETE CASCADE -- Powiązanie z tabelą Items
            );";

            string createMapTable = @"
            CREATE TABLE IF NOT EXISTS MapTable (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ImageOrder INTEGER,
            Name TEXT NOT NULL,
            ImageData VARBINARY(100),
            IsActive BOOL DEFAULT True
            );";

            string createProgressTable = @"
            CREATE TABLE IF NOT EXISTS ProgressTable (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            IsGameWasLaunch BOOL,
            CurrentChapterId INTEGER,
            CurrentParagraphId INTEGER,
            IsOnGamePanel BOOL,
            IsGameEnd BOOL,
            CurrentTrackedQuest INTEGER
            );";

            string createQuestsTable = @"
            CREATE TABLE IF NOT EXISTS QuestsTable (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Description TEXT NOT NULL,
            IsActive BOOL,
            CurrentObjectiveOrder INTEGER NOT NULL,
            IsSelected BOOL,
            IsExcecuted BOOL,
            ContinueHistory BOOL
            )";

            string createObjectiveTable = @"
            CREATE TABLE IF NOT EXISTS ObjectivesTable (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Description TEXT NOT NULL,
            QuestId INTEGER NOT NULL,
            OrderObjective INT NOT NULL,
            InteractionActionId INTEGER NOT NULL,
            InteractionActionNPCId INTEGER NOT NULL,
            IsActive BOOL NOT NULL,
            IsExcecuted BOOL NOT NULL,
            IsOptionalObjective BOOL NOT NULL
            )";

            #endregion

            #region createQueries
            using (var command = new SqliteCommand(createChaptersTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createStoryTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createLocationsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createLocationConnectionsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createCharactersTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createItemsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createInteractionsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createInteractionActions, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createInteractParargraphsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createInteractionsForNPC, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createInteractionActionsForNPC, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createDialoguesTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createDialogueRowsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createPlayerInventoryTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createMapTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createProgressTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createQuestsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createObjectiveTable, connection))
            {
                command.ExecuteNonQuery();
            }
            #endregion
        }

        public void InsertInitialData()
        {
            #region InsertInTable
            string insertChapters = @"
                INSERT INTO Chapters (Id, Title)
                VALUES
(0, 'Prolog'),
(1, 'Ucieczka z sierocińca'),
(2, 'Pobódka w kompleksie');
                    ";

            string insertStory = @"
                INSERT INTO Story (Id, ChapterId, Text, BreakText)
                VALUES
(1,0, 'Nikt do końca nie wierzył, że coś takiego mogłoby się wydarzyć jeszcze raz. Inkwizytor złamał swoje śluby, ponownie. Więził i torturował niewinnych, ponownie. Kreował i realizował swoje chore pomysły. Ponownie.
	Ksiądz biskup Szczepanik studiował jeszcze raz wszystkie zebrane na miejscu notatki, tym razem omijając zdjęcia z miejsca zdarzenia. Te, mimo że, zobaczył je tylko raz, już na zawsze wypaliły mu się pod powiekami, zmuszając do powrotu myślami do tego miejsca. 
	Do pomieszczenia wszedł jeden z biskupów pomocniczych.
- [i]Przyjechał Freita Pelhor.[/i]
- [i]Wprowadź go.[/i]', 0),
(2, 0, 'Mężczyzna wszedł do pomieszczenia nie czekając na zaproszenie. Bez słowa usiadł na krześle i gestem odprawił młodego biskupa.
- [i]Trudności na drodze?[/i] - spytał biskup Szczepanik.
- [i] W Bellborn trwają zamieszki. Musiałem czekać na zgodę z biskupa diecezjalnego. Na marginesie, powinieneś mnie wezwać bezpośrednio na miejsce. Miałbym pełniejszy obraz całej sytuacji.[/i]
- [i]Dobrze wiesz, że nie zależało to ode mnie.[/i]
- [i]W takim razie nie oczekuj, że wydobędę z niego całą prawdę. To co zobaczymy będzie zniekształcone przez moją interpretację zdjęć, które mi przysłałeś, a te pokazują tylko wycinek.[/i]
- [i]Sensacja i rozgłos nadały tej sprawie złego wydźwięku. Chodzi nam przede wszystkim o ustalenie, jak do tego doszło i jak zapobiec podobnym… incydentom. Nie o to, kto był winny.[/i]
- [i]Zdaje sobie sprawę, że z chęcią zamieciecie sprawę pod dywan, ale nie o tym teraz. W takim razie zacznijmy.[/i]', 0),
(3, 0, 'Pomieszczenie, w którym przetrzymywany był ocalały, było wypełnione dymem z kadzidełek rozstawionych po całym pokoju tak, żeby w każdym miejscu w pokoju, pokrzywdzony był w zasięgu otępiającego dymu. Jak się okazało, większość z nich była zbędna, ponieważ chłopak siedział cały czas w jednym miejscu, wpatrując się pustym wzrokiem w ścianę przed sobą. Zareagował dopiero, kiedy biskup Szczepanik się odezwał.
- [i]Szczęść Boże.
- [i]Boga nie ma. – odparł chłopak, przenosząc swój pusty wzrok na księdza. – I nigdy go nie było.
- [i]Na samym początku chciałem cię przeprosić, że nie pozwalamy ci zwyczajnie odreagować całej sytuacji. I zamiast pomóc ci to przepracować, cały czas czegoś od ciebie wymagamy.[/i]
- [i]Czegoś takiego nie da się przepracować.[/i] – powiedział Pelhor siadając naprzeciwko chłopaka. - [i]Rozumiem przez co przechodzisz…[/i]
- [i]Gówno rozumiesz! – krzyknął chłopak rzucając się na freitę, jednak ten chwycił go mocno za ramię.[/i]
- [i]Rozumiem przez co przechodzisz. – powtórzył freita patrząc chłopakowi w oczy.[/i]',0),
(4, 0, '	Chłopak poczuł się jakby patrzył właśnie w lustro. Te samo puste, pozbawione jakiejkolwiek emocji spojrzenie, które widział codziennie rano. Spojrzenie, które nie powinno należeć do człowieka, podszyte desperacką próbą wyrażenia niewypowiedzianego cierpienia, które powinno się w nim znaleźć. Obce spojrzenie, które teraz było jego częścią.
	Chłopak rozluźnił mięśnie i dał się posadzić z powrotem na krześle.
- [i]Czego ode mnie chcecie?[/i]
- [i]Musisz opowiedzieć nam wszystko co się tam stało.[/i]
- [i]Nie… nie chcę… Nie chcę…[/i] - krzyczał chłopak głosem pełnym przerażenia. – [i]Nie wrócę tam… nie wrócę.[/i]
Biskup Szczepanik chciał jakoś zareagować, ale frieta powstrzymał go.',0),
(5, 0, '- [i]Jeżeli się nie zgodzisz, po prostu wejdę w twój umysł. Będę wywoływał te wspomnienia z twojej podświadomości. Będziesz to przeżywał we śnie raz za razem dopóki nie znajdę wszystkiego bez twojego udziału. My zdobędziemy co będziemy chcieli, ale ciebie doprowadzi to na skraj załamania nerwowego i sprawi, że nawet za kilka lat, jak będziesz kładł się spać będziesz to śnił tak wyraźnie, jakby się to zdarzyło przed chwilą. Całymi dniami będziesz chciał, żeby to się skończyło, aż wreszcie targniesz się na swoje życie.[/i]
Słowa freity wywołały w chłopaku niemałe poruszenie. Ciężko było stwierdzić czy to były zwykłe przewidywania czy mówił z własnego doświadczenia. Mimo wszystko podziałało.
- [i]Zacznijmy od początku. Jak i dlaczego w ogóle pojechaliście do starego kompleksu Inkwizycji?[/i]
- [i]W ogóle nie chcieliśmy tam jechać. Mieliśmy we czwórkę uciec z tego przeklętego sierocińca do stolicy, albo i dalej. Po drodze wszystko się spieprzyło.[/i]', 0),
(6, 1,'Sierociniec powoli szykował się do snu. Rozmowy stawały się cichsze a budynek coraz bardziej pogrążał się w ciemnościach. Savik leżał w łóżku rzekomo przygotowany do snu, niecierpliwie wyczekując momentu, aż będą mogli uciec.
	Przygotowywali się na to od dłuższego czasu i wreszcie nadszedł ten dzień. Dzień ostatecznej wolności. Savik leżał nasłuchując odgłosów na zewnątrz. W takiej ciszy dosłownie każdy mniejszy dźwięk był wyraźnie słyszalny, kiedy ktoś potrafił się wsłuchać. Miał tylko nadzieje, że tym razem nie wydarzy się żaden niespodziewany incydent.
	Wreszcie wybiła określona godzina. W końcu mogli uciec z tego miejsca. Savik wstał po cichu z łóżka i przebrał się w przygotowane wcześniej ubrania. Następnie zabrał plecak spod łóżka, odpalił latarkę.',1),
(7, 1, 'Kiedy Savik był już gotowy do otwarcia drzwi, reszta jego grupy przyszła w wyznaczone miesjce.
- [i]Wszystko gotowe?[/i] - spytała Sylvia, starając się nie dyszeć zbyt głośno.
- [i]Tak.[/i] - opdarł Savik pokazując im urządzenie dezaktywujące. - [i]Kiedy tego użyjemy, będziemy musieli się sprężać. System rejestruje każde otwarcie drzwi.[/i]
- [i]Eric jest daleko, a do tego jest powolny. Nie złapie nas.[/i]
- [i]Nie znamy lokalizacji innych. To nie może być spacerek, tylko szybka akcja, rozumiemy się? Ja zostanę z tyłu i zamknę drzwi.[/i]
Wszyscy przytaknęli i przygotowali się do biegu. Savik przystawił urządzenie do terminala drzwi.', 0),
(8, 1, 'Zamek otworzył się i w jednej chwili puścili się biegem przez ogród w kierunku furtki. Sevik tak jak powiedział, został z tyłu, zamykając drzwi. Widząc jeszcze obok ciężki pojemnik, przewrócił go, blokując drzwi i również zaczął biec. 
	 W oddali słyszeli krzyki i szczekanie psów, jednak nie miało to dla nich już większego znaczenia. Furtka była jeszcze tylko kilka metrów przed nimi, więc odgłosy pogoni jeszcze bardziej ich motywowały. 
	Sevik z satysfakcją dostrzegł, że strażnicy zgodnie z planem musieli wyjść innym wyjściem, więc kiedy wszyscy wybiegli przez furtkę mieli sporą przewagę. Mimo to nie zwolnili, śmiejąc się sami do siebie, że wszystko się udało.
    Pomimo, że mieli spory zapas czasowy, postanowili nie tracić czasu. Władzę sierocińca napewno zaalarmują strażników miejskich, więc niezwłocznie udali się na miejsce, skąd miał ich zabrać przewoźnik.
    Ten powitał ich skinieniem głowy i wyciągniętą dłonią, na której Savik położył mieszek z odliczoną kwotą. Wtedy dopiero, zaprosił ich do środka swojego wozu i kiedy wszyscy już byli w środku, strzepnąl lejcami i wyjechali z miasta.', 0),
(9, 1, '- [i]Kim był ten przewoźnik?[/i] - spytał Biskup Szczepanik.
- [i]Nie wiem.[/i] - odparł Savik. - [i]Nie podał nazwiska, ani nic. Wiedzieliśmy tylko, że za opłatą przewozi ludzi do wyznaczonych miejsc. Unikając kontroli na granicach miast.[/i]
- [i]Na tamtych terenach działały przynajmniej trzy grupy przemytnicze. Roześlę ludzi, żeby zbadali sprawę.[/i]
- [i]Co było dalej?[/i]
- [i]Udało nam się wyjechać z miasta bez kontroli. Kiedy myśleliśmy, że już wszystko się uda, zostaliśmy uśpieni. Obudziliśmy się osobno w nieznanym nam miejscu.[/i]', 0),
(10, 2, '   Obudziło go przenikliwe zimno, musiała minąć chwila zanim doszedł do siebie i przypomniał sobie co się faktycznie stało. Zerwał się z łóżka na równe nogi i od razu tego pożałował. Ból i zawroty głowy przyćmiły jego wzrok i sprawiły, że musiał usiąść.
Kiedy dolegliwości osłabły chłopak rozejrzał się po pokoju, w którym się obecnie znajdował. Była to mała cela z pryczą przywieszoną łańcuchami do ściany i małym stolikiem. Bez wątpienia była to cela więzienna. Źródłem chłodu były nieszczelności w murowanych ścianach, przez, które wiał chłodny wiatr.
""[i]Będę musiał znaleźć pozostałych[/i] - pomyślał Savik stając niepewnie na nogach. - [i]O ile jeszcze żyją.[/i]""', 1),
(11, 2, '   Klucz zadźwięczał w zamku umożliwiając Savikowi przejście do drugiej części budynku. Kiedy odchylił drzwi znalazł się w takim samym korytarzu jak przed chwilą. Wiele drzwi ciągnęło się w wzdłuż obydwu ścian, jednak te miały małe szybki, pozwalające zajrzeć do środka. Kiedy Savik zajrzał przez jedną, szybko cofnął się i skulił w odruchu wymiotnym. 
W celi znajdowały się rozkładające się już zwłoki, których gnijący odór przedostawał się przez nieszczelności w drzwiach. Na każdych zwłokach widać było ślady tortur, a niektóre były połączone rurkami z dziwnymi aparaturami, o których nie potrafił powiedzieć do czego służą.', 0),
(12, 2, '   Idąc dalej Savik do sali widokowej z widokiem na jedno z laboratoriów. Na stole leżała Weronika, nad którą stało dwóch ludzi w białych fartuchach. Zanim jednak zdążył jakkolwiek zareagować, został zauważony przez uzbrojonych strażników, którzy w jednej chwili odcięli mu drogę ucieczki. Jeden z nich obezwładnił go i unieruchomił a drugi wbił mu strzykawkę w szyje. Wzrok Savika w jednej chwili rozmył się, a jego ciało stało się bezwładne.',0);
                ";

            string insertLocations = @"
            INSERT INTO Locations (Id, Name, Description) VALUES
            (0, 'Pusta lokacja', ''),
            (1,'Pokój Sevika', 'Mały, dwuosobowy pokój mieszkalny zapewaniający absolutne minimum komfortu.'),
            (2, 'Korytarz', 'Pomimo światła latarki, końce korytarza ginęły w mroku. Savik trzymał latarkę nisko, starając się nie świecić na drzwi, których było bardzo dużo w obawie, że kogoś obudzi.'),
            (3, 'Główny hol', ''),
            (4, 'Zachodni korytarz.','Na jednej z ławek korytarza leżał strażnik w przyciasnym mundurze, przysnął z na wpół pustą butelką alkoholu w ręce, który przemycał mu jeden z kucharzy.'),
            (5, 'Pokój strażników','Pomieszczenie niczym nie przypominało pokoju strażników. Bardziej przywodziło na myśl, starą kanciapę, schowek, albo zamieszkałą przez pijaków melinę.'),
            (6, 'Wyjście na ogród', 'Na końcu zachodniego korytarza znajdowały się drzwi, do których było potrzebny kod, albo urządzenie dezaktywujące.'),
            (7, 'Cela Savika', 'Mała cela więzienna z pryczą przywieszoną łańcuchami do ściany i małym stolikiem.'),
            (8, 'Korytarz więzienny', 'Długi korytarz z mnóstwem drzwi, prowadzących takich samych cel. Pod każdą znajdowały się ślady krwi i czegoś jeszcze.'),
            (9, 'Cela 125', 'Jedyna otwarta cela, w której siedział przykuty do ściany, wychudzony mężczyzna, który pustym wzrokiem wpatrywał się w ścianę.'),
            (10,'Drzwi do drugiego skrzydła', 'Na końcu korytarza znajdowały się dzrzwi, najpewniej prowadzące do drugiej części budynku.'),
            (11,'Drugi korytarz', 'Ciągnący się w nieskończoność korytarz prowadzący do dużej ilości pomieszczeń');
            ";

            string insertLocConnections = @"
            INSERT INTO LocationConnections (FromLocationId, ToLocationId) VALUES
            (1,2),
            (2,1),
            (2,3),
            (3,2),
            (3,4),
            (4,3),
            (4,6),
            (6,4),
            (7,8),
            (8,7),
            (8,9),
            (9,8);

            ";

            string insertCharacters = @"
            INSERT INTO Characters (Id, Name, CurrentLocationId, DialogueId) VALUES
            (1, 'Player', 1, 0),
            (2, 'Wychudzony mężczyzna z celi 125',9,1);

            ";

            string insertItems = @"
            INSERT INTO Items (Id, Name, Description, DescriptionInGame, LocationId, IsCollectible, UsedOnce) VALUES
            (0,'Pusty item', '','',0,0,0),
            (1,'Klucze strażnika', 'Klucze potrzebne do otwracia pokoju strażników', '', 4,1,0),
            (2,'Urządzenie dezaktywujące', 'Urządzenie wyłączające zabezpieczenia drzwi, którymi mieli uciec.', '', 5, 1,1),
            (3,'Drzwi strażników', 'Drzwi prowadzące do pomieszczenia strażników', 'Drzwi prowadzące do pomieszczenia strażników', 4, 0,0),
            (4,'Śpiący, pijący strażnik', 'Śpiący, pijący strażnik', 'Strażnik w przyciasnym mundurze, przysnął z na wpół pustą butelką alkoholu w ręce, który przemycał mu jeden z kucharzy.', 4, 0,0),
            (5,'Biurko strażników', 'Stare biurko z zamkniętą na klucz szufladą.', 'Stare biurko z zamkniętą na klucz szufladą.', 5, 0,0),
            (6,'Drzwi na ogród', 'Wzmocnione drzwi na kod.', 'Wzmocnione drzwi na kod', 6, 0,0),
            (7,'Klucz do drugiego skrzydła','Klucz do drugiego skrzydła','Klucz do drugiego skrzydła', 9,1,1),
            (8,'Drzwi do drugiego skrzydła','Wzmocnione drzwi, których nie da się wyszarpać', '', 8,0,0);
            ";

            string insertInteractions = @"
            INSERT INTO Interactions (Id, Description, LocationId, ItemId, IsActive) 
            VALUES 
            (1, 'Przeszukaj biurko', 5, 5,true),
            (2, 'Podejdź do drzwi', 6, 6, true),
            (3, 'Podejdź do drzwi strażników', 4, 3, true),
            (4, 'Podejdź do strażnika', 4, 4, true),
            (5, 'Podejdź do drzwi', 8,8,true);
            ";

            string insertInteractionActions = @"
            INSERT INTO InteractionActions (Id, ActionDescription, ActionResultDescription, ActionResultFailed, RewardItemId, 
RequiredItemId, ConnectedLocationId, UnconnectedLocationId, IsActive, InteractionId, RequiredOneInteraction, HasOptionalInteraction, TwoSidedConnection, 
IsExcecuted, IsMainAction)
            VALUES
            (1, 'Otwórz drzwi', 'Drzwi otwarte', 'Potrzebujesz klucza', 0,1,5,0,true,3, true, false, true,false, true),
            (2, 'Otwórz zamkniętą szufladę', 'Znalazłeś urządzenie dezaktywujące', 'Potrzebujesz klucza', 2,1,0,0,true,1, true, false, false,false, true),
            (3, 'Zabierz mu klucze', 'Zdobyłeś klucze strażnika.', '', 1,0,0,0,true,4, true, false, false,false, true),
            (4, 'Otwórz drzwi na ogród', 'Drzwi zostały otwarte', 'Potrzebujesz urządznenia dezaktywującego', 0,2,0,0,true,2, true, false, false,false, true),
            (5, 'Otwórz drzwi','Drzwi otwarte','Potrzebujesz klucza',0,0,11,0,true,5,false,false,true,false,true),
            (6, 'Użyj klucza','Zamek otwarty', 'Klucz nie pasuje', 0,7,0,0,false,5,false,false,false,false,false);
            ";

            string insertInteractionsForNPC = @"
            INSERT INTO InteractionsForNPC (Id, Description, DescriptionInGame, CharacterId, IsActive)
            VALUES
            (1, 'Porozmawiaj z mężczyzną', 'Pod ścianą celi siedział wychudzony mężczyzna przykuty łańcuchami do ściany.', 2, true);
            ";

            string insertInteractionActionsForNPC = @"
            INSERT INTO InteractionActionsForNPC (Id, Name, RowDialogue, RowDialogueId, NextDialogueRowId, DialogueId, NextDialogueId, IsTree, LocationId, RewardItemId, 
            RequiredItemId, FirstConnectedLocationId, SecondConnectedLocationId, 
            FirstUnconnectedLocationId, SecondUnconnectedLocationId, ToSidedConnectionLocation, ItemId, QuestId, ParagraphId, InteractionId, InteractionNPCId, MapImageId, MapImageActive, IsActive, IsEndingDialogue)
            VALUES
            (1, 'Czy wszystko w porządku?', '- [i]Wszystko w porządku?[/i]',1,2,1,0,false,9,0,0,0,0,0,0,false,0,0,0,0,1,0,false,true,false),
            (2, 'Odejdź', '', 1,0,1,0,false,9,0,0,0,0,0,0,false,0,0,0,0,0,0,false,true,true),
            (3, 'Zaproponuj pomoc', '- [i]Mogę ci jakoś pomóc?[/i]', 1,3,1,0,false,9,0,0,0,0,0,0,false,0,0,0,0,0,0,false,true,false),
            (4, 'Dziękuję', 'Savik wziął klucz od mężczyzny.', 4,0,1,0,true,9,7,0,0,0,0,0,false,0,0,0,0,0,0,false,true,true),
            (5, 'Spytaj o swoich kolegów', '- [i]Widziałeś tutaj kogoś jeszcze? Dwóch chłopaków i dziewczynę, mniej więcej w moim wieku?[/i]', 1,4,1,0,true,9,0,0,0,0,0,0,false,0,0,0,0,0,0,false,true,true);
            ";

            string insertDialogueTable = @"
            INSERT INTO DialogueTable (Id, CharacterId, DialogueRowOrderId, NextDialogueId)
            VALUES
            (1, 2, 1, 2);
            ";

            string insertDialogueRowTable = @"
            INSERT INTO DialogueRowsTable (Id, Dialogue, DialogueId, DialogueRowOrderId)
            VALUES
            (1, 'Mężczyzna nie zaregował kiedy Savik do niego podszedł. Wpatrywał się pustym wzrokiem w podłogę nie poruszając się nawet o milimetr.', 1,1),
            (2, 'Mężczyzna słysząc głos popatrzył na Savika.
- [i]Uciekaj stąd. Nie możesz tu zostać niezależnie od wszystkiego.[/i]', 1,2),
            (3, '- [i]Dla mnie jest już za późno.[/i]', 1,3),
            (4, '- [i]Zabrali ich do drugiego skrzydła. Drzwi są zamknięte na klucz. Weź go, mi się i tak już na nic nie przyda.[/i]', 1,4);
            ";

            string insertInteractionsForParagraphs = @"
            INSERT INTO InteractionsParagraphs (Id, Description, LocationId, RewardItemId, RequiredItemId, FirstConnectedLocationId, SecondConnectedLocationId, 
            FirstUnconnectedLocationId, SecondUnconnectedLocationId, ToSidedConnectionLocation, ItemId, QuestId, ParagraphId, InteractionId, InteractionNPCId, MapImageId, MapImageActive, DialogueId, DialogueRowId)
            VALUES
            (1,'Wyłącz mape 1',7,0,0,0,0,0,false,0,0,0,10,0,0,1,false,0,0),
            (2,'Wyłącz mape 2',7,0,0,0,0,0,false,0,0,2,10,0,0,2,false,0,0),
            (3,'Usuń przedmiot.',7,0,1,0,0,0,false,0,0,2,10,0,0,2,false,0,0);

            ";

            string insertProgressTable = @"
            INSERT INTO ProgressTable (IsGameWasLaunch, CurrentChapterId, CurrentParagraphId, IsOnGamePanel, IsGameEnd, CurrentTrackedQuest) VALUES
            ('false', 0, 0, false, false, 0);";

            string insertPlayerInventoryTable = @"
            INSERT INTO PlayerInventory (Id, ItemId) VALUES

            ";

            string insertQuestsTable = @"
            INSERT INTO QuestsTable (Id, Name, Description, IsActive, CurrentObjectiveOrder, IsSelected, IsExcecuted, ContinueHistory)
            VALUES
(1, 'Ucieczka z sierocińca', 'Wszystkie drzwi, którymi możnaby uciec są zablokowane kodem. Na szczęście strażnicy mają urządzenia, które dezaktywują zabezpieczenia.', 1, 1, 1, 0, 1),
(2, 'Odnajdź pozostałych', 'Obudziłeś się w nieznanym ci miejscu. Postaraj się odszukać swoich towarzyszy.', 0,1,0,0,1);
            ";

            string insertObjectivesTable = @"
            INSERT INTO ObjectivesTable (Id, Description, QuestId, OrderObjective, InteractionActionId,InteractionActionNPCId, IsActive, IsExcecuted, IsOptionalObjective)
            VALUES
(1, 'Znajdź urządzenie dezaktywujące', 1, 1, 2, 0, true, false,false),
(2, 'Otwórz drzwi znalezionym urządzeniem', 1, 2, 4, 0, true, false,false),
(3, 'Przeszukaj pomieszczenia', 2, 1, 5, 0, true, false, false);
            ";

            #endregion

            //string insertMapTable = @"
            //INSERT INTO MapTable (Id, Name, MapImage)

            //";
            #region InsertQueries
            string relativeFolderPath = $"{Form1.defaultStoryPath}";
            string absoluteFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeFolderPath);

            InsertImagesToDatabase(relativeFolderPath, absoluteFolderPath, "GameDB.db");

            using (var command = new SqliteCommand(insertChapters, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertStory, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertLocations, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertLocConnections, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertCharacters, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertItems, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertInteractions, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertInteractionsForNPC, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertDialogueTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertDialogueRowTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertInteractionActions, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertInteractionActionsForNPC, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertInteractionsForParagraphs, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertProgressTable, connection))
            {
                command.ExecuteNonQuery();
            }

            //using (var command = new SqliteCommand(insertPlayerInventoryTable, connection))
            //{
            //    command.ExecuteNonQuery();
            //}

            using (var command = new SqliteCommand(insertQuestsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(insertObjectivesTable, connection))
            {
                command.ExecuteNonQuery();
            }

            #endregion

        }

        public string GetDialogueRowByCharacterId(int characterId, int dialogueRowId)
        {
            //var dialogue = new List<Dialogue>();
            string query = @"SELECT Dialogue FROM Characters ch
            JOIN DialogueTable dt ON dt.CharacterId = ch.Id
            JOIN DialogueRowsTable drt ON drt.DialogueId = dt.Id
            WHERE dt.CharacterId = @CharacterId AND drt.DialogueRowOrderId = @DialogueRowId AND drt.DialogueId = ch.DialogueId;";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CharacterId", characterId);
                command.Parameters.AddWithValue("@DialogueRowId", dialogueRowId);
                string dialogueRow = command.ExecuteScalar().ToString();
                return dialogueRow;
            }

        }

        public void SetDialogueIdForCharacter(int characterId, int dialogueId)
        {
            string query = "UPDATE Characters SET DialogueId = @DialogueId WHERE Id = @CharacterId";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CharacterId", characterId);
                command.Parameters.AddWithValue("@DialogueId", dialogueId);
                command.ExecuteNonQuery();
            }
        }

        public void SetDialogueActionNPCIdActive(int actionNpcId, bool isActive)
        {
            string query = "UPDATE InteractionActionsForNPC SET IsActive = @IsActive WHERE Id = @ActionNpcId";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ActionNpcId", actionNpcId);
                command.Parameters.AddWithValue("@IsActive", isActive);
                command.ExecuteNonQuery();
            }
        }

        public void SetInteractionActionForNpcActive(bool isActive, int actionId)
        {
            string query = "UPDATE InteractionActionsForNPC SET IsActive = @DIsActive WHERE Id = @ActionNPCId AND IsEndingDialogue = false";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DIsActive", isActive);
                command.Parameters.AddWithValue("@ActionNPCId", actionId);
                command.ExecuteNonQuery();
            }
        }

        public List<InteractionActionForNPC> GetInteractionActionForNPCsByDialogueRowId(int dialogueRowId)
        {
            var interactionActionsForNPC = new List<InteractionActionForNPC>();

            string query = "SELECT DISTINCT iafNPC.Name, RowDialogue, NextDialogueRowId, iafNPC.NextDialogueId, iafNPC.IsTree, LocationId, RewardItemId, RequiredItemId, FirstConnectedLocationId, SecondConnectedLocationId, FirstUnconnectedLocationId, SecondUnconnectedLocationId, ToSidedConnectionLocation, ItemId, QuestId, InteractionId, InteractionNPCId, MapImageId, MapImageActive, IsActive, iafNPC.Id, IsEndingDialogue FROM InteractionActionsForNPC iafNPC JOIN DialogueRowsTable drt ON drt.DialogueRowOrderId = iafNPC.RowDialogueId JOIN DialogueTable dt ON drt.DialogueRowOrderId = @DialogueRowId JOIN Characters ch ON ch.DialogueId = iafNPC.DialogueId\r\nWHERE iafNPC.DialogueId = drt.DialogueId ORDER BY NextDialogueRowId DESC;";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DialogueRowId", dialogueRowId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string rowDialogue = reader.GetString(1);
                        int nextDialogueRowId = reader.GetInt32(2);
                        int nextDialogueId = reader.GetInt32(3);
                        bool isTree = reader.GetBoolean(4);

                        int locationId = reader.GetInt32(5);
                        int rewardItemId = reader.GetInt32(6);
                        int requiredItemId = reader.GetInt32(7);
                        int firstConnectedLocationId = reader.GetInt32(8);
                        int secondConnectedLocationId = reader.GetInt32(9);
                        int firstUnconnectedLocationId = reader.GetInt32(10);
                        int secondUnconnectedLocationId = reader.GetInt32(11);
                        bool toSidedConnectedLocation = reader.GetBoolean(12);
                        int itemId = reader.GetInt32(13);
                        int questId = reader.GetInt32(14);
                        int interactionId = reader.GetInt32(15);
                        int interactionNPCId = reader.GetInt32(16);
                        int mapImageId = reader.GetInt32(17);
                        bool mapImageActive = reader.GetBoolean(18);
                        bool isActive = reader.GetBoolean(19);
                        int actionNPC = reader.GetInt32(20);
                        bool isEndingDialogue = reader.GetBoolean(21);
                        //string dialogue = reader.GetString(0);
                        //int dialogueRowOrderId = reader.GetInt32(1);

                        interactionActionsForNPC.Add(new InteractionActionForNPC(actionNPC, name, rowDialogue, nextDialogueRowId, nextDialogueId, isTree, locationId, rewardItemId, requiredItemId, firstConnectedLocationId, secondConnectedLocationId, firstUnconnectedLocationId, secondUnconnectedLocationId, toSidedConnectedLocation, itemId, questId, interactionId, interactionNPCId, mapImageId, mapImageActive, isActive, isEndingDialogue));
                    }
                }

            }
            return interactionActionsForNPC;
        }

        public int GetQuestIdByObjectiveFromActionId(int actionId, int actionNPCId)
        {
            string query = "";
            int globalAction = 0;
            if (actionId != 0 && actionNPCId == 0)
            {
                query = @"SELECT QuestId FROM ObjectivesTable WHERE InteractionActionId = @ActionId";
                globalAction = actionId;
            }
            else if (actionId == 0 && actionNPCId != 0)
            {
                query = @"SELECT QuestId FROM ObjectivesTable WHERE InteractionActionNPCId = @ActionId";
                globalAction = actionNPCId;
            }
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ActionId", globalAction);
                int returnActionId = Convert.ToInt32(command.ExecuteScalar());
                return returnActionId;
            }


        }

        public int GetCountFromMapTable()
        {
            string query = "SELECT COUNT(Id) FROM MapTable;";
            using (var command = new SqliteCommand(query, connection))
            {
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count;
            }

        }

        public void InsertImagesToDatabase(string relativeFolderPath, string absoluteFolderPath, string databaseName)
        {
            int count = GetCountFromMapTable();

            if (count == 0)
            {
                string[] supportedExtensions = { "*.jpg", "*.png", "*.jpeg" };

                foreach (string extension in supportedExtensions)
                {
                    string[] files = Directory.GetFiles(absoluteFolderPath, extension);

                    if (files.Length > 0)
                    {

                        foreach (string file in files)
                        {
                            // Pobieranie nazwy pliku
                            string fileName = Path.GetFileNameWithoutExtension(file);
                            string firstPart = fileName.Split('_')[0];
                            int orderImage = int.TryParse(firstPart, out int number) ? number : 0;

                            string newFileName = Regex.Replace(fileName, @"^\d+_", "");

                            // Konwersja zdjęcia na byte[]
                            byte[] imageBytes = File.ReadAllBytes(file);

                            // Zapytanie SQL do dodania obrazu do bazy
                            string query = "INSERT INTO MapTable (ImageOrder, Name, ImageData) VALUES (@ImageOrder, @Name, @ImageData)";
                            using (SqliteCommand command = new SqliteCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@ImageOrder", orderImage);  // Ustal Id, np. autoinkrementowane
                                command.Parameters.AddWithValue("@Name", newFileName);  // Ustal Id, np. autoinkrementowane
                                command.Parameters.AddWithValue("@ImageData", imageBytes);

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else if (databaseName == "GameDB.db")
                    {
                        string[] pngFiles = Directory.GetFiles(Form1.defaultDatabaseScheme, "*.png");

                        foreach (string file in pngFiles)
                        {
                            // Pobierz nazwę pliku
                            string fileName = Path.GetFileName(file);

                            // Ścieżka docelowa w folderze bazy danych
                            string destinationPath = Path.Combine(Form1.defaultStoryPath, fileName);

                            // Skopiuj plik
                            File.Copy(file, destinationPath, overwrite: true);
                            Console.WriteLine($"Skopiowano plik: {fileName}");
                        }
                    }
                }
            }

        }

        public void SetMapImageActive(int mapImageId, bool isActive)
        {
            string query = "UPDATE MapTable SET IsActive = @IsActive WHERE ImageOrder = @MapImageId";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IsActive", isActive);
                command.Parameters.AddWithValue("@MapImageId", mapImageId);
                command.ExecuteNonQuery();
            }

        }

        public int GetChaptersCount()
        {
            string query = @"SELECT COUNT(*) FROM Chapters";
            int chaptersCount = 0;

            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        chaptersCount = reader.GetInt32(0);
                    }
                }
            }
            return chaptersCount;
        }

        public (string chapterTitle, List<(int Id, string text, bool breakText)>) GetChapterData(int chapterId)
        {
            string chapterTitle = string.Empty;
            var storyData = new List<(int Id, string text, bool breakText)>();

            string queryChapter = "SELECT Title FROM Chapters WHERE Id = @ChapterId;";
            string queryStory = "SELECT Id, Text, BreakText FROM Story WHERE ChapterId = @ChapterId;";

            using (var command = new SqliteCommand(queryChapter, connection))
            {
                command.Parameters.AddWithValue("@ChapterId", chapterId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        chapterTitle = reader.GetString(0);
                    }
                }
            }

            using (var command = new SqliteCommand(queryStory, connection))
            {
                command.Parameters.AddWithValue("@ChapterId", chapterId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int Id = reader.GetInt32(0);
                        string text = reader.GetString(1);
                        bool breakText = reader.GetInt32(2) == 1; // 1 = true
                        storyData.Add((Id, text, breakText));
                    }
                }
            }

            return (chapterTitle, storyData);
        }

        public List<Quest> GetQuests(bool isExcecuted)
        {
            var quests = new List<Quest>();

            string query = "SELECT Id, Name, Description, IsActive, CurrentObjectiveOrder, IsSelected, IsExcecuted, ContinueHistory FROM QuestsTable WHERE IsActive = true AND IsExcecuted = @IsExcecuted";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IsExcecuted", isExcecuted);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int questId = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.GetString(2);
                        bool isActive = reader.GetBoolean(3);
                        int currentObjectiveOrder = reader.GetInt32(4);
                        bool isSelected = reader.GetBoolean(5);
                        //bool isExcecuted = reader.GetBoolean(6);
                        bool continueHistory = reader.GetBoolean(7);

                        quests.Add(new Quest(questId, name, description, isActive, currentObjectiveOrder, isSelected, isExcecuted, continueHistory));
                    }
                }

            }

            return quests;

        }

        public List<Quest> GetQuestsById(int questId)
        {
            var quests = new List<Quest>();

            string query = "SELECT Id, Name, Description, IsActive, CurrentObjectiveOrder, IsSelected, IsExcecuted, ContinueHistory FROM QuestsTable WHERE Id = @QuestId";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //int questId = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.GetString(2);
                        bool isActive = reader.GetBoolean(3);
                        int currentObjectiveOrder = reader.GetInt32(4);
                        bool isSelected = reader.GetBoolean(5);
                        bool isExcecuted = reader.GetBoolean(6);
                        bool continueHistory = reader.GetBoolean(7);

                        quests.Add(new Quest(questId, name, description, isActive, currentObjectiveOrder, isSelected, isExcecuted, continueHistory));
                    }
                }

            }

            return quests;

        }

        public int GetQuestIdBySelected()
        {
            string query = "SELECT Id FROM QuestsTable WHERE IsSelected = true";
            using(var command = new SqliteCommand(query, connection))
            {
                int questId = Convert.ToInt32(command.ExecuteScalar());
                return questId;
            }
        }

        public void SetQuestSelected(int questId)
        {
            string query = "UPDATE QuestsTable SET IsSelected = false";

            using (var command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }

            query = "UPDATE QuestsTable SET IsSelected = true WHERE Id = @QuestId";


            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.ExecuteNonQuery();
            }
        }

        public List<Objective> GetObjectives(int questId, int currentObjectiveOrder)
        {
            var objectives = new List<Objective>();
            string query;
            int countObjectives = GetCountObjectivesByOrderObjective(questId, currentObjectiveOrder);

            if (countObjectives == 1)
            {
                query = "SELECT Description, QuestId, OrderObjective, InteractionActionId, InteractionActionNPCId, IsActive, IsExcecuted, IsOptionalObjective FROM ObjectivesTable WHERE QuestId = @QuestId AND OrderObjective <= @CurrentObjectiveOrder ORDER BY OrderObjective DESC LIMIT @CurrentObjectiveOrder;"; 
            }
            else
            {
                query = "SELECT Description, QuestId, OrderObjective, InteractionActionId, InteractionActionNPCId, IsActive, IsExcecuted, IsOptionalObjective FROM ObjectivesTable WHERE OrderObjective = @CurrentObjectiveOrder AND QuestId = @QuestId;";
            }

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.Parameters.AddWithValue("@CurrentObjectiveOrder", currentObjectiveOrder);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string description = reader.GetString(0);
                        //int questId = reader.GetInt32(1);
                        int orderObjective = reader.GetInt32(2);
                        int interactionActionId = reader.GetInt32(3);
                        int interactionActionNPCId = reader.GetInt32(4);
                        bool isActive = reader.GetBoolean(5);
                        bool isExcecuted = reader.GetBoolean(6);
                        bool isOptionalObjective = reader.GetBoolean(7);

                        objectives.Add(new Objective(description, questId, orderObjective, interactionActionId, interactionActionNPCId, isActive, isExcecuted, isOptionalObjective));
                    }
                }

            }

            return objectives;

        }

        public int GetMaxObjectiveOrderByQuestId(int questId)
        {
            string query = @"SELECT MAX(OrderObjective) FROM ObjectivesTable WHERE QuestId = @QuestId;";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                int maxObjectiveOrder = Convert.ToInt32(command.ExecuteScalar());
                return maxObjectiveOrder;
            }
        }

        public int GetCountObjectivesByOrderObjective(int questId, int currentObjectiveOrder)
        {
            string query = @"SELECT COUNT(Id) FROM ObjectivesTable WHERE OrderObjective = @CurrentObjectiveOrder AND QuestId = @QuestId;";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.Parameters.AddWithValue("@CurrentObjectiveOrder", currentObjectiveOrder);
                int countObjectives = Convert.ToInt32(command.ExecuteScalar());
                return countObjectives;
            }
        }

        public int GetCountExcecutedObjectivesByOrderObj(int questId, int currentObjectiveOrder)
        {
            string query = @"SELECT COUNT(Id) FROM ObjectivesTable WHERE OrderObjective = @CurrentObjectiveOrder AND QuestId = @QuestId AND IsExcecuted = true;";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.Parameters.AddWithValue("@CurrentObjectiveOrder", currentObjectiveOrder);
                int countExcecutedObjectives = Convert.ToInt32(command.ExecuteScalar());
                return countExcecutedObjectives;
            }
        }

        public List<Objective> GetObjectiveByActionId(int interactionActionId, int interactionActionNPCId)
        {
            var objectives = new List<Objective>();
            int globalInteractionId = 0;
            string query = "";
            if (interactionActionId != 0 && interactionActionNPCId == 0)
            {
                query = "SELECT Description, QuestId, OrderObjective, InteractionActionId, InteractionActionNPCId, IsActive, IsExcecuted, IsOptionalObjective FROM ObjectivesTable WHERE InteractionActionId = @ActionId";
                globalInteractionId = interactionActionId;
            }
            else if(interactionActionId == 0 && interactionActionNPCId != 0)
            {
                query = "SELECT Description, QuestId, OrderObjective, InteractionActionId, InteractionActionNPCId, IsActive, IsExcecuted, IsOptionalObjective FROM ObjectivesTable WHERE InteractionActionNPCId = @ActionId";
                globalInteractionId = interactionActionNPCId;
            }
            using (var command = new SqliteCommand(query, connection))
            {
               
                command.Parameters.AddWithValue("@ActionId", globalInteractionId);
                
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string description = reader.GetString(0);
                            int questId = reader.GetInt32(1);
                            int orderObjective = reader.GetInt32(2);
                            //int interactionActionId = reader.GetInt32(3);
                            //int interactionActionNPCId = reader.GetInt32(4);
                            bool isActive = reader.GetBoolean(5);
                            bool isExcecuted = reader.GetBoolean(6);
                            bool isOptionalObjective = reader.GetBoolean(7);

                            objectives.Add(new Objective(description, questId, orderObjective, interactionActionId, interactionActionNPCId, isActive, isExcecuted, isOptionalObjective));
                        }
                }

            }
            return objectives;
        }

        public List<Objective> GetObjectiveByExcecuted(int questId, bool isExcecuted)
        {
            var objectives = new List<Objective>();
            string query = "SELECT Description, QuestId, OrderObjective, InteractionActionId, InteractionActionNPCId, IsActive, IsExcecuted, IsOptionalObjective FROM ObjectivesTable WHERE QuestId = @QuestId AND IsExcecuted = @IsExcecuted";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.Parameters.AddWithValue("@IsExcecuted", isExcecuted);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string description = reader.GetString(0);
                        //int questId = reader.GetInt32(1);
                        int orderObjective = reader.GetInt32(2);
                        int interactionActionId = reader.GetInt32(3);
                        int interactionActionNPCId = reader.GetInt32(3);
                        bool isActive = reader.GetBoolean(4);
                        //bool isExcecuted = reader.GetBoolean(5);
                        bool isOptionalObjective = (reader.GetBoolean(6));

                        objectives.Add(new Objective(description, questId, orderObjective, interactionActionId, interactionActionNPCId, isActive, isExcecuted, isOptionalObjective));
                    }
                }

            }
            return objectives;
        }

        public void UpdateCurrentObjectiveOrder(int questId)
        {
            string query = "UPDATE QuestsTable SET CurrentObjectiveOrder = CurrentObjectiveOrder + 1 WHERE Id = @QuestId ";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.ExecuteNonQuery();
            }
        }

        public void SetObjectiveIsExcecutedByDescription(string objectiveDescription)
        {
            string query = "UPDATE ObjectivesTable SET IsExcecuted = true WHERE Description = @ObjectiveDescription";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ObjectiveDescription", objectiveDescription);
                command.ExecuteNonQuery();
            }
        }

        public void SetQuestIsActive(int questId)
        {
            string query = "UPDATE QuestsTable SET IsActive = true WHERE Id = @QuestId";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.ExecuteNonQuery();
            }
        }

        public void SetQuestIsExcecuted(int questId)
        {
            string query = "UPDATE QuestsTable SET IsExcecuted = true WHERE Id = @QuestId";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@QuestId", questId);
                command.ExecuteNonQuery();
            }
        }


        public (string name, string description) GetCurrentLocation(int locationId)
        {
            string name = string.Empty;
            string description = string.Empty;

            string query = "SELECT Name, Description FROM Locations WHERE Id = @LocationId;";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@LocationId", locationId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        name = reader.GetString(0);
                        description = reader.GetString(1);
                    }
                }
            }

            return (name, description);
        }

        public List<int> GetConnectedLocations(int fromLocationId)
        {
            var connectedLocations = new List<int>();

            string query = "SELECT ToLocationId FROM LocationConnections WHERE FromLocationId = @FromLocationId;";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FromLocationId", fromLocationId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        connectedLocations.Add(reader.GetInt32(0));
                    }
                }
            }

            return connectedLocations;
        }

        public int GetPlayerCurrentLocation()
        {
            int currentLocationId = -1; // Domyślna wartość, gdyby nie znaleziono rekordu

            string query = "SELECT CurrentLocationId FROM Characters WHERE Name = 'Player';";
            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        currentLocationId = reader.GetInt32(0);
                    }
                }
            }

            return currentLocationId;
        }

        public void UpdatePlayerLocation(int newLocationId)
        {
            string query = "UPDATE Characters SET CurrentLocationId = @NewLocationId WHERE Name = 'Player';";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NewLocationId", newLocationId);
                command.ExecuteNonQuery();
            }
        }

        public List<InteractionItem> GetInteractionsForLocation(int locationId)
        {
            var interactions = new List<InteractionItem>();

            string query = "SELECT Id, Description, ItemId FROM Interactions WHERE LocationId = @LocationId AND IsActive = true";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@LocationId", locationId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int interactionId = reader.GetInt32(0);
                        string description = reader.GetString(1);
                        //bool hasItem = reader.GetInt32(2) == 1; // 1 oznacza, że interakcja dotyczy przedmiotu
                        int itemId = reader.GetInt32(2); // ItemId może być null
                        //string actionDescription = reader.GetString(4);
                        //string actionResultDescription = reader.GetString(5);
                        //string actionResultFailed = reader.GetString(6);
                        //int? rewardItemId = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7); // ItemId może być null
                        //int? requiredItemId = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8); // ItemId może być null
                        //int? connectedLocationId = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9); // ItemId może być null
                        //int? unconnectedLocationId = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10); // ItemId może być null


                        interactions.Add(new InteractionItem(interactionId, description, locationId, itemId));
                    }
                }
            }

            return interactions;
        }

        public List<InteractionsForNPC> GetInteractionsForNPCs(int locationId)
        {
            var npc_interactions = new List<InteractionsForNPC>();

            string query = "SELECT npc.Id, npc.Description, npc.DescriptionInGame, ch.Name, ch.Id FROM InteractionsForNPC npc JOIN Characters ch on ch.Id = npc.CharacterId WHERE ch.CurrentLocationid = @locationId AND ch.DialogueId = npc.Id AND IsActive = true;";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@locationId", locationId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string description = reader.GetString(1);
                        string descriptionInGame = reader.GetString(2);
                        string characterName = reader.GetString(3);
                        int characterId = reader.GetInt32(4);

                        npc_interactions.Add(new InteractionsForNPC(id, description, descriptionInGame, characterName, characterId));
                    }
                }
                return npc_interactions;
            }
        }

        public int GetInteractionIdByActionId(int actionId)
        {
            string query = "SELECT InteractionId FROM InteractionActions WHERE Id = @ActionId;";

            using(var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ActionId", actionId);
                int interactionId = Convert.ToInt32(command.ExecuteScalar());
                //MessageBox.Show(countExcecutedObjectives.ToString());
                return interactionId;
            }
        }

        public int GetInteractionNPCIdByActionNPCId(int actionNPCId)
        {
            string query = "SELECT InteractionNPCId FROM InteractionActionsForNPC WHERE Id = @ActionNPCId;";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ActionNPCId", actionNPCId);
                int interactionNPCId = Convert.ToInt32(command.ExecuteScalar());
                //MessageBox.Show(countExcecutedObjectives.ToString());
                return interactionNPCId;
            }
        }

        public List<InteractionsForParagraphs> GetInteractionsForParagraphs(int paragraphId)
        {
            var paragraphInteractions = new List<InteractionsForParagraphs>();
            string query = "SELECT Id, LocationId, RewardItemId, RequiredItemId, FirstConnectedLocationId, SecondConnectedLocationId, FirstUnconnectedLocationId, SecondUnconnectedLocationId, ToSidedConnectionLocation, ItemId, QuestId, InteractionId, InteractionNPCId, MapImageId, MapImageActive, DialogueId, DialogueRowId FROM InteractionsParagraphs WHERE ParagraphId = @ParagraphId";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ParagraphId", paragraphId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int locationId = reader.GetInt32(1);
                        int rewardItemId = reader.GetInt32(2);
                        int requiredItemId = reader.GetInt32(3);
                        int firstConnectedLocationId = reader.GetInt32(4);
                        int secondConnectedLocationId = reader.GetInt32(5);
                        int firstUnconnectedLocationId = reader.GetInt32(6);
                        int secondUnconnectedLocationId = reader.GetInt32(7);
                        bool toSidedConnectedLocation = reader.GetBoolean(8);
                        int itemId = reader.GetInt32(9);
                        int questId = reader.GetInt32(10);
                        int interactionId = reader.GetInt32(11);
                        int interactionNPCId = reader.GetInt32(12);
                        int mapImageId = reader.GetInt32(13);
                        bool mapImageActive = reader.GetBoolean(14);
                        int dialogueId = reader.GetInt32(15);
                        int dialogueRowId = reader.GetInt32(16);
                        //int paragraphId = reader.GetInt32(10);

                        paragraphInteractions.Add(new InteractionsForParagraphs(id, locationId, rewardItemId, requiredItemId, firstConnectedLocationId, secondConnectedLocationId, firstUnconnectedLocationId, secondUnconnectedLocationId, toSidedConnectedLocation, itemId, questId, interactionId, interactionNPCId, mapImageId, mapImageActive, dialogueId, dialogueRowId));
                    }
                }

                return paragraphInteractions;
            }

        }

            public Item GetItemById(int itemId)
            {
                string query = "SELECT Id, Name, Description, DescriptionInGame, UsedOnce FROM Items WHERE Id = @ItemId;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemId", itemId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string description = reader.GetString(2); // Pobieramy opis przedmiotu
                            string descriptionInGame = reader.GetString(3);
                            bool usedOnce = reader.GetBoolean(4);

                            return new Item(id, name, description, descriptionInGame, usedOnce); // Przekazujemy przedmiot z opisem
                        }
                    }
                }

                return null; // Jeśli przedmiot nie istnieje
            }

            public List<InteractionAction> GetActionsForInteraction(int interactionId, bool isActive)
            {
                var actions = new List<InteractionAction>();
                string query = @"
                SELECT Id, ActionDescription, ActionResultDescription, ActionResultFailed, RewardItemId, RequiredItemId, ConnectedLocationId, UnconnectedLocationId, IsActive, RequiredOneInteraction, HasOptionalInteraction, TwoSidedConnection, IsExcecuted, IsMainAction
                FROM InteractionActions 
                WHERE InteractionId = @InteractionId AND IsActive = @isActive;
                 ";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InteractionId", interactionId);
                    command.Parameters.AddWithValue("@isActive", isActive);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int interactActionId = reader.GetInt32(0);
                            string actionDescription = reader.GetString(1);
                            string actionResultDescription = reader.GetString(2);
                            string actionResultFailed = reader.GetString(3);
                            int rewardItemId = reader.GetInt32(4);
                            int requiredItemId = reader.GetInt32(5);
                            int connectedLocationId = reader.GetInt32(6);
                            int unconnectedLocationId = reader.GetInt32(7);

                            bool requiredOneInteraction = reader.GetBoolean(9); 
                            bool hasOptionalInteraction = reader.GetBoolean(10);
                            bool twoSidedConnection = reader.GetBoolean(11);
                            bool isExcecuted = reader.GetBoolean(12);
                            bool isMainAction =reader.GetBoolean(13);
                            //var interactionId = reader.GetInt32(9);

                        actions.Add(new InteractionAction(interactActionId, actionDescription, actionResultDescription, actionResultFailed,rewardItemId,requiredItemId, connectedLocationId, unconnectedLocationId, isActive, interactionId, requiredOneInteraction, hasOptionalInteraction, twoSidedConnection, isExcecuted, isMainAction));
                        }
                    }
                }

                return actions;
            }

        public List<InteractionAction> GetAllActionsForInteraction(int interactionId, bool isActive)
        {
            var actions = new List<InteractionAction>();
            string query = @"
                SELECT Id, ActionDescription, ActionResultDescription, ActionResultFailed, RewardItemId, RequiredItemId, ConnectedLocationId, UnconnectedLocationId, IsActive, RequiredOneInteraction, HasOptionalInteraction, TwoSidedConnection, IsExcecuted, IsMainAction
                FROM InteractionActions 
                WHERE InteractionId = @InteractionId;
                 ";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@InteractionId", interactionId);
                command.Parameters.AddWithValue("@isActive", isActive);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int interactActionId = reader.GetInt32(0);
                        string actionDescription = reader.GetString(1);
                        string actionResultDescription = reader.GetString(2);
                        string actionResultFailed = reader.GetString(3);
                        int rewardItemId = reader.GetInt32(4);
                        int requiredItemId = reader.GetInt32(5);
                        int connectedLocationId = reader.GetInt32(6);
                        int unconnectedLocationId = reader.GetInt32(7);

                        bool requiredOneInteraction = reader.GetBoolean(9);
                        bool hasOptionalInteraction = reader.GetBoolean(10);
                        bool twoSidedConnection = reader.GetBoolean(11);
                        bool isExcecuted = reader.GetBoolean(12);
                        bool isMainAction = reader.GetBoolean(13);
                        //var interactionId = reader.GetInt32(9);

                        actions.Add(new InteractionAction(interactActionId, actionDescription, actionResultDescription, actionResultFailed, rewardItemId, requiredItemId, connectedLocationId, unconnectedLocationId, isActive, interactionId, requiredOneInteraction, hasOptionalInteraction, twoSidedConnection, isExcecuted, isMainAction));
                    }
                }
            }

            return actions;
        }

        public List<InteractionAction> GetActionsForInteractionByExcecuted(int interactionId, bool isActive)
        {
            var actions = new List<InteractionAction>();
            string query = @"
                SELECT Id, ActionDescription, ActionResultDescription, ActionResultFailed, RewardItemId, RequiredItemId, ConnectedLocationId, 
                UnconnectedLocationId, IsActive, RequiredOneInteraction, HasOptionalInteraction, TwoSidedConnection, IsExcecuted, IsMainAction
                FROM InteractionActions 
                WHERE InteractionId = @InteractionId AND IsExcecuted = @isActive;
                 ";

            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@InteractionId", interactionId);
                command.Parameters.AddWithValue("@isActive", isActive);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int interactActionId = reader.GetInt32(0);
                        string actionDescription = reader.GetString(1);
                        string actionResultDescription = reader.GetString(2);
                        string actionResultFailed = reader.GetString(3);
                        int rewardItemId = reader.GetInt32(4);
                        int requiredItemId = reader.GetInt32(5);
                        int connectedLocationId = reader.GetInt32(6);
                        int unconnectedLocationId = reader.GetInt32(7);

                        bool requiredOneInteraction = reader.GetBoolean(9);
                        bool hasOptionalInteraction = reader.GetBoolean(10);
                        bool twoSidedConnection = reader.GetBoolean(11);
                        bool isExcecuted = reader.GetBoolean(12);
                        bool isMainAction = reader.GetBoolean(13);
                        //var interactionId = reader.GetInt32(9);

                        actions.Add(new InteractionAction(interactActionId, actionDescription, actionResultDescription, actionResultFailed, rewardItemId, requiredItemId, connectedLocationId, unconnectedLocationId, isActive, interactionId, requiredOneInteraction, hasOptionalInteraction, twoSidedConnection, isExcecuted, isMainAction));
                    }
                }
            }

            return actions;
        }

            public void SetInteractionsActive(int interactionId, bool isActive)
            {
                string query = @"UPDATE Interactions SET IsActive = @IsActive WHERE Id = @InteractionId";
                using(var command = new SqliteCommand(query,connection))
                {
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@InteractionId", interactionId);

                    command.ExecuteNonQuery();

                }
            }

            public void SetInteractionsNPCActive(int interactionNpcId, bool isActive)
            {
                string query = @"UPDATE InteractionsForNPC SET IsActive = @IsActive WHERE Id = @InteractionNpcId";
                using(var command = new SqliteCommand(query,connection))
                {
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@InteractionNpcId", interactionNpcId);

                    command.ExecuteNonQuery();

                }
            }

            public void SetInteractionActionsExcecuted(int interactionId, bool isActive, bool? isMainAction)
            {
  
                if (isMainAction != null)
                {
                    query = @"UPDATE InteractionActions SET IsExcecuted = @IsActive WHERE InteractionId = @InteractionId AND IsMainAction = @IsMainAction";
                }
                else
                {
                    query = @"UPDATE InteractionActions SET IsExcecuted = @IsActive WHERE Id = @InteractionId";
                }
            
            
                using(var command = new SqliteCommand(query,connection))
                {
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@IsMainAction", isMainAction);
                    command.Parameters.AddWithValue("@InteractionId", interactionId);

                    command.ExecuteNonQuery();

                }
            }

            public void SetInteractionActionsActive(int interactionId, bool isActive, bool? isMainAction)
            {

                if (isMainAction != null)
                {
                    query = @"UPDATE InteractionActions SET IsActive = @IsActive WHERE InteractionId = @InteractionId AND IsMainAction = @IsMainAction";
            }
                else
                {
                    query = @"UPDATE InteractionActions SET isActive = @IsActive WHERE Id = @InteractionId";
                }

                using(var command = new SqliteCommand(query,connection))
                {
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@IsMainAction", isMainAction);
                    command.Parameters.AddWithValue("@InteractionId", interactionId);

                    command.ExecuteNonQuery();

                }
            }

            public void SetInteractionActionsActiveByRequiredItemId(int requiredItemId, bool isActive)
            {
                string query = @"UPDATE InteractionActions SET IsActive = @IsActive WHERE RequiredItemId = @requiredItemId";
                using(var command = new SqliteCommand(query,connection))
                {
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@requiredItemId", requiredItemId);
                    command.ExecuteNonQuery();

                }
            }

            public List<Item> GetPlayerInventory()
            {
                List<Item> inventory = new List<Item>();
                string query = @"
                SELECT Items.Id, Items.Name, Items.Description, Items.DescriptionInGame, Items.UsedOnce
                FROM PlayerInventory
                JOIN Items ON PlayerInventory.ItemId = Items.Id
                ";

                using (var command = new SqliteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.GetString(2);
                        string descriptionInGame = reader.GetString(3);
                        bool usedOnce = reader.GetBoolean(4);

                        inventory.Add(new Item(id, name, description, descriptionInGame, usedOnce));
                    }
                }

                return inventory;
            }

            public void AddItemToInventory(int itemId)
            {
                string query = @"
            INSERT INTO PlayerInventory (ItemId)
            VALUES (@ItemId);
            ";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemId", itemId);
                    command.ExecuteNonQuery();
                }
            }

            public bool PlayerHasItem(int itemId)
            {
                string query = "SELECT COUNT(*) FROM PlayerInventory WHERE ItemId = @ItemId";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemId", itemId);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }

            public void AddLocationConnection(int fromLocationId, int toLocationId)
            {
                string query = @"
                INSERT INTO LocationConnections (FromLocationId, ToLocationId)
                VALUES (@FromLocationId, @ToLocationId)";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FromLocationId", fromLocationId);
                    command.Parameters.AddWithValue("@ToLocationId", toLocationId);
                    command.ExecuteNonQuery();
                }
            }

        //    public void RemoveLocationConnection(int fromLocationId, int toLocationId)
        //    {
        //        string query = @"
        //        DELETE FROM LocationConnections WHERE FromLocationId = @FromLocationId AND ToLocationId = @ToLocationId;";
        //        using (SqliteCommand command = new SqliteCommand(query, connection))
        //        {
        //        command.Parameters.AddWithValue("@FromLocationId", fromLocationId);
        //        command.Parameters.AddWithValue("@ToLocationId", toLocationId);
        //        command.ExecuteNonQuery();
        //        }
        //}

            public void RemoveItemFromPlayerInventory(int itemId)
            {
                string query = "DELETE FROM PlayerInventory WHERE ItemId = @ItemId;";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemId", itemId);
                    command.ExecuteNonQuery();
                }
            }

            public List<ProgressValues> GetProgressValues()
            {
                List<ProgressValues> progressValues = new List<ProgressValues>();

                string query = "SELECT IsGameWasLaunch, CurrentChapterId, CurrentParagraphId, IsOnGamePanel, IsGameEnd, CurrentTrackedQuest FROM ProgressTable";

                using(var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bool isGameWasLaunch = reader.GetBoolean(0);
                            int currentChapterId = reader.GetInt32(1);
                            int currentParagraphId = reader.GetInt32(2);
                            bool isOnGamePanel = reader.GetBoolean(3);
                            bool isGameEnd = reader.GetBoolean(4);
                            int currentTrackedQuest = reader.GetInt32(5);

                            progressValues.Add(new ProgressValues(isGameWasLaunch, currentChapterId, currentParagraphId, isOnGamePanel,isGameEnd, currentTrackedQuest));
                        }
                    }
                }
                return progressValues;
            }

            public void SetProgressValueGameLaunch()
            {
                string query = "UPDATE ProgressTable SET IsGameWasLaunch = true WHERE Id = 1";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

             public void SetProgressChapterId()
            {
                string query = "UPDATE ProgressTable SET CurrentChapterId = CurrentChapterId + 1 WHERE Id = 1";

                using (var command = new SqliteCommand(query, connection))
                {
                    //command.Parameters.AddWithValue("@CurrentChapterId", currentChapterId);
                    command.ExecuteNonQuery();
                }
            }

            public void SetProgressIsOnGamePanel(bool isOnGamePanel)
            {
                string query = "UPDATE ProgressTable SET IsOnGamePanel = @IsOnGamePanel WHERE Id = 1";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsOnGamePanel", isOnGamePanel);
                    command.ExecuteNonQuery();
                }
            }

            public void SetProgressCurrentParagraphIndex(int currentParagraphIndex)
            {
                string query = "UPDATE ProgressTable SET CurrentParagraphId = @CurrentParagraphIndex WHERE Id = 1";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrentParagraphIndex", currentParagraphIndex);
                    command.ExecuteNonQuery();
                }
            }

            public void SetProgressIsGameEnd(bool isGameEnd)
            {
                string query = "UPDATE ProgressTable SET IsGameEnd = @IsGameEnd WHERE Id = 1";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsGameEnd", isGameEnd);
                    command.ExecuteNonQuery();
                }
            }

        public List<MapImage> GetMapsFromMap(bool isActive)
        {
            List<MapImage> mapImages = new List<MapImage>();

            string query = "SELECT Name, ImageData FROM MapTable WHERE IsActive = @IsActive";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IsActive", isActive);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);
                        byte[] imageData = (byte[])reader["ImageData"];
                        mapImages.Add(new MapImage(name, imageData));
                    }

                }

            }

                return mapImages;
        }

    }

}