using Scraper.Models;
using System;

namespace Scraper
{
    public class GameRetrievedEventArgs : EventArgs
    {
        public SwitchGame Game { get; set; }

        #region Constructor
        public GameRetrievedEventArgs(SwitchGame game) : base()
        {
            this.Game = game;
        }
        #endregion
    }
}
