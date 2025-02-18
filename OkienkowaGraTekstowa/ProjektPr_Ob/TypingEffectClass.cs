using System;
using System.Timers;
using System.Windows.Forms;

namespace ProjektPr_Ob
{
    public class TypingEffectClass
    {
        private string text1;
        private string text2;
        private bool isPrimaryActive = true;
        private RichTextBox label1;
        private RichTextBox label2;
        private Button buttonNext;
        private Button buttonSkip;
        private Action onTypingCompleted;
        private int charIndex = 0;
        private System.Windows.Forms.Timer typingTimer;
        private bool isHistoryPanel;
        private int typingInterval;
        private FontStyle currentStyle = FontStyle.Regular;
        private string fullTextForDialogues = "";

        public TypingEffectClass(string text1, string text2, RichTextBox label1, RichTextBox label2, Button buttonNext, Button buttonSkip, bool isHistoryPanel, Action onTypingCompleted, int typingInterval)
        {
            this.text1 = text1;
            this.text2 = text2;
            this.label1 = label1;
            this.label2 = label2;
            this.buttonNext = buttonNext;
            this.buttonSkip = buttonSkip;
            this.onTypingCompleted = onTypingCompleted;
            this.isHistoryPanel = isHistoryPanel;
            this.typingInterval = typingInterval;


            typingTimer = new System.Windows.Forms.Timer();
            typingTimer.Interval = typingInterval; // Czas między literami (100ms)
            typingTimer.Tick += TypingTimer_Tick;
        }

        public void StartTypingText()
        {
            if (label2 != null)
            {
                label2.Text = "";
            }

            if (label1.Name != "RTX_DialogueTexts")
            {
                label1.Text = "";  // Wyczyszczenie labela przed rozpoczęciem
            }
            else
            {
                fullTextForDialogues += text1;
            }    

            charIndex = 0;
            isPrimaryActive = true;

            if (buttonNext != null)
                buttonNext.Enabled = false; // Ustawienie przycisku Next na disabled przed rozpoczęciem animacji

            typingTimer.Start(); // Rozpoczęcie wypisywania tekstu
        }

        private void EnableNextButton()
        {
            if (buttonNext != null)
                buttonNext.Enabled = true; // Aktywowanie przycisku Next po zakończeniu wypisywania
        }

        public void EnableDisableSkipBtn(Button button, bool enabled)
        {
            if(enabled)
            {
                button.Enabled = true;
            }
            else
            {
                button.Enabled = false;
            }
        }

        //private void AdjustFontSize(Label label)
        //{
        //    using (Graphics g = label.CreateGraphics())
        //    {
        //        float fontSize = label.Font.Size; // Aktualny rozmiar czcionki
        //        SizeF textSize = g.MeasureString(label.Text, new Font(label.Font.FontFamily, fontSize), label.Width); // Uwzględniamy szerokość labela

        //        // Sprawdzamy, czy wysokość tekstu przekracza wysokość Label (ignorujemy szerokość)
        //        while (textSize.Height > label.Height && fontSize > 8)
        //        {
        //            fontSize -= 0.8f; // Stopniowe zmniejszanie czcionki
        //            textSize = g.MeasureString(label.Text, new Font(label.Font.FontFamily, fontSize), label.Width);
        //        }

        //        // Ustaw nową czcionkę
        //        label.Font = new Font(label.Font.FontFamily, fontSize);
        //    }
        //}

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            string currentText = isPrimaryActive ? text1 : text2;
            RichTextBox currentRTB = isPrimaryActive ? label1 : label2;
            if (charIndex < currentText.Length)
            {
                //currentRTB.SuspendLayout(); // 🔹 Wyłącz odświeżanie dla płynności

                string nextChar = currentText[charIndex].ToString();

                //Obsługa tagów formatowania
                if (nextChar == "[" && charIndex + 2 < currentText.Length)
                {
                    string tag = currentText.Substring(charIndex, 3);

                    if (tag == "[b]" || tag == "[i]")
                    {
                        currentStyle = tag == "[b]" ? FontStyle.Bold : FontStyle.Italic;
                        charIndex += 3;
                        //currentRTB.ResumeLayout();
                        return;
                    }
                    else if (currentText.Substring(charIndex, 4) == "[/b]" || currentText.Substring(charIndex, 4) == "[/i]")
                    {
                        currentStyle = FontStyle.Regular;
                        charIndex += 4;
                        //currentRTB.ResumeLayout();
                        return;
                    }
                }

                if (label1.Name == "RTX_Chapter" && label2.Name == "RTX_Chapter_Name")
                {
                    currentStyle = FontStyle.Bold;
                    //return;
                }
                else if (label1.Name == "RTX_LocationName" && label2.Name == "RTX_Location_Description")
                {
                    label1.SelectionFont = new Font(label1.Font, FontStyle.Bold);
                    label2.SelectionFont = new Font(label2.Font, FontStyle.Regular);
                }
                else
                {
                    // Dodanie nowej litery
                    currentRTB.SelectionFont = new Font(currentRTB.Font, currentStyle);
                }

                //currentRTB.SuspendLayout();
                currentRTB.SelectionFont = new Font(currentRTB.Font.FontFamily, currentRTB.Font.Size, currentStyle);
                currentRTB.AppendText(nextChar);
                //currentRTB.ResumeLayout();
                //currentRTB.AppendText(nextChar);
                //Form1.DisplayFormattedText(label1, label1.Text);
                charIndex++;
                if (nextChar == "\n")
                {
                    Form1.AdjustFontSize(currentRTB);
                    //Form1.DisplayFormattedText(currentRTB, currentRTB.Text);
                }



                //currentRTB.ResumeLayout(); // 🔹 Przywrócenie aktualizacji UI
            }
            else
            {
                if (isPrimaryActive && !string.IsNullOrEmpty(text2))
                {
                    // Przejście do drugiego RichTextBox
                    isPrimaryActive = false;
                    charIndex = 0;
                }
                else
                {
                    //MessageBox.Show("END");
                    EnableNextButton();
                    typingTimer.Stop();
                    Form1.AdjustFontSize(currentRTB);
                    //Form1.DisplayFormattedText(currentRTB, currentRTB.Text);
                    onTypingCompleted.Invoke();
                }
                //return;
            }
        }

        public void SkipText()
        {
            typingTimer.Stop();
            if (label1.Name == "RTX_DialogueTexts")
            {
                //label1.Text = Form1.fullTextForDialogue;
            }
            else
            {
                label1.Text = text1;  // Natychmiastowe wyświetlenie pełnego tekstu
            }

            if (label2 != null)
            {
                label2.Text = text2;
            }
            //MessageBox.Show(label1.Name);

            if (label1.Name == "RTX_HistoryText" || label1.Name == "RTX_ItemDescription")
            {
                Form1.DisplayFormattedText(label1, text1);
                Form1.AdjustFontSize(label1);
                label1.Enabled = true;
            }
            else if(label1.Name == "RTX_DialogueTexts")
            {
                Form1.AdjustFontSize(label1);
                Form1.DisplayFormattedText(label1, Form1.fullTextForDialogue);
                label1.Enabled = true;
            }

            EnableNextButton(); // Natychmiastowe włączenie przycisku Next po pominięciu animacji
            onTypingCompleted?.Invoke(); // Wywołanie akcji po pominięciu
        }
    }

}
