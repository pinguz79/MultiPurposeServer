namespace Skating.Core
{
    /// <summary>Accesso alla singola manifestazione selezionata. Non espone dettagli del database o della UI.</summary>
    public interface IManifestationStore
    {
        /// <summary>Crea una nuova manifestazione senza sostituire dati esistenti.</summary>
        void Create(Manifestation manifestation);

        /// <summary>Legge la manifestazione esistente senza crearla o convertirne implicitamente il formato.</summary>
        Manifestation Load();

        /// <summary>Indica se un archivio leggibile richiede un aggiornamento esplicito per le nuove funzionalità.</summary>
        bool NeedsUpgrade();

        /// <summary>Aggiorna un formato supportato. Crea una copia preventiva salvo rinuncia esplicita; non modifica archivi estranei.</summary>
        void Upgrade(UpgradeBackupMode backupMode = UpgradeBackupMode.CreateBackup);
    }
}
