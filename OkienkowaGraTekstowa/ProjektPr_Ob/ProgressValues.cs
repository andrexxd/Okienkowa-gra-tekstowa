using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//IsGameWasLaunch, CurrentChapterId, IsOnGamePanel, IsGameEnd
namespace ProjektPr_Ob
{
    public class ProgressValues
    {
        public bool IsGameWasLaunch {  get; set; }
        public int CurrentChapterId { get; set; }
        public int CurrentParagraphId { get; set; }
        public bool IsOnGamePanel { get; set; }
        public bool IsGameEnd { get; set; }
        public int CurrentTrackedQuest {  get; set; }

        public ProgressValues(bool isGameWasLaunch, int currentChapterId, int currentParagraphId, bool isOnGamePanel, bool isGameEnd, int currentTrackedQuest)
        {
            IsGameWasLaunch = isGameWasLaunch;
            CurrentChapterId = currentChapterId;
            CurrentParagraphId = currentParagraphId;
            IsOnGamePanel = isOnGamePanel;
            IsGameEnd = isGameEnd;
            CurrentTrackedQuest = currentTrackedQuest;
        }

    }
}
