using System.Collections.Generic;
using System.IO;
using System.Text;
using Spectre.Console;

namespace OpenTOC;

internal class TableOfContent
{
    private const int NumSnoGroups = 70;

    private TableOfContentHeader Header { get; }
    public ITableOfContentEntry[] EntryData { get; }

    private TableOfContent(TableOfContentHeader header, ITableOfContentEntry[] entryData)
    {
        Header = header;
        EntryData = entryData;
    }

    public static TableOfContent Read(BinaryReader reader)
    {
        // This is a function that allows us to move the position of the reader, and return the old position.
        long Move(long pos)
        {
            var old = reader.BaseStream.Position;
            reader.BaseStream.Position = pos;
            return old;
        }
        
        var header = TableOfContentHeader.Read(reader);
        // This is a list of all the entries in the TOC.
        var groupData = new List<ITableOfContentEntry>();
        // We loop through all the groups.

        for (var i = 0; i < NumSnoGroups; i++)
            // If the group has entries, we read them.
        {
            if (header.EntryCounts[i] > 0)
            {
                // We save the current position, so that we can return to it once this loop is finished.
                var currentPosition = Move(header.EntryOffsets[i] + header.SizeOf);
                // We loop through all the entries in the current group.
                for (var j = 0; j < header.EntryCounts[i]; j++)
                {
                    // We read 12 bytes, 4 for each of the 3 values.
                    var snoGroup = reader.ReadInt32(); // + 4 bytes
                    var snoId = reader.ReadInt32(); // + 4 bytes
                    var pName = reader.ReadInt32(); // + 4 bytes

                    // We utilize the "move" function in order to save our current position, so that we can return to it once this loop is finished.
                    var startOfStringPosition = Move(header.EntryOffsets[i] + header.SizeOf +
                                                     12 * header.EntryCounts[i] + pName);

                    // We read the string, byte by byte, until we reach a null byte.
                    StringBuilder name = new();
                    byte currentByte = 0;
                    while ((currentByte = reader.ReadByte()) != 0)
                        name.Append((char)currentByte);

                    // We return to the position before the string, so that we can continue reading the next entry.
                    Move(startOfStringPosition);

                    var nameBuilt = name.ToString();
                    // We add the entry to the list of entries in the current group.
                    groupData.Add(new TableOfContentEntry(snoGroup, snoId, pName, nameBuilt));
                }

                // We return to the position before the group, so that we can continue reading the next group.
                Move(currentPosition);
            }
        }

        return new TableOfContent(header, groupData.ToArray());
    }

    private struct TableOfContentHeader
    {
        // This is the number of groups in the TOC.
        private const int NUM_SNO_GROUPS = 70;
        public readonly int[] EntryCounts = new int[NUM_SNO_GROUPS];
        public readonly int[] EntryOffsets = new int[NUM_SNO_GROUPS];
        public readonly int[] EntryUnkCounts = new int[NUM_SNO_GROUPS];
        public int I0;

        /// <summary>
        /// Calculates sizeof this struct. An array is the length * sizeof(T).
        /// </summary>
        public int SizeOf => EntryCounts.Length * 4 + EntryOffsets.Length * 4 + EntryUnkCounts.Length * 4 + 4;

        public TableOfContentHeader()
        {
            EntryCounts = new int[NUM_SNO_GROUPS];
            EntryOffsets = new int[NUM_SNO_GROUPS];
            EntryUnkCounts = new int[NUM_SNO_GROUPS];
            I0 = 0;
        }

        public static TableOfContentHeader Read(BinaryReader reader)
        {
            var header = new TableOfContentHeader();
            for (var i = 0; i < NUM_SNO_GROUPS; i++) header.EntryCounts[i] = reader.ReadInt32();
            for (var i = 0; i < NUM_SNO_GROUPS; i++) header.EntryOffsets[i] = reader.ReadInt32();
            for (var i = 0; i < NUM_SNO_GROUPS; i++) header.EntryUnkCounts[i] = reader.ReadInt32();
            header.I0 = reader.ReadInt32();
            return header;
        }
    }

    private class TableOfContentEntry : ITableOfContentEntry
    {
        public int SnoGroup { get; }
        public int SnoId { get; }
        public int PName { get; }
        public string Name { get; }

        public TableOfContentEntry(int snoGroup, int snoId, int pName, string name)
        {
            SnoGroup = snoGroup;
            SnoId = snoId;
            PName = pName;
            Name = name;
        }
    }
}