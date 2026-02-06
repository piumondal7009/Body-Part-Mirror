using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BodyPartMirror;

public partial class MainWindow : Window
{
    private readonly List<BodyPart> _bodyParts;
    private readonly Random _random = new();
    private BodyPart? _currentQuizPart;
    private int _quizScore;
    private int _quizAttempts;

    public MainWindow()
    {
        InitializeComponent();
        _bodyParts = new List<BodyPart>
        {
            new("Head", "The head holds your brain, eyes, ears, nose, and mouth.", "Tip: Your brain helps you think and learn!"),
            new("Chest", "The chest protects your heart and lungs.", "Tip: Your heart beats like a drum to move blood."),
            new("Left Arm", "Your left arm helps you hug, wave, and lift.", "Tip: Arms have elbows that bend."),
            new("Right Arm", "Your right arm helps you throw, catch, and reach.", "Tip: Arms are super helpers!"),
            new("Left Leg", "Your left leg helps you run, jump, and balance.", "Tip: Legs have knees that bend."),
            new("Right Leg", "Your right leg helps you walk and kick.", "Tip: Legs are strong and steady!"),
        };

        LearningPartsList.ItemsSource = _bodyParts.Select(part => part.Name).ToList();
        QuizPartsList.ItemsSource = _bodyParts.Select(part => part.Name).ToList();

        SetLearningCard(_bodyParts.First());
        SetNewQuizQuestion();
    }

    private void LearningPartsList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        StartDragFromListBox(LearningPartsList, e);
    }

    private void QuizPartsList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        StartDragFromListBox(QuizPartsList, e);
    }

    private static void StartDragFromListBox(ListBox listBox, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (listBox.SelectedItem is not string selected)
        {
            return;
        }

        DragDrop.DoDragDrop(listBox, selected, DragDropEffects.Move);
    }

    private void LearningTarget_DragEnter(object sender, DragEventArgs e)
    {
        HandleDragEnter(sender, e);
    }

    private void QuizTarget_DragEnter(object sender, DragEventArgs e)
    {
        HandleDragEnter(sender, e);
    }

    private static void HandleDragEnter(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.StringFormat))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        e.Effects = DragDropEffects.Move;
        if (sender is Border border)
        {
            border.Background = new SolidColorBrush(Color.FromRgb(255, 226, 240));
        }
    }

    private void LearningTarget_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetDroppedPart(e, out var partName) || sender is not Border border)
        {
            return;
        }

        ResetTargetBackground(border);
        var targetName = border.Tag?.ToString();
        if (string.Equals(targetName, partName, StringComparison.OrdinalIgnoreCase))
        {
            border.Child = new TextBlock
            {
                Text = partName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold
            };
            var part = _bodyParts.First(item => item.Name == partName);
            SetLearningCard(part);
        }
        else
        {
            SetLearningCardMessage("Oops! Try another spot.", "Tip: Match the word to the same body part.");
        }
    }

    private void QuizTarget_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetDroppedPart(e, out var partName) || sender is not Border border || _currentQuizPart is null)
        {
            return;
        }

        ResetTargetBackground(border);
        var targetName = border.Tag?.ToString();
        _quizAttempts++;
        if (string.Equals(targetName, partName, StringComparison.OrdinalIgnoreCase) && partName == _currentQuizPart.Name)
        {
            _quizScore++;
            QuizFeedback.Text = $"Great job! {partName} is correct.";
            border.Child = new TextBlock
            {
                Text = partName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold
            };
            SetNewQuizQuestion();
        }
        else
        {
            QuizFeedback.Text = "Not quite. Try again!";
        }

        QuizScore.Text = $"Score: {_quizScore} / {_quizAttempts}";
    }

    private static bool TryGetDroppedPart(DragEventArgs e, out string partName)
    {
        partName = string.Empty;
        if (!e.Data.GetDataPresent(DataFormats.StringFormat))
        {
            return false;
        }

        partName = (string)e.Data.GetData(DataFormats.StringFormat);
        return !string.IsNullOrWhiteSpace(partName);
    }

    private static void ResetTargetBackground(Border border)
    {
        border.Background = new SolidColorBrush(Color.FromRgb(255, 245, 238));
    }

    private void SetLearningCard(BodyPart part)
    {
        LearningCardTitle.Text = part.Name;
        LearningCardDescription.Text = part.Description;
        LearningCardTip.Text = part.Tip;
    }

    private void SetLearningCardMessage(string title, string tip)
    {
        LearningCardTitle.Text = title;
        LearningCardDescription.Text = string.Empty;
        LearningCardTip.Text = tip;
    }

    private void SetNewQuizQuestion()
    {
        var availableParts = _bodyParts.ToList();
        _currentQuizPart = availableParts[_random.Next(availableParts.Count)];
        QuizPrompt.Text = _currentQuizPart.Name;
        QuizFeedback.Text = "Drag the matching word to the right spot.";
        QuizScore.Text = $"Score: {_quizScore} / {_quizAttempts}";
    }

    private void NewQuestionButton_Click(object sender, RoutedEventArgs e)
    {
        SetNewQuizQuestion();
    }
}

public record BodyPart(string Name, string Description, string Tip);
