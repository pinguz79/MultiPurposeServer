namespace Skating.Core
{
    /// <summary>Anagrafiche della manifestazione selezionata. Gli elenchi sono snapshot indipendenti dalla connessione.</summary>
    public interface IRegistryStore
    {
        /// <summary>Elenca i giudici per nome.</summary>
        IReadOnlyList<Judge> GetJudges();

        /// <summary>Inserisce un giudice con nome univoco nell'anagrafica del prototipo.</summary>
        void AddJudge(Judge judge);

        /// <summary>Rinomina un giudice mantenendo i collegamenti alle giurie.</summary>
        void UpdateJudge(Judge judge);

        /// <summary>Elenca i criteri comuni della manifestazione.</summary>
        IReadOnlyList<EvaluationCriterion> GetCriteria();

        /// <summary>Inserisce un criterio comune con nome univoco.</summary>
        void AddCriterion(EvaluationCriterion criterion);

        /// <summary>Rinomina un criterio comune.</summary>
        void UpdateCriterion(EvaluationCriterion criterion);

        /// <summary>Elimina un criterio solo prima dell'avvio della manifestazione.</summary>
        void DeleteCriterion(Guid id);

        /// <summary>Elenca le scuole per nome.</summary>
        IReadOnlyList<School> GetSchools();

        /// <summary>Inserisce una scuola con nome univoco, ignorando le maiuscole.</summary>
        void AddSchool(School school);

        /// <summary>Rinomina una scuola senza cambiare l'identità usata nelle coreografie.</summary>
        void UpdateSchool(School school);

        /// <summary>Elenca tutti gli atleti, anche non iscritti, ordinati per cognome e nome.</summary>
        IReadOnlyList<Athlete> GetAthletes();

        /// <summary>Inserisce una nuova identità; nomi uguali non implicano che si tratti della stessa persona.</summary>
        void AddAthlete(Athlete athlete);

        /// <summary>Modifica nome e cognome dell'identità esistente; non crea atleti se l'identificativo manca.</summary>
        void UpdateAthlete(Athlete athlete);

        /// <summary>Elenca le discipline della manifestazione per nome.</summary>
        IReadOnlyList<Discipline> GetDisciplines();

        /// <summary>Inserisce una disciplina; un nome già presente, anche con maiuscole diverse, non è accettato.</summary>
        void AddDiscipline(Discipline discipline);

        /// <summary>Rinomina una disciplina esistente mantenendone l'identità e l'univocità del nome.</summary>
        void UpdateDiscipline(Discipline discipline);

        /// <summary>Elenca le categorie condivise da tutte le discipline, per nome.</summary>
        IReadOnlyList<Category> GetCategories();

        /// <summary>Inserisce una categoria; un nome già presente, anche con maiuscole diverse, non è accettato.</summary>
        void AddCategory(Category category);

        /// <summary>Rinomina una categoria esistente mantenendone l'identità e l'univocità del nome.</summary>
        void UpdateCategory(Category category);

        /// <summary>Elenca i livelli condivisi da tutte le discipline, per nome.</summary>
        IReadOnlyList<Level> GetLevels();

        /// <summary>Inserisce un livello con percorso esplicito; un nome già presente non è accettato.</summary>
        void AddLevel(Level level);

        /// <summary>Modifica nome e percorso PRO di un livello esistente, mantenendone l'identità.</summary>
        void UpdateLevel(Level level);
    }
}
