
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace game_rps
{
    public partial class MainPage : ContentPage
    {
        private int score = Preferences.Get("Score", 0);
        private Dictionary<string, string> beats = new Dictionary<string, string>
        {
            { "Rock", "Scissors" },
            { "Paper", "Rock" },
            { "Scissors", "Paper" }
        };

        private Dictionary<string, Image> choiceImages = new();
        private Dictionary<string, Frame> choiceFrames = new();
        private bool isAnimating = false;

        public MainPage()
        {
            InitializeComponent();
            ScoreLabel.Text = $"Score: {score}";

            // Initialize mappings
            choiceImages["Rock"] = RockButton;
            choiceImages["Paper"] = PaperButton;
            choiceImages["Scissors"] = ScissorsButton;

            choiceFrames["Rock"] = RockFrame;
            choiceFrames["Paper"] = PaperFrame;
            choiceFrames["Scissors"] = ScissorsFrame;
        }

        private async void SelectChoice(object sender, TappedEventArgs e)
        {
            if (isAnimating || e.Parameter is not string userChoice) return;

            isAnimating = true;
            var selectedImage = choiceImages[userChoice];
            var selectedFrame = choiceFrames[userChoice];

            // 1. Immediately show user's selection
            PlayerChoiceImage.Source = selectedImage.Source;
            PlayerChoiceFrame.BackgroundColor = selectedFrame.BackgroundColor;
            ChoicesDisplayGrid.IsVisible = true;
            PlayerChoiceFrame.IsVisible = true;
            ComputerChoiceFrame.IsVisible = false; // Hide computer choice initially

            // 2. Hide all other options
            await HideAllChoices();

            // 3. Show thinking indicator
            ThinkingIndicator.IsVisible = true;
            ThinkingIndicator.IsRunning = true;

            // 4. Computer "thinking" delay (1 second)
            await Task.Delay(1000);

            // 5. Get and show computer choice
            string computerChoice = GetComputerChoice();
            ComputerChoiceImage.Source = choiceImages[computerChoice].Source;
            ComputerChoiceFrame.BackgroundColor = choiceFrames[computerChoice].BackgroundColor;
            ComputerChoiceFrame.IsVisible = true;

            // 6. Hide thinking indicator
            ThinkingIndicator.IsVisible = false;

            // 7. Animate the results
            await Task.WhenAll(
                PlayerChoiceImage.ScaleTo(1.2, 200),
                ComputerChoiceImage.ScaleTo(1.2, 200)
            );
            await Task.WhenAll(
                PlayerChoiceImage.ScaleTo(1.0, 200),
                ComputerChoiceImage.ScaleTo(1.0, 200)
            );

            // 8. Determine and show result
            await DetermineResult(userChoice, computerChoice);
            isAnimating = false;
        }

        //private async Task HideOtherChoices(Image selectedImage)
        //{
        //    var animations = new List<Task>();
        //    foreach (var choice in choiceImages.Values)
        //    {
        //        if (choice != selectedImage)
        //        {
        //            animations.Add(choice.FadeTo(0, 200));
        //            choice.IsVisible = false;
        //            choiceFrames[GetChoiceName(choice)].IsVisible = false;
        //        }
        //    }
        //    await Task.WhenAll(animations);
        //    ChoiceButtonsGrid.IsEnabled = false;
        //}
        private async Task HideAllChoices()
        {
            var animations = new List<Task>();
            foreach (var choice in choiceImages.Values)
            {
                animations.Add(choice.FadeTo(0, 200));
                choice.IsVisible = false;
                choiceFrames[GetChoiceName(choice)].IsVisible = false;
            }
            await Task.WhenAll(animations);
            ChoiceButtonsGrid.IsEnabled = false;
        }

        private async Task DetermineResult(string userChoice, string computerChoice)
        {
            if (userChoice == computerChoice)
            {
                ResultLabel.Text = "It's a Tie!";
                await AnimateResult(ResultLabel);
            }
            else if (beats[userChoice] == computerChoice)
            {
                score++;
                ResultLabel.Text = "You Win!";
                await AnimateWin();
            }
            else
            {
                score = Math.Max(0, score - 1);
                ResultLabel.Text = "You Lose!";
                await AnimateLoss();
            }

            UpdateScore();
            PlayAgainButton.IsVisible = true;
        }

        private async Task AnimateResult(Label label)
        {
            await label.ScaleTo(1.2, 200);
            await label.ScaleTo(1.0, 200);
        }

        private async Task AnimateWin()
        {
            await PlayerChoiceImage.ScaleTo(1.3, 200);
            await PlayerChoiceImage.ScaleTo(1.0, 200);
        }

        private async Task AnimateLoss()
        {
            await ComputerChoiceImage.ScaleTo(1.3, 200);
            await ComputerChoiceImage.ScaleTo(1.0, 200);
        }

        private void UpdateScore()
        {
            ScoreLabel.Text = $"Score: {score}";
            Preferences.Set("Score", score);
        }

        private string GetChoiceName(Image image)
        {
            if (image == RockButton) return "Rock";
            if (image == PaperButton) return "Paper";
            if (image == ScissorsButton) return "Scissors";
            return string.Empty;
        }

        private async void PlayAgainClicked(object sender, EventArgs e)
        {
            PlayAgainButton.IsVisible = false;
            ChoicesDisplayGrid.IsVisible = false;
            ResultLabel.Text = "Choose an option!";
            ResultLabel.Scale = 1;

            var animations = new List<Task>();
            foreach (var choice in choiceImages.Values)
            {
                choice.IsVisible = true;
                choice.Opacity = 1;
                choice.Scale = 1;
                choiceFrames[GetChoiceName(choice)].IsVisible = true;
                animations.Add(choice.FadeTo(1, 200));
            }

            await Task.WhenAll(animations);
            ChoiceButtonsGrid.IsEnabled = true;
        }

        private string GetComputerChoice()
        {
            string[] choices = { "Rock", "Paper", "Scissors" };
            return choices[new Random().Next(choices.Length)];
        }

        private async void ShowRulesPopup(object sender, EventArgs e)
        {
            var popup = new RulesPopup();
            await this.ShowPopupAsync(popup);
        }
    }
}