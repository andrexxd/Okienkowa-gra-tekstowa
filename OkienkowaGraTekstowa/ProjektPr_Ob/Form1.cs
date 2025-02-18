using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ProjektPr_Ob
{
    public partial class Form1 : Form
    {

        // Nazwa pliku bazy danych do sprawdzenia
        static string databaseFileName = "GameDB.db";

        // Ścieżka do katalogu aplikacji (gdzie znajdują się pliki projektu w runtime)
        static string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private TypingEffectClass typingEffect;
        private int currentChapterIndex = 0;
        private bool isHistoryPanel = false;
        private bool isGamePanel = false;

        private int currentChapter = 1; // Numer bieżącego rozdziału
        private List<string> storyTexts; // Lista tekstów rozdziału
        private string chapterTitle; // Tytuł rozdziału
        private int currentTextIndex = 0; // Indeks aktualnego tekstu

        private List<(int Id, string text, bool breakText)> storyData;
        private List<Item> playerInventory = new List<Item>();
        private List<Button> inventoryItemButtons = new List<Button>();
        private List<Button> questsButtons = new List<Button>();
        private List<Button> mapImageButtons = new List<Button>();

        private int currentTrackedQuest = -1;
        private int currentSelectedQuest = 0;
        private int btnIndex = 0;
        private int currentShowParagraphIndex = 0;
        private int intervalTyping = 100;
        private int dialogueRowId = 1;
        private int currentDialogueCharacterId = 0;

        // Ścieżka do pliku bazy danych
        private string databaseFilePath = Path.Combine(appDirectory, databaseFileName);
        private static string historyDirectoryPath = Path.Combine(appDirectory, "History");
        public static string defaultStoryPath = Path.Combine(historyDirectoryPath, "Kompleks Inkwizycji");
        public static string defaultDatabaseStoryPath = Path.Combine(defaultStoryPath, databaseFileName);
        public static string currentDatabaseDirectory = "";
        public static string currentDatabaseName = "";
        public static string MainFolderProject = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

        public List<Panel> panelList = new List<Panel>();
        public List<RichTextBox> rtx_Textboxes = new List<RichTextBox>();

        public static string fullTextForDialogue = "";


        public static string defaultDatabaseScheme = Path.Combine(MainFolderProject, "DefaultHistoryScheme");

        List<Button> buttons = new List<Button>();
        //private SqliteConnection connection;


        public Form1()
        {
            InitializeComponent();

            panelList.Add(Panel_ChapterPanel);
            panelList.Add(Panel_GamePanel);
            panelList.Add(Panel_HistoryPanel);
            panelList.Add(Panel_MainMenuPanel);
            panelList.Add(Panel_NewGame);
            panelList.Add(Panel_InventoryPanel);
            panelList.Add(Panel_InteractionPanel);
            panelList.Add(Panel_SelectHistoryPanel);
            panelList.Add(Panel_MapPanel);
            panelList.Add(Panel_QuestPanel);
            panelList.Add(Panel_EndhistoryPanel);
            panelList.Add(Panel_SelectHistoryPanel);
            panelList.Add(Panel_SecondHistoryPanel);
            panelList.Add(Panel_DialoguePanel);

            rtx_Textboxes.Add(RTX_HistoryText);
            rtx_Textboxes.Add(RTX_Chapter);
            rtx_Textboxes.Add(RTX_Chapter_Name);
            rtx_Textboxes.Add(RTX_InteractionDescription);
            rtx_Textboxes.Add(RTX_ItemDescription);
            rtx_Textboxes.Add(RTX_LocationName);
            rtx_Textboxes.Add(RTX_Location_Description);
            rtx_Textboxes.Add(RTX_Paragraphs);

            RTX_LocationName.SelectionAlignment = HorizontalAlignment.Center;
            RTX_Chapter.SelectionAlignment = HorizontalAlignment.Center;
            RTX_Chapter_Name.SelectionAlignment = HorizontalAlignment.Center;

            foreach (var richtextbox in rtx_Textboxes)
            {
                richtextbox.ReadOnly = true;   // Tylko do odczytu
                richtextbox.Cursor = Cursors.Default; // 🔹 Zmiana kursora na standardowy (usuwa "text cursor")
                richtextbox.TabStop = false;   // 🔹 Wyłączenie focusa (nie da się kliknąć i edytować)
                richtextbox.Enter += (s, e) => richtextbox.Parent.Focus(); // 🔹 Przekierowanie focusa na panel (usuwa kursor)

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Otwórz połączenie z bazą danych przy rozpoczęciu nowej gry
            DatabaseManager.Instance.OpenConnectionForCreateDefaultStory();
        }

        public static void AdjustFontSize(RichTextBox richTextBox)
        {
            if (string.IsNullOrWhiteSpace(richTextBox.Text)) return; // Jeśli pusty, nic nie rób

            using (Graphics g = richTextBox.CreateGraphics())
            {
                float fontSize = richTextBox.Font.Size;
                float minFontSize = 8; // Minimalny rozmiar czcionki
                int maxHeight = richTextBox.Height;
                int textHeight;

                do
                {
                    Font testFont = new Font(richTextBox.Font.FontFamily, fontSize, richTextBox.Font.Style);
                    textHeight = TextRenderer.MeasureText(richTextBox.Text, testFont, new Size(richTextBox.Width, int.MaxValue), TextFormatFlags.WordBreak).Height;

                    if (textHeight > maxHeight)
                        fontSize -= 0.5f; // Stopniowo zmniejszamy czcionkę
                    else
                        break;

                } while (fontSize > minFontSize);

                // 🔹 Zamiast zmieniać cały `Font`, aktualizujemy każdą literę osobno
                for (int i = 0; i < richTextBox.Text.Length; i++)
                {
                    richTextBox.Select(i, 1); // Wybór znaku
                    Font currentFont = richTextBox.SelectionFont ?? richTextBox.Font;
                    richTextBox.SelectionFont = new Font(currentFont.FontFamily, fontSize, currentFont.Style);
                }

                richTextBox.Select(0, 1);
                Font firstCharFont = richTextBox.SelectionFont ?? richTextBox.Font;
                richTextBox.SelectionFont = new Font(firstCharFont.FontFamily, fontSize, firstCharFont.Style);
            }
        }

        public static void DisplayFormattedText(RichTextBox richTextBox, string text)
        {
            richTextBox.Clear();

            Dictionary<string, FontStyle> formatTags = new Dictionary<string, FontStyle>
    {
        { "[b]", FontStyle.Bold },
        { "[/b]", FontStyle.Regular },
        { "[i]", FontStyle.Italic },
        { "[/i]", FontStyle.Regular }
    };

            int startIndex = 0;
            FontStyle currentStyle = FontStyle.Regular;

            while (startIndex < text.Length)
            {
                // Jeśli wykryto otwierający nawias "["
                if (text[startIndex] == '[' && startIndex + 2 < text.Length)
                {
                    // Pobieramy potencjalny tag
                    string tag = text.Substring(startIndex, 3);

                    if (tag == "[b]" || tag == "[i]")
                    {
                        currentStyle = tag == "[b]" ? FontStyle.Bold : FontStyle.Italic;
                        startIndex += 3; // **Przesuwamy wskaźnik o długość tagu**
                        continue; // **Przechodzimy do kolejnego znaku, nie dodając "[" do RichTextBox**
                    }
                    else if (text.Substring(startIndex, 4) == "[/b]" || text.Substring(startIndex, 4) == "[/i]")
                    {
                        currentStyle = FontStyle.Regular; // Resetujemy styl po zamknięciu znacznika
                        startIndex += 4; // **Przesuwamy wskaźnik o długość zamykającego tagu**
                        continue;
                    }
                }

                richTextBox.SelectionFont = new Font(richTextBox.Font, currentStyle);
                richTextBox.AppendText(text[startIndex].ToString());
                //richTextBox.Text += text[startIndex].ToString();
                startIndex++;
            }

        }



        private void SelectHistory(string history)
        {
            string historyName = Path.GetFileName(history);
            string historyLocation = Path.Combine(historyDirectoryPath, historyName);
            string databaseHistoryName = "";
            DatabaseManager.Instance.CloseConnection();
            //MessageBox.Show(history);
            foreach (string directories in Directory.GetFiles(historyLocation, "*.db"))
            {
                //MessageBox.Show(directories);
                if (!Path.GetFileName(directories).StartsWith("Copy_"))
                {
                    databaseHistoryName = Path.GetFileName(directories);
                    break;
                }

            };

            if (databaseHistoryName != "")
            {
                DatabaseManager.Instance.OpenConnection(historyLocation, databaseHistoryName);
                TXT_HistoryName.Text = historyName;
                TXT_SelectHistoryName.Text = historyName;
                BTN_NewGame.Enabled = true;
                currentDatabaseDirectory = historyLocation;
                currentDatabaseName = databaseHistoryName;
                //MessageBox.Show(currentDatabaseDirectory + "\\" + currentDatabaseName);
                //MessageBox.Show(currentDatabaseName);

                //MessageBox.Show(currentDatabaseName);

                if (databaseHistoryName != "GameDB.db")
                {
                    DatabaseManager.Instance.OpenConnection(historyLocation, databaseHistoryName);
                    string absoluteFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, history);
                    //DatabaseManager.Instance.OpenConnection()
                    //MessageBox.Show(historyLocation);
                    //MessageBox.Show(databaseHistoryName);
                    DatabaseManager.Instance.InsertImagesToDatabase(historyLocation, historyLocation, databaseHistoryName);

                }
                else if (databaseHistoryName == "GameDB.db")
                {

                }

                else
                {
                    MessageBox.Show("Folder nie posiada bazy danych.");
                    TXT_HistoryName.Text = "";
                    TXT_SelectHistoryName.Text = "";
                    BTN_NewGame.Enabled = false;
                    LoadHistories();
                }
            }

        }

        private void LoadHistories()
        {
            Panel_HistoryMainPanel.Controls.Clear();
            string[] histories = Directory.GetDirectories(historyDirectoryPath);
            int yOffset = 10;
            foreach (string history in histories)
            {
                string historyName = Path.GetFileName(history);
                //MessageBox.Show(Path.GetFileName(history));

                Panel historyPanel = new Panel
                {
                    Width = Panel_HistoryMainPanel.Width - 30, // Szerokość dopasowana do głównego panelu
                    Height = 60,
                    BorderStyle = BorderStyle.FixedSingle,
                    //BackColor = Color.White,
                    Location = new Point(10, yOffset)
                };

                Panel_HistoryMainPanel.Controls.Add(historyPanel);
                yOffset += historyPanel.Height + 10;

                Label LBL_historyName = new Label
                {
                    Text = Path.GetFileName(history),
                    AutoSize = false,
                    Width = historyPanel.Width - 170,
                    Height = 40,
                    Location = new Point(10, 10),
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    ForeColor = Color.White,

                };

                historyPanel.Controls.Add(LBL_historyName);
                //MessageBox.Show(historyName);
                Button BTN_SelectButton = new Button
                {
                    Text = "Wybierz",
                    ForeColor = Color.White,
                    Width = 100,
                    Height = 40,
                    Location = new Point(historyPanel.Width - 130, 10),
                    Tag = history, // Przechowujemy obiekt historii w tagu
                    UseVisualStyleBackColor = false,
                    UseCompatibleTextRendering = true,
                    Enabled = historyName == TXT_HistoryName.Text ? false : true
                };

                historyPanel.Controls.Add(BTN_SelectButton);
                buttons.Add(BTN_SelectButton);

                BTN_SelectButton.Click += (s, e) =>
                {
                    foreach (Button btn in buttons)
                    {
                        btn.Enabled = true;
                    }
                    if (BTN_SelectButton.Tag == history)
                    {
                        BTN_SelectButton.Enabled = false;
                        // Wywołujemy metodę obsługującą wybór historii
                        //MessageBox.Show(currentDatabaseDirectory);
                        //MessageBox.Show(currentDatabaseName);
                        SelectHistory(historyName);
                        DatabaseManager.Instance.CloseConnection();
                        DatabaseManager.Instance.OpenConnection(currentDatabaseDirectory, currentDatabaseName);

                        var progressValues = DatabaseManager.Instance.GetProgressValues();
                        if (progressValues[0].IsGameWasLaunch)
                        {
                            BTN_LoadGame.Enabled = true;
                        }
                        else
                        {
                            BTN_LoadGame.Enabled = false;
                        }
                    }
                };

            }
        }

        private void StartChapterMenu(bool updateChapterIndex)
        {
            var (chapterTitle, data) = DatabaseManager.Instance.GetChapterData(currentChapterIndex);

            // Zapisz pobrane dane do pola storyData
            storyData = data;
            //currentTextIndex = 0;

            if (updateChapterIndex)
            {
                DatabaseManager.Instance.SetProgressChapterId();
            }

            var progressValues = DatabaseManager.Instance.GetProgressValues();
            currentChapterIndex = progressValues[0].CurrentChapterId;
            currentTextIndex = progressValues[0].CurrentParagraphId;
            // Pobranie danych rozdziału z bazy danych
            var chapterData = DatabaseManager.Instance.GetChapterData(currentChapterIndex);
            DatabaseManager.Instance.SetProgressIsOnGamePanel(false);
            //MessageBox.Show(currentChapter.ToString());

            if (!string.IsNullOrEmpty(chapterData.chapterTitle))
            {
                ShowPanel(Panel_ChapterPanel);
                Panel_HistoryPanel.Visible = false;
                BTN_NextChapterMenu.Enabled = false;

                StartTypingChapterMenu(
                    $"Rozdział {currentChapterIndex}",
                    chapterData.chapterTitle
                );


            }
            else
            {
                MessageBox.Show($"Nie znaleziono danych dla rozdziału {currentChapter}.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartHistory()
        {
            if (storyData.Count > 0)
            {
                ShowPanel(Panel_HistoryPanel);
                BTN_Next.Enabled = false;
                StartTypingCurrentParagraph();

            }
        }

        private void StartTypingChapterMenu(string chapterNumberText, string chapterNameText)
        {
            intervalTyping = 120;
            if (chapterNameText == "Prolog")
            {
                chapterNumberText = chapterNameText;
                chapterNameText = "";
            }
            typingEffect = new TypingEffectClass(
                chapterNumberText,
                chapterNameText,
                RTX_Chapter,
                RTX_Chapter_Name,
                BTN_NextChapterMenu,
                null, // Brak przycisku Skip w ChapterMenu
                isHistoryPanel,
                () => OnHistoryTypingCompleted(),
                intervalTyping
            );

            typingEffect.StartTypingText();
        }

        private void StartTypingTextLocations(string texts1, string texts2)
        {
            //MessageBox.Show("TextLocations");
            intervalTyping = 35;
            // Rozpoczynamy wypisywanie tekstów w dwóch labelach
            typingEffect = new TypingEffectClass(
                texts1,
                texts2,
                RTX_LocationName,
                RTX_Location_Description,
                BTN_NextChapterMenu, // Przycisk Next w ChapterMenu
                BTN_Skip,
                isGamePanel,
                () => UpdateGamePanel(),
                intervalTyping
            );
            typingEffect.StartTypingText();
        }

        // Funkcja wywoływana po zakończeniu wypisywania tekstu w PanelHistory
        private void OnHistoryTypingCompleted()
        {
            // Umożliwienie kliknięcia przycisku "Next"
            BTN_NextChapterMenu.Enabled = true;
            BTN_Next.Enabled = true;

            if (Panel_HistoryPanel.Visible)
            {
                var paragraphsInteractions = DatabaseManager.Instance.GetInteractionsForParagraphs(storyData[currentTextIndex].Id);

                foreach (var interaction in paragraphsInteractions)
                {
                    if (interaction.FirstConnectedLocationId != 0 && interaction.SecondConnectedLocationId != 0)
                    {
                        if (interaction.ToSiededConnectionLocation)
                        {
                            DatabaseManager.Instance.AddLocationConnection(interaction.FirstConnectedLocationId, interaction.SecondConnectedLocationId);
                            DatabaseManager.Instance.AddLocationConnection(interaction.SecondConnectedLocationId, interaction.FirstConnectedLocationId);
                        }
                        else
                        {
                            DatabaseManager.Instance.AddLocationConnection(interaction.FirstConnectedLocationId, interaction.SecondConnectedLocationId);
                        }
                    }
                    if (interaction.RewardItemId != 0)
                    {
                        DatabaseManager.Instance.AddItemToInventory(interaction.RewardItemId);
                    }
                    if (interaction.RequiredItemId != 0)
                    {
                        if (DatabaseManager.Instance.PlayerHasItem(interaction.RequiredItemId))
                        {
                            DatabaseManager.Instance.RemoveItemFromPlayerInventory(interaction.RequiredItemId);
                        }
                    }
                    if (interaction.InteractionId != 0)
                    {
                        DatabaseManager.Instance.SetInteractionsActive(interaction.InteractionId, true);
                    }
                    if(interaction.InteractionNPCId != 0)
                    {
                        DatabaseManager.Instance.SetInteractionsNPCActive(interaction.InteractionNPCId, true);
                    }
                    if (interaction.QuestId != 0)
                    {
                        DatabaseManager.Instance.SetQuestIsActive(interaction.QuestId);
                        DatabaseManager.Instance.SetQuestSelected(interaction.QuestId);
                        currentTrackedQuest = interaction.QuestId;
                    }
                    if (interaction.MapImageId != 0)
                    {
                        DatabaseManager.Instance.SetMapImageActive(interaction.MapImageId, interaction.MapImageActive);
                    }
                    if(interaction.DialogueId != 0 && interaction.DialogueRowId != 0)
                    {
                        DatabaseManager.Instance.SetDialogueActionNPCIdActive(interaction.DialogueRowId, true);
                    }
                    if(interaction.LocationId != 0)
                    {
                        DatabaseManager.Instance.UpdatePlayerLocation(interaction.LocationId);
                    }

                    //DatabaseManager.Instance.RemoveInteractionById(interaction.InteractionId);
                }

                if (storyData[currentTextIndex].breakText)
                {
                    BTN_Next.Text = "Przejdź do gry";
                }
                else
                {
                    int chaptersCount = DatabaseManager.Instance.GetChaptersCount();
                    var (chapterTitle, data) = DatabaseManager.Instance.GetChapterData(currentChapterIndex);
                    storyData = data;

                    //MessageBox.Show(storyData.Count().ToString());
                    if (currentTextIndex >= storyData.Count - 1 && chaptersCount == currentChapterIndex + 1)
                    {
                        BTN_Next.Text = "Koniec";
                    }
                    else
                    {
                        //Sprawdzamy, czy to ostatni paragraf rozdziału
                        if (currentTextIndex >= storyData.Count - 1)
                        {
                            // Jeżeli to ostatni paragraf, zmieniamy tekst na "Następny rozdział"
                            BTN_Next.Text = "Następny rozdział";
                        }
                        else
                        {
                            // W przeciwnym razie przywracamy tekst na "Następny"
                            BTN_Next.Text = "Następny";
                        }
                    }

                }
            }
        }

        //private RichTextBox myRichTextBox = new RichTextBox();

        private void StartTypingCurrentParagraph()
        {
            TXT_HistoryText.Font = new Font("Segoe UI", 13);
            RTX_HistoryText.Font = new Font("Segoe UI", 12);

            if (currentTextIndex < storyData.Count)
            {
                var currentStory = storyData[currentTextIndex]; // Pobierz bieżący tekst i breakText
                string text = currentStory.text;
                bool breakText = currentStory.breakText;
                intervalTyping = 20;

                DisplayFormattedText(RTX_HistoryText, text);



                // Rozpocznij efekt pisania
                typingEffect = new TypingEffectClass(
                    text,
                    string.Empty,
                    RTX_HistoryText,
                    null,
                    BTN_Next,
                    BTN_Skip,
                    true,
                    OnHistoryTypingCompleted,
                    intervalTyping
                );

                typingEffect.StartTypingText();


            }
        }

        private void StartTypingDialogueRows(string dialogueRows, bool isTree, bool isEndDialogue)
        {

                TXT_HistoryText.Font = new Font("Segoe UI", 13);

                intervalTyping = 35;
                fullTextForDialogue += $"{dialogueRows}";

            if (!isTree)
            {
                dialogueRowId = 1;
            }

            var interactionactionforNpc = DatabaseManager.Instance.GetInteractionActionForNPCsByDialogueRowId(dialogueRowId);

                // Rozpocznij efekt pisania
            typingEffect = new TypingEffectClass(
               dialogueRows,
               string.Empty,
               RTX_DialogueTexts,
               null,
               BTN_Next,
               BTN_Skip,
               true,
               () => LoadInteractionActionsButtonsForNPC(interactionactionforNpc, isEndDialogue),
               intervalTyping
            );

            typingEffect.StartTypingText();
           
        }

        private void StartTypingDialogueAnswerAndRow(string dialogueAnswer, string dialogueRows, bool isTree, bool isEndDialogue)
        {
            TXT_HistoryText.Font = new Font("Segoe UI", 13);

            intervalTyping = 35;

            //    DisplayFormattedText(RTX_HistoryText, text);
            var interactionactionforNpc = DatabaseManager.Instance.GetInteractionActionForNPCsByDialogueRowId(dialogueRowId);
            fullTextForDialogue += $"{dialogueAnswer}";

            // Rozpocznij efekt pisania
            typingEffect = new TypingEffectClass(
                dialogueAnswer,
                string.Empty,
                RTX_DialogueTexts,
                null,
                BTN_Next,
                BTN_Skip,
                true,
                () => StartTypingDialogueRows($"{dialogueRows}", isTree, isEndDialogue),
                intervalTyping
            );

            typingEffect.StartTypingText();

        }

        private void StartTypingItemDescription(Item item, InteractionItem interaction, RichTextBox label, string description, bool hasTyping)
        {
            intervalTyping = 35;
            typingEffect = new TypingEffectClass(
                description,        // Opis przedmiotu
                "",                      // Drugi tekst jest pusty
                RTX_ItemDescription,     // Label na opis przedmiotu
                null,
                BTN_SkipIP,  // Przycisk powrotu
                null,
                false,
                () =>
                {
                    if (hasTyping)
                    {
                        LoadActionsForInteraction(interaction.InteractionId);
                    }
                    typingEffect.EnableDisableSkipBtn(BTN_SkipIP, false);
                },
                intervalTyping
            );

            typingEffect.StartTypingText();
            typingEffect.EnableDisableSkipBtn(BTN_SkipIP, true);
        }

        private void ShowPanel(Panel panel)
        {

            foreach (var panel1 in panelList)
            {
                if (panel1 == panel)
                {
                    panel.Visible = true;
                }
                else
                {
                    panel1.Visible = false;
                }
            }

        }

        private void InitializeGamePanel()
        {
            ShowPanel(Panel_GamePanel);
            RTX_InteractionDescription.Text = "";

            DatabaseManager.Instance.SetProgressIsOnGamePanel(true);

            var progressValues = DatabaseManager.Instance.GetProgressValues();

            currentChapterIndex = progressValues[0].CurrentChapterId;
            currentTextIndex = progressValues[0].CurrentParagraphId;

            int playerLocationId = DatabaseManager.Instance.GetPlayerCurrentLocation();
            var locationData = DatabaseManager.Instance.GetCurrentLocation(playerLocationId);

            string loc_name = locationData.name;
            string loc_description = locationData.description;

            var imagesList = DatabaseManager.Instance.GetMapsFromMap(true);
            if (imagesList.Count() > 0)
            {
                BTN_MapPanel.Enabled = true;
            }
            else
            {
                BTN_MapPanel.Enabled = false;
            }

            FLP_LocationButtons.Controls.Clear();
            FLP_InteractionsMenu.Controls.Clear();
            //typingEffect.SkipText();
            StartTypingTextLocations(loc_name, loc_description);
        }

        private void UpdateGamePanel()
        {
            // Pobierz bieżącą lokację gracza z bazy danych
            int playerLocationId = DatabaseManager.Instance.GetPlayerCurrentLocation();
            var locationData = DatabaseManager.Instance.GetCurrentLocation(playerLocationId);
            var connectedLocations = DatabaseManager.Instance.GetConnectedLocations(playerLocationId);

            string loc_name = locationData.name;
            string loc_description = locationData.description;

            // Usuń stare przyciski
            FLP_LocationButtons.Controls.Clear();
            FLP_InteractionsMenu.Controls.Clear();

            // Dodaj przyciski dla połączeń
            foreach (var locationId in connectedLocations)
            {
                var locationName = DatabaseManager.Instance.GetCurrentLocation(locationId).name;

                var button = new Button
                {
                    Text = locationName,
                    Tag = locationId,
                    AutoSize = false,
                    Width = FLP_LocationButtons.Width - 40,
                    Height = 35,
                    Margin = new Padding(5),
                    ForeColor = Color.White
                };

                button.Click += BTN_LocationButton_Click;
                FLP_LocationButtons.Controls.Add(button);
            }

            DisplayInteractionsDescription(playerLocationId);

        }
        private void DisplayInteractionsDescription(int currentLocationId)
        {
            // Pobierz interakcje dostępne w tej lokacji (krótkie opisy)
            var interactions = DatabaseManager.Instance.GetInteractionsForLocation(currentLocationId);
            var npc_interactions = DatabaseManager.Instance.GetInteractionsForNPCs(currentLocationId);

            // Przygotowanie tekstu o interakcjach do wyświetlenia w drugim Labelu
            StringBuilder interactionsDescription = new StringBuilder();
            foreach (var interaction in interactions)
            {
                var has = DatabaseManager.Instance.GetItemById(interaction.ItemId.Value);
                interactionsDescription.AppendLine($"{has.DescriptionInGame}");
            }
            foreach (var npc in npc_interactions)
            {
                interactionsDescription.AppendLine($"{npc.DescriptionInGame}");
            }

            RTX_InteractionDescription.Text = interactionsDescription.ToString();
            DisplayFormattedText(RTX_InteractionDescription, interactionsDescription.ToString());

            LoadInteractionButtonsForInteractions(interactions);
            LoadInteractionButtonsForNPC(npc_interactions);;
        }

        private void LoadInteractionButtonsForInteractions(List<InteractionItem> interactions)
        {
            // Czyścimy poprzednie przyciski interakcji
            FLP_InteractionsMenu.Controls.Clear();

            foreach (var interaction in interactions)
            {
                // Tworzymy przycisk dla każdej interakcji
                var btnInteraction = new Button
                {
                    Text = interaction.Description, // Krótkie opis interakcji
                    Tag = interaction, // Tag zawiera całą interakcję
                    AutoSize = false,
                    Width = FLP_InteractionsMenu.Width - 20,
                    Height = 35,
                    Margin = new Padding(5),
                    ForeColor = Color.White,
                };

                // Dodajemy zdarzenie kliknięcia przycisku
                btnInteraction.Click += BtnInteraction_Click;

                // Dodajemy przycisk do panelu
                FLP_InteractionsMenu.Controls.Add(btnInteraction);
            }
        }

        private void LoadInteractionButtonsForNPC(List<InteractionsForNPC> interactionForNPC)
        {
            foreach (var interaction in interactionForNPC)
            {
                // Tworzymy przycisk dla każdej interakcji
                var btnInteraction = new Button
                {
                    Text = interaction.Description, // Krótkie opis interakcji
                    Tag = interaction, // Tag zawiera całą interakcję
                    AutoSize = false,
                    Width = FLP_InteractionsMenu.Width - 20,
                    Height = 35,
                    Margin = new Padding(5),
                    ForeColor = Color.White,
                };

                // Dodajemy zdarzenie kliknięcia przycisku
                btnInteraction.Click += BtnInteractionNPC_Click;

                // Dodajemy przycisk do panelu
                FLP_InteractionsMenu.Controls.Add(btnInteraction);
            }
        }

        private void LoadInteractionActionsButtonsForNPC(List<InteractionActionForNPC> interactionActionForNPC, bool isEndDialogue)
        {
            FLP_DialogueActions.Controls.Clear();

            if (!isEndDialogue)
            {
                foreach (var interaction in interactionActionForNPC)
                {
                    if (interaction.IsActive)
                    {
                        // Tworzymy przycisk dla każdej interakcji
                        var btnInteraction = new Button
                        {
                            Text = interaction.Name, // Krótkie opis interakcji
                            Tag = interaction, // Tag zawiera całą interakcję
                            AutoSize = false,
                            Width = FLP_InteractionsMenu.Width - 20,
                            Height = 35,
                            Margin = new Padding(5),
                            ForeColor = Color.White,
                            Enabled = interaction.IsActive == true ? true : false,
                            UseCompatibleTextRendering = true,
                            UseVisualStyleBackColor = false
                        };

                        // Dodajemy zdarzenie kliknięcia przycisku
                        btnInteraction.Click += BtnInteractionActionForNPC_Click;

                        // Dodajemy przycisk do panelu
                        FLP_DialogueActions.Controls.Add(btnInteraction);

                    }

                }
            }
            else
            {
                BTN_BackDialogue.Enabled = true;
            }
        }

        //private void AddItemToInventory(Item item)
        //{
        //    // Logika dodawania przedmiotu do ekwipunku (możesz dodać go do listy w grze)
        //    //playerInventory.Add(item);
        //    DatabaseManager.Instance.AddItemToInventory(item.ItemId);
        //}

        private void LoadActionsForInteraction(int interactionId)
        {
            FLP_InteractionsMenuItems.Controls.Clear();

            // Pobierz szczegóły akcji z bazy
            var actions = DatabaseManager.Instance.GetActionsForInteraction(interactionId, true);

            foreach (var action in actions)
            {
                Button btnAction = new Button
                {
                    Text = action.ActionDescription,
                    Tag = action,
                    Width = FLP_InteractionsMenuItems.Width - 20,
                    Height = 35,
                    ForeColor = Color.White
                };
                btnAction.Click += BtnAction_Click;
                FLP_InteractionsMenuItems.Controls.Add(btnAction);
            }
        }

        private void LoadPlayerInventory()
        {
            // Pobierz listę przedmiotów w ekwipunku gracza
            var playerInventory = DatabaseManager.Instance.GetPlayerInventory();

            // Wyczyść poprzednie elementy w liście
            FLP_InventoryItems.Controls.Clear();
            inventoryItemButtons.Clear();
            TXT_ItemDetails.Text = "Wybierz przedmiot, aby zobaczyć szczegóły.";

            if (playerInventory.Count() == 0)
            {
                TXT_ItemDetails.Text = "Brak przedmiotów w ekwipunku.";
            }
            else
            {
                // Dodaj przedmioty do ListBox
                foreach (var item in playerInventory)
                {
                    Button btnItem = new Button
                    {
                        Text = item.Name,
                        //? "Podnieś przedmiot" : "Zbadaj szuflady",
                        Tag = item,
                        //Width = FLP_InteractionsMenuItems.Width,
                        AutoSize = false,
                        Width = FLP_InventoryItems.Width - 30,
                        Height = 35,
                        Margin = new Padding(5),
                        ForeColor = Color.White,
                        UseVisualStyleBackColor = false,
                        UseCompatibleTextRendering = true
                    };

                    btnItem.Click += BtnItem_Click;
                    FLP_InventoryItems.Controls.Add(btnItem);
                    inventoryItemButtons.Add(btnItem);
                }
            }
        }

        private void LoadPlayerQuests(bool isExcecuted)
        {
            // Pobierz listę przedmiotów w ekwipunku gracza
            var playerQuests = DatabaseManager.Instance.GetQuests(isExcecuted);
            // Wyczyść poprzednie elementy w liście
            FLP_QuestList.Controls.Clear();
            questsButtons.Clear();
            TXT_QuestDescription.Text = "Wybierz przedmiot, aby zobaczyć szczegóły.";

            if (playerQuests.Count() == 0)
            {
                TXT_QuestDescription.Text = "Nie masz żadnych zadań.";
                TXT_Objectives.Text = "";
                FLP_QuestDetails.Controls.Clear();
                FLP_QuestObjectivesList.Controls.Clear();

            }
            else
            {
                // Dodaj przedmioty do ListBox
                foreach (var quest in playerQuests)
                {
                    Button btnQuest = new Button
                    {
                        Text = quest.Name,
                        //? "Podnieś przedmiot" : "Zbadaj szuflady",
                        Tag = quest,
                        //Width = FLP_InteractionsMenuItems.Width,
                        AutoSize = false,
                        Width = FLP_QuestList.Width - 20,
                        Height = 35,
                        Margin = new Padding(5),
                        ForeColor = Color.White,
                        UseVisualStyleBackColor = false,
                        UseCompatibleTextRendering = true,
                        Enabled = quest.QuestId == currentTrackedQuest ? false : true
                    };
                    if (isExcecuted == quest.IsExcecuted)
                    {
                        btnQuest.Click += BtnQuest_Click;
                        FLP_QuestList.Controls.Add(btnQuest);
                        questsButtons.Add(btnQuest);
                        //LB_InventoryItems.Items.Add(item);
                    }

                    if (quest.IsExcecuted)
                    {
                        btnQuest.Enabled = true;
                    }
                }
            }
        }

        private void BtnAction_Click(object sender, EventArgs e)
        {

            if (sender is Button btnClicked && btnClicked.Tag is InteractionAction action)
            {

                if (action.RequiredItemId != 0)
                {
                    var item = DatabaseManager.Instance.GetItemById(action.RequiredItemId);
                    if (DatabaseManager.Instance.PlayerHasItem(action.RequiredItemId))
                    {
                        CheckingActionStatus(action);
                        if (item.UsedOnce)
                        {
                            DatabaseManager.Instance.RemoveItemFromPlayerInventory(action.RequiredItemId);
                        }
                    }
                    else
                    {
                        TXT_InteractionResult.Text = action.ActionResultFailed;
                        return;
                    }
                }
                else
                {
                    CheckingActionStatus(action);
                }

                var actions = DatabaseManager.Instance.GetAllActionsForInteraction(action.InteractionId, false);

                //MessageBox.Show(actions.Count().ToString());
                foreach (var action1 in actions)
                {
                    if (DatabaseManager.Instance.PlayerHasItem(action1.RequiredItemId) && !action1.IsExcecuted)
                    {
                        DatabaseManager.Instance.SetInteractionActionsActiveByRequiredItemId(action1.RequiredItemId, true);
                    }
                }
                //MessageBox.Show(actions.Count().ToString());
                LoadActionsForInteraction(action.InteractionId);

                UpdateQuestObjectiveDescription(false);

            }
        }

        private void CheckingActionStatus(InteractionAction action)
        {
            int locationId = DatabaseManager.Instance.GetPlayerCurrentLocation();
            var countOfNonExcecutedActions = DatabaseManager.Instance.GetActionsForInteractionByExcecuted(action.InteractionId, false);
            var countOfExcecutedActions = DatabaseManager.Instance.GetActionsForInteractionByExcecuted(action.InteractionId, true);
            var countOfActions = DatabaseManager.Instance.GetAllActionsForInteraction(action.InteractionId, true);

            if (action.HasOptionalInteraction)
            {
                if (action.IsMainAction)
                {
                    if (countOfNonExcecutedActions.Count() < countOfActions.Count())
                    {
                        DatabaseManager.Instance.SetInteractionActionsActive(action.InteractionId, false, true);
                        DatabaseManager.Instance.SetInteractionActionsExcecuted(action.InteractionId, true, true);
                        DatabaseManager.Instance.SetInteractionsActive(action.InteractionId, false);
                     
                        TXT_InteractionResult.Text = action.ActionResultDescription;
                     
                        LoadActionsForInteraction(action.InteractionId);
                    
                    }
                    else
                    {
                        TXT_InteractionResult.Text = action.ActionResultFailed;
                        return;
                    }
                }
                else
                {
                    foreach (var action1 in countOfActions)
                    {
                        DatabaseManager.Instance.SetInteractionActionsActive(action.InteractionId, false, false);
                        DatabaseManager.Instance.SetInteractionActionsExcecuted(action.InteractionId, true, false);

                        TXT_InteractionResult.Text = action.ActionResultDescription;
                        LoadActionsForInteraction(action.InteractionId);
                    }

                }

            }

            else if (!action.RequiredOneInteraction)
            {
                if (action.IsMainAction)
                {
                    if (countOfExcecutedActions.Count() == countOfActions.Count() - 1)
                    {
                        TXT_InteractionResult.Text = action.ActionResultDescription;

                        DatabaseManager.Instance.SetInteractionActionsActive(action.InteractActionId, false, null);
                        DatabaseManager.Instance.SetInteractionActionsExcecuted(action.InteractActionId, true, null);
                        DatabaseManager.Instance.SetInteractionsActive(action.InteractionId, false);
                        LoadActionsForInteraction(action.InteractionId);

                    }
                    else
                    {
                        TXT_InteractionResult.Text = action.ActionResultFailed;
                        return;
                    }
                }
                else
                {
                    TXT_InteractionResult.Text = action.ActionResultDescription;

                    DatabaseManager.Instance.SetInteractionActionsActive(action.InteractActionId, false, null);
                    DatabaseManager.Instance.SetInteractionActionsExcecuted(action.InteractActionId, true, null);

                    LoadActionsForInteraction(action.InteractionId);

                }

            }
            else
            {
                TXT_InteractionResult.Text = action.ActionResultDescription;

                DatabaseManager.Instance.SetInteractionActionsActive(action.InteractActionId, false, null);
                DatabaseManager.Instance.SetInteractionActionsExcecuted(action.InteractActionId, true, null);
                DatabaseManager.Instance.SetInteractionsActive(action.InteractionId, false);
                LoadActionsForInteraction(action.InteractionId);


            }

            if (action.RewardItemId != 0)
            {
                DatabaseManager.Instance.AddItemToInventory(action.RewardItemId);
            }
            
            if (action.ConnectedLocationId != 0)
            {
                DatabaseManager.Instance.AddLocationConnection(locationId, action.ConnectedLocationId);
                if (action.TwoSidedConnection)
                {
                    DatabaseManager.Instance.AddLocationConnection(action.ConnectedLocationId, locationId);
                }
            }

            int a = DatabaseManager.Instance.GetQuestIdByObjectiveFromActionId(action.InteractActionId, 0);
            var b = DatabaseManager.Instance.GetQuestsById(a);

            var objectives = DatabaseManager.Instance.GetObjectiveByActionId(action.InteractActionId, 0);
            int countObjectives = 1;

            if (b.Count != 0)
            {
                countObjectives = DatabaseManager.Instance.GetCountObjectivesByOrderObjective(currentTrackedQuest, b[0].CurrentObjectiveOrder);
            }

            if(countObjectives != 1 && !objectives[0].IsOptionalObjective)
            {
                DatabaseManager.Instance.SetObjectiveIsExcecutedByDescription(objectives[0].Description);
                int countExcecutedObjectives = DatabaseManager.Instance.GetCountExcecutedObjectivesByOrderObj(currentTrackedQuest, b[0].CurrentObjectiveOrder);
                UpdateQuestObjectiveDescription(false);

                if (DatabaseManager.Instance.GetCountExcecutedObjectivesByOrderObj(currentTrackedQuest, b[0].CurrentObjectiveOrder) == countObjectives)
                {
                    DatabaseManager.Instance.UpdateCurrentObjectiveOrder(objectives[0].QuestId);
                    UpdateQuestObjectiveDescription(false);
                    
                }

                if (countExcecutedObjectives == countObjectives)
                {
                    var quests = DatabaseManager.Instance.GetQuestsById(objectives[0].QuestId);
                    

                    if (quests[0].ContinueHistory)
                    {
                        var objectivesCheck = DatabaseManager.Instance.GetObjectiveByExcecuted(objectives[0].QuestId, false);
                        int chaptersCount = DatabaseManager.Instance.GetChaptersCount();

                        if (objectivesCheck.Count() == 0)
                        {
                            DatabaseManager.Instance.SetQuestIsExcecuted(objectives[0].QuestId);
                            TXT_Objectives.Text = "";

                            if (storyData.Count != currentTextIndex)
                            {
                                StartHistory();
                            }
                            else if (storyData.Count != currentChapterIndex + 1)
                            {
                                StartChapterMenu(true);

                            }
                            else if (chaptersCount != currentChapterIndex + 1)
                            {
                                ShowPanel(Panel_EndhistoryPanel);
                                DatabaseManager.Instance.SetProgressIsGameEnd(true);
                            }
                        }
                        else
                        {
                            UpdateQuestObjectiveDescription(false);
                        }
                    }
                }

            }
            else if (countObjectives != 1 && objectives[0].IsOptionalObjective)
            {
                var quests = DatabaseManager.Instance.GetQuestsById(objectives[0].QuestId);
                DatabaseManager.Instance.SetObjectiveIsExcecutedByDescription(objectives[0].Description);

                var objectives2 = DatabaseManager.Instance.GetObjectives(currentTrackedQuest, b[0].CurrentObjectiveOrder);
                foreach (var objective2 in objectives2)
                {
                    int interactionId = DatabaseManager.Instance.GetInteractionIdByActionId(objective2.InteractionActionId);
                    int interactionNPCId = DatabaseManager.Instance.GetInteractionNPCIdByActionNPCId(objective2.InteractionActionNPCId);

                    DatabaseManager.Instance.SetInteractionsActive(interactionId, false);
                    DatabaseManager.Instance.SetInteractionsNPCActive(interactionNPCId, false);
                }

                if (quests[0].CurrentObjectiveOrder < DatabaseManager.Instance.GetMaxObjectiveOrderByQuestId(objectives[0].QuestId))
                {
                    DatabaseManager.Instance.UpdateCurrentObjectiveOrder(objectives[0].QuestId);
                }
                UpdateQuestObjectiveDescription(false);

                if (quests[0].ContinueHistory)
                {
                    var objectivesCheck = DatabaseManager.Instance.GetObjectiveByExcecuted(objectives[0].QuestId, false);
                    int chaptersCount = DatabaseManager.Instance.GetChaptersCount();

                        DatabaseManager.Instance.SetQuestIsExcecuted(objectives[0].QuestId);
                        TXT_Objectives.Text = "";

                        if (storyData.Count != currentTextIndex)
                        {
                            StartHistory();
                        }
                        else if (storyData.Count != currentChapterIndex + 1)
                        {
                            StartChapterMenu(true);

                        }
                        else if (chaptersCount != currentChapterIndex + 1)
                        {
                            ShowPanel(Panel_EndhistoryPanel);
                            DatabaseManager.Instance.SetProgressIsGameEnd(true);
                        }
                }
            }

                else if (objectives.Count() != 0)
                {
                    var quests = DatabaseManager.Instance.GetQuestsById(objectives[0].QuestId);
                    DatabaseManager.Instance.SetObjectiveIsExcecutedByDescription(objectives[0].Description);
                    if (quests[0].CurrentObjectiveOrder < DatabaseManager.Instance.GetMaxObjectiveOrderByQuestId(objectives[0].QuestId))
                    {
                        DatabaseManager.Instance.UpdateCurrentObjectiveOrder(objectives[0].QuestId);
                    }
                    UpdateQuestObjectiveDescription(false);

                    if (quests[0].ContinueHistory)
                    {
                        var objectivesCheck = DatabaseManager.Instance.GetObjectiveByExcecuted(objectives[0].QuestId, false);
                        int chaptersCount = DatabaseManager.Instance.GetChaptersCount();

                        if (objectivesCheck.Count() == 0)
                        {
                            DatabaseManager.Instance.SetQuestIsExcecuted(objectives[0].QuestId);
                            TXT_Objectives.Text = "";

                            if (storyData.Count != currentTextIndex)
                            {
                                StartHistory();
                            }
                            else if (storyData.Count != currentChapterIndex + 1)
                            {
                                StartChapterMenu(true);

                            }
                            else if (chaptersCount != currentChapterIndex + 1)
                            {
                                ShowPanel(Panel_EndhistoryPanel);
                                DatabaseManager.Instance.SetProgressIsGameEnd(true);
                            }
                        }
                        else
                        {

                        }
                    }
                }
            
        }

        private void BtnInteraction_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText();
            FLP_InteractionsMenuItems.Controls.Clear();
            if (sender is Button btnClicked && btnClicked.Tag is InteractionItem interaction)
            {
                //if (!interaction.HasItem && interaction.ItemId == 0)
                //{
                //    // Obsługa interakcji z przedmiotem
                //    var item = DatabaseManager.Instance.GetItemById(interaction.ItemId.Value);
                //    if (item != null)
                //    {
                //        if (item.DescriptionInGame != " ")
                //        {
                //            // Przełącz na panel interakcji z przedmiotem
                //            ShowPanel(Panel_InteractionPanel);
                //            FLP_InteractionsMenuItems.Controls.Clear();
                //            TXT_InteractionResult.Text = string.Empty;
                //            // Rozpocznij wpisywanie opisu przedmiotu
                //            typingEffect.SkipText();
                //            FLP_InteractionsMenuItems.Controls.Clear();
                //            StartTypingItemDescription(item, interaction, RTX_ItemDescription, item.DescriptionInGame, true);
                //        }
                //        else
                //        {
                //            // Przełącz na panel interakcji z przedmiotem
                //            ShowPanel(Panel_InteractionPanel);
                //            FLP_InteractionsMenuItems.Controls.Clear();
                //            TXT_InteractionResult.Text = string.Empty;
                //            // Rozpocznij wpisywanie opisu przedmiotu
                //            typingEffect.SkipText();
                //            FLP_InteractionsMenuItems.Controls.Clear();
                //            StartTypingItemDescription(item, interaction, RTX_ItemDescription, item.Description, true);
                //        }
                //    }
                //}
                //if (interaction.HasItem && interaction.ItemId != 0)
                //{
                    //if (!string.IsNullOrEmpty(interaction.ActionDescription) && !string.IsNullOrEmpty(interaction.ActionResultDescription))
                    //{
                        var item = DatabaseManager.Instance.GetItemById(interaction.ItemId.Value);
                        int locationId = DatabaseManager.Instance.GetPlayerCurrentLocation();
                        FLP_InteractionsMenuItems.Controls.Clear();
                        TXT_InteractionResult.Text = string.Empty;
                        // Wyświetl opis akcji (np. "Przeszukaj szuflady")

                        var actions = DatabaseManager.Instance.GetAllActionsForInteraction(interaction.InteractionId, false);

                        //MessageBox.Show(actions.Count().ToString());
                        foreach (var action in actions)
                        {
                            if (DatabaseManager.Instance.PlayerHasItem(action.RequiredItemId) && !action.IsExcecuted)
                            {
                                DatabaseManager.Instance.SetInteractionActionsActiveByRequiredItemId(action.RequiredItemId, true);
                            }
                        }

                        ShowPanel(Panel_InteractionPanel);
                        typingEffect.SkipText();
                        FLP_InteractionsMenuItems.Controls.Clear();

                        StartTypingItemDescription(item, interaction, RTX_ItemDescription, item.Description, true);

                    //}
                    //else
                    //{
                    //    MessageBox.Show("NOT");
                    //}

                //}
            }
        }

        private void BtnInteractionNPC_Click(object sender, EventArgs e)
        {
            if (sender is Button btnClicked && btnClicked.Tag is InteractionsForNPC npc_interaction)
            {
                BTN_BackDialogue.Enabled = false;
                dialogueRowId = 1;
                currentDialogueCharacterId = npc_interaction.CharacterId;
                ShowPanel(Panel_DialoguePanel);;
                fullTextForDialogue = "";

                RTX_DialogueTexts.Clear();

                string dialogueRow = DatabaseManager.Instance.GetDialogueRowByCharacterId(currentDialogueCharacterId, dialogueRowId);

                StartTypingDialogueRows(dialogueRow, false, false);

            }
        }

        private void CheckingActionNPCStatus(InteractionActionForNPC interaction)
        {
            if (interaction.FirstConnectedLocationId != 0 && interaction.SecondConnectedLocationId != 0)
            {
                if (interaction.ToSiededConnectionLocation)
                {
                    DatabaseManager.Instance.AddLocationConnection(interaction.FirstConnectedLocationId, interaction.SecondConnectedLocationId);
                    DatabaseManager.Instance.AddLocationConnection(interaction.SecondConnectedLocationId, interaction.FirstConnectedLocationId);
                }
                else
                {
                    DatabaseManager.Instance.AddLocationConnection(interaction.FirstConnectedLocationId, interaction.SecondConnectedLocationId);
                }
            }
            if (interaction.RewardItemId != 0)
            {
                DatabaseManager.Instance.AddItemToInventory(interaction.RewardItemId);
            }
            if (interaction.RequiredItemId != 0)
            {
                if (DatabaseManager.Instance.PlayerHasItem(interaction.RequiredItemId))
                {
                    DatabaseManager.Instance.RemoveItemFromPlayerInventory(interaction.RequiredItemId);
                }
            }
            if (interaction.InteractionId != 0)
            {
                DatabaseManager.Instance.SetInteractionsActive(interaction.InteractionId, true);
            }
            if (interaction.InteractionNPCId != 0)
            {
                DatabaseManager.Instance.SetInteractionsNPCActive(interaction.InteractionNPCId, true);
            }
            if (interaction.QuestId != 0)
            {
                DatabaseManager.Instance.SetQuestIsActive(interaction.QuestId);
                DatabaseManager.Instance.SetQuestSelected(interaction.QuestId);
                currentTrackedQuest = interaction.QuestId;
            }
            if (interaction.MapImageId != 0)
            {
                DatabaseManager.Instance.SetMapImageActive(interaction.MapImageId, interaction.MapImageActive);
            }
            int a = DatabaseManager.Instance.GetQuestIdByObjectiveFromActionId(0, interaction.ActionNPCId);
            var b = DatabaseManager.Instance.GetQuestsById(a);

            var objectives = DatabaseManager.Instance.GetObjectiveByActionId(0, interaction.ActionNPCId);
            int countObjectives = 1;

            if (b.Count != 0)
            {
                countObjectives = DatabaseManager.Instance.GetCountObjectivesByOrderObjective(currentTrackedQuest, b[0].CurrentObjectiveOrder);
            }

            if (countObjectives != 1 && !objectives[0].IsOptionalObjective)
            {
                DatabaseManager.Instance.SetObjectiveIsExcecutedByDescription(objectives[0].Description);
                UpdateQuestObjectiveDescription(false);
                int countExcecutedObjectives = DatabaseManager.Instance.GetCountExcecutedObjectivesByOrderObj(currentTrackedQuest, b[0].CurrentObjectiveOrder);

                if (DatabaseManager.Instance.GetCountExcecutedObjectivesByOrderObj(currentTrackedQuest, b[0].CurrentObjectiveOrder) == countObjectives)
                {
                    DatabaseManager.Instance.UpdateCurrentObjectiveOrder(objectives[0].QuestId);
                    UpdateQuestObjectiveDescription(false);

                }

                if (countExcecutedObjectives == countObjectives)
                {
                    var quests = DatabaseManager.Instance.GetQuestsById(objectives[0].QuestId);

                    if (quests[0].ContinueHistory)
                    {
                        var objectivesCheck = DatabaseManager.Instance.GetObjectiveByExcecuted(objectives[0].QuestId, false);
                        int chaptersCount = DatabaseManager.Instance.GetChaptersCount();

                        DatabaseManager.Instance.SetQuestIsExcecuted(objectives[0].QuestId);
                        TXT_Objectives.Text = "";

                        if (storyData.Count != currentTextIndex)
                        {
                            StartHistory();
                        }
                        else if (storyData.Count != currentChapterIndex + 1)
                        {
                            StartChapterMenu(true);

                        }
                        else if (chaptersCount != currentChapterIndex + 1)
                        {
                            ShowPanel(Panel_EndhistoryPanel);
                            DatabaseManager.Instance.SetProgressIsGameEnd(true);
                        }
                    }
                }

            }

            else if (countObjectives != 1 && objectives[0].IsOptionalObjective)
            {
                var quests = DatabaseManager.Instance.GetQuestsById(objectives[0].QuestId);
                DatabaseManager.Instance.SetObjectiveIsExcecutedByDescription(objectives[0].Description);
                var objectives2 = DatabaseManager.Instance.GetObjectives(currentTrackedQuest, b[0].CurrentObjectiveOrder);
                foreach (var objective2 in objectives2)
                {
                    int interactionId = DatabaseManager.Instance.GetInteractionIdByActionId(objective2.InteractionActionId);
                    int interactionNPCId = DatabaseManager.Instance.GetInteractionNPCIdByActionNPCId(objective2.InteractionActionNPCId);
                    DatabaseManager.Instance.SetInteractionsActive(interactionId, false);
                    DatabaseManager.Instance.SetInteractionsNPCActive(interactionNPCId, false);
                }

                if (quests[0].CurrentObjectiveOrder < DatabaseManager.Instance.GetMaxObjectiveOrderByQuestId(objectives[0].QuestId))
                {
                    DatabaseManager.Instance.UpdateCurrentObjectiveOrder(objectives[0].QuestId);
                }
                UpdateQuestObjectiveDescription(false);

                if (quests[0].ContinueHistory)
                {
                    var objectivesCheck = DatabaseManager.Instance.GetObjectiveByExcecuted(objectives[0].QuestId, false);
                    int chaptersCount = DatabaseManager.Instance.GetChaptersCount();

                    DatabaseManager.Instance.SetQuestIsExcecuted(objectives[0].QuestId);
                    TXT_Objectives.Text = "";

                    if (storyData.Count != currentTextIndex)
                    {
                        StartHistory();
                    }
                    else if (storyData.Count != currentChapterIndex + 1)
                    {
                        StartChapterMenu(true);

                    }
                    else if (chaptersCount != currentChapterIndex + 1)
                    {
                        ShowPanel(Panel_EndhistoryPanel);
                        DatabaseManager.Instance.SetProgressIsGameEnd(true);
                    }
                }
            }

            else if (objectives.Count() != 0)
            {
                var quests = DatabaseManager.Instance.GetQuestsById(objectives[0].QuestId);
                if (quests[0].CurrentObjectiveOrder < DatabaseManager.Instance.GetMaxObjectiveOrderByQuestId(objectives[0].QuestId))
                {
                    DatabaseManager.Instance.UpdateCurrentObjectiveOrder(objectives[0].QuestId);
                }
                UpdateQuestObjectiveDescription(true);
                DatabaseManager.Instance.SetObjectiveIsExcecutedByDescription(objectives[0].Description);

                if (quests[0].ContinueHistory)
                {
                    var objectivesCheck = DatabaseManager.Instance.GetObjectiveByExcecuted(objectives[0].QuestId, false);
                    int chaptersCount = DatabaseManager.Instance.GetChaptersCount();

                    if (objectivesCheck.Count() == 0)
                    {
                        DatabaseManager.Instance.SetQuestIsExcecuted(objectives[0].QuestId);
                        TXT_Objectives.Text = "";

                        if (storyData.Count != currentTextIndex)
                        {
                            StartHistory();
                        }
                        else if (storyData.Count != currentChapterIndex + 1)
                        {
                            StartChapterMenu(true);

                        }
                        else if (chaptersCount != currentChapterIndex + 1)
                        {
                            ShowPanel(Panel_EndhistoryPanel);
                            DatabaseManager.Instance.SetProgressIsGameEnd(true);
                        }
                    }
                    else
                    {

                    }
                }
            }

        }

        private void BtnInteractionActionForNPC_Click(object sender, EventArgs e)
        {
            if (sender is Button btnClicked && btnClicked.Tag is InteractionActionForNPC npc_interactionAction)
            {
                FLP_DialogueActions.Controls.Clear();;

                dialogueRowId = npc_interactionAction.NextDialogueRowId;

                int locationId = DatabaseManager.Instance.GetPlayerCurrentLocation();

                if (npc_interactionAction.NextDialogueRowId != 0)
                {
                    string dialogueRow = DatabaseManager.Instance.GetDialogueRowByCharacterId(currentDialogueCharacterId, dialogueRowId);
                    StartTypingDialogueAnswerAndRow($"\n\n{npc_interactionAction.RowDialouge}", $"\n\n{dialogueRow}", npc_interactionAction.IsTree, false);
                    FLP_DialogueActions.Controls.Remove( btnClicked );
                }
                else
                {
                    StartTypingDialogueRows($"\n\n{npc_interactionAction.RowDialouge}", npc_interactionAction.IsTree, true);
                }

                if (npc_interactionAction.NextDialogueId != 0)
                {
                    DatabaseManager.Instance.SetDialogueIdForCharacter(currentDialogueCharacterId, npc_interactionAction.NextDialogueId);
                }

                if (npc_interactionAction.RequiredItemId != 0)
                {
                    var item = DatabaseManager.Instance.GetItemById(npc_interactionAction.RequiredItemId);
                    if (DatabaseManager.Instance.PlayerHasItem(npc_interactionAction.RequiredItemId))
                    {
                        CheckingActionNPCStatus(npc_interactionAction);
                        if (item.UsedOnce)
                        {
                            DatabaseManager.Instance.RemoveItemFromPlayerInventory(npc_interactionAction.RequiredItemId);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {

                    CheckingActionNPCStatus(npc_interactionAction);
                }

                UpdateQuestObjectiveDescription(false);

                DatabaseManager.Instance.SetInteractionActionForNpcActive(false, npc_interactionAction.ActionNPCId);
                //DatabaseManager.Instance.SetInteractionsNPCActive( npc_interactionAction.NextDialogueId, true);

            }
        }

        private void BtnItem_Click(object sender, EventArgs e)
        {

            foreach (var button in inventoryItemButtons)
            {
                button.Enabled = true;
            }
            if (sender is Button btnClicked && btnClicked.Tag is Item selectedItem)
            {
                TXT_ItemDetails.Text = $"Nazwa: {selectedItem.Name}\n\n Opis: {selectedItem.Description}";
                btnClicked.Enabled = false;

            }
            else
            {
                TXT_ItemDetails.Text = "Wybierz przedmiot, aby zobaczyć szczegóły.";
            }

        }

        private void BtnQuest_Click(object sender, EventArgs e)
        {

            foreach (var button in questsButtons)
            {
                button.Enabled = true;
            }
            if (sender is Button btnClicked && btnClicked.Tag is Quest selectedQuest)
            {

                var objectives = DatabaseManager.Instance.GetObjectives(selectedQuest.QuestId, selectedQuest.CurrentObjectiveOrder);

                if (!selectedQuest.IsExcecuted)
                {
                    currentTrackedQuest = selectedQuest.QuestId;
                    UpdateQuestObjectiveDescription(false);
                }
                else
                {
                    currentSelectedQuest = selectedQuest.QuestId;
                    UpdateQuestObjectiveDescription(true);
                }
                btnClicked.Enabled = false;

            }
            else
            {
                TXT_QuestDescription.Text = "Wybierz zadanie, aby zobaczyć szczegóły.";
            }

        }

        private void UpdateQuestObjectiveDescription(bool endingQuests)
        {
            if (!endingQuests)
            {
                if (currentTrackedQuest != 0)
                {
                    var quests = DatabaseManager.Instance.GetQuests(false);

                    foreach (var quest in quests)
                    {
                        if (quest.QuestId == currentTrackedQuest)
                        {
                            var objectives = DatabaseManager.Instance.GetObjectives(currentTrackedQuest, quest.CurrentObjectiveOrder);

                            int countObjectives = DatabaseManager.Instance.GetCountObjectivesByOrderObjective(quest.QuestId, quest.CurrentObjectiveOrder);
                            FLP_QuestDetails.Controls.Clear();

                            Label questDetails = new Label
                            {
                                Text = $"Nazwa: {quest.Name}\n\nOpis: {quest.Description}",
                                ForeColor = Color.White,
                                Width = FLP_QuestDetails.Width - 20,
                                Height = FLP_QuestDetails.Height - 20,
                            };

                            FLP_QuestDetails.Controls.Add(questDetails);

                            currentTrackedQuest = quest.QuestId;
                            FLP_QuestObjectivesList.Controls.Clear();
                            FLP_ObjectivesGamePanel.Controls.Clear();
                           

                            foreach (var objective in objectives)
                            {
                                Label obejctiveDetails = new Label
                                {
                                    Width = FLP_QuestObjectivesList.Width - 40,
                                    AutoSize = false,
                                    Margin = new Padding(10, 10, 10, 10),
                                    Text = objective.Description,
                                    ForeColor = objective.IsExcecuted ? Color.Gray : Color.White,
                                };

                                FLP_QuestObjectivesList.Controls.Add(obejctiveDetails);
                            }

                            if (objectives.Count != 0)
                            {

                                if (DatabaseManager.Instance.GetCountObjectivesByOrderObjective(currentTrackedQuest, quest.CurrentObjectiveOrder) != 1)
                                {
                                    foreach (var objective in objectives)
                                    {
                                        Label obejctiveDetails = new Label
                                        {
                                            //Width = FLP_ObjectivesGamePanel.Width - 20,
                                            AutoSize = true,
                                            Margin = new Padding(10, 10, 10, 10),
                                            Text = objective.Description,
                                            ForeColor = objective.IsExcecuted ? Color.Gray : Color.White,
                                        };

                                        FLP_ObjectivesGamePanel.Controls.Add(obejctiveDetails);
                                    }
                                }
                                else
                                {
                                    Label obejctiveDetails1 = new Label
                                    {
                                        //Width = FLP_ObjectivesGamePanel.Width - 20,
                                        AutoSize = true,
                                        Margin = new Padding(10, 10, 10, 10),
                                        Text = objectives[0].Description,
                                        ForeColor = objectives[0].IsExcecuted ? Color.Gray : Color.White,
                                    };

                                    FLP_ObjectivesGamePanel.Controls.Add(obejctiveDetails1);
                                }
                            }
                               
                         }

                    }

                }
                
            }
            else
            {
                if (currentSelectedQuest != 0)
                {
                   
                    var quests = DatabaseManager.Instance.GetQuests(true);

                    foreach (var quest in quests)
                    {

                        if (quest.QuestId == currentSelectedQuest)
                        {
                            var objectives = DatabaseManager.Instance.GetObjectives(currentSelectedQuest, quest.CurrentObjectiveOrder);
                            FLP_QuestDetails.Controls.Clear();

                            Label questDetails = new Label
                            {
                                Text = $"Nazwa: {quest.Name}\n\nOpis: {quest.Description}",
                                ForeColor = Color.White,
                                Width = FLP_QuestDetails.Width - 20,
                                Height = FLP_QuestDetails.Height - 20,
                            };

                            FLP_QuestDetails.Controls.Add(questDetails);

                            currentSelectedQuest = quest.QuestId;
                            FLP_QuestObjectivesList.Controls.Clear();

                            foreach (var objective in objectives)
                            {

                                Label obejctiveDetails = new Label
                                {
                                    Width = FLP_QuestObjectivesList.Width - 40,
                                    AutoSize = false,
                                    Margin = new Padding(10, 10, 10, 10),
                                    Text = objective.Description,
                                    ForeColor = objective.IsExcecuted ? Color.Gray : Color.White,
                                    Font = new Font("Segoe UI", 9, !objective.IsExcecuted && objective.IsOptionalObjective ? FontStyle.Strikeout : FontStyle.Regular),
                                };

                                FLP_QuestObjectivesList.Controls.Add(obejctiveDetails);
                            }
                        }
                    }
                }
                else
                {
                    FLP_QuestDetails.Controls.Clear();
                    FLP_QuestObjectivesList.Controls.Clear();
                }
            }
          
        }
        private void BtnPerformNonItemAction_Click(object sender, EventArgs e)
        {
            if (sender is Button btnClicked && btnClicked.Tag is InteractionAction interaction)
            {
                int locationId = DatabaseManager.Instance.GetPlayerCurrentLocation();
            }
        }

        private void BTN_Exit_Click(object sender, EventArgs e)
        {
            Panel_AcceptExitPanel.Visible = true;
            Panel_AcceptExitPanel.BringToFront();
        }

        private void BTN_NewGame_Click(object sender, EventArgs e)
        {
            var progressValues = DatabaseManager.Instance.GetProgressValues();

            if (progressValues[0].IsGameWasLaunch)
            {
                ShowPanel(Panel_NewGame);

            }
            else
            {
                string cur_database = currentDatabaseDirectory + "\\" + currentDatabaseName;
                string cur_databaseCopy = currentDatabaseDirectory + "\\" + "Copy_" + currentDatabaseName;
                File.Copy(cur_database, cur_databaseCopy);
                StartChapterMenu(false);
                DatabaseManager.Instance.SetProgressValueGameLaunch();
            }

        }

        private void BTN_Wstecz_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_MainMenuPanel);
        }

        private void BTN_NewGameAccept_Click(object sender, EventArgs e)
        {
            DatabaseManager.Instance.CloseConnection();
            File.Copy(currentDatabaseDirectory + "\\" + "Copy_" + currentDatabaseName, currentDatabaseDirectory + "\\" + currentDatabaseName, overwrite: true);

            DatabaseManager.Instance.OpenConnection(currentDatabaseDirectory, currentDatabaseName);


            DatabaseManager.Instance.SetProgressValueGameLaunch();
            StartChapterMenu(false);

        }

        private void BTN_LoadGame_Click(object sender, EventArgs e)
        {
            var progressValues = DatabaseManager.Instance.GetProgressValues();

            if (!progressValues[0].IsGameEnd)
            {
                if (progressValues[0].IsOnGamePanel)
                {
                    InitializeGamePanel();
                    var (chapterTitle, data) = DatabaseManager.Instance.GetChapterData(currentChapterIndex);
                    currentTrackedQuest = DatabaseManager.Instance.GetQuestIdBySelected();
                    storyData = data; // Przypisz pobrane dane do storyData
                    UpdateQuestObjectiveDescription(false);
                    ShowPanel(Panel_GamePanel);
                }
                else
                {
                    StartChapterMenu(false);
                }
            }
            else
            {
                MessageBox.Show("Gra została ukończona");
            }

        }

        private void BTN_NextChapterMenu_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_HistoryPanel);

            // Pobierz paragrafy rozdziału z bazy danych
            var (chapterTitle, data) = DatabaseManager.Instance.GetChapterData(currentChapterIndex);
            storyData = data; // Przypisz pobrane dane do storyData
            currentTextIndex = 0;
            DatabaseManager.Instance.SetProgressCurrentParagraphIndex(currentTextIndex);

            if (storyData != null && storyData.Count > 0)
            {
                StartTypingCurrentParagraph(); // Rozpocznij wyświetlanie pierwszego paragrafu
            }

        }

        private void BTN_Skip_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText(); // Pomiń animację
            BTN_Next.Enabled = true;
        }

        private void BTN_Next_Click(object sender, EventArgs e)
        {
            if (BTN_Next.Text == "Przejdź do gry")
            {
                InitializeGamePanel();
                currentTrackedQuest = DatabaseManager.Instance.GetQuestIdBySelected();
                UpdateQuestObjectiveDescription(false);
                currentTextIndex++;
                DatabaseManager.Instance.SetProgressCurrentParagraphIndex(currentTextIndex);
            }
            else if (BTN_Next.Text == "Następny rozdział")
            {
                // Ostatni paragraf, przejdź do następnego rozdziału
                BTN_Next.Text = "Next"; // Reset tekstu przycisku
                currentTextIndex = 0;
                StartChapterMenu(true); // Powrót do ChapterMenu
            }
            else if (BTN_Next.Text == "Następny")
            {
                currentTextIndex++;
                DatabaseManager.Instance.SetProgressCurrentParagraphIndex(currentTextIndex);
                StartTypingCurrentParagraph();
            }
            else if (BTN_Next.Text == "Koniec")
            {
                DatabaseManager.Instance.SetProgressIsGameEnd(true);
                ShowPanel(Panel_EndhistoryPanel);
            }

        }

        private void BTN_ContinueStory_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText(); // Pomiń animację
        }

        private void BTN_LocationButton_Click(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is int nextLocationId)
            {
                // Zaktualizuj lokację gracza w bazie danych
                DatabaseManager.Instance.UpdatePlayerLocation(nextLocationId);
                typingEffect.SkipText();
                // Odśwież panel gry
                InitializeGamePanel();
            }
        }

        private void BTN_BackToGP_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText();
            ShowPanel(Panel_GamePanel);
            UpdateGamePanel();
        }

        private void BTN_SkipIP_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText();
        }

        private void BTN_BackToGP_Inv_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText();
            ShowPanel(Panel_GamePanel);
        }

        private void BTN_InventoryButton_Click_1(object sender, EventArgs e)
        {
            ShowPanel(Panel_InventoryPanel);
            TXT_ItemDetails.Text = "Wybierz przedmiot, aby zobaczyć szczegóły.";
            LoadPlayerInventory();
        }

        private void BTN_SelectHistory_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_SelectHistoryPanel);
            LoadHistories();
        }

        private void BTN_BackToMP_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_MainMenuPanel);
        }

        private void BTN_QuestPanel_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_QuestPanel);
            BTN_EndQuests.Enabled = true;
            BTN_MainQuests.Enabled = false;
            currentSelectedQuest = 0;
            LoadPlayerQuests(false);
            UpdateQuestObjectiveDescription(false);
        }

        private void BTN_QPBack_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText();
            ShowPanel(Panel_GamePanel);
        }

        private void BTN_EPtoMNP_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_MainMenuPanel);
        }

        private void BTN_MapPanel_Click(object sender, EventArgs e)
        {
            var imagesList = DatabaseManager.Instance.GetMapsFromMap(true);

            FLP_MapLevels.Controls.Clear();
            mapImageButtons.Clear();
            int btnTagIndex = 0;

            foreach (var image in imagesList)
            {
                Button mapBTN = new Button
                {
                    ForeColor = Color.White,
                    //BackColor = Color.Black,
                    UseCompatibleTextRendering = true,
                    UseVisualStyleBackColor = false,
                    Width = FLP_MapLevels.Width - 40,
                    Height = 40,
                    Text = image.Name,
                    Tag = btnTagIndex
                };

                mapBTN.Click += BTN_MapBTN_Click;
                btnTagIndex++;
                FLP_MapLevels.Controls.Add(mapBTN);
                mapImageButtons.Add(mapBTN);
            }

            using (MemoryStream ms = new MemoryStream(imagesList[btnIndex].ImageData))
            {
                Image image = Image.FromStream(ms);
                PB_MapImage.Image = image;  // Załaduj obraz do kontrolki PictureBox

                mapImageButtons[btnIndex].Enabled = false;
                mapImageButtons[btnIndex].ForeColor = Color.Gray;

                TXT_CurrentLocation.Text = RTX_LocationName.Text;
            }


            ShowPanel(Panel_MapPanel);


        }

        private void BTN_MapBTN_Click(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is int btnTagIndex)
            {
                btnIndex = btnTagIndex;

                var imagesList = DatabaseManager.Instance.GetMapsFromMap(true);
                using (MemoryStream ms = new MemoryStream(imagesList[btnIndex].ImageData))
                {
                    Image image = Image.FromStream(ms);
                    PB_MapImage.Image = image;  // Załaduj obraz do kontrolki PictureBox
                }

                foreach (Button buttonImage in mapImageButtons)
                {
                    buttonImage.Enabled = true;
                    buttonImage.ForeColor = Color.White;
                }

                button.Enabled = false;
                button.ForeColor = Color.Gray;

                TXT_CurrentLocation.Text = RTX_LocationName.Text;
            }
        }

        private void BTN_BackMPtoGP_Click_1(object sender, EventArgs e)
        {
            typingEffect.SkipText();
            ShowPanel(Panel_GamePanel);
        }

        private void BTN_SecHistoryPanel_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_SecondHistoryPanel);

            var (chapter, storyData) = DatabaseManager.Instance.GetChapterData(currentChapterIndex);
            currentShowParagraphIndex = currentTextIndex - 1;

            BTN_SecHistoryNext.Enabled = false;
            BTN_SecHistoryBack.Enabled = true;

            DisplayFormattedText(RTX_HistoryText, storyData[currentShowParagraphIndex].text);

            if (chapter.ToString() == "Prolog")
            {
                TXT_ChapterNumber.Text = chapter.ToString();
                TXT_ChapterName.Text = "";
            }
            else
            {
                TXT_ChapterNumber.Text = "Chapter " + currentChapterIndex.ToString();
                TXT_ChapterName.Text = chapter.ToString();
            }

            RTX_Paragraphs.Text = RTX_HistoryText.Text;
            AdjustFontSize(RTX_HistoryText);

            if (currentShowParagraphIndex == 0)
            {
                BTN_SecHistoryBack.Enabled = false;
            }
            else
            {
                BTN_SecHistoryBack.Enabled = true;
            }
            if (currentShowParagraphIndex == currentTextIndex - 1)
            {
                BTN_SecHistoryNext.Enabled = false;
            }
            else
            {
                BTN_SecHistoryNext.Enabled = true;
            }
        }

        private void ShowParagraphByIndex()
        {
            var (chapter, storyData) = DatabaseManager.Instance.GetChapterData(currentChapterIndex);

            AdjustFontSize(RTX_Paragraphs);
            DisplayFormattedText(RTX_Paragraphs, storyData[currentShowParagraphIndex].text);

            if (currentShowParagraphIndex == 0)
            {
                BTN_SecHistoryBack.Enabled = false;
            }
            else
            {
                BTN_SecHistoryBack.Enabled = true;
            }
            if (currentShowParagraphIndex == currentTextIndex - 1)
            {
                BTN_SecHistoryNext.Enabled = false;
            }
            else
            {
                BTN_SecHistoryNext.Enabled = true;
            }
        }

        private void BTN_BackSHPtoGP_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_GamePanel);
        }

        private void BTN_SecHistoryBack_Click(object sender, EventArgs e)
        {
            currentShowParagraphIndex--;
            ShowParagraphByIndex();

        }

        private void BTN_SecHistoryNext_Click(object sender, EventArgs e)
        {
            currentShowParagraphIndex++;
            ShowParagraphByIndex();

        }

        private void BTN_MainQuests_Click(object sender, EventArgs e)
        {
            LoadPlayerQuests(false);
            UpdateQuestObjectiveDescription(false);
            currentSelectedQuest = 0;
            BTN_EndQuests.Enabled = true;
            BTN_MainQuests.Enabled = false;
        }

        private void BTN_EndQuests_Click(object sender, EventArgs e)
        {
            LoadPlayerQuests(true);
            UpdateQuestObjectiveDescription(true);
            currentSelectedQuest = 0;
            BTN_MainQuests.Enabled = true;
            BTN_EndQuests.Enabled = false;
        }

        private void BTN_AcceptBack_Click(object sender, EventArgs e)
        {
            Panel_AcceptExitPanel.Visible = false;
        }

        private void BTN_AcceptExit_Click(object sender, EventArgs e)
        {
            Panel_AcceptExitPanel.Visible = false;
            if (Panel_MainMenuPanel.Visible)
            {
                Application.Exit();
            }
            else
            {
                BTN_LoadGame.Enabled = true;
                ShowPanel(Panel_MainMenuPanel);
            }
        }

        private void BTN_GPExit_Click(object sender, EventArgs e)
        {
            Panel_AcceptExitPanel.Visible = true;
            Panel_AcceptExitPanel.BringToFront();
        }

        private void BTN_HPExit_Click(object sender, EventArgs e)
        {
            Panel_AcceptExitPanel.Visible = true;
            Panel_AcceptExitPanel.BringToFront();
        }

        private void BTN_BackDialogue_Click(object sender, EventArgs e)
        {
            ShowPanel(Panel_GamePanel);
            UpdateGamePanel();
        }

        private void BTN_SkipButton_Click(object sender, EventArgs e)
        {
            typingEffect.SkipText();
        }
    }
}
