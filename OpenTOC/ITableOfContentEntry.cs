namespace OpenTOC;

interface ITableOfContentEntry
{
    int SnoGroup { get; }
    int SnoId { get; }
    int PName { get; }
    string Name { get; }
}