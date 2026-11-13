using System.Reflection;

using FluentAssertions;

using Skating.Desktop.Common;

namespace Skating.Desktop.Tests
{
    public sealed class PopupFormTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Escape_WhenClosingMayBeCancelled_RespectsClosingHandler(bool cancel) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var form = new PopupForm();
            var closingRequested = false;
            form.FormClosing += (_, e) => { closingRequested = true; e.Cancel = cancel; };
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.Show();
            var method = typeof(PopupForm).GetMethod("ProcessDialogKey", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

            // Act
            var handled = method.Invoke(form, [Keys.Escape]);

            // Assert
            handled.Should().Be(true);
            closingRequested.Should().BeTrue();
            form.IsDisposed.Should().Be(!cancel);
        });
    }
}
