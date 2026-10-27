using System.Data.Common;
using System.Text.Json;

using Skating.Core;
using Skating.Desktop.Choreographies;
using Skating.Desktop.Configuration;
using Skating.Desktop.Manifestations;
using Skating.Desktop.Races;
using Skating.Desktop.Rankings;
using Skating.Desktop.Registry;
using Skating.Desktop.Scheduling;
using Skating.Desktop.Scoring;

namespace Skating.Desktop.Shell
{
    internal sealed partial class MainForm : Form
    {
        private readonly Func<string, IManifestationStore> _storeFactory;
        private readonly Func<string, IRegistryStore> _registryFactory;
        private readonly Func<string, IChoreographyStore> _choreographyFactory;
        private readonly Func<string, IRaceStore> _raceFactory;
        private readonly RecentManifestationStore _recentStore;
        private readonly Func<string, IPerformanceStore> _performanceFactory;
        private readonly ScorePreferencesStore _scorePreferences;
        private IPerformanceStore? _performanceStore;
        private IRegistryStore? _registryStore;
        private IChoreographyStore? _choreographyStore;
        private IRaceStore? _raceStore;
        private readonly Func<string, IIntermediateAwardStore>? _awardFactory;
        private readonly AwardAggregation? _aggregation;
        private IIntermediateAwardStore? _awardStore;
        private readonly ToolStripMenuItem _awardsMenu = new("Premi intermedi...") { Enabled = false };
        private readonly Func<string, IAbsoluteAwardStore>? _absoluteFactory;
        private IAbsoluteAwardStore? _absoluteStore;
        private readonly ToolStripMenuItem _absoluteMenu = new("Assolute") { Enabled = false };
        private readonly string _settingsPath;

        public MainForm(Func<string, IManifestationStore> storeFactory,
            Func<string, IRegistryStore> registryFactory,
            Func<string, IChoreographyStore> choreographyFactory, Func<string, IRaceStore> raceFactory,
            RecentManifestationStore recentStore, Func<string, IPerformanceStore> performanceFactory,
            ScorePreferencesStore scorePreferences,
            Func<string, IIntermediateAwardStore>? awardFactory = null, AwardAggregation? aggregation = null,
            Func<string, IAbsoluteAwardStore>? absoluteFactory = null, string? settingsPath = null)
        {
            _storeFactory = storeFactory;
            _registryFactory = registryFactory;
            _choreographyFactory = choreographyFactory;
            _raceFactory = raceFactory;
            _recentStore = recentStore;
            _performanceFactory = performanceFactory;
            _scorePreferences = scorePreferences;
            _awardFactory = awardFactory;
            _aggregation = aggregation;
            _absoluteFactory = absoluteFactory;
            _settingsPath = settingsPath ?? Path.Combine(AppContext.BaseDirectory, "skating-settings.json");
            InitializeComponent();
            menuStrip.Items.Add(_awardsMenu);
            _awardsMenu.Click += (_, _) => OpenAwards();
            menuStrip.Items.Add(_absoluteMenu);
            _absoluteMenu.DropDownItems.Add("Assoluta non PRO...", null, (_, _) => OpenAwards(AbsoluteAwardKind.NonPro));
            _absoluteMenu.DropDownItems.Add("Assoluta PRO...", null, (_, _) => OpenAwards(AbsoluteAwardKind.Pro));
            var configurationMenu = new ToolStripMenuItem("Configurazione");
            configurationMenu.DropDownItems.Add("Impostazioni applicazione...", null, (_, _) => OpenSettings());
            menuStrip.Items.Add(configurationMenu);
            RefreshRecentMenu();
        }

        private void OpenSettings()
        {
            try
            {
                using var dialog = new ApplicationSettingsDialog(_settingsPath, _aggregation);
                dialog.ShowDialog(this);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                MessageBox.Show(this, $"Impossibile leggere le impostazioni: {exception.Message}", "Configurazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void NewManifestationClick(object? sender, EventArgs e)
        {
            using var details = new NewManifestationDialog();
            if (details.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Manifestazione Skating (*.skating)|*.skating", DefaultExt = "skating", AddExtension = true,
                Title = "Salva la nuova manifestazione", OverwritePrompt = true,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                OpenManifestation(dialog.FileName, details.ManifestationName);
            }
        }

        private void OpenManifestationClick(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Manifestazione Skating (*.skating)|*.skating", CheckFileExists = true, Title = "Apri manifestazione",
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                OpenManifestation(dialog.FileName, null);
            }
        }

        private void OpenManifestation(string path, string? newName)
        {
            try
            {
                var store = _storeFactory(path);
                if (newName is not null)
                {
                    store.Create(new Manifestation(Guid.NewGuid(), newName));
                }

                var manifestation = store.Load();
                if (store.NeedsUpgrade())
                {
                    using var upgradeDialog = new ArchiveUpgradeDialog();
                    var answer = upgradeDialog.ShowDialog(this);
                    if (answer is not (DialogResult.Yes or DialogResult.No))
                    {
                        return;
                    }

                    store.Upgrade(answer == DialogResult.Yes ? UpgradeBackupMode.CreateBackup : UpgradeBackupMode.SkipBackup);
                }

                _registryStore = _registryFactory(path);
                _choreographyStore = _choreographyFactory(path);
                _raceStore = _raceFactory(path);
                _performanceStore = _performanceFactory(path);
                _awardStore = _awardFactory?.Invoke(path);
                _awardsMenu.Enabled = _awardStore is not null;
                _absoluteStore = _absoluteFactory?.Invoke(path);
                _absoluteMenu.Enabled = _absoluteStore is not null;
                scheduleMenuItem.Enabled = true;
                registryMenuItem.Enabled = true;
                choreographiesMenuItem.Enabled = true;
                racesMenuItem.Enabled = true;
                Text = $"Skating System - {manifestation.Name}";
                welcomeLabel.Text = $"{manifestation.Name}\r\n\r\nArchivio locale: {path}\r\n\r\nAnagrafiche → Coreografie → Gare → Scaletta e voti → Premi intermedi → Assolute.\r\nClassifiche disponibili da Gare. Prototipo: usare dati dimostrativi; ripristino guidato non ancora disponibile.";
                try
                {
                    _recentStore.Remember(manifestation.Name, path);
                    RefreshRecentMenu();
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
                {
                    MessageBox.Show(this, $"La manifestazione è aperta, ma non è stato possibile aggiornare l'elenco dei recenti.\r\n\r\n{exception.Message}", "Manifestazioni recenti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or ArgumentException)
            {
                MessageBox.Show(this, $"Operazione non completata. La manifestazione visualizzata non è stata cambiata.\r\n\r\n{exception.Message}", "Archivio manifestazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AthletesMenuItemClick(object? sender, EventArgs e) => ShowRegistry(RegistryKind.Athlete);

        private void OpenAwards(AbsoluteAwardKind? absoluteKind = null)
        {
            if (_awardStore is null || _registryStore is null || _choreographyStore is null || _raceStore is null || _performanceStore is null)
            {
                return;
            }

            if (absoluteKind != AbsoluteAwardKind.Pro && _aggregation is null)
            {
                MessageBox.Show(this, "Scegliere l'aggregazione in Configurazione → Impostazioni applicazione, poi riavviare l'applicazione.", "Configurazione premi");
                return;
            }

            try
            {
                if (absoluteKind != AbsoluteAwardKind.Pro && _aggregation is AwardAggregation mode && _awardStore.GetAggregation() != mode)
                {
                    var label = mode == AwardAggregation.Discipline ? "disciplina" : "categoria";
                    if (MessageBox.Show(this, $"Associare a questa manifestazione i premi intermedi per {label}, come indicato nel JSON? Dopo l'avvio la scelta non potrà cambiare.",
                        "Configurazione premi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }

                    _awardStore.Configure(mode);
                }

                if (absoluteKind is AbsoluteAwardKind kind && _absoluteStore is not null)
                {
                    using var absolute = new AbsoluteAwardForm(_absoluteStore, _registryStore, _choreographyStore, kind, _aggregation, OpenAwardSource, () => OpenAwards());
                    absolute.ShowDialog(this);
                }
                else if (_aggregation is AwardAggregation intermediateMode)
                {
                    using var dialog = new IntermediateAwardsForm(_awardStore, _registryStore, _choreographyStore, intermediateMode, OpenAwardSource);
                    dialog.ShowDialog(this);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, exception.Message, "Premi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenAwardSource(RaceKey key)
        {
            if (_registryStore is null || _choreographyStore is null || _raceStore is null || _performanceStore is null)
            {
                return;
            }

            var title = $"{_registryStore.GetDisciplines().Single(entry => entry.Id == key.DisciplineId).Name} / {_registryStore.GetCategories().Single(entry => entry.Id == key.CategoryId).Name} / {_registryStore.GetLevels().Single(entry => entry.Id == key.LevelId).Name}";
            using var ranking = new RankingForm(_raceStore, _registryStore, _choreographyStore, key, title, block =>
            {
                using var scores = new ScoreEntryForm(_performanceStore, _registryStore, _choreographyStore, block, _scorePreferences, races: _raceStore);
                scores.ShowDialog(this);
            });
            ranking.ShowDialog(this);
        }

        private void ScheduleMenuItemClick(object? sender, EventArgs e)
        {
            if (_performanceStore is null || _registryStore is null || _choreographyStore is null)
            {
                return;
            }

            using var dialog = new ScheduleForm(_performanceStore, _registryStore, _choreographyStore, _scorePreferences, _raceStore);
            dialog.ShowDialog(this);
        }

        private void RefreshRecentMenu()
        {
            while (manifestationMenuItem.DropDownItems.Count > 3)
            {
                var item = manifestationMenuItem.DropDownItems[3];
                manifestationMenuItem.DropDownItems.Remove(item);
                item.Dispose();
            }

            var entries = _recentStore.Load();
            recentSeparator.Visible = entries.Count > 0;
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var label = $"{entry.Name} ({Path.GetFileName(entry.Path)})".Replace("&", "&&", StringComparison.Ordinal);
                var item = new ToolStripMenuItem($"&{index + 1} {label}") { Tag = entry.Path, ToolTipText = entry.Path };
                item.Click += RecentManifestationClick;
                manifestationMenuItem.DropDownItems.Add(item);
            }
        }

        private void RecentManifestationClick(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem { Tag: string path })
            {
                OpenManifestation(path, null);
            }
        }

        private void DisciplinesMenuItemClick(object? sender, EventArgs e) => ShowRegistry(RegistryKind.Discipline);

        private void CategoriesMenuItemClick(object? sender, EventArgs e) => ShowRegistry(RegistryKind.Category);

        private void LevelsMenuItemClick(object? sender, EventArgs e) => ShowRegistry(RegistryKind.Level);

        private void SchoolsMenuItemClick(object? sender, EventArgs e) => ShowRegistry(RegistryKind.School);

        private void JudgesMenuItemClick(object? sender, EventArgs e) => ShowRegistry(RegistryKind.Judge);

        private void CriteriaMenuItemClick(object? sender, EventArgs e) => ShowRegistry(RegistryKind.Criterion);

        private void RacesMenuItemClick(object? sender, EventArgs e)
        {
            if (_registryStore is null || _choreographyStore is null || _raceStore is null)
            {
                return;
            }

            using var dialog = new RacesForm(_registryStore, _choreographyStore, _raceStore, block =>
            {
                if (_performanceStore is not null)
                {
                    using var scores = new ScoreEntryForm(_performanceStore, _registryStore, _choreographyStore, block, _scorePreferences, races: _raceStore);
                    scores.ShowDialog(this);
                }
            });
            dialog.ShowDialog(this);
        }

        private void ChoreographiesMenuItemClick(object? sender, EventArgs e)
        {
            if (_registryStore is null || _choreographyStore is null)
            {
                return;
            }

            using var dialog = new ChoreographyForm(_registryStore, _choreographyStore);
            dialog.ShowDialog(this);
        }

        private void ShowRegistry(RegistryKind kind)
        {
            if (_registryStore is null)
            {
                return;
            }

            using var dialog = new RegistryForm(_registryStore, kind);
            dialog.ShowDialog(this);
        }
    }
}
