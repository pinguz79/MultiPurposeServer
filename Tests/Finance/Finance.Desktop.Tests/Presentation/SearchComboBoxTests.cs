using System.Reflection;

using Finance.Desktop.Controls;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class SearchComboBoxTests
    {
        [Fact]
        public Task OnKeyPress_WhenTypingPrefix_SuppressesNativeSingleCharacterSearch() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var combo = new SearchComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            combo.Items.AddRange(["Gallarate", "Genova", "Empoli"]);
            MethodInfo onKeyPress = typeof(SearchComboBox).GetMethod("OnKeyPress", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var first = new KeyPressEventArgs('G');
            var second = new KeyPressEventArgs('E');

            // Act
            onKeyPress.Invoke(combo, [first]);
            onKeyPress.Invoke(combo, [second]);

            // Assert
            combo.SelectedItem.Should().Be("Genova");
            first.Handled.Should().BeTrue();
            second.Handled.Should().BeTrue();
            return Task.CompletedTask;
        });

        [Fact]
        public Task Search_WhenTypingQuickly_UsesPrefixAndResetsAfterPause() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var combo = new SearchComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            combo.Items.AddRange(["Gallarate", "Genova Est", "Genova Ovest", "Empoli"]);
            // Act
            combo.Search('G', 10000);
            // Assert
            combo.SelectedIndex.Should().Be(0);
            combo.Search('e', 10200);
            combo.SelectedIndex.Should().Be(1);
            foreach (char character in "nova o")
            {
                combo.Search(character, 10300);
            }
            combo.SelectedIndex.Should().Be(2);
            combo.Search('E', 12000);
            combo.SelectedIndex.Should().Be(3);
            return Task.CompletedTask;
        });

        [Fact]
        public Task Search_WhenUsingDisplayMember_KeepsSelectionOnNoMatch() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var combo = new SearchComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Label" };
            combo.Items.AddRange([new { Label = "Émpoli" }, new { Label = "Genova" }]);
            // Act
            combo.Search('e', 10000);
            // Assert
            combo.SelectedIndex.Should().Be(0);
            combo.Search('x', 10100);
            combo.SelectedIndex.Should().Be(0);
            combo.Search('\b', 10200);
            combo.Search('m', 10300);
            combo.SelectedIndex.Should().Be(0);
            return Task.CompletedTask;
        });
    }
}
