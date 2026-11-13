using FluentAssertions;

using Skating.Desktop.Rankings;

namespace Skating.Desktop.Tests
{
    public sealed class RankingPrintDocumentTests
    {
        [Fact]
        public Task RenderPage_WhenManyRows_PaginatesUntilComplete() => WinFormsTest.Run(() =>
        {
            // Arrange
            var rows = Enumerable.Range(1, 70).Select(index => new[] { index.ToString(), $"Coreografia {index} — Tecnica e musicalità", "Scuola di danza", "123,4" }).ToArray();
            using var document = new RankingPrintDocument("Caraibico / Senior / Open", "PROVVISORIA · Classifica completa", ["Pos.", "Coreografia", "Scuola", "Punti"], rows, [1, 5, 3, 1]);
            using var bitmap = new Bitmap(1100, 780);
            using var graphics = Graphics.FromImage(bitmap);
            var pages = 0;
            bool more;

            // Act
            do
            {
                graphics.Clear(Color.White);
                more = document.RenderPage(graphics, new Rectangle(40, 40, 1020, 700));
                pages++;
                var directory = Environment.GetEnvironmentVariable("SKATING_PREVIEW_DIRECTORY");
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                    bitmap.Save(Path.Combine(directory, $"stampa-classifica-{pages}.png"));
                }
            }
            while (more && pages < 20);

            // Assert
            more.Should().BeFalse();
            pages.Should().BeInRange(2, 10);
        });

        [Fact]
        public Task RenderPage_WhenEmpty_ProducesSinglePage() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var document = new RankingPrintDocument("Assoluto PRO", "PROVVISORIA · Solo podio", ["Pos.", "Coreografia"], [], [1, 5]);
            using var bitmap = new Bitmap(1000, 700);
            using var graphics = Graphics.FromImage(bitmap);

            // Act
            var more = document.RenderPage(graphics, new Rectangle(40, 40, 920, 620));

            // Assert
            more.Should().BeFalse();
        });
    }
}
