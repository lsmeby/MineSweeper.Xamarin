using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using MineSweeper.Forms.CustomControls;
using MineSweeper.Core;

namespace MineSweeper.Forms
{
    public partial class Game
    {
        private const int rows = 10;
        private const int columns = 10;
        private const int mines = 10;
        private const string MinesLeftText = "Mines left: {0}";

        private int test = 0;

        private MineSweeper.Core.MineSweeper _core;
        private int _minesLeft;

        public Game()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            foreach (var child in MineGrid.Children)
            {
                var button = child as MineButton;
                button.TextColor = Color.Black;
                button.Font = Font.SystemFontOfSize(NamedSize.Small);
                button.BorderRadius = 5;
                button.Clicked += MineButtonClicked;
            }
            MineGrid.SizeChanged += (sender, e) =>
            {
                var g = sender as Grid;
                g.HeightRequest = g.Width;
            };
            ResetGame();
        }

        protected void NewGameClicked(object sender, EventArgs e)
        {
            ResetGame();
        }

        protected void MineButtonClicked(object sender, EventArgs e)
        {
            if(!DigButton.IsEnabled)
            {
                OpenField(sender as MineButton);
            }
            
            else
            {
                MarkField(sender as MineButton);
            }
        }

        protected void SwitchToDig(object sender, EventArgs e)
        {
            DigButton.IsEnabled = false;
            FlagButton.IsEnabled = true;
        }

        protected void SwitchToFlag(object sender, EventArgs e)
        {
            DigButton.IsEnabled = true;
            FlagButton.IsEnabled = false;
        }

        private void ResetGame()
        {
            foreach (var child in MineGrid.Children)
            {
                var button = child as MineButton;
                button.IsEnabled = true;
                button.BackgroundColor = Color.Gray;
                button.Text = "";
                button.TextColor = Color.Black;
                button.Image = null;
            }
            _core = null;
            _minesLeft = mines;
            MinesLeftLabel.Text = String.Format(MinesLeftText, _minesLeft);
        }

        private void OpenField(MineButton button)
        {
            if(_core == null)
            {
                _core = new MineSweeper.Core.MineSweeper(columns, rows, mines, button.XPosition, button.YPosition);
            }
            
            if(_core.IsMarked(button.XPosition, button.YPosition))
            {
                return;
            }

            button.IsEnabled = false;
            button.BackgroundColor = Color.Silver;
            var status = _core.Open(button.XPosition, button.YPosition);

            switch(status)
            {
                case FieldStatus.FieldIsMine:
                    ServeGameOver(button);
                    return;
                case FieldStatus.MinesNearby0:
                    button.Text = "";
                    OpenNearbyFields(button);
                    break;
                case FieldStatus.MinesNearby1:
                    button.Text = "1";
                    button.TextColor = Color.Blue;
                    break;
                case FieldStatus.MinesNearby2:
                    button.Text = "2";
                    button.TextColor = Color.Green;
                    break;
                case FieldStatus.MinesNearby3:
                    button.Text = "3";
                    button.TextColor = Color.Red;
                    break;
                case FieldStatus.MinesNearby4:
                    button.Text = "4";
                    button.TextColor = Color.FromHex("001085");
                    break;
                case FieldStatus.MinesNearby5:
                    button.Text = "5";
                    button.TextColor = Color.FromHex("850000");
                    break;
                case FieldStatus.MinesNearby6:
                    button.Text = "6";
                    button.TextColor = Color.FromHex("008585");
                    break;
                case FieldStatus.MinesNearby7:
                    button.Text = "7";
                    button.TextColor = Color.Purple;
                    break;
                default:
                    button.Text = "8";
                    button.TextColor = Color.Black;
                    break;
            }
            
            if(MineGrid.Children.OfType<MineButton>().Count(b => b.IsEnabled) == mines)
            {
                ServeGameWin();
            }
        }

        private void MarkField(MineButton button)
        {
            if(_core == null)
            {
                return;
            }

            var status = _core.Mark(button.XPosition, button.YPosition);

            if(status == MarkFieldStatus.FieldIsMarked)
            {
                _minesLeft--;
                MinesLeftLabel.Text = String.Format(MinesLeftText, _minesLeft > 0 ? _minesLeft : 0);
                button.Image = "flag.png";
            }
            else
            {
                _minesLeft++;
                MinesLeftLabel.Text = String.Format(MinesLeftText, _minesLeft > 0 ? _minesLeft : 0);
                button.Image = null;
            }
        }

        private void ServeGameOver(MineButton button)
        {
            foreach(var child in MineGrid.Children)
            {
                var b = child as MineButton;
                b.IsEnabled = false;
                b.BackgroundColor = Color.Silver;

                if(!_core.IsMarked(b.XPosition, b.YPosition))
                {
                    if(_core.HasMine(b.XPosition, b.YPosition))
                    {
                        b.Image = "mine.png";
                    }
                }
                else if(!_core.HasMine(b.XPosition, b.YPosition))
                {
                    b.Image = "nomine.png";
                }
            }

            button.Image = "explodedmine.png";
            DisplayAlert("Game over", "BOOM! You lose!", "OK");
        }

        private void ServeGameWin()
        {
            foreach(var child in MineGrid.Children)
            {
                var button = child as MineButton;
                button.IsEnabled = false;

                if(!_core.IsMarked(button.XPosition, button.YPosition) &&
                    _core.HasMine(button.XPosition, button.YPosition))
                {
                    button.Image = "flag.png";
                }
            }

            MinesLeftLabel.Text = String.Format(MinesLeftText, 0);
            DisplayAlert("Game over", "You win!", "OK");
        }

        private void OpenNearbyFields(MineButton button)
        {
            var nearbyButtons = GetNearbyButtons(button);
            
            foreach(var b in nearbyButtons)
            {
                OpenField(b);
            }
        }

        private List<MineButton> GetNearbyButtons(MineButton button)
        {
            if(!AreValidCoordinates(button.XPosition, button.YPosition))
            {
                return new List<MineButton>();
            }

            return MineGrid.Children
                .OfType<MineButton>()
                .Where(b => b.IsEnabled
                    && (b.XPosition != button.XPosition || b.YPosition != button.YPosition)
                    && b.XPosition > button.XPosition - 2
                    && b.XPosition < button.XPosition + 2
                    && b.YPosition > button.YPosition - 2
                    && b.YPosition < button.YPosition + 2)
                    .ToList();
        }

        private bool AreValidCoordinates(int x, int y)
        {
            return x >= 0
                && x < columns
                && y >= 0
                && y < rows;
        }
    }
}
