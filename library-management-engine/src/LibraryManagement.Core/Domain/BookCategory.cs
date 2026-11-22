namespace LibraryManagement.Core.Domain;

/// <summary>How a title circulates. The category drives which borrowing rules apply.</summary>
public enum BookCategory
{
    /// <summary>Normal lending collection.</summary>
    General,

    /// <summary>High-demand items (e.g. course texts): short loan period, no renewals.</summary>
    ShortLoan,

    /// <summary>In-library use only (dictionaries, encyclopaedias): never lent out.</summary>
    Reference,
}
