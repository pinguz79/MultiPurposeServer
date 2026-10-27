using FluentAssertions;

using Skating.Desktop.Manifestations;

namespace Skating.Desktop.Tests
{
    public sealed class ArchiveUpgradeDialogTests
    {
        [Fact]
        public Task ArchiveUpgradeDialog_WhenCreated_OffersThreeChoicesWithBackupDefault() => WinFormsTest.Run(() =>
        {
            // Arrange

            // Act
            using var dialog = new ArchiveUpgradeDialog();

            // Assert
            var backup = dialog.Controls.Find("backupButton", true).OfType<Button>().Single();
            var skip = dialog.Controls.Find("skipBackupButton", true).OfType<Button>().Single();
            var cancel = dialog.Controls.Find("cancelButton", true).OfType<Button>().Single();
            backup.DialogResult.Should().Be(DialogResult.Yes);
            skip.DialogResult.Should().Be(DialogResult.No);
            skip.Text.Should().Be("Continua senza backup");
            cancel.DialogResult.Should().Be(DialogResult.Cancel);
            dialog.AcceptButton.Should().BeSameAs(backup);
            dialog.CancelButton.Should().BeSameAs(cancel);
        });
    }
}
